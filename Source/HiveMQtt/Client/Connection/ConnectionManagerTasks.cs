namespace HiveMQtt.Client.Connection;

public partial class ConnectionManager
{
    internal Task? ConnectionPublishWriterTask { get; set; }

    internal Task? ConnectionWriterTask { get; set; }

    internal Task? ConnectionReaderTask { get; set; }

    internal Task? ReceivedPacketsHandlerTask { get; set; }

    internal Task? ConnectionMonitorTask { get; set; }

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
                    // If connected and no recent packets have been sent, send a ping
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
                    Logger.Debug($"{this.Options.ClientId}-(CM)- PacketIDsInUse:..........{this.PacketIDManager.Count}");

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

                    if (publishPacket.Message.QoS is QualityOfService.AtLeastOnceDelivery ||
                        publishPacket.Message.QoS is QualityOfService.ExactlyOnceDelivery)
                    {
                        Logger.Trace($"{this.Options.ClientId}-(PW)- --> Sending QoS={publishPacket.Message.QoS} PublishPacket id={publishPacket.PacketIdentifier}");

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
                        Logger.Trace($"{this.Options.ClientId}-(PW)- --> Sending QoS 0 PublishPacket");
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
                            Logger.Trace($"{this.Options.ClientId}-(W)- --> Sending ConnectPacket");
                            writeResult = await this.WriteAsync(connectPacket.Encode()).ConfigureAwait(false);
                            this.OnConnectSentEventLauncher(connectPacket);
                            break;
                        case DisconnectPacket disconnectPacket:
                            Logger.Trace($"{this.Options.ClientId}-(W)- --> Sending DisconnectPacket");
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
                            await this.HandleSentPubAckPacketAsync(pubAckPacket).ConfigureAwait(false);
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
                            await this.HandleSentPubCompPacketAsync(pubCompPacket).ConfigureAwait(false);
                            break;

                        case PingReqPacket pingReqPacket:
                            Logger.Trace($"{this.Options.ClientId}-(W)- --> Sending PingReqPacket");
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
                                if (publishPacket.Message.Duplicate)
                                {
                                    // We've received a retransmitted publish packet.
                                    // Remove any prior transaction chain and reprocess the packet.
                                    Logger.Debug($"{this.Options.ClientId}-(R)- Received a retransmitted publish packet with id={publishPacket.PacketIdentifier}.  Removing any prior transaction chain.");
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
                            Logger.Trace($"{this.Options.ClientId}-(RPH)- <-- Received PingResp");
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


}
