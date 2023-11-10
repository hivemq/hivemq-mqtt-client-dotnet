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
using HiveMQtt.Client.Options;
using HiveMQtt.MQTT5;
using HiveMQtt.MQTT5.Packets;
using HiveMQtt.MQTT5.ReasonCodes;

/// <inheritdoc />
public partial class HiveMQClient : IDisposable, IHiveMQClient
{
    // The outgoing packet queue.  Packets queued to be sent.
    private readonly ConcurrentQueue<ControlPacket> sendQueue = new();

    private readonly ConcurrentQueue<ControlPacket> receivedQueue = new();

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

            Logger.Trace($"{Environment.CurrentManagedThreadId}: TrafficOutflowProcessor Starting...{this.connectState}");

            while (this.connectState != ConnectState.Disconnected)
            {
                elapsed = stopWatch.Elapsed;

                if (elapsed > TimeSpan.FromSeconds(keepAlivePeriod))
                {
                    // Send PingReq
                    Logger.Trace("--> PingReq");
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
                        await Task.Delay(50).ConfigureAwait(false);
                        continue;
                    }
                }

                Logger.Trace($"TrafficOutflowProcessor: {this.sendQueue.Count} packets waiting to be sent.");

                // Batch load up to 20 queued packets
                List<ControlPacket> packetsToSend = new();
                while (this.sendQueue.TryDequeue(out var p))
                {
                    packetsToSend.Add(p);
                    if (packetsToSend.Count >= 50)
                    {
                        break;
                    }
                }

