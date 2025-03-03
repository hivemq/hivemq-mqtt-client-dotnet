namespace HiveMQtt.Client.Connection;

using HiveMQtt.Client.Exceptions;
using HiveMQtt.Client.Internal;
using HiveMQtt.Client.Options;
using HiveMQtt.MQTT5;
using HiveMQtt.MQTT5.Packets;
using HiveMQtt.MQTT5.ReasonCodes;
using HiveMQtt.MQTT5.Types;

/// <summary>
/// Represents the connection manager for the MQTT client.
/// </summary>
public partial class ConnectionManager
{
    internal Task? ConnectionPublishWriterTask { get; set; }

    internal Task? ConnectionWriterTask { get; set; }

    internal Task? ConnectionReaderTask { get; set; }

    internal Task? ReceivedPacketsHandlerTask { get; set; }

    internal Thread? ConnectionMonitorThread { get; set; }

    /// <summary>
    /// Health check method to assure that tasks haven't faulted unexpectedly.
    /// </summary>
    private void RunTaskHealthCheck(Task? task, string taskName)
    {
        if (task is null)
        {
            Logger.Info($"{this.Client.Options.ClientId}-(CM)- {taskName} is not running.");
        }
        else
        {
            if (task.IsFaulted)
            {
                Logger.Error($"{this.Client.Options.ClientId}-(CM)- {taskName} Faulted: {task.Exception}");
                Logger.Error($"{this.Client.Options.ClientId}-(CM)- {taskName} died.  Disconnecting.");
                _ = Task.Run(async () => await this.HandleDisconnectionAsync(false).ConfigureAwait(false));
            }
        }
    }

    private Thread LaunchConnectionMonitorThread()
    {
        var thread = new Thread(this.ConnectionMonitor);
        thread.Start();
        return thread;
    }

    /// <summary>
    /// Asynchronous background task that monitors the connection state and sends PingReq packets when
    /// necessary.
    /// </summary>
    private void ConnectionMonitor()
    {
        Logger.Trace($"{this.Client.Options.ClientId}-(CM)- Starting...{this.State}");
        if (this.Client.Options.KeepAlive == 0)
        {
            Logger.Debug($"{this.Client.Options.ClientId}-(CM)- KeepAlive is 0.  No pings will be sent.");
        }

        var keepAlivePeriod = this.Client.Options.KeepAlive;
        this.lastCommunicationTimer.Start();

        while (true)
        {
            try
            {
                // If connected and no recent packets have been sent, send a ping
                if (this.State == ConnectState.Connected)
                {
                    if (this.Client.Options.KeepAlive > 0 && this.lastCommunicationTimer.Elapsed > TimeSpan.FromSeconds(keepAlivePeriod))
                    {
                        // Send PingReq
                        Logger.Trace($"{this.Client.Options.ClientId}-(CM)- --> PingReq");
                        this.SendQueue.Enqueue(new PingReqPacket());
                    }
                }

                // Dumping Client State
                Logger.Debug($"{this.Client.Options.ClientId}-(CM)- {this.State}: last communications {this.lastCommunicationTimer.Elapsed} ago");
                Logger.Debug($"{this.Client.Options.ClientId}-(CM)- SendQueue:...............{this.SendQueue.Count}");
                Logger.Debug($"{this.Client.Options.ClientId}-(CM)- ReceivedQueue:...........{this.ReceivedQueue.Count}");
                Logger.Debug($"{this.Client.Options.ClientId}-(CM)- OutgoingPublishQueue:....{this.OutgoingPublishQueue.Count}");
                Logger.Debug($"{this.Client.Options.ClientId}-(CM)- OPubTransactionQueue:....{this.OPubTransactionQueue.Count}/{this.OPubTransactionQueue.Capacity}");
                Logger.Debug($"{this.Client.Options.ClientId}-(CM)- IPubTransactionQueue:....{this.IPubTransactionQueue.Count}/{this.IPubTransactionQueue.Capacity}");
                Logger.Debug($"{this.Client.Options.ClientId}-(CM)- # of Subscriptions:......{this.Client.Subscriptions.Count}");
                Logger.Debug($"{this.Client.Options.ClientId}-(CM)- PacketIDsInUse:..........{this.PacketIDManager.Count}");

                // Background Tasks Health Check
                this.RunTaskHealthCheck(this.ConnectionWriterTask, "ConnectionWriter");
                this.RunTaskHealthCheck(this.ConnectionReaderTask, "ConnectionReader");
                this.RunTaskHealthCheck(this.ConnectionPublishWriterTask, "ConnectionPublishWriter");
                this.RunTaskHealthCheck(this.ReceivedPacketsHandlerTask, "ReceivedPacketsHandler");

                // Sleep cycle
                Thread.Sleep(2000);
            }
            catch (Exception ex)
            {
                Logger.Error($"{this.Client.Options.ClientId}-(CM)- Exception: {ex}");
                throw;
            }
        } // while (true)
    }

