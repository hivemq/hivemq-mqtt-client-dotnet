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

    // Incoming Publish QoS > 0 in-flight transactions indexed by packet identifier
    internal BoundedDictionaryX<int, List<ControlPacket>> IPubTransactionQueue { get; set; }

    // Outgoing Publish QoS > 0 in-flight transactions indexed by packet identifier
    internal BoundedDictionaryX<int, List<ControlPacket>> OPubTransactionQueue { get; set; }

    private readonly Stopwatch lastCommunicationTimer = new();

    /// <summary>
    /// Health check method to assure that tasks haven't faulted unexpectedly.
    /// </summary>
    private async Task RunTaskHealthCheckAsync(Task? task, string taskName)
    {
        if (task is null)
        {
            Logger.Info($"{this.Options.ClientId}-(CM)- {taskName} is not running.");
        }
        else
        {
            if (task.IsFaulted)
            {
                Logger.Error($"{this.Options.ClientId}-(CM)- {taskName} Faulted: {task.Exception}");
                Logger.Error($"{this.Options.ClientId}-(CM)- {taskName} died.  Disconnecting.");
                _ = await this.HandleDisconnectionAsync(false).ConfigureAwait(false);
            }
        }
    }

    /// <summary>
    /// Asynchronous background task that monitors the connection state and sends PingReq packets when
    /// necessary.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    private Task ConnectionMonitorAsync(CancellationToken cancellationToken) => Task.Run(
        async () =>
        {
            var keepAlivePeriod = this.Options.KeepAlive / 2;
            Logger.Trace($"{this.Options.ClientId}-(CM)- Starting...{this.ConnectState}");
            this.lastCommunicationTimer.Start();

            while (true)
            {
                try
                {
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
                    Logger.Debug($"{this.Options.ClientId}-(CM)- {this.ConnectState}: last communications {this.lastCommunicationTimer.Elapsed} ago");
                    Logger.Debug($"{this.Options.ClientId}-(CM)- SendQueue:...............{this.SendQueue.Count}");
                    Logger.Debug($"{this.Options.ClientId}-(CM)- ReceivedQueue:...........{this.ReceivedQueue.Count}");
                    Logger.Debug($"{this.Options.ClientId}-(CM)- OutgoingPublishQueue:....{this.OutgoingPublishQueue.Count}");
                    Logger.Debug($"{this.Options.ClientId}-(CM)- OPubTransactionQueue:....{this.OPubTransactionQueue.Count}/{this.OPubTransactionQueue.Capacity}");
                    Logger.Debug($"{this.Options.ClientId}-(CM)- IPubTransactionQueue:....{this.IPubTransactionQueue.Count}/{this.IPubTransactionQueue.Capacity}");
                    Logger.Debug($"{this.Options.ClientId}-(CM)- # of Subscriptions:......{this.Subscriptions.Count}");

                    // Background Tasks Health Check
                    await this.RunTaskHealthCheckAsync(this.ConnectionWriterTask, "ConnectionWriter").ConfigureAwait(false);
                    await this.RunTaskHealthCheckAsync(this.ConnectionReaderTask, "ConnectionReader").ConfigureAwait(false);
                    await this.RunTaskHealthCheckAsync(this.ConnectionPublishWriterTask, "ConnectionPublishWriter").ConfigureAwait(false);
                    await this.RunTaskHealthCheckAsync(this.ReceivedPacketsHandlerTask, "ReceivedPacketsHandler").ConfigureAwait(false);

                    // Sleep cycle
                    await Task.Delay(2000, cancellationToken).ConfigureAwait(false);

                    // Check for cancellation
                    if (cancellationToken.IsCancellationRequested)
                    {
                        Logger.Trace($"{this.Options.ClientId}-(CM)- Canceled & exiting...");
                        break;
                    }
                }
                catch (Exception ex)
                {
                    if (ex is TaskCanceledException || cancellationToken.IsCancellationRequested)
                    {
                        break;
                    }
                    else
                    {
                        Logger.Error($"{this.Options.ClientId}-(CM)- Exception: {ex}");
                        throw;
                    }
                }
            }

            Logger.Debug($"{this.Options.ClientId}-(CM)- Exiting...{this.ConnectState}, cancellationRequested={cancellationToken.IsCancellationRequested}");
        }, cancellationToken);

    /// <summary>
    /// Asynchronous background task that handles the outgoing publish packets queued in OutgoingPublishQueue.
    /// </summary>
    private Task ConnectionPublishWriterAsync(CancellationToken cancellationToken) => Task.Run(
        async () =>
        {
            Logger.Trace($"{this.Options.ClientId}-(PW)- Starting...{this.ConnectState}");

            while (true)
            {
                try
                {
                    while (this.ConnectState != ConnectState.Connected)
                    {
                        Logger.Trace($"{this.Options.ClientId}-(PW)- Not connected.  Waiting for connect...");
                        await Task.Delay(500).ConfigureAwait(false);
                        continue;
                    }

                    FlushResult writeResult = default;
                    var publishPacket = await this.OutgoingPublishQueue.DequeueAsync(cancellationToken).ConfigureAwait(false);

                    Logger.Trace($"{this.Options.ClientId}-(PW)- --> Sending PublishPacket id={publishPacket.PacketIdentifier}");
                    if (publishPacket.Message.QoS is QualityOfService.AtLeastOnceDelivery ||
                        publishPacket.Message.QoS is QualityOfService.ExactlyOnceDelivery)
                    {
                        // QoS > 0 - Add to transaction queue.  OPubTransactionQueue will block when necessary
                        // to respect the broker's ReceiveMaximum
                        var success = await this.OPubTransactionQueue.AddAsync(
                            publishPacket.PacketIdentifier,
                            new List<ControlPacket> { publishPacket },
                            cancellationToken).ConfigureAwait(false);

                        if (!success)
                        {
                            Logger.Warn($"Duplicate packet ID detected {publishPacket.PacketIdentifier} while queueing to transaction queue for an outgoing QoS {publishPacket.Message.QoS} publish .");
                            continue;
                        }
                    }

                    writeResult = await this.WriteAsync(publishPacket.Encode()).ConfigureAwait(false);
                    this.OnPublishSentEventLauncher(publishPacket);

                    if (writeResult.IsCompleted || writeResult.IsCanceled)
                    {
                        Logger.Trace($"{this.Options.ClientId}-(PW)- ConnectionPublishWriter: IsCompleted={writeResult.IsCompleted} IsCancelled={writeResult.IsCanceled}");

                        if (this.ConnectState == ConnectState.Connected)
                        {
                            // This is an unexpected exit and may be due to a network failure.
                            Logger.Debug($"{this.Options.ClientId}-(PW)- ConnectionPublishWriter: unexpected exit.  Disconnecting...");
                            await this.HandleDisconnectionAsync(false).ConfigureAwait(false);
                        }

                        break;
                    }

                    if (cancellationToken.IsCancellationRequested)
                    {
                        Logger.Trace($"{this.Options.ClientId}-(PW)- Cancelled & existing with {this.OutgoingPublishQueue.Count} publish packets remaining.");
                        break;
                    }
                }
                catch (Exception ex)
                {
                    if (ex is TaskCanceledException || cancellationToken.IsCancellationRequested)
                    {
                        break;
                    }
                    else
                    {
                        Logger.Error($"{this.Options.ClientId}-(PW)- Exception: {ex}");
                        throw;
                    }
                }
            } // while(true)

            Logger.Debug($"{this.Options.ClientId}-(PW)- ConnectionPublishWriter Exiting...{this.ConnectState}, cancellationRequested={cancellationToken.IsCancellationRequested}");
        }, cancellationToken);

    /// <summary>
    /// Asynchronous background task that handles the outgoing traffic of packets queued in the sendQueue.
    /// </summary>
    private Task ConnectionWriterAsync(CancellationToken cancellationToken) => Task.Run(
        async () =>
        {
            Logger.Trace($"{this.Options.ClientId}-(W)- Starting...{this.ConnectState}");

            while (true)
            {
                try
                {
                    // We allow this task to run in Connecting, Connected, and Disconnecting states
                    // because it is the one that has to send the CONNECT and DISCONNECT packets.
                    while (this.ConnectState == ConnectState.Disconnected)
                    {
                        Logger.Trace($"{this.Options.ClientId}-(W)- Not connected.  Waiting for connect...");
                        await Task.Delay(2000).ConfigureAwait(false);
                        continue;
                    }

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

                        case PubAckPacket pubAckPacket:
                            Logger.Trace($"{this.Options.ClientId}-(W)- --> Sending PubAckPacket id={pubAckPacket.PacketIdentifier} reason={pubAckPacket.ReasonCode}");
                            writeResult = await this.WriteAsync(pubAckPacket.Encode()).ConfigureAwait(false);
                            this.HandleSentPubAckPacket(pubAckPacket);
                            break;
                        case PubRecPacket pubRecPacket:
                            Logger.Trace($"{this.Options.ClientId}-(W)- --> Sending PubRecPacket id={pubRecPacket.PacketIdentifier} reason={pubRecPacket.ReasonCode}");
                            writeResult = await this.WriteAsync(pubRecPacket.Encode()).ConfigureAwait(false);
                            this.OnPubRecSentEventLauncher(pubRecPacket);
                            break;
                        case PubRelPacket pubRelPacket:
                            Logger.Trace($"{this.Options.ClientId}-(W)- --> Sending PubRelPacket id={pubRelPacket.PacketIdentifier} reason={pubRelPacket.ReasonCode}");
                            writeResult = await this.WriteAsync(pubRelPacket.Encode()).ConfigureAwait(false);
                            this.OnPubRelSentEventLauncher(pubRelPacket);
                            break;
                        case PubCompPacket pubCompPacket:
                            Logger.Trace($"{this.Options.ClientId}-(W)- --> Sending PubCompPacket id={pubCompPacket.PacketIdentifier} reason={pubCompPacket.ReasonCode}");
                            writeResult = await this.WriteAsync(pubCompPacket.Encode()).ConfigureAwait(false);
                            this.HandleSentPubCompPacket(pubCompPacket);
                            break;

                        case PingReqPacket pingReqPacket:
                            Logger.Trace($"{this.Options.ClientId}-(W)- --> Sending PingReqPacket id={pingReqPacket.PacketIdentifier}");
                            writeResult = await this.WriteAsync(PingReqPacket.Encode()).ConfigureAwait(false);
                            this.OnPingReqSentEventLauncher(pingReqPacket);
                            break;

                        case PublishPacket publishPacket:
                            throw new HiveMQttClientException("PublishPacket should be sent via ConnectionPublishWriterAsync.");
                        default:
                            Logger.Trace($"{this.Options.ClientId}-(W)- --> Unknown packet type {packet}");
                            break;
                    } // switch

                    if (writeResult.IsCompleted || writeResult.IsCanceled)
                    {
                        Logger.Debug($"{this.Options.ClientId}-(W)- ConnectionWriter exiting: IsCompleted={writeResult.IsCompleted} IsCancelled={writeResult.IsCanceled}");
                        if (this.ConnectState == ConnectState.Connected)
                        {
                            await this.HandleDisconnectionAsync(false).ConfigureAwait(false);
                        }

                        break;
                    }

                    if (cancellationToken.IsCancellationRequested)
                    {
                        Logger.Trace($"{this.Options.ClientId}-(W)- Cancelled & exiting with {this.SendQueue.Count} packets remaining.");
                        break;
                    }
                }
                catch (Exception ex)
                {
                    if (ex is TaskCanceledException || cancellationToken.IsCancellationRequested)
                    {
                        break;
                    }
                    else
                    {
                        Logger.Error($"{this.Options.ClientId}-(W)- Exception: {ex}");
                        throw;
                    }
                }
            } // while(true)

            Logger.Debug($"{this.Options.ClientId}-(W)- ConnectionWriter Exiting...{this.ConnectState}, cancellationRequested={cancellationToken.IsCancellationRequested}");
            return;
        }, cancellationToken);

    /// <summary>
    /// Asynchronous background task that handles the incoming traffic of packets.  Received packets
    /// are queued into this.ReceivedQueue for processing by ReceivedPacketsHandlerAsync.
    /// </summary>
    private Task<bool> ConnectionReaderAsync(CancellationToken cancellationToken) => Task.Run(
        async () =>
        {
            ReadResult readResult;
            Logger.Trace($"{this.Options.ClientId}-(R)- ConnectionReader Starting...{this.ConnectState}");

            while (this.ConnectState is ConnectState.Connecting or ConnectState.Connected)
            {
                try
                {
                    readResult = await this.ReadAsync().ConfigureAwait(false);

                    if (readResult.IsCanceled || readResult.IsCompleted)
                    {
                        Logger.Debug($"{this.Options.ClientId}-(R)- ConnectionReader exiting: IsCompleted={readResult.IsCompleted} IsCancelled={readResult.IsCanceled}");
                        if (this.ConnectState == ConnectState.Connected)
                        {
                            await this.HandleDisconnectionAsync(false).ConfigureAwait(false);
                        }

                        return true;
                    }

                    var buffer = readResult.Buffer;

                    while (buffer.Length > 0)
                    {
                        if (!PacketDecoder.TryDecode(buffer, out var decodedPacket, out var consumed))
                        {
                            if (decodedPacket is MalformedPacket)
                            {
                                Logger.Error($"Malformed packet received.  Disconnecting...");
                                Logger.Debug($"{this.Options.ClientId}-(R)- Malformed packet received: {decodedPacket}");

                                var opts = new DisconnectOptions
                                {
                                    ReasonCode = DisconnectReasonCode.MalformedPacket,
                                    ReasonString = "Client couldn't decode packet.",
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
                            await this.HandleIncomingDisconnectPacketAsync(disconnectPacket).ConfigureAwait(false);
                            break;
                        }

                        // Check that maximum packet size has not been exceeded
                        if (this.Options.ClientMaximumPacketSize is not null && decodedPacket.PacketSize > this.Options.ClientMaximumPacketSize)
                        {
                            Logger.Error($"Received a packet that exceeds the requested maximum of {this.Options.ClientMaximumPacketSize}.  Disconnecting.");
                            Logger.Debug($"{this.Options.ClientId}-(RPH)- Received packet size {decodedPacket.PacketSize} for packet {decodedPacket.GetType().Name}");

                            var opts = new DisconnectOptions
                            {
                                ReasonCode = DisconnectReasonCode.PacketTooLarge,
                                ReasonString = "Packet size is larger than the requested Maximum Packet Size.",
                            };
                            await this.DisconnectAsync(opts).ConfigureAwait(false);
                            return false;
                        }

                        // For QoS 1 and 2 publishes, potentially apply back pressure according to ReceiveMaximum
                        if (decodedPacket is PublishPacket publishPacket)
                        {
                            // Limit the number of concurrent incoming QoS 1 and QoS 2 transactions
                            if (publishPacket.Message.QoS is QualityOfService.ExactlyOnceDelivery ||
                                publishPacket.Message.QoS is QualityOfService.AtLeastOnceDelivery)
                            {
                                var success = await this.IPubTransactionQueue.AddAsync(
                                    publishPacket.PacketIdentifier,
                                    new List<ControlPacket> { publishPacket }).ConfigureAwait(false);

                                if (!success)
                                {
                                    Logger.Error($"Received a publish with a duplicate packet identifier {publishPacket.PacketIdentifier} for a transaction already in progress.  Disconnecting.");

                                    var opts = new DisconnectOptions
                                    {
                                        ReasonCode = DisconnectReasonCode.UnspecifiedError,
                                        ReasonString = "Client received a publish with duplicate packet identifier for a transaction already in progress.",
                                    };
                                    return await this.DisconnectAsync(opts).ConfigureAwait(false);
                                }
                            }
                        }

                        Logger.Trace($"{this.Options.ClientId}-(R)- <-- Received {decodedPacket.GetType().Name} id: {decodedPacket.PacketIdentifier}.  Adding to receivedQueue.");
                        this.ReceivedQueue.Enqueue(decodedPacket);
                    } // while (buffer.Length > 0

                    // Check for cancellation
                    if (cancellationToken.IsCancellationRequested)
                    {
                        Logger.Trace($"{this.Options.ClientId}-(R)- Cancelled & exiting...");
                        break;
                    }
                }
                catch (Exception ex)
                {
                    if (ex is TaskCanceledException || cancellationToken.IsCancellationRequested)
                    {
                        break;
                    }
                    else
                    {
                        Logger.Error($"{this.Options.ClientId}-(R)- Exception: {ex}");
                        throw;
                    }
                }
            } // while (this.ConnectState is ConnectState.Connecting or ConnectState.Connected)

            Logger.Debug($"{this.Options.ClientId}-(R)- ConnectionReader Exiting...{this.ConnectState}, cancellationRequested={cancellationToken.IsCancellationRequested}");
            return true;
        }, cancellationToken);

    /// <summary>
    /// Continually processes the packets queued in the receivedQueue.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token to stop the task.</param>
    private Task ReceivedPacketsHandlerAsync(CancellationToken cancellationToken) => Task.Run(
        async () =>
        {
            Logger.Trace($"{this.Options.ClientId}-(RPH)- Starting...{this.ConnectState}");

            while (true)
            {
                try
                {
                    var packet = await this.ReceivedQueue.DequeueAsync(cancellationToken).ConfigureAwait(false);

                    switch (packet)
                    {
                        case ConnAckPacket connAckPacket:
                            this.HandleIncomingConnAckPacket(connAckPacket);
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

                        case PingRespPacket pingRespPacket:
                            Logger.Trace($"{this.Options.ClientId}-(RPH)- <-- Received PingResp id={pingRespPacket.PacketIdentifier}");
                            this.OnPingRespReceivedEventLauncher(pingRespPacket);
                            break;

                        case DisconnectPacket disconnectPacket:
                            // Disconnects are handled immediate and shouldn't be received here
                            // We leave this just as a sanity backup
                            Logger.Error($"{this.Options.ClientId}-(RPH)- Incorrectly received Disconnect packet in ReceivedPacketsHandlerAsync");
                            throw new HiveMQttClientException("Received Disconnect packet in ReceivedPacketsHandlerAsync");
                        default:
                            Logger.Trace($"{this.Options.ClientId}-(RPH)- <-- Received Unknown packet type.  Will discard.");
                            Logger.Error($"Unrecognized packet received.  Will discard. {packet}");
                            break;
                    } // switch (packet)

                    if (cancellationToken.IsCancellationRequested)
                    {
                        Logger.Trace($"{this.Options.ClientId}-(RPH)- Cancelled with {this.ReceivedQueue.Count} received packets remaining.  Exiting...");
                        break;
                    }
                }
                catch (Exception ex)
                {
                    if (ex is TaskCanceledException || cancellationToken.IsCancellationRequested)
                    {
                        break;
                    }
                    else
                    {
                        Logger.Error($"{this.Options.ClientId}-(RPH)- Exception: {ex}");
                        throw;
                    }
                }
            } // while (true)

            Logger.Debug($"{this.Options.ClientId}-(RPH)- ReceivedPacketsHandler Exiting...{this.ConnectState}, cancellationRequested={cancellationToken.IsCancellationRequested}");
            return;
        }, cancellationToken);

    /// <summary>
    /// Handle an incoming ConnAck packet.
    /// </summary>
    /// <param name="connAckPacket">The received ConnAck packet.</param>
    internal void HandleIncomingConnAckPacket(ConnAckPacket connAckPacket)
    {
        Logger.Trace($"{this.Options.ClientId}-(RPH)- <-- Received ConnAck id={connAckPacket.PacketIdentifier}");
        if (connAckPacket.ReasonCode == ConnAckReasonCode.Success && connAckPacket.Properties.ReceiveMaximum != null)
        {
            Logger.Debug($"{this.Options.ClientId}-(RPH)- <-- Broker says limit concurrent incoming QoS 1 and QoS 2 publishes to {connAckPacket.Properties.ReceiveMaximum}.");

            // FIXME: A resize would be better to not lose any existing.  Can we send publishes before the CONNACK?
            // Replace the OPubTransactionQueue BoundedDictionary with a new one with the broker's ReceiveMaximum
            this.OPubTransactionQueue = new BoundedDictionaryX<int, List<ControlPacket>>((int)connAckPacket.Properties.ReceiveMaximum);
        }

        this.ConnectionProperties = connAckPacket.Properties;
        this.OnConnAckReceivedEventLauncher(connAckPacket);
    }

    /// <summary>
    /// Handle an incoming Disconnect packet.
    /// </summary>
    /// <param name="disconnectPacket">The received Disconnect packet.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    internal async Task HandleIncomingDisconnectPacketAsync(DisconnectPacket disconnectPacket)
    {
        Logger.Error($"--> Disconnect received <--: {disconnectPacket.DisconnectReasonCode} {disconnectPacket.Properties.ReasonString}");
        await this.HandleDisconnectionAsync(false).ConfigureAwait(false);
        this.OnDisconnectReceivedEventLauncher(disconnectPacket);
    }

    /// <summary>
    /// Handle an incoming Publish packet.
    /// </summary>
    /// <param name="publishPacket">The received publish packet.</param>
    internal async void HandleIncomingPublishPacket(PublishPacket publishPacket)
    {
        Logger.Trace($"{this.Options.ClientId}-(RPH)- <-- Received Publish id={publishPacket.PacketIdentifier}");
        this.OnPublishReceivedEventLauncher(publishPacket);
        bool success;

        if (publishPacket.Message.QoS is QualityOfService.AtMostOnceDelivery)
        {
            this.OnMessageReceivedEventLauncher(publishPacket);
        }
        else if (publishPacket.Message.QoS is QualityOfService.AtLeastOnceDelivery)
        {
            // We've received a QoS 1 publish.  The transaction chain was created & added
            // by ConnectionReaderAsync to enforce the client's ReceiveMaximum
            // Send a PubAck and update the chain.  Once the PubAckPacket is sent,
            // the transaction chain will be deleted and the appropriate events will be
            // launched in HandleSentPubAckPacket.
            var pubAckResponse = new PubAckPacket(publishPacket.PacketIdentifier, PubAckReasonCode.Success);

            success = this.IPubTransactionQueue.TryGetValue(publishPacket.PacketIdentifier, out var publishQoS1Chain);
            publishQoS1Chain.Add(pubAckResponse);

            if (success)
            {
                // Update the chain in the queue
                if (this.IPubTransactionQueue.TryUpdate(publishPacket.PacketIdentifier, publishQoS1Chain, publishQoS1Chain))
                {
                    this.SendQueue.Enqueue(pubAckResponse);
                }
                else
                {
                    Logger.Error($"QoS1: Couldn't update Publish --> PubAck QoS1 Chain for packet identifier {publishPacket.PacketIdentifier}. Discarded.");
                    this.IPubTransactionQueue.Remove(publishPacket.PacketIdentifier, out _);

                    var opts = new DisconnectOptions
                    {
                        ReasonCode = DisconnectReasonCode.UnspecifiedError,
                        ReasonString = "Client internal error updating publish transaction chain.",
                    };
                    await this.DisconnectAsync(opts).ConfigureAwait(false);
                }
            }
            else
            {
                throw new HiveMQttClientException($"QoS1: Received Publish with an unknown packet identifier {publishPacket.PacketIdentifier}. Discarded.");
            }
        }
        else if (publishPacket.Message.QoS is QualityOfService.ExactlyOnceDelivery)
        {
            // We've received a QoS 2 publish.  The transaction chain was created & added
            // by ConnectionReaderAsync to enforce the client's ReceiveMaximum.
            // Send a PubRec and add to QoS2 transaction register.  Once PubComp is sent,
            // Subscribers will be notified and the transaction chain will be deleted.
            var pubRecResponse = new PubRecPacket(publishPacket.PacketIdentifier, PubRecReasonCode.Success);

            // Get the QoS2 transaction chain for this packet identifier and add the PubRec to it
            success = this.IPubTransactionQueue.TryGetValue(publishPacket.PacketIdentifier, out var publishQoS2Chain);
            publishQoS2Chain.Add(pubRecResponse);

            if (success)
            {
                // Update the chain in the queue
                if (!this.IPubTransactionQueue.TryUpdate(publishPacket.PacketIdentifier, publishQoS2Chain, publishQoS2Chain))
                {
                    Logger.Error($"QoS2: Couldn't update Publish --> PubRec QoS2 Chain for packet identifier {publishPacket.PacketIdentifier}. Discarded.");
                    this.IPubTransactionQueue.Remove(publishPacket.PacketIdentifier, out _);
                }
            }
            else
            {
                // FIXME: This should never happen if ConnectionReaderAsync is working correctly
                Logger.Error($"QoS2: Received Publish with an unknown packet identifier {publishPacket.PacketIdentifier}. Discarded.");
                return;
            }

            this.SendQueue.Enqueue(pubRecResponse);
        }
    }

    /// <summary>
    /// Handle an incoming PubAck packet.
    /// </summary>
    /// <param name="pubAckPacket">The received PubAck packet.</param>
    /// <exception cref="HiveMQttClientException">Raised if the packet identifier is unknown.</exception>
    internal void HandleIncomingPubAckPacket(PubAckPacket pubAckPacket)
    {
        Logger.Trace($"{this.Options.ClientId}-(RPH)- <-- Received PubAck id={pubAckPacket.PacketIdentifier} reason={pubAckPacket.ReasonCode}");
        this.OnPubAckReceivedEventLauncher(pubAckPacket);

        // This is in response to a publish that we sent
        // Remove the transaction chain from the transaction queue
        if (this.OPubTransactionQueue.Remove(pubAckPacket.PacketIdentifier, out var publishQoS1Chain))
        {
            var publishPacket = (PublishPacket)publishQoS1Chain.First();

            // We sent a QoS1 publish and received a PubAck.  The transaction is complete.
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
    internal async void HandleIncomingPubRecPacket(PubRecPacket pubRecPacket)
    {
        Logger.Trace($"{this.Options.ClientId}-(RPH)- <-- Received PubRec id={pubRecPacket.PacketIdentifier} reason={pubRecPacket.ReasonCode}");
        this.OnPubRecReceivedEventLauncher(pubRecPacket);

        // This is in response to a publish that we sent
        // Find the QoS2 transaction chain for this packet identifier
        if (this.OPubTransactionQueue.TryGetValue(pubRecPacket.PacketIdentifier, out var originalPublishQoS2Chain))
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
            if (!this.OPubTransactionQueue.TryUpdate(pubRecPacket.PacketIdentifier, newPublishQoS2Chain, originalPublishQoS2Chain))
            {
                Logger.Error($"QoS2: Couldn't update PubRec --> PubRel QoS2 Chain for packet identifier {pubRecPacket.PacketIdentifier}.");
                this.OPubTransactionQueue.Remove(pubRecPacket.PacketIdentifier, out _);

                // FIXME: Send an appropriate disconnect packet?
                await this.HandleDisconnectionAsync(false).ConfigureAwait(false);
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

        PubCompPacket pubCompResponsePacket;

        // This is in response to a publish that we received and already sent a pubrec
        if (this.IPubTransactionQueue.TryGetValue(pubRelPacket.PacketIdentifier, out var publishQoS2Chain))
        {
            // Send a PUBCOMP in response
            pubCompResponsePacket = new PubCompPacket(pubRelPacket.PacketIdentifier, PubCompReasonCode.Success);

            // Update the chain with the latest packets for the event launcher
            publishQoS2Chain.Add(pubRelPacket);
            publishQoS2Chain.Add(pubCompResponsePacket);

            if (!this.IPubTransactionQueue.TryUpdate(pubRelPacket.PacketIdentifier, publishQoS2Chain, publishQoS2Chain))
            {
                Logger.Warn($"QoS2: Couldn't update PubRel --> PubComp QoS2 Chain for packet identifier {pubRelPacket.PacketIdentifier}.");
            }
        }
        else
        {
            Logger.Warn($"QoS2: Received PubRel with an unknown packet identifier {pubRelPacket.PacketIdentifier}. " +
                         "Responding with PubComp PacketIdentifierNotFound.");

            // Send a PUBCOMP with PacketIdentifierNotFound
            pubCompResponsePacket = new PubCompPacket(pubRelPacket.PacketIdentifier, PubCompReasonCode.PacketIdentifierNotFound);
        }

        this.SendQueue.Enqueue(pubCompResponsePacket);
    }

    /// <summary>
    /// Handle an incoming PubComp packet.
    /// </summary>
    /// <param name="pubAckPacket">The received PubComp packet.</param>
    internal void HandleSentPubAckPacket(PubAckPacket pubAckPacket)
    {
        // Remove the transaction chain from the transaction queue
        var success = this.IPubTransactionQueue.Remove(pubAckPacket.PacketIdentifier, out var publishQoS1Chain);
        PublishPacket publishPacket;

        if (success)
        {
            publishPacket = (PublishPacket)publishQoS1Chain.First();

            // Trigger the packet specific event
            publishPacket.OnPublishQoS1CompleteEventLauncher(pubAckPacket);

            // The Application Message Event
            this.OnMessageReceivedEventLauncher(publishPacket);
        }
        else
        {
            // FIXME: Send an appropriate disconnect packet?
            Logger.Warn($"QoS1: Couldn't remove PubAck --> Publish QoS1 Chain for packet identifier {pubAckPacket.PacketIdentifier}.");
        }

        // The Packet Event
        this.OnPubAckSentEventLauncher(pubAckPacket);
    }

    /// <summary>
    /// Action to take once a PubComp packet is sent.
    /// </summary>
    /// <param name="pubCompPacket">The sent PubComp packet.</param>
    internal void HandleSentPubCompPacket(PubCompPacket pubCompPacket)
    {
        Logger.Trace($"{this.Options.ClientId}-(RPH)- <-- Sent PubComp id={pubCompPacket.PacketIdentifier} reason={pubCompPacket.ReasonCode}");

        // PubCompReasonCode is either Success or PacketIdentifierNotFound.  If the latter,
        // there won't be a transaction chain to remove.
        if (pubCompPacket.ReasonCode == PubCompReasonCode.Success)
        {
            // QoS 2 Transaction is done.  Remove the transaction chain from the queue
            if (this.IPubTransactionQueue.Remove(pubCompPacket.PacketIdentifier, out var publishQoS2Chain))
            {
                var originalPublishPacket = (PublishPacket)publishQoS2Chain.First();

                // Trigger the packet specific event
                originalPublishPacket.OnPublishQoS2CompleteEventLauncher(publishQoS2Chain);

                // Trigger the application message event
                this.OnMessageReceivedEventLauncher(originalPublishPacket);
            }
        }

        // Trigger the general event
        this.OnPubCompSentEventLauncher(pubCompPacket);
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

        // This is in response to a QoS2 publish that we sent
        // Remove the QoS 2 transaction chain from the queue
        if (this.OPubTransactionQueue.Remove(pubCompPacket.PacketIdentifier, out var publishQoS2Chain))
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

        var writeResult = this.Writer.WriteAsync(source, cancellationToken);
        this.lastCommunicationTimer.Restart();
        return writeResult;
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
        this.lastCommunicationTimer.Restart();
        return readResult;
    }
}
