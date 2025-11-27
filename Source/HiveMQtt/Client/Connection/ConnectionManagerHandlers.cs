namespace HiveMQtt.Client.Connection;

using Microsoft.Extensions.Logging;
using HiveMQtt.Client.Internal;
using HiveMQtt.Client.Options;
using HiveMQtt.MQTT5;
using HiveMQtt.MQTT5.Packets;
using HiveMQtt.MQTT5.ReasonCodes;
using HiveMQtt.MQTT5.Types;

/// <summary>
/// Represents a connection manager for handling MQTT connection-related operations.
/// </summary>
public partial class ConnectionManager
{
    /// <summary>
    /// Handle an incoming ConnAck packet.
    /// </summary>
    /// <param name="connAckPacket">The received ConnAck packet.</param>
    internal void HandleIncomingConnAckPacket(ConnAckPacket connAckPacket)
    {
        this.logger.LogTrace("{ClientId}-(RPH)- <-- Received ConnAck", this.Client.Options.ClientId);

        // If SessionPresent is false, we need to reset any in-flight transactions
        // To manage disconnections, users should subscribe to the OnPublishSent event and timeout
        // if no response is received from the broker.  This is done to make this client simpler,
        // and to avoid the complexity of managing in-flight transactions across connections.
        if (!connAckPacket.SessionPresent)
        {
            this.IPubTransactionQueue.Clear();
            this.OPubTransactionQueue.Clear();

            // Session is not present on the broker; clear local subscription tracking
            _ = this.Client.ClearSubscriptionsAsync();
        }

        if (connAckPacket.ReasonCode == ConnAckReasonCode.Success && connAckPacket.Properties.ReceiveMaximum != null)
        {
            this.logger.LogDebug("{ClientId}-(RPH)- <-- Broker ReceiveMaximum is {ReceiveMaximum}.", this.Client.Options.ClientId, connAckPacket.Properties.ReceiveMaximum);

            // Replace the OPubTransactionQueue BoundedDictionary with a new one with the broker's ReceiveMaximum
            var loggerFactory = this.Client.Options.LoggerFactory ?? Microsoft.Extensions.Logging.Abstractions.NullLoggerFactory.Instance;
            this.OPubTransactionQueue = new BoundedDictionaryX<int, List<ControlPacket>>((int)connAckPacket.Properties.ReceiveMaximum, loggerFactory);
        }

        this.ConnectionProperties = connAckPacket.Properties;

        // Update cached connection properties for fast access during publish operations
        this.Client.UpdateConnectionPropertyCache(connAckPacket.Properties);

        this.Client.OnConnAckReceivedEventLauncher(connAckPacket);
    }

    /// <summary>
    /// Handle an incoming Disconnect packet.
    /// </summary>
    /// <param name="disconnectPacket">The received Disconnect packet.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    internal async Task HandleIncomingDisconnectPacketAsync(DisconnectPacket disconnectPacket)
    {
        this.logger.LogError("--> Disconnect received <--: {ReasonCode} {ReasonString}", disconnectPacket.DisconnectReasonCode, disconnectPacket.Properties.ReasonString);
        await this.HandleDisconnectionAsync(false).ConfigureAwait(false);
        this.Client.OnDisconnectReceivedEventLauncher(disconnectPacket);
    }