    /// <summary>
    /// Asynchronous background task that handles the outgoing publish packets queued in OutgoingPublishQueue.
    /// </summary>
    private Task ConnectionPublishWriterAsync(CancellationToken cancellationToken) => Task.Run(
        async () =>
        {
            Logger.Trace($"{this.Client.Options.ClientId}-(PW)- Starting...{this.State}");

            while (true)
            {
                try
                {
                    while (this.State != ConnectState.Connected)
                    {
                        Logger.Trace($"{this.Client.Options.ClientId}-(PW)- Not connected.  Waiting for connect...");
                        await Task.Delay(500).ConfigureAwait(false);
                        continue;
                    }

                    var writeSuccess = true;
                    var publishPacket = await this.OutgoingPublishQueue.DequeueAsync(cancellationToken).ConfigureAwait(false);

                    if (publishPacket.Message.QoS is QualityOfService.AtLeastOnceDelivery ||
                        publishPacket.Message.QoS is QualityOfService.ExactlyOnceDelivery)
                    {
                        Logger.Trace($"{this.Client.Options.ClientId}-(PW)- --> Sending QoS={publishPacket.Message.QoS} PublishPacket id={publishPacket.PacketIdentifier}");

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
                    else
                    {
                        Logger.Trace($"{this.Client.Options.ClientId}-(PW)- --> Sending QoS 0 PublishPacket");
                    }

                    writeSuccess = await this.Transport.WriteAsync(publishPacket.Encode()).ConfigureAwait(false);
                    this.Client.OnPublishSentEventLauncher(publishPacket);

                    if (!writeSuccess)
                    {
                        Logger.Trace($"{this.Client.Options.ClientId}-(PW)- ConnectionPublishWriter: Failed to write to transport.");

                        if (this.State == ConnectState.Connected)
                        {
                            // This is an unexpected exit and may be due to a network failure.
                            Logger.Debug($"{this.Client.Options.ClientId}-(PW)- ConnectionPublishWriter: unexpected exit.  Disconnecting...");
                            await this.HandleDisconnectionAsync(false).ConfigureAwait(false);
                        }

                        break;
                    }

                    if (cancellationToken.IsCancellationRequested)
                    {
                        Logger.Trace($"{this.Client.Options.ClientId}-(PW)- Cancelled & existing with {this.OutgoingPublishQueue.Count} publish packets remaining.");
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
                        Logger.Error($"{this.Client.Options.ClientId}-(PW)- Exception: {ex}");
                        throw;
                    }
                }
            } // while(true)

            Logger.Debug($"{this.Client.Options.ClientId}-(PW)- ConnectionPublishWriter Exiting...{this.State}, cancellationRequested={cancellationToken.IsCancellationRequested}");
        }, cancellationToken);

    /// <summary>
    /// Asynchronous background task that handles the outgoing traffic of packets queued in the sendQueue.
    /// </summary>
    private Task ConnectionWriterAsync(CancellationToken cancellationToken) => Task.Run(
        async () =>
        {
            Logger.Trace($"{this.Client.Options.ClientId}-(W)- Starting...{this.State}");

            while (true)
            {
                try
                {
                    // We allow this task to run in Connecting, Connected, and Disconnecting states
                    // because it is the one that has to send the CONNECT and DISCONNECT packets.
                    while (this.State == ConnectState.Disconnected)
                    {
                        Logger.Trace($"{this.Client.Options.ClientId}-(W)- Not connected.  Waiting for connect...");
                        await Task.Delay(2000).ConfigureAwait(false);
                        continue;
                    }

                    var packet = await this.SendQueue.DequeueAsync(cancellationToken).ConfigureAwait(false);
                    var writeSuccess = true;

                    switch (packet)
                    {
                        // FIXME: Only one connect, subscribe or unsubscribe packet can be sent at a time.
                        case ConnectPacket connectPacket:
                            Logger.Trace($"{this.Client.Options.ClientId}-(W)- --> Sending ConnectPacket");
                            writeSuccess = await this.Transport.WriteAsync(connectPacket.Encode(), cancellationToken).ConfigureAwait(false);
                            this.Client.OnConnectSentEventLauncher(connectPacket);
                            break;
                        case DisconnectPacket disconnectPacket:
                            Logger.Trace($"{this.Client.Options.ClientId}-(W)- --> Sending DisconnectPacket");
                            writeSuccess = await this.Transport.WriteAsync(disconnectPacket.Encode(), cancellationToken).ConfigureAwait(false);
                            this.Client.OnDisconnectSentEventLauncher(disconnectPacket);
                            break;
                        case SubscribePacket subscribePacket:
                            Logger.Trace($"{this.Client.Options.ClientId}-(W)- --> Sending SubscribePacket id={subscribePacket.PacketIdentifier}");
                            writeSuccess = await this.Transport.WriteAsync(subscribePacket.Encode(), cancellationToken).ConfigureAwait(false);
                            this.Client.OnSubscribeSentEventLauncher(subscribePacket);
                            break;
                        case UnsubscribePacket unsubscribePacket:
                            Logger.Trace($"{this.Client.Options.ClientId}-(W)- --> Sending UnsubscribePacket id={unsubscribePacket.PacketIdentifier}");
                            writeSuccess = await this.Transport.WriteAsync(unsubscribePacket.Encode(), cancellationToken).ConfigureAwait(false);
                            this.Client.OnUnsubscribeSentEventLauncher(unsubscribePacket);
                            break;

                        case PubAckPacket pubAckPacket:
                            Logger.Trace($"{this.Client.Options.ClientId}-(W)- --> Sending PubAckPacket id={pubAckPacket.PacketIdentifier} reason={pubAckPacket.ReasonCode}");
                            writeSuccess = await this.Transport.WriteAsync(pubAckPacket.Encode(), cancellationToken).ConfigureAwait(false);
                            await this.HandleSentPubAckPacketAsync(pubAckPacket).ConfigureAwait(false);
                            break;
                        case PubRecPacket pubRecPacket:
                            Logger.Trace($"{this.Client.Options.ClientId}-(W)- --> Sending PubRecPacket id={pubRecPacket.PacketIdentifier} reason={pubRecPacket.ReasonCode}");
                            writeSuccess = await this.Transport.WriteAsync(pubRecPacket.Encode(), cancellationToken).ConfigureAwait(false);
                            this.Client.OnPubRecSentEventLauncher(pubRecPacket);
                            break;
                        case PubRelPacket pubRelPacket:
                            Logger.Trace($"{this.Client.Options.ClientId}-(W)- --> Sending PubRelPacket id={pubRelPacket.PacketIdentifier} reason={pubRelPacket.ReasonCode}");
                            writeSuccess = await this.Transport.WriteAsync(pubRelPacket.Encode(), cancellationToken).ConfigureAwait(false);
                            this.Client.OnPubRelSentEventLauncher(pubRelPacket);
                            break;
                        case PubCompPacket pubCompPacket:
                            Logger.Trace($"{this.Client.Options.ClientId}-(W)- --> Sending PubCompPacket id={pubCompPacket.PacketIdentifier} reason={pubCompPacket.ReasonCode}");
                            writeSuccess = await this.Transport.WriteAsync(pubCompPacket.Encode(), cancellationToken).ConfigureAwait(false);
                            await this.HandleSentPubCompPacketAsync(pubCompPacket).ConfigureAwait(false);
                            break;

                        case PingReqPacket pingReqPacket:
                            Logger.Trace($"{this.Client.Options.ClientId}-(W)- --> Sending PingReqPacket");
                            writeSuccess = await this.Transport.WriteAsync(PingReqPacket.Encode(), cancellationToken).ConfigureAwait(false);
                            this.Client.OnPingReqSentEventLauncher(pingReqPacket);
                            break;

                        default:
                            throw new HiveMQttClientException($"{this.Client.Options.ClientId}-(W)- --> Unknown packet type {packet}");
                    } // switch

                    if (!writeSuccess)
                    {
                        Logger.Error($"{this.Client.Options.ClientId}-(W)- Write failed.  Disconnecting...");
                        if (this.State == ConnectState.Connected)
                        {
                            await this.HandleDisconnectionAsync(false).ConfigureAwait(false);
                        }

                        break;
                    }

                    this.lastCommunicationTimer.Restart();

                    if (cancellationToken.IsCancellationRequested)
                    {
                        Logger.Trace($"{this.Client.Options.ClientId}-(W)- Cancelled & exiting with {this.SendQueue.Count} packets remaining.");
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
                        Logger.Error($"{this.Client.Options.ClientId}-(W)- Exception: {ex}");
                        throw;
                    }
                }
            } // while(true)

            Logger.Debug($"{this.Client.Options.ClientId}-(W)- ConnectionWriter Exiting...{this.State}, cancellationRequested={cancellationToken.IsCancellationRequested}");
            return;
        }, cancellationToken);

    /// <summary>
    /// Asynchronous background task that handles the incoming traffic of packets.  Received packets
    /// are queued into this.ReceivedQueue for processing by ReceivedPacketsHandlerAsync.
    /// </summary>
    private Task<bool> ConnectionReaderAsync(CancellationToken cancellationToken) => Task.Run(
        async () =>
        {
            Logger.Trace($"{this.Client.Options.ClientId}-(R)- ConnectionReader Starting...{this.State}");

            while (this.State is ConnectState.Connecting or ConnectState.Connected)
            {
                try
                {
                    var readResult = await this.Transport.ReadAsync().ConfigureAwait(false);

                    if (readResult.Failed)
                    {
                        Logger.Debug($"{this.Client.Options.ClientId}-(R)- ConnectionReader exiting: Read from transport failed.");
                        if (this.State == ConnectState.Connected)
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
                                Logger.Debug($"{this.Client.Options.ClientId}-(R)- Malformed packet received: {decodedPacket}");

                                var opts = new DisconnectOptions
                                {
                                    ReasonCode = DisconnectReasonCode.MalformedPacket,
                                    ReasonString = "Client couldn't decode packet.",
                                };
                                return await this.Client.DisconnectAsync(opts).ConfigureAwait(false);
                            }

                            // Not enough data in the buffer to decode a packet
                            // Advance the reader to the end of the consumed data
                            buffer = buffer.Slice(0, consumed);
                            this.Transport.AdvanceTo(buffer.Start, readResult.Buffer.End);
                            Logger.Trace($"{this.Client.Options.ClientId}-(R)- ConnectionReader: PacketDecoder.TryDecode returned false.  Waiting for more data...");
                            break;
                        }

                        // Advance the reader to indicate how much of the buffer has been consumed
                        buffer = buffer.Slice(consumed);
                        this.Transport.AdvanceTo(buffer.Start);

                        // We handle disconnects immediately
                        if (decodedPacket is DisconnectPacket disconnectPacket)
                        {
                            await this.HandleIncomingDisconnectPacketAsync(disconnectPacket).ConfigureAwait(false);
                            break;
                        }

                        // Check that maximum packet size has not been exceeded
                        if (this.Client.Options.ClientMaximumPacketSize is not null && decodedPacket.PacketSize > this.Client.Options.ClientMaximumPacketSize)
                        {
                            Logger.Error($"Received a packet that exceeds the requested maximum of {this.Client.Options.ClientMaximumPacketSize}.  Disconnecting.");
                            Logger.Debug($"{this.Client.Options.ClientId}-(RPH)- Received packet size {decodedPacket.PacketSize} for packet {decodedPacket.GetType().Name}");

                            var opts = new DisconnectOptions
                            {
                                ReasonCode = DisconnectReasonCode.PacketTooLarge,
                                ReasonString = "Packet size is larger than the requested Maximum Packet Size.",
                            };
                            await this.Client.DisconnectAsync(opts).ConfigureAwait(false);
                            return false;
                        }

                        // For QoS 1 and 2 publishes, potentially apply back pressure according to ReceiveMaximum
                        if (decodedPacket is PublishPacket publishPacket)
                        {
                            // Limit the number of concurrent incoming QoS 1 and QoS 2 transactions
                            if (publishPacket.Message.QoS is QualityOfService.ExactlyOnceDelivery ||
                                publishPacket.Message.QoS is QualityOfService.AtLeastOnceDelivery)
                            {
                                if (publishPacket.Message.Duplicate)
                                {
                                    // We've received a retransmitted publish packet.
                                    // Remove any prior transaction chain and reprocess the packet.
                                    Logger.Debug($"{this.Client.Options.ClientId}-(R)- Received a retransmitted publish packet with id={publishPacket.PacketIdentifier}.  Removing any prior transaction chain.");
                                    _ = this.IPubTransactionQueue.Remove(publishPacket.PacketIdentifier, out _);
                                }

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
                                    return await this.Client.DisconnectAsync(opts).ConfigureAwait(false);
                                }
                            }
                        }

                        Logger.Trace($"{this.Client.Options.ClientId}-(R)- <-- Received {decodedPacket.GetType().Name} id: {decodedPacket.PacketIdentifier}.  Adding to receivedQueue.");
                        this.ReceivedQueue.Enqueue(decodedPacket);
                    } // while (buffer.Length > 0

                    // Check for cancellation
                    if (cancellationToken.IsCancellationRequested)
                    {
                        Logger.Trace($"{this.Client.Options.ClientId}-(R)- Cancelled & exiting...");
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
                        Logger.Error($"{this.Client.Options.ClientId}-(R)- Exception: {ex}");
                        throw;
                    }
                }
            } // while (this.State is ConnectState.Connecting or ConnectState.Connected)

