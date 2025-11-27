namespace HiveMQtt.Client.Connection;

using HiveMQtt.Client.Exceptions;
using HiveMQtt.Client.Internal;
using HiveMQtt.Client.Options;
using HiveMQtt.MQTT5;
using HiveMQtt.MQTT5.Packets;
using HiveMQtt.MQTT5.ReasonCodes;
using HiveMQtt.MQTT5.Types;
using Microsoft.Extensions.Logging;

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
            LogTaskNotRunning(this.logger, this.Client.Options.ClientId ?? string.Empty, taskName);
        }
        else
        {
            if (task.IsFaulted)
            {
                LogTaskFaulted(this.logger, task.Exception!, this.Client.Options.ClientId ?? string.Empty, taskName);
                LogTaskDied(this.logger, this.Client.Options.ClientId ?? string.Empty, taskName);

                // Use semaphore to prevent concurrent disconnection attempts
                // Fire-and-forget but with proper synchronization and exception handling
                _ = Task.Run(async () =>
                {
                    // Check if already disconnected before attempting
                    if (this.State == ConnectState.Disconnected)
                    {
                        LogAlreadyDisconnected(this.logger, this.Client.Options.ClientId ?? string.Empty);
                        return;
                    }

                    // Try to acquire semaphore with zero timeout (non-blocking)
                    if (!await this.disconnectionSemaphore.WaitAsync(0).ConfigureAwait(false))
                    {
                        LogDisconnectionInProgress(this.logger, this.Client.Options.ClientId ?? string.Empty);
                        return;
                    }

                    try
                    {
                        // Double-check state after acquiring semaphore
                        if (this.State == ConnectState.Disconnected)
                        {
                            LogAlreadyDisconnectedAfterSemaphore(this.logger, this.Client.Options.ClientId ?? string.Empty);
                            return;
                        }

                        // Start disconnection
                        await this.HandleDisconnectionAsync(false).ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        LogHealthCheckDisconnectException(this.logger, ex, this.Client.Options.ClientId ?? string.Empty);
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
        LogConnectionMonitorStarting(this.logger, this.Client.Options.ClientId ?? string.Empty, this.State);
        if (this.Client.Options.KeepAlive == 0)
        {
            LogKeepAliveZero(this.logger, this.Client.Options.ClientId ?? string.Empty);
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
                        if (this.logger.IsEnabled(LogLevel.Trace))
                        {
                            LogSendingPingReq(this.logger, this.Client.Options.ClientId ?? string.Empty);
                        }

                        this.SendQueue.Enqueue(new PingReqPacket());
                    }
                }

                // Dumping Client State (only if Debug logging is enabled)
                if (this.logger.IsEnabled(LogLevel.Debug))
                {
                    LogConnectionMonitorState(this.logger, this.Client.Options.ClientId ?? string.Empty, this.State, this.lastCommunicationTimer.Elapsed);
                    LogSendQueueCount(this.logger, this.Client.Options.ClientId ?? string.Empty, this.SendQueue.Count);
                    LogReceivedQueueCount(this.logger, this.Client.Options.ClientId ?? string.Empty, this.ReceivedQueue.Count);
                    LogOutgoingPublishQueueCount(this.logger, this.Client.Options.ClientId ?? string.Empty, this.OutgoingPublishQueue.Count);
                    LogOPubTransactionQueueCount(this.logger, this.Client.Options.ClientId ?? string.Empty, this.OPubTransactionQueue.Count, this.OPubTransactionQueue.Capacity);
                    LogIPubTransactionQueueCount(this.logger, this.Client.Options.ClientId ?? string.Empty, this.IPubTransactionQueue.Count, this.IPubTransactionQueue.Capacity);
                    LogSubscriptionsCount(this.logger, this.Client.Options.ClientId ?? string.Empty, this.Client.Subscriptions.Count);
                    LogPacketIDsInUseCount(this.logger, this.Client.Options.ClientId ?? string.Empty, this.PacketIDManager.Count);
                }

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
                LogConnectionMonitorCancelled(this.logger, this.Client.Options.ClientId ?? string.Empty);
                break;
            }
            catch (Exception ex)
            {
                LogConnectionMonitorException(this.logger, ex, this.Client.Options.ClientId ?? string.Empty);

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
                        LogConnectionMonitorDisconnectException(this.logger, disconnectEx, this.Client.Options.ClientId ?? string.Empty);
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
        LogConnectionPublishWriterStarting(this.logger, this.Client.Options.ClientId ?? string.Empty, this.State);

        while (true)
        {
            try
            {
                // Await connection readiness without polling to avoid arbitrary delay
                if (this.State != ConnectState.Connected)
                {
                    LogPublishWriterWaitingForConnect(this.logger, this.Client.Options.ClientId ?? string.Empty);
                    await this.WaitUntilConnectedAsync(cancellationToken).ConfigureAwait(false);
                }

                var writeSuccess = true;
                var publishPacket = await this.OutgoingPublishQueue.DequeueAsync(cancellationToken).ConfigureAwait(false);

                if (publishPacket.Message.QoS is QualityOfService.AtLeastOnceDelivery ||
                    publishPacket.Message.QoS is QualityOfService.ExactlyOnceDelivery)
                {
                    if (this.logger.IsEnabled(LogLevel.Trace))
                    {
                        LogSendingQoSPublishPacket(this.logger, this.Client.Options.ClientId ?? string.Empty, publishPacket.Message.QoS, publishPacket.PacketIdentifier);
                    }

                    // QoS > 0 - Add to transaction queue.  OPubTransactionQueue will block when necessary
                    // to respect the broker's ReceiveMaximum
                    var success = await this.OPubTransactionQueue.AddAsync(
                        publishPacket.PacketIdentifier,
                        new List<ControlPacket> { publishPacket },
                        cancellationToken).ConfigureAwait(false);

                    if (!success)
                    {
                        LogDuplicatePacketId(this.logger, publishPacket.PacketIdentifier, publishPacket.Message.QoS);
                        continue;
                    }
                }
                else
                {
                    if (this.logger.IsEnabled(LogLevel.Trace))
                    {
                        LogSendingQoS0PublishPacket(this.logger, this.Client.Options.ClientId ?? string.Empty);
                    }
                }

                writeSuccess = await this.Transport.WriteAsync(publishPacket.Encode(), cancellationToken).ConfigureAwait(false);
                this.Client.OnPublishSentEventLauncher(publishPacket);

                if (!writeSuccess)
                {
                    LogPublishWriterWriteFailed(this.logger, this.Client.Options.ClientId ?? string.Empty);

                    // Capture state once to avoid race conditions
                    var currentState = this.State;
                    if (currentState == ConnectState.Connected)
                    {
                        // This is an unexpected exit and may be due to a network failure.
                        LogPublishWriterUnexpectedExit(this.logger, this.Client.Options.ClientId ?? string.Empty);
                        await this.HandleDisconnectionAsync(false).ConfigureAwait(false);
                    }

                    break;
                }

                if (cancellationToken.IsCancellationRequested)
                {
                    LogPublishWriterCancelled(this.logger, this.Client.Options.ClientId ?? string.Empty, this.OutgoingPublishQueue.Count);
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
                    LogPublishWriterException(this.logger, ex, this.Client.Options.ClientId ?? string.Empty);

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
                            LogPublishWriterDisconnectException(this.logger, disconnectEx, this.Client.Options.ClientId ?? string.Empty);
                        }
                    }

                    break;
                }
            }
        } // while(true)

        LogPublishWriterExiting(this.logger, this.Client.Options.ClientId ?? string.Empty, this.State, cancellationToken.IsCancellationRequested);
    }

    /// <summary>
    /// Asynchronous background task that handles the outgoing traffic of packets queued in the sendQueue.
    /// </summary>
    private async Task ConnectionWriterAsync(CancellationToken cancellationToken)
    {
        LogConnectionWriterStarting(this.logger, this.Client.Options.ClientId ?? string.Empty, this.State);

        while (true)
        {
            try
            {
                // We allow this task to run in Connecting, Connected, and Disconnecting states
                // because it is the one that has to send the CONNECT and DISCONNECT packets.
                if (this.State == ConnectState.Disconnected)
                {
                    LogWriterWaitingForConnect(this.logger, this.Client.Options.ClientId ?? string.Empty);
                    await this.WaitUntilNotDisconnectedAsync(cancellationToken).ConfigureAwait(false);
                }

                var packet = await this.SendQueue.DequeueAsync(cancellationToken).ConfigureAwait(false);
                var writeSuccess = true;

                switch (packet)
                {
                    // FIXME: Only one connect, subscribe or unsubscribe packet can be sent at a time.
                    case ConnectPacket connectPacket:
                        if (this.logger.IsEnabled(LogLevel.Trace))
                        {
                            LogSendingConnectPacket(this.logger, this.Client.Options.ClientId ?? string.Empty);
                        }

                        writeSuccess = await this.Transport.WriteAsync(connectPacket.Encode(), cancellationToken).ConfigureAwait(false);
                        this.Client.OnConnectSentEventLauncher(connectPacket);
                        break;
                    case DisconnectPacket disconnectPacket:
                        if (this.logger.IsEnabled(LogLevel.Trace))
                        {
                            LogSendingDisconnectPacket(this.logger, this.Client.Options.ClientId ?? string.Empty);
                        }

                        writeSuccess = await this.Transport.WriteAsync(disconnectPacket.Encode(), cancellationToken).ConfigureAwait(false);
                        this.Client.OnDisconnectSentEventLauncher(disconnectPacket);
                        break;
                    case SubscribePacket subscribePacket:
                        if (this.logger.IsEnabled(LogLevel.Trace))
                        {
                            LogSendingSubscribePacket(this.logger, this.Client.Options.ClientId ?? string.Empty, subscribePacket.PacketIdentifier);
                        }

                        writeSuccess = await this.Transport.WriteAsync(subscribePacket.Encode(), cancellationToken).ConfigureAwait(false);
                        this.Client.OnSubscribeSentEventLauncher(subscribePacket);
                        break;
                    case UnsubscribePacket unsubscribePacket:
                        if (this.logger.IsEnabled(LogLevel.Trace))
                        {
                            LogSendingUnsubscribePacket(this.logger, this.Client.Options.ClientId ?? string.Empty, unsubscribePacket.PacketIdentifier);
                        }

                        writeSuccess = await this.Transport.WriteAsync(unsubscribePacket.Encode(), cancellationToken).ConfigureAwait(false);
                        this.Client.OnUnsubscribeSentEventLauncher(unsubscribePacket);
                        break;

                    case PubAckPacket pubAckPacket:
                        if (this.logger.IsEnabled(LogLevel.Trace))
                        {
                            LogSendingPubAckPacket(this.logger, this.Client.Options.ClientId ?? string.Empty, pubAckPacket.PacketIdentifier, pubAckPacket.ReasonCode);
                        }

                        writeSuccess = await this.Transport.WriteAsync(pubAckPacket.Encode(), cancellationToken).ConfigureAwait(false);
                        await this.HandleSentPubAckPacketAsync(pubAckPacket).ConfigureAwait(false);
                        break;
                    case PubRecPacket pubRecPacket:
                        if (this.logger.IsEnabled(LogLevel.Trace))
                        {
                            LogSendingPubRecPacket(this.logger, this.Client.Options.ClientId ?? string.Empty, pubRecPacket.PacketIdentifier, pubRecPacket.ReasonCode);
                        }

                        writeSuccess = await this.Transport.WriteAsync(pubRecPacket.Encode(), cancellationToken).ConfigureAwait(false);
                        this.Client.OnPubRecSentEventLauncher(pubRecPacket);
                        break;
                    case PubRelPacket pubRelPacket:
                        if (this.logger.IsEnabled(LogLevel.Trace))
                        {
                            LogSendingPubRelPacket(this.logger, this.Client.Options.ClientId ?? string.Empty, pubRelPacket.PacketIdentifier, pubRelPacket.ReasonCode);
                        }

                        writeSuccess = await this.Transport.WriteAsync(pubRelPacket.Encode(), cancellationToken).ConfigureAwait(false);
                        this.Client.OnPubRelSentEventLauncher(pubRelPacket);
                        break;
                    case PubCompPacket pubCompPacket:
                        if (this.logger.IsEnabled(LogLevel.Trace))
                        {
                            LogSendingPubCompPacket(this.logger, this.Client.Options.ClientId ?? string.Empty, pubCompPacket.PacketIdentifier, pubCompPacket.ReasonCode);
                        }

                        writeSuccess = await this.Transport.WriteAsync(pubCompPacket.Encode(), cancellationToken).ConfigureAwait(false);
                        await this.HandleSentPubCompPacketAsync(pubCompPacket).ConfigureAwait(false);
                        break;

                    case PingReqPacket pingReqPacket:
                        if (this.logger.IsEnabled(LogLevel.Trace))
                        {
                            LogSendingPingReqPacket(this.logger, this.Client.Options.ClientId ?? string.Empty);
                        }

                        writeSuccess = await this.Transport.WriteAsync(PingReqPacket.Encode(), cancellationToken).ConfigureAwait(false);
                        this.Client.OnPingReqSentEventLauncher(pingReqPacket);
                        break;

                    default:
                        throw new HiveMQttClientException($"{this.Client.Options.ClientId ?? string.Empty}-(W)- --> Unknown packet type {packet}");
                } // switch

                if (!writeSuccess)
                {
                    LogWriterWriteFailed(this.logger, this.Client.Options.ClientId ?? string.Empty);

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
                    LogWriterCancelled(this.logger, this.Client.Options.ClientId ?? string.Empty, this.SendQueue.Count);
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
                    LogWriterException(this.logger, ex, this.Client.Options.ClientId ?? string.Empty);

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
                            LogWriterDisconnectException(this.logger, disconnectEx, this.Client.Options.ClientId ?? string.Empty);
                        }
                    }

                    break;
                }
            }
        } // while(true)

        LogWriterExiting(this.logger, this.Client.Options.ClientId ?? string.Empty, this.State, cancellationToken.IsCancellationRequested);
    }

    /// <summary>
    /// Asynchronous background task that handles the incoming traffic of packets.  Received packets
    /// are queued into this.ReceivedQueue for processing by ReceivedPacketsHandlerAsync.
    /// </summary>
    private async Task<bool> ConnectionReaderAsync(CancellationToken cancellationToken)
    {
        LogConnectionReaderStarting(this.logger, this.Client.Options.ClientId ?? string.Empty, this.State);

        while (this.State is ConnectState.Connecting or ConnectState.Connected)
        {
            try
            {
                var readResult = await this.Transport.ReadAsync(cancellationToken).ConfigureAwait(false);

                if (readResult.Failed)
                {
                    LogConnectionReaderReadFailed(this.logger, this.Client.Options.ClientId ?? string.Empty);

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
                            LogMalformedPacket(this.logger);
                            LogMalformedPacketDetails(this.logger, this.Client.Options.ClientId ?? string.Empty, decodedPacket.ToString() ?? "null");

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
                        LogWaitingForMoreData(this.logger, this.Client.Options.ClientId ?? string.Empty);
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
                        LogPacketTooLarge(this.logger, this.Client.Options.ClientMaximumPacketSize.Value);
                        LogPacketSizeDetails(this.logger, this.Client.Options.ClientId ?? string.Empty, decodedPacket.PacketSize, decodedPacket.GetType().Name);

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
                                LogRetransmittedPublish(this.logger, this.Client.Options.ClientId ?? string.Empty, publishPacket.PacketIdentifier);
                                _ = this.IPubTransactionQueue.Remove(publishPacket.PacketIdentifier, out _);
                            }

                            var success = await this.IPubTransactionQueue.AddAsync(
                                publishPacket.PacketIdentifier,
                                new List<ControlPacket> { publishPacket },
                                cancellationToken).ConfigureAwait(false);

                            if (!success)
                            {
                                LogDuplicatePublishPacketId(this.logger, publishPacket.PacketIdentifier);

                                var opts = new DisconnectOptions
                                {
                                    ReasonCode = DisconnectReasonCode.UnspecifiedError,
                                    ReasonString = "Client received a publish with duplicate packet identifier for a transaction already in progress.",
                                };
                                return await this.Client.DisconnectAsync(opts).ConfigureAwait(false);
                            }
                        }
                    }

                    if (this.logger.IsEnabled(LogLevel.Trace))
                    {
                        LogReceivedPacket(this.logger, this.Client.Options.ClientId ?? string.Empty, decodedPacket.GetType().Name, decodedPacket.PacketIdentifier);
                    }

                    this.ReceivedQueue.Enqueue(decodedPacket);
                } // while (buffer.Length > 0

                // Check for cancellation
                if (cancellationToken.IsCancellationRequested)
                {
                    LogConnectionReaderCancelled(this.logger, this.Client.Options.ClientId ?? string.Empty);
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
                    LogConnectionReaderException(this.logger, ex, this.Client.Options.ClientId ?? string.Empty);

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
                            LogConnectionReaderDisconnectException(this.logger, disconnectEx, this.Client.Options.ClientId ?? string.Empty);
                        }
                    }

                    break;
                }
            }
        } // while (this.State is ConnectState.Connecting or ConnectState.Connected)

        LogConnectionReaderExiting(this.logger, this.Client.Options.ClientId ?? string.Empty, this.State, cancellationToken.IsCancellationRequested);
        return true;
    }

    /// <summary>
    /// Continually processes the packets queued in the receivedQueue.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token to stop the task.</param>
    private async Task ReceivedPacketsHandlerAsync(CancellationToken cancellationToken)
    {
        LogReceivedPacketsHandlerStarting(this.logger, this.Client.Options.ClientId ?? string.Empty, this.State);

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
                        if (this.logger.IsEnabled(LogLevel.Trace))
                        {
                            LogReceivedSubAck(this.logger, this.Client.Options.ClientId ?? string.Empty, subAckPacket.PacketIdentifier);
                        }

                        this.Client.OnSubAckReceivedEventLauncher(subAckPacket);
                        break;
                    case UnsubAckPacket unsubAckPacket:
                        if (this.logger.IsEnabled(LogLevel.Trace))
                        {
                            LogReceivedUnsubAck(this.logger, this.Client.Options.ClientId ?? string.Empty, unsubAckPacket.PacketIdentifier);
                        }

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
                        LogReceivedPingResp(this.logger, this.Client.Options.ClientId ?? string.Empty);
                        this.Client.OnPingRespReceivedEventLauncher(pingRespPacket);
                        break;

                    case DisconnectPacket disconnectPacket:
                        // Disconnects are handled immediate and shouldn't be received here
                        // We leave this just as a sanity backup
                        LogIncorrectDisconnectPacket(this.logger, this.Client.Options.ClientId ?? string.Empty);
                        throw new HiveMQttClientException("Received Disconnect packet in ReceivedPacketsHandlerAsync");
                    default:
                        LogUnknownPacketType(this.logger, this.Client.Options.ClientId ?? string.Empty);
                        LogUnrecognizedPacket(this.logger, packet.ToString() ?? "null");
                        break;
                } // switch (packet)

                if (cancellationToken.IsCancellationRequested)
                {
                    LogReceivedPacketsHandlerCancelled(this.logger, this.Client.Options.ClientId ?? string.Empty, this.ReceivedQueue.Count);
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
                    LogReceivedPacketsHandlerException(this.logger, ex, this.Client.Options.ClientId ?? string.Empty);

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
                            LogReceivedPacketsHandlerDisconnectException(this.logger, disconnectEx, this.Client.Options.ClientId ?? string.Empty);
                        }
                    }

                    break;
                }
            }
        } // while (true)

        LogReceivedPacketsHandlerExiting(this.logger, this.Client.Options.ClientId ?? string.Empty, this.State, cancellationToken.IsCancellationRequested);
    }
}
