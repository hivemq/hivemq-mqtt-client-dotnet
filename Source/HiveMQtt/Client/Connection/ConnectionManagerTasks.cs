namespace HiveMQtt.Client.Connection;

using Microsoft.Extensions.Logging;
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

    internal Task? ConnectionMonitorThread { get; set; }

    /// <summary>
    /// Health check method to assure that tasks haven't faulted unexpectedly.
    /// </summary>
    private void RunTaskHealthCheck(Task? task, string taskName)
    {
        if (task is null)
        {
            this.logger.LogInformation("{ClientId}-(CM)- {TaskName} is not running.", this.Client.Options.ClientId, taskName);
        }
        else
        {
            if (task.IsFaulted)
            {
                this.logger.LogError(task.Exception, "{ClientId}-(CM)- {TaskName} Faulted", this.Client.Options.ClientId, taskName);
                this.logger.LogError("{ClientId}-(CM)- {TaskName} died.  Disconnecting.", this.Client.Options.ClientId, taskName);

                // Use semaphore to prevent concurrent disconnection attempts
                // Fire-and-forget but with proper synchronization and exception handling
                _ = Task.Run(async () =>
                {
                    // Check if already disconnected before attempting
                    if (this.State == ConnectState.Disconnected)
                    {
                        this.logger.LogTrace("{ClientId}-(CM)- Already disconnected, skipping disconnection.", this.Client.Options.ClientId);
                        return;
                    }

                    // Try to acquire semaphore with zero timeout (non-blocking)
                    if (!await this.disconnectionSemaphore.WaitAsync(0).ConfigureAwait(false))
                    {
                        this.logger.LogTrace("{ClientId}-(CM)- Disconnection already in progress, skipping duplicate call.", this.Client.Options.ClientId);
                        return;
                    }

                    try
                    {
                        // Double-check state after acquiring semaphore
                        if (this.State == ConnectState.Disconnected)
                        {
                            this.logger.LogTrace("{ClientId}-(CM)- Already disconnected after acquiring semaphore.", this.Client.Options.ClientId);
                            return;
                        }

                        // Start disconnection
                        await this.HandleDisconnectionAsync(false).ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        this.logger.LogError(ex, "{ClientId}-(CM)- Exception during disconnection from health check", this.Client.Options.ClientId);
                    }
                    finally
                    {
                        this.disconnectionSemaphore.Release();
                    }
                });
            }
        }
    }

    private Task LaunchConnectionMonitorThreadAsync(CancellationToken cancellationToken) =>
        this.ConnectionMonitorAsync(cancellationToken);

    /// <summary>
    /// Asynchronous background task that monitors the connection state and sends PingReq packets when
    /// necessary.
    /// </summary>
    private async Task ConnectionMonitorAsync(CancellationToken cancellationToken)
    {
        this.logger.LogTrace("{ClientId}-(CM)- Starting...{State}", this.Client.Options.ClientId, this.State);
        if (this.Client.Options.KeepAlive == 0)
        {
            this.logger.LogDebug("{ClientId}-(CM)- KeepAlive is 0.  No pings will be sent.", this.Client.Options.ClientId);
        }

        var keepAlivePeriod = this.Client.Options.KeepAlive;
        this.lastCommunicationTimer.Start();

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                // Capture state once to avoid race conditions
                var currentState = this.State;

                // If connected and no recent packets have been sent, send a ping
                if (currentState == ConnectState.Connected)
                {
                    if (this.Client.Options.KeepAlive > 0 && this.lastCommunicationTimer.Elapsed > TimeSpan.FromSeconds(keepAlivePeriod))
                    {
                        // Send PingReq
                        this.logger.LogTrace("{ClientId}-(CM)- --> PingReq", this.Client.Options.ClientId);
                        this.SendQueue.Enqueue(new PingReqPacket());
                    }
                }

                // Dumping Client State
                this.logger.LogDebug("{ClientId}-(CM)- {State}: last communications {Elapsed} ago", this.Client.Options.ClientId, this.State, this.lastCommunicationTimer.Elapsed);
                this.logger.LogDebug("{ClientId}-(CM)- SendQueue:...............{Count}", this.Client.Options.ClientId, this.SendQueue.Count);
                this.logger.LogDebug("{ClientId}-(CM)- ReceivedQueue:...........{Count}", this.Client.Options.ClientId, this.ReceivedQueue.Count);
                this.logger.LogDebug("{ClientId}-(CM)- OutgoingPublishQueue:....{Count}", this.Client.Options.ClientId, this.OutgoingPublishQueue.Count);
                this.logger.LogDebug("{ClientId}-(CM)- OPubTransactionQueue:....{Count}/{Capacity}", this.Client.Options.ClientId, this.OPubTransactionQueue.Count, this.OPubTransactionQueue.Capacity);
                this.logger.LogDebug("{ClientId}-(CM)- IPubTransactionQueue:....{Count}/{Capacity}", this.Client.Options.ClientId, this.IPubTransactionQueue.Count, this.IPubTransactionQueue.Capacity);
                this.logger.LogDebug("{ClientId}-(CM)- # of Subscriptions:......{Count}", this.Client.Options.ClientId, this.Client.Subscriptions.Count);
                this.logger.LogDebug("{ClientId}-(CM)- PacketIDsInUse:..........{Count}", this.Client.Options.ClientId, this.PacketIDManager.Count);

                // Background Tasks Health Check
                this.RunTaskHealthCheck(this.ConnectionWriterTask, "ConnectionWriter");
                this.RunTaskHealthCheck(this.ConnectionReaderTask, "ConnectionReader");
                this.RunTaskHealthCheck(this.ConnectionPublishWriterTask, "ConnectionPublishWriter");
                this.RunTaskHealthCheck(this.ReceivedPacketsHandlerTask, "ReceivedPacketsHandler");

                // Sleep cycle
                await Task.Delay(2000, cancellationToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                this.logger.LogDebug("{ClientId}-(CM)- Stopped by cancellation token", this.Client.Options.ClientId);
                break;
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "{ClientId}-(CM)- Exception", this.Client.Options.ClientId);

                // Handle exception gracefully - trigger disconnection and exit
                // Capture state once to avoid race conditions
                var currentState = this.State;
                if (currentState == ConnectState.Connected)
                {
                    try
                    {
                        await this.HandleDisconnectionAsync(false).ConfigureAwait(false);
                    }
                    catch (Exception disconnectEx)
                    {
                        this.logger.LogWarning(disconnectEx, "{ClientId}-(CM)- Exception during disconnection", this.Client.Options.ClientId);
                    }
                }

                break;
            }
        } // while (true)
    }

    /// <summary>
    /// Asynchronous background task that handles the outgoing publish packets queued in OutgoingPublishQueue.
    /// </summary>
    private async Task ConnectionPublishWriterAsync(CancellationToken cancellationToken)
    {
        this.logger.LogTrace("{ClientId}-(PW)- Starting...{State}", this.Client.Options.ClientId, this.State);

        while (true)
        {
            try
            {
                // Await connection readiness without polling to avoid arbitrary delay
                if (this.State != ConnectState.Connected)
                {
                    this.logger.LogTrace("{ClientId}-(PW)- Not connected.  Waiting for connect...", this.Client.Options.ClientId);
                    await this.WaitUntilConnectedAsync(cancellationToken).ConfigureAwait(false);
                }

                var writeSuccess = true;
                var publishPacket = await this.OutgoingPublishQueue.DequeueAsync(cancellationToken).ConfigureAwait(false);

                if (publishPacket.Message.QoS is QualityOfService.AtLeastOnceDelivery ||
                    publishPacket.Message.QoS is QualityOfService.ExactlyOnceDelivery)
                {
                    this.logger.LogTrace("{ClientId}-(PW)- --> Sending QoS={QoS} PublishPacket id={PacketId}", this.Client.Options.ClientId, publishPacket.Message.QoS, publishPacket.PacketIdentifier);

                    // QoS > 0 - Add to transaction queue.  OPubTransactionQueue will block when necessary
                    // to respect the broker's ReceiveMaximum
                    var success = await this.OPubTransactionQueue.AddAsync(
                        publishPacket.PacketIdentifier,
                        new List<ControlPacket> { publishPacket },
                        cancellationToken).ConfigureAwait(false);

                    if (!success)
                    {
                        this.logger.LogWarning("Duplicate packet ID detected {PacketId} while queueing to transaction queue for an outgoing QoS {QoS} publish.", publishPacket.PacketIdentifier, publishPacket.Message.QoS);
                        continue;
                    }
                }
                else
                {
                    this.logger.LogTrace("{ClientId}-(PW)- --> Sending QoS 0 PublishPacket", this.Client.Options.ClientId);
                }

                writeSuccess = await this.Transport.WriteAsync(publishPacket.Encode(), cancellationToken).ConfigureAwait(false);
                this.Client.OnPublishSentEventLauncher(publishPacket);

                if (!writeSuccess)
                {
                    this.logger.LogTrace("{ClientId}-(PW)- ConnectionPublishWriter: Failed to write to transport.", this.Client.Options.ClientId);

                    // Capture state once to avoid race conditions
                    var currentState = this.State;
                    if (currentState == ConnectState.Connected)
                    {
                        // This is an unexpected exit and may be due to a network failure.
                        this.logger.LogDebug("{ClientId}-(PW)- ConnectionPublishWriter: unexpected exit.  Disconnecting...", this.Client.Options.ClientId);
                        await this.HandleDisconnectionAsync(false).ConfigureAwait(false);
                    }

                    break;
                }

                if (cancellationToken.IsCancellationRequested)
                {
                    this.logger.LogTrace("{ClientId}-(PW)- Cancelled & existing with {Count} publish packets remaining.", this.Client.Options.ClientId, this.OutgoingPublishQueue.Count);
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
                    this.logger.LogError(ex, "{ClientId}-(PW)- Exception", this.Client.Options.ClientId);

                    // Handle exception gracefully - trigger disconnection and exit
                    // Capture state once to avoid race conditions
                    var currentState = this.State;
                    if (currentState == ConnectState.Connected)
                    {
                        try
                        {
                            await this.HandleDisconnectionAsync(false).ConfigureAwait(false);
                        }
                        catch (Exception disconnectEx)
                        {
                            this.logger.LogWarning(disconnectEx, "{ClientId}-(PW)- Exception during disconnection", this.Client.Options.ClientId);
                        }
                    }

                    break;
                }
            }
        } // while(true)

        this.logger.LogDebug("{ClientId}-(PW)- ConnectionPublishWriter Exiting...{State}, cancellationRequested={CancellationRequested}", this.Client.Options.ClientId, this.State, cancellationToken.IsCancellationRequested);
    }

    /// <summary>
    /// Asynchronous background task that handles the outgoing traffic of packets queued in the sendQueue.
    /// </summary>
    private async Task ConnectionWriterAsync(CancellationToken cancellationToken)
    {
        this.logger.LogTrace("{ClientId}-(W)- Starting...{State}", this.Client.Options.ClientId, this.State);

        while (true)
        {
            try
            {
                // We allow this task to run in Connecting, Connected, and Disconnecting states
                // because it is the one that has to send the CONNECT and DISCONNECT packets.
                if (this.State == ConnectState.Disconnected)
                {
                    this.logger.LogTrace("{ClientId}-(W)- Not connected.  Waiting for connect...", this.Client.Options.ClientId);
                    await this.WaitUntilNotDisconnectedAsync(cancellationToken).ConfigureAwait(false);
                }

                var packet = await this.SendQueue.DequeueAsync(cancellationToken).ConfigureAwait(false);
                var writeSuccess = true;

                switch (packet)
                {
                    // FIXME: Only one connect, subscribe or unsubscribe packet can be sent at a time.
                    case ConnectPacket connectPacket:
                        this.logger.LogTrace("{ClientId}-(W)- --> Sending ConnectPacket", this.Client.Options.ClientId);
                        writeSuccess = await this.Transport.WriteAsync(connectPacket.Encode(), cancellationToken).ConfigureAwait(false);
                        this.Client.OnConnectSentEventLauncher(connectPacket);
                        break;
                    case DisconnectPacket disconnectPacket:
                        this.logger.LogTrace("{ClientId}-(W)- --> Sending DisconnectPacket", this.Client.Options.ClientId);
                        writeSuccess = await this.Transport.WriteAsync(disconnectPacket.Encode(), cancellationToken).ConfigureAwait(false);
                        this.Client.OnDisconnectSentEventLauncher(disconnectPacket);
                        break;
                    case SubscribePacket subscribePacket:
                        this.logger.LogTrace("{ClientId}-(W)- --> Sending SubscribePacket id={PacketId}", this.Client.Options.ClientId, subscribePacket.PacketIdentifier);
                        writeSuccess = await this.Transport.WriteAsync(subscribePacket.Encode(), cancellationToken).ConfigureAwait(false);
                        this.Client.OnSubscribeSentEventLauncher(subscribePacket);
                        break;
                    case UnsubscribePacket unsubscribePacket:
                        this.logger.LogTrace("{ClientId}-(W)- --> Sending UnsubscribePacket id={PacketId}", this.Client.Options.ClientId, unsubscribePacket.PacketIdentifier);
                        writeSuccess = await this.Transport.WriteAsync(unsubscribePacket.Encode(), cancellationToken).ConfigureAwait(false);
                        this.Client.OnUnsubscribeSentEventLauncher(unsubscribePacket);
                        break;

                    case PubAckPacket pubAckPacket:
                        this.logger.LogTrace("{ClientId}-(W)- --> Sending PubAckPacket id={PacketId} reason={ReasonCode}", this.Client.Options.ClientId, pubAckPacket.PacketIdentifier, pubAckPacket.ReasonCode);
                        writeSuccess = await this.Transport.WriteAsync(pubAckPacket.Encode(), cancellationToken).ConfigureAwait(false);
                        await this.HandleSentPubAckPacketAsync(pubAckPacket).ConfigureAwait(false);
                        break;
                    case PubRecPacket pubRecPacket:
                        this.logger.LogTrace("{ClientId}-(W)- --> Sending PubRecPacket id={PacketId} reason={ReasonCode}", this.Client.Options.ClientId, pubRecPacket.PacketIdentifier, pubRecPacket.ReasonCode);
                        writeSuccess = await this.Transport.WriteAsync(pubRecPacket.Encode(), cancellationToken).ConfigureAwait(false);
                        this.Client.OnPubRecSentEventLauncher(pubRecPacket);
                        break;
                    case PubRelPacket pubRelPacket:
                        this.logger.LogTrace("{ClientId}-(W)- --> Sending PubRelPacket id={PacketId} reason={ReasonCode}", this.Client.Options.ClientId, pubRelPacket.PacketIdentifier, pubRelPacket.ReasonCode);
                        writeSuccess = await this.Transport.WriteAsync(pubRelPacket.Encode(), cancellationToken).ConfigureAwait(false);
                        this.Client.OnPubRelSentEventLauncher(pubRelPacket);
                        break;
                    case PubCompPacket pubCompPacket:
                        this.logger.LogTrace("{ClientId}-(W)- --> Sending PubCompPacket id={PacketId} reason={ReasonCode}", this.Client.Options.ClientId, pubCompPacket.PacketIdentifier, pubCompPacket.ReasonCode);
                        writeSuccess = await this.Transport.WriteAsync(pubCompPacket.Encode(), cancellationToken).ConfigureAwait(false);
                        await this.HandleSentPubCompPacketAsync(pubCompPacket).ConfigureAwait(false);
                        break;

                    case PingReqPacket pingReqPacket:
                        this.logger.LogTrace("{ClientId}-(W)- --> Sending PingReqPacket", this.Client.Options.ClientId);
                        writeSuccess = await this.Transport.WriteAsync(PingReqPacket.Encode(), cancellationToken).ConfigureAwait(false);
                        this.Client.OnPingReqSentEventLauncher(pingReqPacket);
                        break;

                    default:
                        throw new HiveMQttClientException($"{this.Client.Options.ClientId}-(W)- --> Unknown packet type {packet}");
                } // switch

                if (!writeSuccess)
                {
                    this.logger.LogError("{ClientId}-(W)- Write failed.  Disconnecting...", this.Client.Options.ClientId);

                    // Capture state once to avoid race conditions
                    var currentState = this.State;
                    if (currentState == ConnectState.Connected)
                    {
                        await this.HandleDisconnectionAsync(false).ConfigureAwait(false);
                    }

                    break;
                }

                this.lastCommunicationTimer.Restart();

                if (cancellationToken.IsCancellationRequested)
                {
                    this.logger.LogTrace("{ClientId}-(W)- Cancelled & exiting with {Count} packets remaining.", this.Client.Options.ClientId, this.SendQueue.Count);
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
                    this.logger.LogError(ex, "{ClientId}-(W)- Exception", this.Client.Options.ClientId);

                    // Handle exception gracefully - trigger disconnection and exit
                    // Capture state once to avoid race conditions
                    var currentState = this.State;
                    if (currentState == ConnectState.Connected)
                    {
                        try
                        {
                            await this.HandleDisconnectionAsync(false).ConfigureAwait(false);
                        }
                        catch (Exception disconnectEx)
                        {
                            this.logger.LogWarning(disconnectEx, "{ClientId}-(W)- Exception during disconnection", this.Client.Options.ClientId);
                        }
                    }

                    break;
                }
            }
        } // while(true)

        this.logger.LogDebug("{ClientId}-(W)- ConnectionWriter Exiting...{State}, cancellationRequested={CancellationRequested}", this.Client.Options.ClientId, this.State, cancellationToken.IsCancellationRequested);
    }

    /// <summary>
    /// Asynchronous background task that handles the incoming traffic of packets.  Received packets
    /// are queued into this.ReceivedQueue for processing by ReceivedPacketsHandlerAsync.
    /// </summary>
    private async Task<bool> ConnectionReaderAsync(CancellationToken cancellationToken)
    {
        this.logger.LogTrace("{ClientId}-(R)- ConnectionReader Starting...{State}", this.Client.Options.ClientId, this.State);

        while (this.State is ConnectState.Connecting or ConnectState.Connected)
        {
            try
            {
                var readResult = await this.Transport.ReadAsync(cancellationToken).ConfigureAwait(false);

                if (readResult.Failed)
                {
                    this.logger.LogDebug("{ClientId}-(R)- ConnectionReader exiting: Read from transport failed.", this.Client.Options.ClientId);

                    // Capture state once to avoid race conditions
                    var currentState = this.State;
                    if (currentState == ConnectState.Connected)
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
                            this.logger.LogError("Malformed packet received.  Disconnecting...");
                            this.logger.LogDebug("{ClientId}-(R)- Malformed packet received: {Packet}", this.Client.Options.ClientId, decodedPacket);

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
                        this.logger.LogTrace("{ClientId}-(R)- ConnectionReader: PacketDecoder.TryDecode returned false.  Waiting for more data...", this.Client.Options.ClientId);
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
                        this.logger.LogError("Received a packet that exceeds the requested maximum of {MaxPacketSize}.  Disconnecting.", this.Client.Options.ClientMaximumPacketSize);
                        this.logger.LogDebug("{ClientId}-(RPH)- Received packet size {PacketSize} for packet {PacketType}", this.Client.Options.ClientId, decodedPacket.PacketSize, decodedPacket.GetType().Name);

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
                                this.logger.LogDebug("{ClientId}-(R)- Received a retransmitted publish packet with id={PacketId}.  Removing any prior transaction chain.", this.Client.Options.ClientId, publishPacket.PacketIdentifier);
                                _ = this.IPubTransactionQueue.Remove(publishPacket.PacketIdentifier, out _);
                            }

                            var success = await this.IPubTransactionQueue.AddAsync(
                                publishPacket.PacketIdentifier,
                                new List<ControlPacket> { publishPacket },
                                cancellationToken).ConfigureAwait(false);

                            if (!success)
                            {
                                this.logger.LogError("Received a publish with a duplicate packet identifier {PacketId} for a transaction already in progress.  Disconnecting.", publishPacket.PacketIdentifier);

                                var opts = new DisconnectOptions
                                {
                                    ReasonCode = DisconnectReasonCode.UnspecifiedError,
                                    ReasonString = "Client received a publish with duplicate packet identifier for a transaction already in progress.",
                                };
                                return await this.Client.DisconnectAsync(opts).ConfigureAwait(false);
                            }
                        }
                    }

                    this.logger.LogTrace("{ClientId}-(R)- <-- Received {PacketType} id: {PacketId}.  Adding to receivedQueue.", this.Client.Options.ClientId, decodedPacket.GetType().Name, decodedPacket.PacketIdentifier);
                    this.ReceivedQueue.Enqueue(decodedPacket);
                } // while (buffer.Length > 0

                // Check for cancellation
                if (cancellationToken.IsCancellationRequested)
                {
                    this.logger.LogTrace("{ClientId}-(R)- Cancelled & exiting...", this.Client.Options.ClientId);
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
                    this.logger.LogError(ex, "{ClientId}-(R)- Exception", this.Client.Options.ClientId);

                    // Handle exception gracefully - trigger disconnection and exit
                    // Capture state once to avoid race conditions
                    var currentState = this.State;
                    if (currentState == ConnectState.Connected)
                    {
                        try
                        {
                            await this.HandleDisconnectionAsync(false).ConfigureAwait(false);
                        }
                        catch (Exception disconnectEx)
                        {
                            this.logger.LogWarning(disconnectEx, "{ClientId}-(R)- Exception during disconnection", this.Client.Options.ClientId);
                        }
                    }

                    break;
                }
            }
        } // while (this.State is ConnectState.Connecting or ConnectState.Connected)

        this.logger.LogDebug("{ClientId}-(R)- ConnectionReader Exiting...{State}, cancellationRequested={CancellationRequested}", this.Client.Options.ClientId, this.State, cancellationToken.IsCancellationRequested);
        return true;
    }

    /// <summary>
    /// Continually processes the packets queued in the receivedQueue.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token to stop the task.</param>
    private async Task ReceivedPacketsHandlerAsync(CancellationToken cancellationToken)
    {
        this.logger.LogTrace("{ClientId}-(RPH)- Starting...{State}", this.Client.Options.ClientId, this.State);

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
                        this.logger.LogTrace("{ClientId}-(RPH)- <-- Received SubAck id={PacketId}", this.Client.Options.ClientId, subAckPacket.PacketIdentifier);
                        this.Client.OnSubAckReceivedEventLauncher(subAckPacket);
                        break;
                    case UnsubAckPacket unsubAckPacket:
                        this.logger.LogTrace("{ClientId}-(RPH)- <-- Received UnsubAck id={PacketId}", this.Client.Options.ClientId, unsubAckPacket.PacketIdentifier);
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
                        this.logger.LogTrace("{ClientId}-(RPH)- <-- Received PingResp", this.Client.Options.ClientId);
                        this.Client.OnPingRespReceivedEventLauncher(pingRespPacket);
                        break;

                    case DisconnectPacket disconnectPacket:
                        // Disconnects are handled immediate and shouldn't be received here
                        // We leave this just as a sanity backup
                        this.logger.LogError("{ClientId}-(RPH)- Incorrectly received Disconnect packet in ReceivedPacketsHandlerAsync", this.Client.Options.ClientId);
                        throw new HiveMQttClientException("Received Disconnect packet in ReceivedPacketsHandlerAsync");
                    default:
                        this.logger.LogTrace("{ClientId}-(RPH)- <-- Received Unknown packet type.  Will discard.", this.Client.Options.ClientId);
                        this.logger.LogError("Unrecognized packet received.  Will discard. {Packet}", packet);
                        break;
                } // switch (packet)

                if (cancellationToken.IsCancellationRequested)
                {
                    this.logger.LogTrace("{ClientId}-(RPH)- Cancelled with {Count} received packets remaining.  Exiting...", this.Client.Options.ClientId, this.ReceivedQueue.Count);
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
                    this.logger.LogError(ex, "{ClientId}-(RPH)- Exception", this.Client.Options.ClientId);

                    // Handle exception gracefully - trigger disconnection and exit
                    // Capture state once to avoid race conditions
                    var currentState = this.State;
                    if (currentState == ConnectState.Connected)
                    {
                        try
                        {
                            await this.HandleDisconnectionAsync(false).ConfigureAwait(false);
                        }
                        catch (Exception disconnectEx)
                        {
                            this.logger.LogWarning(disconnectEx, "{ClientId}-(RPH)- Exception during disconnection", this.Client.Options.ClientId);
                        }
                    }

                    break;
                }
            }
        } // while (true)

        this.logger.LogDebug("{ClientId}-(RPH)- ReceivedPacketsHandler Exiting...{State}, cancellationRequested={CancellationRequested}", this.Client.Options.ClientId, this.State, cancellationToken.IsCancellationRequested);
    }
}