            Logger.Debug($"{this.Client.Options.ClientId}-(R)- ConnectionReader Exiting...{this.State}, cancellationRequested={cancellationToken.IsCancellationRequested}");
            return true;
        }, cancellationToken);

    /// <summary>
    /// Continually processes the packets queued in the receivedQueue.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token to stop the task.</param>
    private Task ReceivedPacketsHandlerAsync(CancellationToken cancellationToken) => Task.Run(
        async () =>
        {
            Logger.Trace($"{this.Client.Options.ClientId}-(RPH)- Starting...{this.State}");

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
                            Logger.Trace($"{this.Client.Options.ClientId}-(RPH)- <-- Received SubAck id={subAckPacket.PacketIdentifier}");
                            this.Client.OnSubAckReceivedEventLauncher(subAckPacket);
                            break;
                        case UnsubAckPacket unsubAckPacket:
                            Logger.Trace($"{this.Client.Options.ClientId}-(RPH)- <-- Received UnsubAck id={unsubAckPacket.PacketIdentifier}");
                            this.Client.OnUnsubAckReceivedEventLauncher(unsubAckPacket);
                            break;

                        case PublishPacket publishPacket:
                            await this.HandleIncomingPublishPacketAsync(publishPacket).ConfigureAwait(false);
                            break;
                        case PubAckPacket pubAckPacket:
                            await this.HandleIncomingPubAckPacketAsync(pubAckPacket).ConfigureAwait(false);
                            break;
                        case PubRecPacket pubRecPacket:
                            await this.HandleIncomingPubRecPacketAsync(pubRecPacket).ConfigureAwait(false);
                            break;
                        case PubRelPacket pubRelPacket:
                            this.HandleIncomingPubRelPacket(pubRelPacket);
                            break;
                        case PubCompPacket pubCompPacket:
                            await this.HandleIncomingPubCompPacketAsync(pubCompPacket).ConfigureAwait(false);
                            break;

                        case PingRespPacket pingRespPacket:
                            Logger.Trace($"{this.Client.Options.ClientId}-(RPH)- <-- Received PingResp");
                            this.Client.OnPingRespReceivedEventLauncher(pingRespPacket);
                            break;

                        case DisconnectPacket disconnectPacket:
                            // Disconnects are handled immediate and shouldn't be received here
                            // We leave this just as a sanity backup
                            Logger.Error($"{this.Client.Options.ClientId}-(RPH)- Incorrectly received Disconnect packet in ReceivedPacketsHandlerAsync");
                            throw new HiveMQttClientException("Received Disconnect packet in ReceivedPacketsHandlerAsync");
                        default:
                            Logger.Trace($"{this.Client.Options.ClientId}-(RPH)- <-- Received Unknown packet type.  Will discard.");
                            Logger.Error($"Unrecognized packet received.  Will discard. {packet}");
                            break;
                    } // switch (packet)

                    if (cancellationToken.IsCancellationRequested)
                    {
                        Logger.Trace($"{this.Client.Options.ClientId}-(RPH)- Cancelled with {this.ReceivedQueue.Count} received packets remaining.  Exiting...");
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
                        Logger.Error($"{this.Client.Options.ClientId}-(RPH)- Exception: {ex}");
                        throw;
                    }
                }
            } // while (true)

            Logger.Debug($"{this.Client.Options.ClientId}-(RPH)- ReceivedPacketsHandler Exiting...{this.State}, cancellationRequested={cancellationToken.IsCancellationRequested}");
            return;
        }, cancellationToken);
}
