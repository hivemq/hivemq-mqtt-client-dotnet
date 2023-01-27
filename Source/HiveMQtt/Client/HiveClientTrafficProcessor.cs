namespace HiveMQtt.Client;

using System;
using System.Diagnostics;
using System.IO.Pipelines;
using System.Threading.Tasks;
using HiveMQtt.Client.Internal;
using HiveMQtt.MQTT5;
using HiveMQtt.MQTT5.Packets;

/// <inheritdoc />
public partial class HiveClient : IDisposable, IHiveClient
{
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
                        // case PublishPacket publishPacket:
                        //     writeResult = await this.writer.WriteAsync(publishPacket.Encode()).ConfigureAwait(false);
                        //     this.OnPublishSentEventLauncher(publishPacket);
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
                    case PublishPacket publishPacket:
                        this.OnMessageReceivedEventLauncher(publishPacket);
                        break;
                    case UnsubAckPacket unsubAckPacket:
                        this.OnUnsubAckReceivedEventLauncher(unsubAckPacket);
                        break;
                    case PubAckPacket pubAckPacket:
                        this.OnPubAckReceivedEventLauncher(pubAckPacket);
                        break;
                    case PubRecPacket pubRecPacket:
                        this.OnPubRecReceivedEventLauncher(pubRecPacket);
                        break;
                    case PubRelPacket pubRelPacket:
                        this.OnPubRelReceivedEventLauncher(pubRelPacket);
                        break;
                    case PubCompPacket pubCompPacket:
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
