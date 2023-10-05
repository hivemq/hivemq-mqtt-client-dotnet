/*
 * Copyright 2022-present HiveMQ and the HiveMQ Community
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
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
public partial class HiveMQClient : IDisposable, IHiveMQClient
{
    // The outgoing packet queue.  Packets queued to be sent.
    private readonly ConcurrentQueue<ControlPacket> sendQueue = new();

    // Transactional packets indexed by packet identifier
    private readonly ConcurrentDictionary<int, List<ControlPacket>> transactionQueue = new();

    /// <summary>
    /// Asynchronous background task that handles the outgoing traffic of packets queued in the sendQueue.
    /// </summary>
    private Task<bool> TrafficOutflowProcessorAsync(CancellationToken cancellationToken) => Task.Run(
        async () =>
        {
            var stopWatch = new Stopwatch();
            var keepAlivePeriod = this.Options.KeepAlive / 2;
            TimeSpan elapsed;

            stopWatch.Start();

            logger.Trace($"{Environment.CurrentManagedThreadId}: TrafficOutflowProcessor Starting...{this.connectState}");

            while (this.connectState != ConnectState.Disconnected)
            {
                elapsed = stopWatch.Elapsed;

                if (elapsed > TimeSpan.FromSeconds(keepAlivePeriod))
                {
                    // Send PingReq
                    logger.Trace("--> PingReq");
                    var writeResult = await this.WriteAsync(PingReqPacket.Encode()).ConfigureAwait(false);
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

                    switch (packet)
                    {
                        // FIXME: Only one connect, subscribe or unsubscribe packet can be sent at a time.
                        case ConnectPacket connectPacket:
                            logger.Trace("--> ConnectPacket");
                            writeResult = await this.WriteAsync(connectPacket.Encode()).ConfigureAwait(false);
                            this.OnConnectSentEventLauncher(connectPacket);
                            break;
                        case DisconnectPacket disconnectPacket:
                            logger.Trace("--> DisconnectPacket");
                            writeResult = await this.WriteAsync(disconnectPacket.Encode()).ConfigureAwait(false);
                            this.OnDisconnectSentEventLauncher(disconnectPacket);
                            break;
                        case SubscribePacket subscribePacket:
                            logger.Trace("--> SubscribePacket");
                            writeResult = await this.WriteAsync(subscribePacket.Encode()).ConfigureAwait(false);
                            this.OnSubscribeSentEventLauncher(subscribePacket);
                            break;
                        case UnsubscribePacket unsubscribePacket:
                            logger.Trace("--> UnsubscribePacket");
                            writeResult = await this.WriteAsync(unsubscribePacket.Encode()).ConfigureAwait(false);
                            this.OnUnsubscribeSentEventLauncher(unsubscribePacket);
                            break;
                        case PublishPacket publishPacket:
                            logger.Trace("--> PublishPacket");
                            if (publishPacket.Message.QoS is MQTT5.Types.QualityOfService.AtLeastOnceDelivery ||
                                publishPacket.Message.QoS is MQTT5.Types.QualityOfService.ExactlyOnceDelivery)
                            {
                                // QoS > 0 - Add to transaction queue
                                if (this.transactionQueue.TryAdd(publishPacket.PacketIdentifier, new List<ControlPacket> { publishPacket }) == false)
                                {
                                    throw new HiveMQttClientException("Duplicate packet ID detected.");
                                }
                            }

                            writeResult = await this.WriteAsync(publishPacket.Encode()).ConfigureAwait(false);

                            this.OnPublishSentEventLauncher(publishPacket);
                            break;
                        case PubAckPacket pubAckPacket:
                            // This is in response to a received Publish packet.  Communication chain management
                            // was done in the receiver code.  Just send the response.
                            logger.Trace("--> PubAckPacket");
                            writeResult = await this.WriteAsync(pubAckPacket.Encode()).ConfigureAwait(false);
                            this.OnPubAckSentEventLauncher(pubAckPacket);
                            break;
                        case PubRecPacket pubRecPacket:
                            // This is in response to a received Publish packet.  Communication chain management
                            // was done in the receiver code.  Just send the response.
                            logger.Trace("--> PubRecPacket");
                            writeResult = await this.WriteAsync(pubRecPacket.Encode()).ConfigureAwait(false);
                            this.OnPubRecSentEventLauncher(pubRecPacket);
                            break;
                        case PubRelPacket pubRelPacket:
                            // This is in response to a received PubRec packet.  Communication chain management
                            // was done in the receiver code.  Just send the response.
                            logger.Trace("--> PubRelPacket");
                            writeResult = await this.WriteAsync(pubRelPacket.Encode()).ConfigureAwait(false);
                            this.OnPubRelSentEventLauncher(pubRelPacket);
                            break;
                        case PubCompPacket pubCompPacket:
                            // This is in response to a received PubRel packet.  Communication chain management
                            // was done in the receiver code.  Just send the response.
                            logger.Trace("--> PubCompPacket");
                            writeResult = await this.WriteAsync(pubCompPacket.Encode()).ConfigureAwait(false);
                            this.OnPubCompSentEventLauncher(pubCompPacket);
                            break;

                        /* case AuthPacket authPacket:
                        /*     writeResult = await this.writer.WriteAsync(authPacket.Encode()).ConfigureAwait(false);
                        /*     this.OnAuthSentEventLauncher(authPacket);
                        /*     break;
                        */

                        default:
                            logger.Trace("--> Unknown packet type");
                            throw new NotImplementedException();
                    }

                    if (writeResult.IsCanceled)
                    {
                        logger.Trace("TrafficOutflowProcessor Write Canceled");
                        break;
                    }

                    if (writeResult.IsCompleted)
                    {
                        logger.Trace("TrafficOutflowProcessor IsCompleted: end of the stream");
                        break;
                    }

                    stopWatch.Restart();
                }
            } // while

            logger.Trace($"{Environment.CurrentManagedThreadId}: TrafficOutflowProcessor Exiting...{this.connectState}");
            return true;
        }, cancellationToken);

    /// <summary>
    /// Asynchronous background task that handles the incoming traffic of packets.
    /// </summary>
    private Task<bool> TrafficInflowProcessorAsync(CancellationToken cancellationToken) => Task.Run(
        async () =>
        {
            logger.Trace($"{Environment.CurrentManagedThreadId}: TrafficInflowProcessor Starting...{this.connectState}");

            ReadResult readResult;

            while (this.connectState is ConnectState.Connecting or ConnectState.Connected)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    logger.Trace("TrafficInflowProcessor Canceled");
                    break;
                }

                readResult = await this.ReadAsync().ConfigureAwait(false);

                if (readResult.IsCanceled)
                {
                    logger.Trace("TrafficInflowProcessor Read Canceled");
                    break;
                }

                if (readResult.IsCompleted)
                {
                    // This is an unexpected exit and may be due to a network failure.
                    logger.Trace("TrafficInflowProcessor IsCompleted: end of the streamx");

                    if (this.connectState == ConnectState.Connected)
                    {
                        logger.Trace("TrafficInflowProcessor IsCompleted: was unexpected");
                        this.connectState = ConnectState.Disconnected;

                        // Launch the AfterDisconnect event with a clean disconnect set to false.
                        this.AfterDisconnectEventLauncher(false);

                        this.cancellationSource.Cancel();
                        return false;
                    }
                    return true;
                }

                if (readResult.Buffer.IsEmpty)
                {
                    logger.Trace("TrafficInflowProcessor Read Buffer Empty");
                    continue;
                }

                ControlPacket packet;
                SequencePosition consumed;
                try
                {
                    // Decode the packet
                    packet = PacketDecoder.Decode(readResult.Buffer, out consumed);
                }
                catch (Exception ex)
                {
                    logger.Trace($"TrafficInflowProcessor Decoding Exception: {ex.Message}");
                    throw;
                }

                if (packet is PartialPacket)
                {
                    continue;
                }
                else if (packet is MalformedPacket)
                {
                    logger.Trace("TrafficInflowProcessor Malformed Packet Detected !!! Skipping...");
                    this.reader?.AdvanceTo(consumed);
                    continue;
                }

                // We have a valid packet.  Mark the data as consumed.
                this.reader?.AdvanceTo(consumed);

                switch (packet)
                {
                    case ConnAckPacket connAckPacket:
                        logger.Trace("<-- ConnAck");
                        this.OnConnAckReceivedEventLauncher(connAckPacket);
                        break;
                    case DisconnectPacket disconnectPacket:
                        logger.Trace("<-- Disconnect");
                        this.OnDisconnectReceivedEventLauncher(disconnectPacket);
                        break;
                    case PingRespPacket pingRespPacket:
                        logger.Trace("<-- PingResp");
                        this.OnPingRespReceivedEventLauncher(pingRespPacket);
                        break;
                    case SubAckPacket subAckPacket:
                        logger.Trace("<-- SubAck");
                        this.OnSubAckReceivedEventLauncher(subAckPacket);
                        break;
                    case UnsubAckPacket unsubAckPacket:
                        logger.Trace("<-- UnsubAck");
                        this.OnUnsubAckReceivedEventLauncher(unsubAckPacket);
                        break;
                    case PublishPacket publishPacket:
                        this.HandleIncomingPublishPacket(publishPacket);
                        break;
                    case PubAckPacket pubAckPacket:
                        this.HandleIncomingPubAckPacket(pubAckPacket);
                        break;
                    case PubRecPacket pubRecPacket:
                        this.HandleIncomingPubRecPacket(pubRecPacket);
                        break;
                    case PubRelPacket pubRelPacket:
                        this.HandleIncomingPubRelPacket(pubRelPacket);
                        break;
                    case PubCompPacket pubCompPacket:
                        this.HandleIncomingPubCompPacket(pubCompPacket);
                        break;
                    default:
                        logger.Trace("<-- Unknown");
                        Console.WriteLine($"Unknown packet received: {packet}");
                        break;
                } // switch (packet)
            } // while

            logger.Trace($"{Environment.CurrentManagedThreadId}: TrafficInflowProcessor Exiting...{this.connectState}");

            return true;
        }, cancellationToken);

    /// <summary>
    /// Handle an incoming Publish packet.
    /// </summary>
    /// <param name="publishPacket"></param>
    internal void HandleIncomingPublishPacket(PublishPacket publishPacket)
    {
        logger.Trace("<-- Publish");
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
    }

    /// <summary>
    /// Handle an incoming ConnAck packet.
    /// </summary>
    /// <param name="pubAckPacket"></param>
    /// <exception cref="HiveMQttClientException"></exception>
    internal void HandleIncomingPubAckPacket(PubAckPacket pubAckPacket)
    {
        logger.Trace("<-- PubAck");
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

        this.OnPubAckReceivedEventLauncher(pubAckPacket);
    }

    /// <summary>
    /// Handle an incoming PubRec packet.
    /// </summary>
    /// <param name="pubRecPacket"></param>
    internal void HandleIncomingPubRecPacket(PubRecPacket pubRecPacket)
    {
        logger.Trace("<-- PubRec");
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
            pubRelResponsePacket = new PubRelPacket(pubRecPacket.PacketIdentifier, PubRelReasonCode.PacketIdentifierNotFound);
            this.sendQueue.Enqueue(pubRelResponsePacket);
        }

        this.OnPubRecReceivedEventLauncher(pubRecPacket);
    }

    /// <summary>
    /// Handle an incoming PubRel packet.
    /// </summary>
    /// <param name="pubRelPacket"></param>
    internal void HandleIncomingPubRelPacket(PubRelPacket pubRelPacket)
    {
        logger.Trace("<-- PubRel");
        PubCompPacket pubCompResponsePacket;
        if (this.transactionQueue.TryGetValue(pubRelPacket.PacketIdentifier, out var pubRelQoS2Chain))
        {
            // Send a PUBCOMP
            pubCompResponsePacket = new PubCompPacket(pubRelPacket.PacketIdentifier, PubCompReasonCode.Success);
        }
        else
        {
            // Send a PUBCOMP with PacketIdentifierNotFound
            pubCompResponsePacket = new PubCompPacket(pubRelPacket.PacketIdentifier, PubCompReasonCode.PacketIdentifierNotFound);
        }

        this.sendQueue.Enqueue(pubCompResponsePacket);
        this.OnPubRelReceivedEventLauncher(pubRelPacket);
    }

    /// <summary>
    /// Handle an incoming PubComp packet.
    /// </summary>
    /// <param name="pubCompPacket"></param>
    /// <exception cref="HiveMQttClientException"></exception>
    internal void HandleIncomingPubCompPacket(PubCompPacket pubCompPacket)
    {
        logger.Trace("<-- PubComp");
        if (!this.transactionQueue.Remove(pubCompPacket.PacketIdentifier, out var pubcompQoS2Chain))
        {
            throw new HiveMQttClientException("Received PubComp with an unknown packet identifier: ¯\\_(ツ)_/¯");
        }

        this.OnPubCompReceivedEventLauncher(pubCompPacket);
    }

    /// <summary>
    /// Write a buffer to the stream.
    /// </summary>
    /// <param name="source"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="HiveMQttClientException"></exception>
    internal ValueTask<FlushResult> WriteAsync(ReadOnlyMemory<byte> source, CancellationToken cancellationToken = default)
    {
        if (this.writer is null)
        {
            throw new HiveMQttClientException("Writer is null");
        }

        return this.writer.WriteAsync(source, cancellationToken);
    }

    /// <summary>
    /// Read a buffer from the stream.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="HiveMQttClientException"></exception>
    internal ValueTask<ReadResult> ReadAsync(CancellationToken cancellationToken = default)
    {
        if (this.reader is null)
        {
            throw new HiveMQttClientException("Reader is null");
        }

        return this.reader.ReadAsync(cancellationToken);
    }
}
