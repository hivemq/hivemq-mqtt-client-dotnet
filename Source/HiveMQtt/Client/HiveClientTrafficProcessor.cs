namespace HiveMQtt.Client;

using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO.Pipelines;
using System.Threading.Tasks;
using HiveMQtt.Client.Exceptions;
using HiveMQtt.Client.Internal;
using HiveMQtt.MQTT5;
using HiveMQtt.MQTT5.Packets;
using HiveMQtt.MQTT5.ReasonCodes;

/// <inheritdoc />
public partial class HiveClient : IDisposable, IHiveClient
{
    // The outgoing packet queue.  Packets queued to be sent.
    private readonly ConcurrentQueue<ControlPacket> sendQueue = new();

    // The incoming packets queue.  Packets to be processed.
    private readonly ConcurrentQueue<ControlPacket> receiveQueue = new();

    // Transactional packets indexed by packet identifer
    private readonly ConcurrentDictionary<int, List<ControlPacket>> transactionQueue = new();

    /// <summary>
    /// Asynchronous background task that handles the outgoing traffic of packets queued in the sendQueue.
    /// </summary>
    private Task<bool> TrafficOutflowProcessorAsync()
    {
        return Task.Run(async () =>
        {
            var stopWatch = new Stopwatch();
            var keepAlivePeriod = this.Options.KeepAlive / 2;
            stopWatch.Start();

            while (this.connectState != ConnectState.Disconnected)
            {
                var elapsed = stopWatch.Elapsed;

                if (elapsed > TimeSpan.FromSeconds(keepAlivePeriod))
                {
                    // Send PingReq
                    var writeResult = await this.writer.WriteAsync(PingReqPacket.Encode()).ConfigureAwait(false);
                    var flushResult = await this.writer.FlushAsync().ConfigureAwait(false);
                    this.OnPingReqSentEventLauncher(new PingReqPacket());
                    stopWatch.Restart();
                }

                if (this.sendQueue.IsEmpty)
                {
                    if (this.connectState == ConnectState.Disconnecting)
                    {
                        return true;
                    }
                    else
                    {
                        await Task.Delay(100).ConfigureAwait(false);
                        continue;
                    }
                }

                if (this.sendQueue.TryDequeue(out var packet))
                {
                    FlushResult writeResult;
                    // FIXME: Handle writeResult.IsCanceled and writeResult.IsCompleted

                    switch (packet)
                    {
                        // FIXME: Only one connect, subscribe or unsubscribe packet can be sent at a time.
                        case ConnectPacket connectPacket:
                            writeResult = await this.writer.WriteAsync(connectPacket.Encode()).ConfigureAwait(false);
                            this.OnConnectSentEventLauncher(connectPacket);
                            break;
                        case DisconnectPacket disconnectPacket:
                            writeResult = await this.writer.WriteAsync(disconnectPacket.Encode()).ConfigureAwait(false);
                            this.OnDisconnectSentEventLauncher(disconnectPacket);
                            break;
                        case SubscribePacket subscribePacket:
                            writeResult = await this.writer.WriteAsync(subscribePacket.Encode()).ConfigureAwait(false);
                            this.OnSubscribeSentEventLauncher(subscribePacket);
                            break;
                        case UnsubscribePacket unsubscribePacket:
                            writeResult = await this.writer.WriteAsync(unsubscribePacket.Encode()).ConfigureAwait(false);
                            this.OnUnsubscribeSentEventLauncher(unsubscribePacket);
                            break;
                        case PublishPacket publishPacket:
                            writeResult = await this.writer.WriteAsync(publishPacket.Encode()).ConfigureAwait(false);
                            if (publishPacket.Message.QoS is MQTT5.Types.QualityOfService.AtLeastOnceDelivery ||
                                publishPacket.Message.QoS is MQTT5.Types.QualityOfService.ExactlyOnceDelivery)
                            {
                                // QoS > 0 - Add to transaction queue
                                if (this.transactionQueue.TryAdd(publishPacket.PacketIdentifier, new List<ControlPacket> { publishPacket }) == false)
                                {
                                    throw new HiveMQttClientException("Duplicate packet ID detected.");
                                }
                            }

                            this.OnPublishSentEventLauncher(publishPacket);
                            break;
                        case PubAckPacket pubAckPacket:
                            // This is in response to a received Publish packet.  Communication chain management
                            // was done in the receiver code.  Just send the response.
                            writeResult = await this.writer.WriteAsync(pubAckPacket.Encode()).ConfigureAwait(false);
                            this.OnPubAckSentEventLauncher(pubAckPacket);
                            break;
                        case PubRecPacket pubRecPacket:
                            // This is in response to a received Publish packet.  Communication chain management
                            // was done in the receiver code.  Just send the response.
                            writeResult = await this.writer.WriteAsync(pubRecPacket.Encode()).ConfigureAwait(false);
                            this.OnPubRecSentEventLauncher(pubRecPacket);
                            break;
                        case PubRelPacket pubRelPacket:
                            // This is in response to a received PubRec packet.  Communication chain management
                            // was done in the receiver code.  Just send the response.
                            writeResult = await this.writer.WriteAsync(pubRelPacket.Encode()).ConfigureAwait(false);
                            this.OnPubRelSentEventLauncher(pubRelPacket);
                            break;
                        case PubCompPacket pubCompPacket:
                            // This is in response to a received PubRel packet.  Communication chain management
                            // was done in the receiver code.  Just send the response.
                            writeResult = await this.writer.WriteAsync(pubCompPacket.Encode()).ConfigureAwait(false);
                            this.OnPubCompSentEventLauncher(pubCompPacket);
                            break;
                        // case AuthPacket authPacket:
                        //     writeResult = await this.writer.WriteAsync(authPacket.Encode()).ConfigureAwait(false);
                        //     this.OnAuthSentEventLauncher(authPacket);
                        //     break;

                        default:
                            throw new NotImplementedException();
                    }

                    var flushResult = await this.writer.FlushAsync().ConfigureAwait(false);
                    stopWatch.Restart();

                    // FIXME: Handle flushResult.IsCanceled and flushResult.IsCompleted
                }

            } // while
            return true;
        });
    }

