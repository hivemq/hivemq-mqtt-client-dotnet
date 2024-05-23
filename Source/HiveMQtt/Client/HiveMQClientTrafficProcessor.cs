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
using HiveMQtt.MQTT5.Types;

/// <inheritdoc />
public partial class HiveMQClient : IDisposable, IHiveMQClient
{
    internal MQTT5Properties ConnectionProperties { get; set; } = new();

    internal AwaitableQueueX<PublishPacket> OutgoingPublishQueue { get; } = new();

    internal AwaitableQueueX<ControlPacket> SendQueue { get; } = new();

    internal AwaitableQueueX<ControlPacket> ReceivedQueue { get; } = new();

    // Transactional packets indexed by packet identifier
    internal ConcurrentDictionary<int, List<ControlPacket>> TransactionQueue { get; } = new();

    private readonly Stopwatch lastCommunicationTimer = new();

    /// <summary>
    /// Asynchronous background task that monitors the connection state and sends PingReq packets when
    /// necessary.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A boolean return indicating exit state.</returns>
    private Task<bool> ConnectionMonitorAsync(CancellationToken cancellationToken) => Task.Run(
        async () =>
        {
            var keepAlivePeriod = this.Options.KeepAlive / 2;
            Logger.Trace($"{this.Options.ClientId}-(CM)- Starting...{this.ConnectState}");

            while (true)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    Logger.Trace($"{this.Options.ClientId}-(CM)- Cancelled");
                    break;
                }

                // If connected and no recent traffic, send a ping
                if (this.ConnectState == ConnectState.Connected)
                {
                    if (this.lastCommunicationTimer.Elapsed > TimeSpan.FromSeconds(keepAlivePeriod))
                    {
                        // Send PingReq
                        Logger.Trace($"{this.Options.ClientId}-(CM)- --> PingReq");
                        this.SendQueue.Enqueue(new PingReqPacket());
                    }
                }

                // Dumping Client State
                Logger.Trace($"{this.Options.ClientId}-(CM)- {this.ConnectState} lastCommunicationTimer:{this.lastCommunicationTimer.Elapsed}");
                Logger.Trace($"{this.Options.ClientId}-(CM)- SendQueue:{this.SendQueue.Count} ReceivedQueue:{this.ReceivedQueue.Count} OutgoingPublishQueue:{this.OutgoingPublishQueue.Count}");
                Logger.Trace($"{this.Options.ClientId}-(CM)- TransactionQueue:{this.TransactionQueue.Count}");
                Logger.Trace($"{this.Options.ClientId}-(CM)- - ConnectionMonitor:{this.ConnectionMonitorTask?.Status}");
                Logger.Trace($"{this.Options.ClientId}-(CM)- - ConnectionPublishWriter:{this.ConnectionPublishWriterTask?.Status}");
                Logger.Trace($"{this.Options.ClientId}-(CM)- - ConnectionWriter:{this.ConnectionWriterTask?.Status}");
                Logger.Trace($"{this.Options.ClientId}-(CM)- - ConnectionReader:{this.ConnectionReaderTask?.Status}");
                Logger.Trace($"{this.Options.ClientId}-(CM)- - ReceivedPacketsHandler:{this.ReceivedPacketsHandlerTask?.Status}");

                try
                {
                    await Task.Delay(2000, cancellationToken).ConfigureAwait(false);
                }
                catch (TaskCanceledException)
                {
                    Logger.Trace($"{this.Options.ClientId}-(CM)- Cancelled");
                    break;
                }
            }

            Logger.Trace($"{this.Options.ClientId}-(CM)- Exiting...{this.ConnectState}");