    /// <summary>
    /// Handle an incoming Publish packet.
    /// </summary>
    /// <param name="publishPacket">The received publish packet.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    internal async Task HandleIncomingPublishPacketAsync(PublishPacket publishPacket)
    {
        bool success;

        this.Client.OnPublishReceivedEventLauncher(publishPacket);

        if (publishPacket.Message.QoS is QualityOfService.AtMostOnceDelivery)
        {
            this.logger.LogTrace("{ClientId}-(RPH)- <-- Received QoS 0 Publish", this.Client.Options.ClientId);
            this.Client.OnMessageReceivedEventLauncher(publishPacket);
        }
        else if (publishPacket.Message.QoS is QualityOfService.AtLeastOnceDelivery)
        {
            // We've received a QoS 1 publish.  The transaction chain was created & added
            // by ConnectionReaderAsync to enforce the client's ReceiveMaximum
            // Send a PubAck and update the chain.  Once the PubAckPacket is sent,
            // the transaction chain will be deleted and the appropriate events will be
            // launched in HandleSentPubAckPacketAsync.
            this.logger.LogTrace("{ClientId}-(RPH)- <-- Received QoS 1 Publish id={PacketId}", this.Client.Options.ClientId, publishPacket.PacketIdentifier);
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
                    this.logger.LogError("QoS1: Couldn't update Publish --> PubAck QoS1 Chain for packet identifier {PacketId}. Discarded.", publishPacket.PacketIdentifier);
                    this.IPubTransactionQueue.Remove(publishPacket.PacketIdentifier, out _);
                    await this.PacketIDManager.MarkPacketIDAsAvailableAsync(publishPacket.PacketIdentifier).ConfigureAwait(false);

                    var opts = new DisconnectOptions
                    {
                        ReasonCode = DisconnectReasonCode.UnspecifiedError,
                        ReasonString = "Client internal error updating publish transaction chain.",
                    };
                    await this.Client.DisconnectAsync(opts).ConfigureAwait(false);
                }
            }
            else
            {
                var opts = new DisconnectOptions
                {
                    ReasonCode = DisconnectReasonCode.UnspecifiedError,
                    ReasonString = "Client internal error managing publish transaction chain.",
                };
                await this.Client.DisconnectAsync(opts).ConfigureAwait(false);
                await this.PacketIDManager.MarkPacketIDAsAvailableAsync(publishPacket.PacketIdentifier).ConfigureAwait(false);
                this.logger.LogError("QoS1: Received Publish with an unknown packet identifier {PacketId}.", publishPacket.PacketIdentifier);
            }
        }
        else if (publishPacket.Message.QoS is QualityOfService.ExactlyOnceDelivery)
        {
            // We've received a QoS 2 publish.  The transaction chain was created & added
            // by ConnectionReaderAsync to enforce the client's ReceiveMaximum.
            // Send a PubRec and add to QoS2 transaction register.  Once PubComp is sent,
            // Subscribers will be notified and the transaction chain will be deleted.
            this.logger.LogTrace("{ClientId}-(RPH)- <-- Received QoS 2 Publish id={PacketId}", this.Client.Options.ClientId, publishPacket.PacketIdentifier);
            var pubRecResponse = new PubRecPacket(publishPacket.PacketIdentifier, PubRecReasonCode.Success);

            // Get the QoS2 transaction chain for this packet identifier and add the PubRec to it
            success = this.IPubTransactionQueue.TryGetValue(publishPacket.PacketIdentifier, out var publishQoS2Chain);
            publishQoS2Chain.Add(pubRecResponse);

            if (success)
            {
                // Update the chain in the queue
                if (!this.IPubTransactionQueue.TryUpdate(publishPacket.PacketIdentifier, publishQoS2Chain, publishQoS2Chain))
                {
                    this.logger.LogError("QoS2: Couldn't update Publish --> PubRec QoS2 Chain for packet identifier {PacketId}. Discarded.", publishPacket.PacketIdentifier);
                    this.IPubTransactionQueue.Remove(publishPacket.PacketIdentifier, out _);
                    await this.PacketIDManager.MarkPacketIDAsAvailableAsync(publishPacket.PacketIdentifier).ConfigureAwait(false);
                }
            }
            else
            {
                var opts = new DisconnectOptions
                {
                    ReasonCode = DisconnectReasonCode.UnspecifiedError,
                    ReasonString = "Client internal error managing publish transaction chain.",
                };
                await this.Client.DisconnectAsync(opts).ConfigureAwait(false);
                await this.PacketIDManager.MarkPacketIDAsAvailableAsync(publishPacket.PacketIdentifier).ConfigureAwait(false);
                this.logger.LogError("QoS2: Received Publish with an unknown packet identifier {PacketId}.", publishPacket.PacketIdentifier);
            }

            this.SendQueue.Enqueue(pubRecResponse);
        }
    }

    /// <summary>
    /// Handle an incoming PubAck packet.
    /// </summary>
    /// <param name="pubAckPacket">The received PubAck packet.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    internal async Task HandleIncomingPubAckPacketAsync(PubAckPacket pubAckPacket)
    {
        this.logger.LogTrace("{ClientId}-(RPH)- <-- Received PubAck id={PacketId} reason={ReasonCode}", this.Client.Options.ClientId, pubAckPacket.PacketIdentifier, pubAckPacket.ReasonCode);
        this.Client.OnPubAckReceivedEventLauncher(pubAckPacket);

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
            this.logger.LogWarning("QoS1: Received PubAck with an unknown packet identifier {PacketId}. Discarded.", pubAckPacket.PacketIdentifier);
        }

        // QoS1 transaction is done.  Release the packet identifier
        await this.PacketIDManager.MarkPacketIDAsAvailableAsync(pubAckPacket.PacketIdentifier).ConfigureAwait(false);
    }

    /// <summary>
    /// Handle an incoming PubRec packet.
    /// </summary>
    /// <param name="pubRecPacket">The received PubRec packet.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    internal async Task HandleIncomingPubRecPacketAsync(PubRecPacket pubRecPacket)
    {
        this.logger.LogTrace("{ClientId}-(RPH)- <-- Received PubRec id={PacketId} reason={ReasonCode}", this.Client.Options.ClientId, pubRecPacket.PacketIdentifier, pubRecPacket.ReasonCode);
        this.Client.OnPubRecReceivedEventLauncher(pubRecPacket);

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
                this.logger.LogError("QoS2: Couldn't update PubRec --> PubRel QoS2 Chain for packet identifier {PacketId}.", pubRecPacket.PacketIdentifier);
                this.OPubTransactionQueue.Remove(pubRecPacket.PacketIdentifier, out _);
                await this.PacketIDManager.MarkPacketIDAsAvailableAsync(pubRecPacket.PacketIdentifier).ConfigureAwait(false);

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
        this.logger.LogTrace("{ClientId}-(RPH)- <-- Received PubRel id={PacketId} reason={ReasonCode}", this.Client.Options.ClientId, pubRelPacket.PacketIdentifier, pubRelPacket.ReasonCode);
        this.Client.OnPubRelReceivedEventLauncher(pubRelPacket);

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
                this.logger.LogWarning("QoS2: Couldn't update PubRel --> PubComp QoS2 Chain for packet identifier {PacketId}.", pubRelPacket.PacketIdentifier);
            }
        }
        else
        {
            this.logger.LogWarning("QoS2: Received PubRel with an unknown packet identifier {PacketId}. Responding with PubComp PacketIdentifierNotFound.", pubRelPacket.PacketIdentifier);

            // Send a PUBCOMP with PacketIdentifierNotFound
            pubCompResponsePacket = new PubCompPacket(pubRelPacket.PacketIdentifier, PubCompReasonCode.PacketIdentifierNotFound);
        }

        this.SendQueue.Enqueue(pubCompResponsePacket);
    }

    /// <summary>
    /// Handle an incoming PubComp packet.
    /// </summary>
    /// <param name="pubAckPacket">The received PubComp packet.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    internal async Task HandleSentPubAckPacketAsync(PubAckPacket pubAckPacket)
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
            this.Client.OnMessageReceivedEventLauncher(publishPacket);
        }
        else
        {
            // FIXME: Send an appropriate disconnect packet?
            this.logger.LogWarning("QoS1: Couldn't remove PubAck --> Publish QoS1 Chain for packet identifier {PacketId}.", pubAckPacket.PacketIdentifier);
        }

        // QoS1 transaction is done.  Release the packet identifier
        await this.PacketIDManager.MarkPacketIDAsAvailableAsync(pubAckPacket.PacketIdentifier).ConfigureAwait(false);

        // The Packet Event
        this.Client.OnPubAckSentEventLauncher(pubAckPacket);
    }

    /// <summary>
    /// Action to take once a PubComp packet is sent.
    /// </summary>
    /// <param name="pubCompPacket">The sent PubComp packet.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    internal async Task HandleSentPubCompPacketAsync(PubCompPacket pubCompPacket)
    {
        this.logger.LogTrace("{ClientId}-(RPH)- <-- Sent PubComp id={PacketId} reason={ReasonCode}", this.Client.Options.ClientId, pubCompPacket.PacketIdentifier, pubCompPacket.ReasonCode);

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
                this.Client.OnMessageReceivedEventLauncher(originalPublishPacket);
            }
        }

        // QoS2 transaction is done.  Release the packet identifier
        await this.PacketIDManager.MarkPacketIDAsAvailableAsync(pubCompPacket.PacketIdentifier).ConfigureAwait(false);

        // Trigger the general event
        this.Client.OnPubCompSentEventLauncher(pubCompPacket);
    }

    /// <summary>
    /// Handle an incoming PubComp packet.
    /// </summary>
    /// <param name="pubCompPacket">The received PubComp packet.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    internal async Task HandleIncomingPubCompPacketAsync(PubCompPacket pubCompPacket)
    {
        this.logger.LogTrace("{ClientId}-(RPH)- <-- Received PubComp id={PacketId} reason={ReasonCode}", this.Client.Options.ClientId, pubCompPacket.PacketIdentifier, pubCompPacket.ReasonCode);
        this.Client.OnPubCompReceivedEventLauncher(pubCompPacket);

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
            this.logger.LogWarning("QoS2: Received PubComp with an unknown packet identifier {PacketId}. Discarded.", pubCompPacket.PacketIdentifier);
        }

        // QoS2 transaction is done.  Release the packet identifier
        await this.PacketIDManager.MarkPacketIDAsAvailableAsync(pubCompPacket.PacketIdentifier).ConfigureAwait(false);
    }

    /// <summary>
    /// Close the socket and set the connect state to disconnected.
    /// </summary>
    /// <param name="clean">Indicates whether the disconnect was intended or not.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    internal async Task<bool> HandleDisconnectionAsync(bool clean = true)
    {
        // Thread-safe check: if already disconnected, return early
        if (this.State == ConnectState.Disconnected)
        {
            this.logger.LogTrace("HandleDisconnection: Already disconnected.");
            return false;
        }

        this.logger.LogDebug("HandleDisconnection: Handling disconnection. clean={Clean}.", clean);

        // Reset the connection-ready signal for the next connect cycle
        this.ResetConnectedSignal();

        // Cancel all background tasks BEFORE setting state to Disconnected
        // This prevents race conditions where tasks check state after it's set but before cancellation
        await this.CancelBackgroundTasksAsync().ConfigureAwait(false);

        // Close the Transport
        await this.Transport.CloseAsync().ConfigureAwait(false);

        // Set state to Disconnected AFTER tasks are cancelled and transport is closed
        // This ensures tasks see the correct state when they check during cancellation
        this.State = ConnectState.Disconnected;
        this.ResetNotDisconnectedSignal();

        // Clear cached connection properties since we're disconnected
        this.Client.UpdateConnectionPropertyCache(null);

        if (clean)
        {
            if (!this.SendQueue.IsEmpty)
            {
                this.logger.LogWarning("HandleDisconnection: Send queue not empty. {Count} packets pending but we are disconnecting.", this.SendQueue.Count);
            }

            if (!this.OutgoingPublishQueue.IsEmpty)
            {
                this.logger.LogWarning("HandleDisconnection: Outgoing publish queue not empty. {Count} packets pending but we are disconnecting.", this.OutgoingPublishQueue.Count);
            }

            // We only clear the queues on explicit disconnect
            this.SendQueue.Clear();
            this.OutgoingPublishQueue.Clear();
        }

        // Delay for 1 seconds before launching the AfterDisconnect event
        await Task.Delay(1000).ConfigureAwait(false);

        // Fire the corresponding after event
        this.Client.AfterDisconnectEventLauncher(clean);
        return true;
    }
}