    /// <summary>
    /// Asynchronous background task that handles the incoming traffic of packets.
    /// </summary>
    private Task<bool> TrafficInflowProcessorAsync()
    {
        return Task.Run(async () =>
        {
            while (this.connectState == ConnectState.Connecting || this.connectState == ConnectState.Connected)
            {
                var readResult = await this.reader.ReadAsync().ConfigureAwait(false);
                var packet = PacketDecoder.Decode(readResult.Buffer, out var consumed);

                if (packet is PartialPacket)
                {
                    continue;
                }
                else if (packet is MalformedPacket)
                {
                    // FIXME: Handle malformed packets
                    this.reader.AdvanceTo(consumed);
                    continue;
                }

                // We have a valid packet.  Mark the data as consumed.
                this.reader.AdvanceTo(consumed);

                switch (packet)
                {
                    case ConnAckPacket connAckPacket:
                        this.OnConnAckReceivedEventLauncher(connAckPacket);
                        this.receiveQueue.Enqueue(connAckPacket);
                        break;
                    case PingRespPacket pingRespPacket:
                        this.OnPingRespReceivedEventLauncher(pingRespPacket);
                        break;
                    case SubAckPacket subAckPacket:
                        this.OnSubAckReceivedEventLauncher(subAckPacket);
                        break;
                    case UnsubAckPacket unsubAckPacket:
                        this.OnUnsubAckReceivedEventLauncher(unsubAckPacket);
                        break;
                    case PublishPacket publishPacket:
                        if (publishPacket.Message.QoS is MQTT5.Types.QualityOfService.AtLeastOnceDelivery)
                        {
                            // We've received a QoS 1 publish.  Send a PubAck.
                            var pubAckResponse = new PubAckPacket(publishPacket.PacketIdentifier, PubAckReasonCode.Success);
                            this.sendQueue.Enqueue(pubAckResponse);
                        }
                        else if (publishPacket.Message.QoS is MQTT5.Types.QualityOfService.ExactlyOnceDelivery)
                        {
                            // We've received a QoS 2 publish.  Send a PubRec and add to transaction list.
                            var pubRecResponse = new PubRecPacket(publishPacket.PacketIdentifier, PubRecReasonCode.Success);
                            var transaction = new List<ControlPacket> { publishPacket, pubRecResponse };

                            if (this.transactionQueue.TryAdd(publishPacket.PacketIdentifier, new List<ControlPacket> { publishPacket }) == false)
                            {
                                // FIXME: Log, trace to assist debugging
                                pubRecResponse.ReasonCode = PubRecReasonCode.PacketIdentifierInUse;
                            }
                            this.sendQueue.Enqueue(pubRecResponse);
                        }
                        this.OnMessageReceivedEventLauncher(publishPacket);
                        break;
                    case PubAckPacket pubAckPacket:
                        this.OnPubAckReceivedEventLauncher(pubAckPacket);
                        if (this.transactionQueue.Remove(pubAckPacket.PacketIdentifier, out var publishQoS1Chain))
                        {
                            var publishPacket = (PublishPacket)publishQoS1Chain.First();
                            // Trigger the packet specific event
                            publishPacket.OnPublishQoS1CompleteEventLauncher(pubAckPacket);
                        }
                        else
                        {
                            throw new HiveMQttClientException("Received PubAck with an unknown packet identifier: ¯\\_(ツ)_/¯");
                        }

                        break;
                    case PubRecPacket pubRecPacket:
                        this.OnPubRecReceivedEventLauncher(pubRecPacket);
                        PubRelPacket pubRelResponsePacket;
                        if (this.transactionQueue.TryGetValue(pubRecPacket.PacketIdentifier, out var publishQoS2Chain))
                        {
                            var publishPacket = (PublishPacket)publishQoS2Chain.First();
                            // Trigger the packet specific event
                            publishPacket.OnPublishQoS2CompleteEventLauncher(pubRecPacket);

                            // Add the PUBREC to the chain
                            publishQoS2Chain.Add(pubRecPacket);

                            // Send and add a PUBREL to the chain
                            pubRelResponsePacket = new PubRelPacket(pubRecPacket.PacketIdentifier, PubRelReasonCode.Success);
                            this.sendQueue.Enqueue(pubRelResponsePacket);
                            publishQoS2Chain.Add(pubRelResponsePacket);
                        }
                        else
                        {
                            // Send a PUBREL with PacketIdentifierNotFound
                            pubRelResponsePacket = new PubRelPacket(pubRecPacket.PacketIdentifier, PubRelReasonCode.PacketIdentifierNotFound);
                            this.sendQueue.Enqueue(pubRelResponsePacket);
                        }

                        break;
                    case PubRelPacket pubRelPacket:
                        PubCompPacket pubCompResponsePacket;
                        if (this.transactionQueue.TryGetValue(pubRelPacket.PacketIdentifier, out var pubRelQoS2Chain))
                        {
                        }
                        else
                        {
                            // Send a PUBCOMP with PacketIdentifierNotFound
                            pubCompResponsePacket = new PubCompPacket(pubRelPacket.PacketIdentifier, PubCompReasonCode.PacketIdentifierNotFound);
                            this.sendQueue.Enqueue(pubCompResponsePacket);
                        }

                        this.OnPubRelReceivedEventLauncher(pubRelPacket);
                        break;
                    case PubCompPacket pubCompPacket:
                        if (this.transactionQueue.Remove(pubCompPacket.PacketIdentifier, out var pubcompQoS2Chain))
                        {

                        }
                        else
                        {
                            throw new HiveMQttClientException("Received PubComp with an unknown packet identifier: ¯\\_(ツ)_/¯");
                        }
                        this.OnPubCompReceivedEventLauncher(pubCompPacket);

                        break;
                    default:
                        Console.WriteLine($"Unknown packet received: {packet}");
                        break;
                } // switch (packet)
            } // while

            return true;
        });
    }
}