            return true;
        }, cancellationToken);

    /// <summary>
    /// Asynchronous background task that handles the outgoing publish packets queued in OutgoingPublishQueue.
    /// </summary>
    private Task<bool> ConnectionPublishWriterAsync(CancellationToken cancellationToken) => Task.Run(
        async () =>
        {
            this.lastCommunicationTimer.Start();
            Logger.Trace($"{this.Options.ClientId}-(PW)- Starting...{this.ConnectState}");

            while (true)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    Logger.Trace($"{this.Options.ClientId}-(PW)- Cancelled with {this.OutgoingPublishQueue.Count} publish packets remaining.");
                    break;
                }

                while (this.ConnectState == ConnectState.Disconnected)
                {
                    Logger.Trace($"{this.Options.ClientId}-(PW)- Not connected.  Waiting for connect...");
                    await Task.Delay(2000).ConfigureAwait(false);
                    continue;
                }

                // Logger.Trace($"{this.Options.ClientId}-(PW)- {this.OutgoingPublishQueue.Count} publish packets waiting to be sent.");
                var receiveMaximum = this.ConnectionProperties.ReceiveMaximum ?? 65535;
                if (this.TransactionQueue.Count >= receiveMaximum)
                {
                    Logger.Debug($"The Maximum number of publishes have been sent to broker.  Waiting for existing transactions to complete.");
                    await Task.Delay(10).ConfigureAwait(false);
                    continue;
                }

                var publishPacket = await this.OutgoingPublishQueue.DequeueAsync(cancellationToken).ConfigureAwait(false);
                FlushResult writeResult = default;

                Logger.Trace($"{this.Options.ClientId}-(PW)- --> Sending PublishPacket id={publishPacket.PacketIdentifier}");
                if (publishPacket.Message.QoS is QualityOfService.AtLeastOnceDelivery ||
                    publishPacket.Message.QoS is QualityOfService.ExactlyOnceDelivery)
                {
                    // QoS > 0 - Add to transaction queue
                    if (!this.TransactionQueue.TryAdd(publishPacket.PacketIdentifier, new List<ControlPacket> { publishPacket }))
                    {
                        Logger.Warn($"Duplicate packet ID detected {publishPacket.PacketIdentifier} while queueing to transaction queue for an outgoing QoS {publishPacket.Message.QoS} publish .");
                        continue;
                    }
                }

                writeResult = await this.WriteAsync(publishPacket.Encode()).ConfigureAwait(false);
                this.OnPublishSentEventLauncher(publishPacket);

                if (writeResult.IsCanceled)
                {
                    Logger.Trace($"{this.Options.ClientId}-(PW)- ConnectionPublishWriter Write Cancelled");
                    break;
                }

                if (writeResult.IsCompleted)
                {
                    Logger.Trace($"{this.Options.ClientId}-(PW)- ConnectionPublishWriter IsCompleted: end of the stream");
                    break;
                }

                this.lastCommunicationTimer.Restart();
            } // while(true)

            Logger.Trace($"{this.Options.ClientId}-(PW)- ConnectionPublishWriter Exiting...{this.ConnectState}");
            return true;
        }, cancellationToken);

    /// <summary>
    /// Asynchronous background task that handles the outgoing traffic of packets queued in the sendQueue.
    /// </summary>
    private Task<bool> ConnectionWriterAsync(CancellationToken cancellationToken) => Task.Run(
        async () =>
        {
            this.lastCommunicationTimer.Start();
            Logger.Trace($"{this.Options.ClientId}-(W)- Starting...{this.ConnectState}");

            while (true)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    Logger.Trace($"{this.Options.ClientId}-(W)- Cancelled with {this.SendQueue.Count} packets remaining.");
                    break;
                }

                while (this.ConnectState == ConnectState.Disconnected)
                {
                    Logger.Trace($"{this.Options.ClientId}-(W)- Not connected.  Waiting for connect...");
                    await Task.Delay(2000).ConfigureAwait(false);
                    continue;
                }

                // Logger.Trace($"{this.Options.ClientId}-(W)- {this.SendQueue.Count} packets waiting to be sent.");
                var packet = await this.SendQueue.DequeueAsync(cancellationToken).ConfigureAwait(false);
                FlushResult writeResult = default;

                switch (packet)
                {
                    // FIXME: Only one connect, subscribe or unsubscribe packet can be sent at a time.
                    case ConnectPacket connectPacket:
                        Logger.Trace($"{this.Options.ClientId}-(W)- --> Sending ConnectPacket id={connectPacket.PacketIdentifier}");
                        writeResult = await this.WriteAsync(connectPacket.Encode()).ConfigureAwait(false);
                        this.OnConnectSentEventLauncher(connectPacket);
                        break;
                    case DisconnectPacket disconnectPacket:
                        Logger.Trace($"{this.Options.ClientId}-(W)- --> Sending DisconnectPacket id={disconnectPacket.PacketIdentifier}");
                        writeResult = await this.WriteAsync(disconnectPacket.Encode()).ConfigureAwait(false);
                        this.OnDisconnectSentEventLauncher(disconnectPacket);
                        break;
                    case SubscribePacket subscribePacket:
                        Logger.Trace($"{this.Options.ClientId}-(W)- --> Sending SubscribePacket id={subscribePacket.PacketIdentifier}");
                        writeResult = await this.WriteAsync(subscribePacket.Encode()).ConfigureAwait(false);
                        this.OnSubscribeSentEventLauncher(subscribePacket);
                        break;
                    case UnsubscribePacket unsubscribePacket:
                        Logger.Trace($"{this.Options.ClientId}-(W)- --> Sending UnsubscribePacket id={unsubscribePacket.PacketIdentifier}");
                        writeResult = await this.WriteAsync(unsubscribePacket.Encode()).ConfigureAwait(false);
                        this.OnUnsubscribeSentEventLauncher(unsubscribePacket);
                        break;
                    case PublishPacket publishPacket:
                        throw new HiveMQttClientException("PublishPacket should be sent via ConnectionPublishWriterAsync.");
                    case PubAckPacket pubAckPacket:
                        // This is in response to a received Publish packet.  Communication chain management
                        // was done in the receiver code.  Just send the response.
                        Logger.Trace($"{this.Options.ClientId}-(W)- --> Sending PubAckPacket id={pubAckPacket.PacketIdentifier} reason={pubAckPacket.ReasonCode}");
                        writeResult = await this.WriteAsync(pubAckPacket.Encode()).ConfigureAwait(false);
                        this.OnPubAckSentEventLauncher(pubAckPacket);
                        break;
                    case PubRecPacket pubRecPacket:
                        // This is in response to a received Publish packet.  Communication chain management
                        // was done in the receiver code.  Just send the response.
                        Logger.Trace($"{this.Options.ClientId}-(W)- --> Sending PubRecPacket id={pubRecPacket.PacketIdentifier} reason={pubRecPacket.ReasonCode}");
                        writeResult = await this.WriteAsync(pubRecPacket.Encode()).ConfigureAwait(false);
                        this.OnPubRecSentEventLauncher(pubRecPacket);
                        break;
                    case PubRelPacket pubRelPacket:
                        // This is in response to a received PubRec packet.  Communication chain management
                        // was done in the receiver code.  Just send the response.
                        Logger.Trace($"{this.Options.ClientId}-(W)- --> Sending PubRelPacket id={pubRelPacket.PacketIdentifier} reason={pubRelPacket.ReasonCode}");
                        writeResult = await this.WriteAsync(pubRelPacket.Encode()).ConfigureAwait(false);
                        this.OnPubRelSentEventLauncher(pubRelPacket);
                        break;
                    case PubCompPacket pubCompPacket:
                        // This is in response to a received PubRel packet.  Communication chain management
                        // was done in the receiver code.  Just send the response.
                        Logger.Trace($"{this.Options.ClientId}-(W)- --> Sending PubCompPacket id={pubCompPacket.PacketIdentifier} reason={pubCompPacket.ReasonCode}");
                        writeResult = await this.WriteAsync(pubCompPacket.Encode()).ConfigureAwait(false);
                        this.OnPubCompSentEventLauncher(pubCompPacket);
                        break;
                    case PingReqPacket pingReqPacket:
                        Logger.Trace($"{this.Options.ClientId}-(W)- --> Sending PingReqPacket id={pingReqPacket.PacketIdentifier}");
                        writeResult = await this.WriteAsync(PingReqPacket.Encode()).ConfigureAwait(false);
                        this.OnPingReqSentEventLauncher(pingReqPacket);
                        break;
                    default:
                        Logger.Trace($"{this.Options.ClientId}-(W)- --> Unknown packet type {packet}");
                        break;
                } // switch

                if (writeResult.IsCanceled)
                {
                    Logger.Trace($"{this.Options.ClientId}-(W)- ConnectionWriter Write Cancelled");
                    break;
                }

                if (writeResult.IsCompleted)
                {
                    Logger.Trace($"{this.Options.ClientId}-(W)- ConnectionWriter IsCompleted: end of the stream");
                    break;
                }

                this.lastCommunicationTimer.Restart();
            } // while(true)

            Logger.Trace($"{this.Options.ClientId}-(W)- ConnectionWriter Exiting...{this.ConnectState}");
            return true;
        }, cancellationToken);

    /// <summary>
    /// Asynchronous background task that handles the incoming traffic of packets.  Received packets
    /// are queued into this.ReceivedQueue for processing by ReceivedPacketsHandlerAsync.
    /// </summary>
    private Task<bool> ConnectionReaderAsync(CancellationToken cancellationToken) => Task.Run(
        async () =>
        {
            Logger.Trace($"{this.Options.ClientId}-(R)- ConnectionReader Starting...{this.ConnectState}");

            ReadResult readResult;

            while (this.ConnectState is ConnectState.Connecting or ConnectState.Connected)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    Logger.Trace($"{this.Options.ClientId}-(R)- Cancelled");
                    break;
                }

                readResult = await this.ReadAsync().ConfigureAwait(false);

                if (readResult.IsCanceled)
                {
                    Logger.Trace($"{this.Options.ClientId}-(R)- Cancelled read result.");
                    break;
                }

                if (readResult.IsCompleted)
                {
                    Logger.Trace($"{this.Options.ClientId}-(R)- ConnectionReader IsCompleted: end of the stream");
                    if (this.ConnectState == ConnectState.Connected)
                    {
                        // This is an unexpected exit and may be due to a network failure.
                        Logger.Trace($"{this.Options.ClientId}-(R)- ConnectionReader IsCompleted: this was unexpected");
                        await this.HandleDisconnectionAsync(false).ConfigureAwait(false);
                    }

                    Logger.Trace($"{this.Options.ClientId}-(R)- ConnectionReader Exiting...{this.ConnectState}");
                    return true;
                }

                var buffer = readResult.Buffer;

                while (buffer.Length > 0)
                {
                    if (!PacketDecoder.TryDecode(buffer, out var decodedPacket, out var consumed))
                    {
                        if (decodedPacket is MalformedPacket)
                        {
                            Logger.Warn($"Malformed packet received.  Disconnecting.");
                            Logger.Debug($"{this.Options.ClientId}-(R)- Malformed packet received: {decodedPacket}");

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
                        this.Reader?.AdvanceTo(buffer.Start, readResult.Buffer.End);
                        Logger.Trace($"{this.Options.ClientId}-(R)- ConnectionReader: PacketDecoder.TryDecode returned false.  Waiting for more data...");
                        break;
                    }

                    // Advance the reader to indicate how much of the buffer has been consumed
                    buffer = buffer.Slice(consumed);
                    this.Reader?.AdvanceTo(buffer.Start);

                    // We handle disconnects immediately
                    if (decodedPacket is DisconnectPacket disconnectPacket)
                    {
                        Logger.Warn($"-(R)- <-- Disconnect received: {disconnectPacket.DisconnectReasonCode} {disconnectPacket.Properties.ReasonString}");
                        await this.HandleDisconnectionAsync(false).ConfigureAwait(false);
                        this.OnDisconnectReceivedEventLauncher(disconnectPacket);
                        break;
                    }
                    else
                    {
                        Logger.Trace($"{this.Options.ClientId}-(R)- <-- Received {decodedPacket.GetType().Name} id: {decodedPacket.PacketIdentifier}.  Adding to receivedQueue.");

                        // Add the packet to the received queue for processing later by ReceivedPacketsHandlerAsync
                        this.ReceivedQueue.Enqueue(decodedPacket);
                    }
                } // while (buffer.Length > 0

                await Task.Yield();
            } // while (this.ConnectState is ConnectState.Connecting or ConnectState.Connected)

            Logger.Trace($"{this.Options.ClientId}-(R)- ConnectionReader Exiting...{this.ConnectState}");
            return true;
        }, cancellationToken);

    /// <summary>
    /// Continually processes the packets queued in the receivedQueue.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token to stop the task.</param>
    /// <returns>A fairly worthless boolean.</returns>
    private Task<bool> ReceivedPacketsHandlerAsync(CancellationToken cancellationToken) => Task.Run(
        async () =>
        {
            Logger.Trace($"{this.Options.ClientId}-(RPH)- Starting...{this.ConnectState}");

            while (true)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    Logger.Trace($"{this.Options.ClientId}-(RPH)- Cancelled with {this.ReceivedQueue.Count} received packets remaining.");
                    break;
                }

                // Logger.Trace($"{this.Options.ClientId}-(RPH)- {this.ReceivedQueue.Count} received packets currently waiting to be processed.");
                var packet = await this.ReceivedQueue.DequeueAsync(cancellationToken).ConfigureAwait(false);
                if (this.Options.ClientMaximumPacketSize != null)
                {
                    if (packet.PacketSize > this.Options.ClientMaximumPacketSize)
                    {
                        Logger.Warn($"Received packet size {packet.PacketSize} exceeds maximum packet size {this.Options.ClientMaximumPacketSize}.  Disconnecting.");
                        Logger.Debug($"{this.Options.ClientId}-(RPH)- Received packet size {packet.PacketSize} exceeds maximum packet size {this.Options.ClientMaximumPacketSize}.  Disconnecting.");

                        var opts = new DisconnectOptions
                        {
                            ReasonCode = DisconnectReasonCode.PacketTooLarge,
                            ReasonString = "Packet Too Large",
                        };
                        await this.DisconnectAsync(opts).ConfigureAwait(false);
                        return false;
                    }
                }

                switch (packet)
                {
                    case ConnAckPacket connAckPacket:
                        Logger.Trace($"{this.Options.ClientId}-(RPH)- <-- Received ConnAck id={connAckPacket.PacketIdentifier}");
                        this.ConnectionProperties = connAckPacket.Properties;
                        this.OnConnAckReceivedEventLauncher(connAckPacket);
                        break;
                    case DisconnectPacket disconnectPacket:
                        Logger.Trace($"{this.Options.ClientId}-(RPH)- <-- Received Disconnect id={disconnectPacket.PacketIdentifier} {disconnectPacket.DisconnectReasonCode} {disconnectPacket.Properties.ReasonString}");
                        Logger.Warn($"We shouldn't get the disconnect here - Disconnect received: {disconnectPacket.DisconnectReasonCode} {disconnectPacket.Properties.ReasonString}");
                        throw new HiveMQttClientException("Received Disconnect packet in ReceivedPacketsHandlerAsync");
                    case PingRespPacket pingRespPacket:
                        Logger.Trace($"{this.Options.ClientId}-(RPH)- <-- Received PingResp id={pingRespPacket.PacketIdentifier}");
                        this.OnPingRespReceivedEventLauncher(pingRespPacket);
                        break;
                    case SubAckPacket subAckPacket:
                        Logger.Trace($"{this.Options.ClientId}-(RPH)- <-- Received SubAck id={subAckPacket.PacketIdentifier}");
                        this.OnSubAckReceivedEventLauncher(subAckPacket);
                        break;
                    case UnsubAckPacket unsubAckPacket:
                        Logger.Trace($"{this.Options.ClientId}-(RPH)- <-- Received UnsubAck id={unsubAckPacket.PacketIdentifier}");
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
                        Logger.Trace($"{this.Options.ClientId}-(RPH)- <-- Received Unknown packet type.  Will discard.");
                        Logger.Error($"Unrecognized packet received.  Will discard. {packet}");
                        break;
                } // switch (packet)
            } // while (true)

            Logger.Trace($"{this.Options.ClientId}-(RPH)- ReceivedPacketsHandler Exiting...{this.ConnectState}");
            return true;
        }, cancellationToken);

    /// <summary>
    /// Handle an incoming Publish packet.
    /// </summary>
    /// <param name="publishPacket">The received publish packet.</param>
    internal void HandleIncomingPublishPacket(PublishPacket publishPacket)
    {
        Logger.Trace($"{this.Options.ClientId}-(RPH)- <-- Received Publish id={publishPacket.PacketIdentifier}");
        this.OnPublishReceivedEventLauncher(publishPacket);

        if (publishPacket.Message.QoS is QualityOfService.AtLeastOnceDelivery)
        {
            // We've received a QoS 1 publish.  Send a PubAck.
            var pubAckResponse = new PubAckPacket(publishPacket.PacketIdentifier, PubAckReasonCode.Success);

            // FIXME We should wait until puback is sent before launching event
            // FIXME Check DUP flag setting
            this.SendQueue.Enqueue(pubAckResponse);
            publishPacket.OnPublishQoS1CompleteEventLauncher(pubAckResponse);
        }
        else if (publishPacket.Message.QoS is QualityOfService.ExactlyOnceDelivery)
        {
            // We've received a QoS 2 publish.  Send a PubRec and add to QoS2 transaction register.
            var pubRecResponse = new PubRecPacket(publishPacket.PacketIdentifier, PubRecReasonCode.Success);
            var publishQoS2Chain = new List<ControlPacket> { publishPacket, pubRecResponse };

            // FIXME:  Wait for QoS 2 transaction to complete before calling OnMessageReceivedEventLauncher???
            if (!this.TransactionQueue.TryAdd(publishPacket.PacketIdentifier, publishQoS2Chain))
            {
                Logger.Warn($"Duplicate packet ID detected {publishPacket.PacketIdentifier} while queueing to transaction queue for an incoming QoS {publishPacket.Message.QoS} publish .");
                pubRecResponse.ReasonCode = PubRecReasonCode.PacketIdentifierInUse;
            }

            this.SendQueue.Enqueue(pubRecResponse);
        }

        this.OnMessageReceivedEventLauncher(publishPacket);
    }

    /// <summary>
    /// Handle an incoming ConnAck packet.
    /// </summary>
    /// <param name="pubAckPacket">The received PubAck packet.</param>
    /// <exception cref="HiveMQttClientException">Raised if the packet identifier is unknown.</exception>
    internal void HandleIncomingPubAckPacket(PubAckPacket pubAckPacket)
    {
        Logger.Trace($"{this.Options.ClientId}-(RPH)- <-- Received PubAck id={pubAckPacket.PacketIdentifier} reason={pubAckPacket.ReasonCode}");
        this.OnPubAckReceivedEventLauncher(pubAckPacket);

        // Remove the transaction chain from the transaction queue
        if (this.TransactionQueue.Remove(pubAckPacket.PacketIdentifier, out var publishQoS1Chain))
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
        Logger.Trace($"{this.Options.ClientId}-(RPH)- <-- Received PubRec id={pubRecPacket.PacketIdentifier} reason={pubRecPacket.ReasonCode}");
        this.OnPubRecReceivedEventLauncher(pubRecPacket);

        // Find the QoS2 transaction chain for this packet identifier
        if (this.TransactionQueue.TryGetValue(pubRecPacket.PacketIdentifier, out var originalPublishQoS2Chain))
        {
            var originalPublishPacket = (PublishPacket)originalPublishQoS2Chain.First();

            // Create a PUBREL response packet
            var pubRelResponsePacket = new PubRelPacket(pubRecPacket.PacketIdentifier, PubRelReasonCode.Success);

            // Create an updated transaction chain
            var newPublishQoS2Chain = new List<ControlPacket>
            {
                originalPublishPacket,
                pubRecPacket,
                pubRelResponsePacket,
            };

            // Update the chain in the queue
            if (!this.TransactionQueue.TryUpdate(pubRecPacket.PacketIdentifier, newPublishQoS2Chain, originalPublishQoS2Chain))
            {
                Logger.Warn($"QoS2: Couldn't update PubRec --> PubRel QoS2 Chain for packet identifier {pubRecPacket.PacketIdentifier}.");
            }

            // Send the PUBREL response
            this.SendQueue.Enqueue(pubRelResponsePacket);
        }
        else
        {
            // Send a PUBREL with PacketIdentifierNotFound
            var pubRelResponsePacket = new PubRelPacket(pubRecPacket.PacketIdentifier, PubRelReasonCode.PacketIdentifierNotFound);
            this.SendQueue.Enqueue(pubRelResponsePacket);
        }
    }

    /// <summary>
    /// Handle an incoming PubRel packet.
    /// </summary>
    /// <param name="pubRelPacket">The received PubRel packet.</param>
    internal void HandleIncomingPubRelPacket(PubRelPacket pubRelPacket)
    {
        Logger.Trace($"{this.Options.ClientId}-(RPH)- <-- Received PubRel id={pubRelPacket.PacketIdentifier} reason={pubRelPacket.ReasonCode}");
        this.OnPubRelReceivedEventLauncher(pubRelPacket);

        if (this.TransactionQueue.TryGetValue(pubRelPacket.PacketIdentifier, out var originalPublishQoS2Chain))
        {
            var originalPublishPacket = (PublishPacket)originalPublishQoS2Chain.First();

            // Send a PUBCOMP in response
            var pubCompResponsePacket = new PubCompPacket(pubRelPacket.PacketIdentifier, PubCompReasonCode.Success);

            // This QoS2 transaction chain is done.  Remove it from the transaction queue.
            if (this.TransactionQueue.TryRemove(pubRelPacket.PacketIdentifier, out var publishQoS2Chain))
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

            this.SendQueue.Enqueue(pubCompResponsePacket);
        }
        else
        {
            Logger.Warn($"QoS2: Received PubRel with an unknown packet identifier {pubRelPacket.PacketIdentifier}. " +
                         "Responding with PubComp PacketIdentifierNotFound.");

            // Send a PUBCOMP with PacketIdentifierNotFound
            var pubCompResponsePacket = new PubCompPacket(pubRelPacket.PacketIdentifier, PubCompReasonCode.PacketIdentifierNotFound);
            this.SendQueue.Enqueue(pubCompResponsePacket);
        }
    }

    /// <summary>
    /// Handle an incoming PubComp packet.
    /// </summary>
    /// <param name="pubCompPacket">The received PubComp packet.</param>
    /// <exception cref="HiveMQttClientException">Raised if the packet identifier is unknown.</exception>
    internal void HandleIncomingPubCompPacket(PubCompPacket pubCompPacket)
    {
        Logger.Trace($"{this.Options.ClientId}-(RPH)- <-- Received PubComp id={pubCompPacket.PacketIdentifier} reason={pubCompPacket.ReasonCode}");
        this.OnPubCompReceivedEventLauncher(pubCompPacket);

        // Remove the QoS 2 transaction chain from the queue
        if (this.TransactionQueue.Remove(pubCompPacket.PacketIdentifier, out var publishQoS2Chain))
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
        if (this.Writer is null)
        {
            throw new HiveMQttClientException("Writer is null");
        }

        return this.Writer.WriteAsync(source, cancellationToken);
    }

    /// <summary>
    /// Read a buffer from the stream.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A ReadResult wrapped in a ValueTask.</returns>
    /// <exception cref="HiveMQttClientException">Raised if the reader is null.</exception>
    internal async ValueTask<ReadResult> ReadAsync(CancellationToken cancellationToken = default)
    {
        if (this.Reader is null)
        {
            throw new HiveMQttClientException("Reader is null");
        }

        var readResult = await this.Reader.ReadAsync(cancellationToken).ConfigureAwait(false);
        Logger.Trace($"ReadAsync: Read Buffer Length {readResult.Buffer.Length}");
        return readResult;
    }
}