                Logger.Trace($"TrafficOutflowProcessor: Sending a batch of {packetsToSend.Count} packets.");
                foreach (var packet in packetsToSend)
                {
                    FlushResult writeResult = default;

                    switch (packet)
                    {
                        // FIXME: Only one connect, subscribe or unsubscribe packet can be sent at a time.
                        case ConnectPacket connectPacket:
                            Logger.Trace($"--> ConnectPacket id={connectPacket.PacketIdentifier}");
                            writeResult = await this.WriteAsync(connectPacket.Encode()).ConfigureAwait(false);
                            this.OnConnectSentEventLauncher(connectPacket);
                            break;
                        case DisconnectPacket disconnectPacket:
                            Logger.Trace($"--> DisconnectPacket id={disconnectPacket.PacketIdentifier}");
                            writeResult = await this.WriteAsync(disconnectPacket.Encode()).ConfigureAwait(false);
                            this.OnDisconnectSentEventLauncher(disconnectPacket);
                            break;
                        case SubscribePacket subscribePacket:
                            Logger.Trace($"--> SubscribePacket id={subscribePacket.PacketIdentifier}");
                            writeResult = await this.WriteAsync(subscribePacket.Encode()).ConfigureAwait(false);
                            this.OnSubscribeSentEventLauncher(subscribePacket);
                            break;
                        case UnsubscribePacket unsubscribePacket:
                            Logger.Trace($"--> UnsubscribePacket id={unsubscribePacket.PacketIdentifier}");
                            writeResult = await this.WriteAsync(unsubscribePacket.Encode()).ConfigureAwait(false);
                            this.OnUnsubscribeSentEventLauncher(unsubscribePacket);
                            break;
                        case PublishPacket publishPacket:
                            Logger.Trace($"--> PublishPacket id={publishPacket.PacketIdentifier}");
                            if (publishPacket.Message.QoS is MQTT5.Types.QualityOfService.AtLeastOnceDelivery ||
                                publishPacket.Message.QoS is MQTT5.Types.QualityOfService.ExactlyOnceDelivery)
                            {
                                // QoS > 0 - Add to transaction queue
                                if (this.transactionQueue.TryAdd(publishPacket.PacketIdentifier, new List<ControlPacket> { publishPacket }) == false)
                                {
                                    Logger.Warn("Duplicate packet ID detected.");
                                    continue;
                                }
                            }

                            writeResult = await this.WriteAsync(publishPacket.Encode()).ConfigureAwait(false);
                            this.OnPublishSentEventLauncher(publishPacket);
                            break;
                        case PubAckPacket pubAckPacket:
                            // This is in response to a received Publish packet.  Communication chain management
                            // was done in the receiver code.  Just send the response.
                            Logger.Trace($"--> PubAckPacket id={pubAckPacket.PacketIdentifier} reason={pubAckPacket.ReasonCode}");
                            writeResult = await this.WriteAsync(pubAckPacket.Encode()).ConfigureAwait(false);
                            this.OnPubAckSentEventLauncher(pubAckPacket);
                            break;
                        case PubRecPacket pubRecPacket:
                            // This is in response to a received Publish packet.  Communication chain management
                            // was done in the receiver code.  Just send the response.
                            Logger.Trace($"--> PubRecPacket id={pubRecPacket.PacketIdentifier} reason={pubRecPacket.ReasonCode}");
                            writeResult = await this.WriteAsync(pubRecPacket.Encode()).ConfigureAwait(false);
                            this.OnPubRecSentEventLauncher(pubRecPacket);
                            break;
                        case PubRelPacket pubRelPacket:
                            // This is in response to a received PubRec packet.  Communication chain management
                            // was done in the receiver code.  Just send the response.
                            Logger.Trace($"--> PubRelPacket id={pubRelPacket.PacketIdentifier} reason={pubRelPacket.ReasonCode}");
                            writeResult = await this.WriteAsync(pubRelPacket.Encode()).ConfigureAwait(false);
                            this.OnPubRelSentEventLauncher(pubRelPacket);
                            break;
                        case PubCompPacket pubCompPacket:
                            // This is in response to a received PubRel packet.  Communication chain management
                            // was done in the receiver code.  Just send the response.
                            Logger.Trace($"--> PubCompPacket id={pubCompPacket.PacketIdentifier} reason={pubCompPacket.ReasonCode}");
                            writeResult = await this.WriteAsync(pubCompPacket.Encode()).ConfigureAwait(false);
                            this.OnPubCompSentEventLauncher(pubCompPacket);
                            break;
                        /* case AuthPacket authPacket:
                        /*     writeResult = await this.writer.WriteAsync(authPacket.Encode()).ConfigureAwait(false);
                        /*     this.OnAuthSentEventLauncher(authPacket);
                        /*     break;
                        */
                        default:
                            Logger.Trace($"--> Unknown packet type {packet}");
                            break;

                    } // switch

                    if (writeResult.IsCanceled)
                    {
                        Logger.Trace("TrafficOutflowProcessor Write Canceled");
                        break;
                    }

                    if (writeResult.IsCompleted)
                    {
                        Logger.Trace("TrafficOutflowProcessor IsCompleted: end of the stream");
                        break;
                    }

                    stopWatch.Restart();
                } // foreach
            } // while

            Logger.Trace($"{Environment.CurrentManagedThreadId}: TrafficOutflowProcessor Exiting...{this.connectState}");
            return true;
        }, cancellationToken);

    /// <summary>
    /// Asynchronous background task that handles the incoming traffic of packets.  Received packets
    /// are queued into this.receivedQueue for processing by ReceivedPacketsProcessorAsync.
    /// </summary>
    private Task<bool> TrafficInflowProcessorAsync(CancellationToken cancellationToken) => Task.Run(
        async () =>
        {
            Logger.Trace($"{Environment.CurrentManagedThreadId}: TrafficInflowProcessor Starting...{this.connectState}");

            ReadResult readResult;

            while (this.connectState is ConnectState.Connecting or ConnectState.Connected)
            {
                readResult = await this.ReadAsync().ConfigureAwait(false);

                if (cancellationToken.IsCancellationRequested || readResult.IsCanceled)
                {
                    Logger.Trace("TrafficInflowProcessor exiting due to cancellation: {cancellationToken.IsCancellationRequested} {readResult.IsCanceled}");
                    break;
                }

                if (readResult.IsCompleted)
                {
                    Logger.Trace("TrafficInflowProcessor IsCompleted: end of the stream");

                    if (this.connectState == ConnectState.Connected)
                    {
                        // This is an unexpected exit and may be due to a network failure.
                        Logger.Trace("TrafficInflowProcessor IsCompleted: was unexpected");
                        this.connectState = ConnectState.Disconnected;

                        // Launch the AfterDisconnect event with a clean disconnect set to false.
                        this.AfterDisconnectEventLauncher(false);

                        this.cancellationSource.Cancel();
                        return false;
                    }

                    return true;
                }

                var buffer = readResult.Buffer;

                while (buffer.Length > 0)
                {
                    if (PacketDecoder.TryDecode(buffer, out var decodedPacket, out var consumed) == false)
                    {
                        if (decodedPacket is MalformedPacket)
                        {
                            var opts = new DisconnectOptions
                            {
                                ReasonCode = DisconnectReasonCode.MalformedPacket,
                                ReasonString = "Malformed Packet",
                            };
                            return await this.DisconnectAsync(opts).ConfigureAwait(false);
                        }

                        // Not enough data in the buffer to decode a packet
                        // Advance the reader to the end of the consumed data
                        buffer = buffer.Slice(0, consumed);
                        this.reader?.AdvanceTo(buffer.Start, readResult.Buffer.End);
                        Logger.Trace("TrafficInflowProcessor: PacketDecoder.TryDecode returned false.  Waiting for more data...");
                        break;
                    }

                    // Advance the reader to indicate how much of the buffer has been consumed
                    buffer = buffer.Slice(consumed);
                    this.reader?.AdvanceTo(buffer.Start);

                    // Add the packet to the received queue for processing later
                    // by ReceivedPacketsProcessorAsync
                    this.receivedQueue.Enqueue(decodedPacket);
                } // while (buffer.Length > 0

            } // while (this.connectState is ConnectState.Connecting or ConnectState.Connected)

            Logger.Trace($"{Environment.CurrentManagedThreadId}: TrafficInflowProcessor Exiting...{this.connectState}");

            return true;
        }, cancellationToken);

    /// <summary>
    /// Continually processes the packets queued in the receivedQueue.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token to stop the task.</param>
    /// <returns>A fairly worthless boolean.</returns>
    private Task<bool> ReceivedPacketsProcessorAsync(CancellationToken cancellationToken) => Task.Run(
        async () =>
        {
            Logger.Trace($"{Environment.CurrentManagedThreadId}: ReceivedPacketsProcessor Starting...{this.connectState}");

            while (true)
            {
                if (this.receivedQueue.IsEmpty)
                {
                    await Task.Delay(50).ConfigureAwait(false);
                    continue;
                }
                else
                {
                    Logger.Trace($"ReceivedPacketsProcessor: {this.receivedQueue.Count} received packets waiting to be processed.");
                }

                if (this.receivedQueue.TryDequeue(out var packet))
                {
                    switch (packet)
                    {
                        case ConnAckPacket connAckPacket:
                            Logger.Trace($"<-- ConnAck id={connAckPacket.PacketIdentifier}");
                            this.OnConnAckReceivedEventLauncher(connAckPacket);
                            break;
                        case DisconnectPacket disconnectPacket:
                            Logger.Trace($"<-- Disconnect id={disconnectPacket.PacketIdentifier}");
                            this.OnDisconnectReceivedEventLauncher(disconnectPacket);
                            break;
                        case PingRespPacket pingRespPacket:
                            Logger.Trace($"<-- PingResp id={pingRespPacket.PacketIdentifier}");
                            this.OnPingRespReceivedEventLauncher(pingRespPacket);
                            break;
                        case SubAckPacket subAckPacket:
                            Logger.Trace($"<-- SubAck id={subAckPacket.PacketIdentifier}");
                            this.OnSubAckReceivedEventLauncher(subAckPacket);
                            break;
                        case UnsubAckPacket unsubAckPacket:
                            Logger.Trace($"<-- UnsubAck id={unsubAckPacket.PacketIdentifier}");
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
                            Logger.Trace("<-- Unknown");
                            Console.WriteLine($"Unknown packet received: {packet}");
                            break;
                    } // switch (packet)
                }

                if (cancellationToken.IsCancellationRequested)
                {
                    Logger.Trace("ReceivedPacketsProcessor Canceled");
                    break;
                }

            } // while

            Logger.Trace($"{Environment.CurrentManagedThreadId}: ReceivedPacketsProcessor Exiting...{this.connectState}");

            return true;
        }, cancellationToken);

    /// <summary>
    /// Handle an incoming Publish packet.
    /// </summary>
    /// <param name="publishPacket">The received publish packet.</param>
    internal void HandleIncomingPublishPacket(PublishPacket publishPacket)
    {
        Logger.Trace($"<-- Publish id={publishPacket.PacketIdentifier}");
        this.OnMessageReceivedEventLauncher(publishPacket);

        if (publishPacket.Message.QoS is MQTT5.Types.QualityOfService.AtLeastOnceDelivery)
        {
            // We've received a QoS 1 publish.  Send a PubAck.
            var pubAckResponse = new PubAckPacket(publishPacket.PacketIdentifier, PubAckReasonCode.Success);
            this.sendQueue.Enqueue(pubAckResponse);
        }
        else if (publishPacket.Message.QoS is MQTT5.Types.QualityOfService.ExactlyOnceDelivery)
        {
            // We've received a QoS 2 publish.  Send a PubRec and add to QoS2 transaction register.
            var pubRecResponse = new PubRecPacket(publishPacket.PacketIdentifier, PubRecReasonCode.Success);
            var publishQoS2Chain = new List<ControlPacket> { publishPacket, pubRecResponse };

            if (this.transactionQueue.TryAdd(publishPacket.PacketIdentifier, publishQoS2Chain) == false)
            {
                Logger.Warn("QoS2: Duplicate packet ID detected.");
                pubRecResponse.ReasonCode = PubRecReasonCode.PacketIdentifierInUse;
            }

            this.sendQueue.Enqueue(pubRecResponse);
        }
    }

    /// <summary>
    /// Handle an incoming ConnAck packet.
    /// </summary>
    /// <param name="pubAckPacket">The received PubAck packet.</param>
    /// <exception cref="HiveMQttClientException">Raised if the packet identifier is unknown.</exception>
    internal void HandleIncomingPubAckPacket(PubAckPacket pubAckPacket)
    {
        Logger.Trace($"<-- PubAck id={pubAckPacket.PacketIdentifier} reason={pubAckPacket.ReasonCode}");
        this.OnPubAckReceivedEventLauncher(pubAckPacket);

        // Remove the transaction chain from the transaction queue
        if (this.transactionQueue.Remove(pubAckPacket.PacketIdentifier, out var publishQoS1Chain))
        {
            var publishPacket = (PublishPacket)publishQoS1Chain.First();

            // Trigger the packet specific event
            publishPacket.OnPublishQoS1CompleteEventLauncher(pubAckPacket);
        }
        else
        {
            Logger.Warn($"QoS1: Received PubAck with an unknown packet identifier {pubAckPacket.PacketIdentifier}. Discarded.");
        }

    }

    /// <summary>
    /// Handle an incoming PubRec packet.
    /// </summary>
    /// <param name="pubRecPacket">The received PubRec packet.</param>
    internal void HandleIncomingPubRecPacket(PubRecPacket pubRecPacket)
    {
        Logger.Trace($"<-- PubRec id={pubRecPacket.PacketIdentifier} reason={pubRecPacket.ReasonCode}");
        this.OnPubRecReceivedEventLauncher(pubRecPacket);

        // Find the QoS2 transaction chain for this packet identifier
        if (this.transactionQueue.TryGetValue(pubRecPacket.PacketIdentifier, out var originalPublishQoS2Chain))
        {
            var originalPublishPacket = (PublishPacket)originalPublishQoS2Chain.First();

            // Create a PUBREL response packet
            var pubRelResponsePacket = new PubRelPacket(pubRecPacket.PacketIdentifier, PubRelReasonCode.Success);

            // Create an updated transaction chain
            var newPublishQoS2Chain = new List<ControlPacket>();
            newPublishQoS2Chain.Add(originalPublishPacket);
            newPublishQoS2Chain.Add(pubRecPacket);
            newPublishQoS2Chain.Add(pubRelResponsePacket);

            // Update the chain in the queue
            if (this.transactionQueue.TryUpdate(pubRecPacket.PacketIdentifier, newPublishQoS2Chain, originalPublishQoS2Chain) == false)
            {
                Logger.Warn($"QoS2: Couldn't update PubRec --> PubRel QoS2 Chain for packet identifier {pubRecPacket.PacketIdentifier}.");
            }

            // Send the PUBREL response
            this.sendQueue.Enqueue(pubRelResponsePacket);
        }
        else
        {
            // Send a PUBREL with PacketIdentifierNotFound
            var pubRelResponsePacket = new PubRelPacket(pubRecPacket.PacketIdentifier, PubRelReasonCode.PacketIdentifierNotFound);
            this.sendQueue.Enqueue(pubRelResponsePacket);
        }

    }

    /// <summary>
    /// Handle an incoming PubRel packet.
    /// </summary>
    /// <param name="pubRelPacket">The received PubRel packet.</param>
    internal void HandleIncomingPubRelPacket(PubRelPacket pubRelPacket)
    {
        Logger.Trace($"<-- PubRel id={pubRelPacket.PacketIdentifier} reason={pubRelPacket.ReasonCode}");
        this.OnPubRelReceivedEventLauncher(pubRelPacket);

        if (this.transactionQueue.TryGetValue(pubRelPacket.PacketIdentifier, out var originalPublishQoS2Chain))
        {
            var originalPublishPacket = (PublishPacket)originalPublishQoS2Chain.First();

            // Send a PUBCOMP in response
            var pubCompResponsePacket = new PubCompPacket(pubRelPacket.PacketIdentifier, PubCompReasonCode.Success);

            // This QoS2 transaction chain is done.  Remove it from the transaction queue.
            if (this.transactionQueue.TryRemove(pubRelPacket.PacketIdentifier, out var publishQoS2Chain))
            {
                // Update the chain with the latest packets for the event launcher
                publishQoS2Chain.Add(pubRelPacket);
                publishQoS2Chain.Add(pubCompResponsePacket);

                // Trigger the packet specific event
                originalPublishPacket.OnPublishQoS2CompleteEventLauncher(publishQoS2Chain);
            }
            else
            {
                Logger.Warn($"QoS2: Couldn't remove PubRel --> PubComp QoS2 Chain for packet identifier {pubRelPacket.PacketIdentifier}.");
            }

            this.sendQueue.Enqueue(pubCompResponsePacket);
        }
        else
        {
            Logger.Warn($"QoS2: Received PubRel with an unknown packet identifier {pubRelPacket.PacketIdentifier}. " +
                         "Responding with PubComp PacketIdentifierNotFound.");

            // Send a PUBCOMP with PacketIdentifierNotFound
            var pubCompResponsePacket = new PubCompPacket(pubRelPacket.PacketIdentifier, PubCompReasonCode.PacketIdentifierNotFound);
            this.sendQueue.Enqueue(pubCompResponsePacket);
        }
    }

    /// <summary>
    /// Handle an incoming PubComp packet.
    /// </summary>
    /// <param name="pubCompPacket">The received PubComp packet.</param>
    /// <exception cref="HiveMQttClientException">Raised if the packet identifier is unknown.</exception>
    internal void HandleIncomingPubCompPacket(PubCompPacket pubCompPacket)
    {
        Logger.Trace($"<-- PubComp id={pubCompPacket.PacketIdentifier} reason={pubCompPacket.ReasonCode}");
        this.OnPubCompReceivedEventLauncher(pubCompPacket);

        // Remove the QoS 2 transaction chain from the queue
        if (this.transactionQueue.Remove(pubCompPacket.PacketIdentifier, out var publishQoS2Chain))
        {
            var originalPublishPacket = (PublishPacket)publishQoS2Chain.First();

            // Update the chain with this PubComp packet for the event launcher
            publishQoS2Chain.Add(pubCompPacket);

            // Trigger the packet specific event with the entire chain
            originalPublishPacket.OnPublishQoS2CompleteEventLauncher(publishQoS2Chain);
        }
        else
        {
            Logger.Warn($"QoS2: Received PubComp with an unknown packet identifier {pubCompPacket.PacketIdentifier}. Discarded.");
        }
    }

    /// <summary>
    /// Write a buffer to the stream.
    /// </summary>
    /// <param name="source">The buffer to write.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A FlushResult wrapped in a ValueTask.</returns>
    /// <exception cref="HiveMQttClientException">Raised if the writer is null.</exception>
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
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A ReadResult wrapped in a ValueTask.</returns>
    /// <exception cref="HiveMQttClientException">Raised if the reader is null.</exception>
    internal async ValueTask<ReadResult> ReadAsync(CancellationToken cancellationToken = default)
    {
        if (this.reader is null)
        {
            throw new HiveMQttClientException("Reader is null");
        }

        var readResult = await this.reader.ReadAsync(cancellationToken).ConfigureAwait(false);
        Logger.Trace($"ReadAsync: Read Buffer Length {readResult.Buffer.Length}");
        return readResult;
    }
}
