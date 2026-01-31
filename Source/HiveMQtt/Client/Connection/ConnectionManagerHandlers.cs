namespace HiveMQtt.Client.Connection;

using HiveMQtt.Client.Exceptions;
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
        Logger.Trace($"{this.Client.Options.ClientId}-(RPH)- <-- Received ConnAck");

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
            Logger.Debug($"{this.Client.Options.ClientId}-(RPH)- <-- Broker ReceiveMaximum is {connAckPacket.Properties.ReceiveMaximum}.");

            // Replace the OPubTransactionQueue BoundedDictionary with a new one with the broker's ReceiveMaximum
            this.OPubTransactionQueue = new BoundedDictionaryX<int, List<ControlPacket>>((int)connAckPacket.Properties.ReceiveMaximum);
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
        Logger.Error($"--> Disconnect received <--: {disconnectPacket.DisconnectReasonCode} {disconnectPacket.Properties.ReasonString}");
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
            Logger.Trace($"{this.Client.Options.ClientId}-(RPH)- <-- Received QoS 0 Publish");
            this.Client.OnMessageReceivedEventLauncher(publishPacket);
        }
        else if (publishPacket.Message.QoS is QualityOfService.AtLeastOnceDelivery)
        {
            // We've received a QoS 1 publish.  The transaction chain was created & added
            // by ConnectionReaderAsync to enforce the client's ReceiveMaximum
            Logger.Trace($"{this.Client.Options.ClientId}-(RPH)- <-- Received QoS 1 Publish id={publishPacket.PacketIdentifier}");

            if (this.Client.Options.ManualAckEnabled)
            {
                // Manual ack: do not send PubAck yet; deliver message to application. Application will call AckAsync.
                this.Client.OnMessageReceivedEventLauncher(publishPacket);
            }
            else
            {
                // Send a PubAck and update the chain.  Once the PubAckPacket is sent,
                // the transaction chain will be deleted and the appropriate events will be
                // launched in HandleSentPubAckPacketAsync.
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
                    Logger.Error($"QoS1: Received Publish with an unknown packet identifier {publishPacket.PacketIdentifier}.");
                }
            }
        }
        else if (publishPacket.Message.QoS is QualityOfService.ExactlyOnceDelivery)
        {
            // We've received a QoS 2 publish.  The transaction chain was created & added
            // by ConnectionReaderAsync to enforce the client's ReceiveMaximum.
            Logger.Trace($"{this.Client.Options.ClientId}-(RPH)- <-- Received QoS 2 Publish id={publishPacket.PacketIdentifier}");

            if (this.Client.Options.ManualAckEnabled)
            {
                // Manual ack: do not send PubRec yet; deliver message to application. Application will call AckAsync.
                this.Client.OnMessageReceivedEventLauncher(publishPacket);
            }
            else
            {
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
                    Logger.Error($"QoS2: Received Publish with an unknown packet identifier {publishPacket.PacketIdentifier}.");
                }

                this.SendQueue.Enqueue(pubRecResponse);
            }
        }
    }

    /// <summary>
    /// Handle an incoming PubAck packet.
    /// </summary>
    /// <param name="pubAckPacket">The received PubAck packet.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    internal async Task HandleIncomingPubAckPacketAsync(PubAckPacket pubAckPacket)
    {
        Logger.Trace($"{this.Client.Options.ClientId}-(RPH)- <-- Received PubAck id={pubAckPacket.PacketIdentifier} reason={pubAckPacket.ReasonCode}");
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
            Logger.Warn($"QoS1: Received PubAck with an unknown packet identifier {pubAckPacket.PacketIdentifier}. Discarded.");
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
        Logger.Trace($"{this.Client.Options.ClientId}-(RPH)- <-- Received PubRec id={pubRecPacket.PacketIdentifier} reason={pubRecPacket.ReasonCode}");
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
                Logger.Error($"QoS2: Couldn't update PubRec --> PubRel QoS2 Chain for packet identifier {pubRecPacket.PacketIdentifier}.");
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
        Logger.Trace($"{this.Client.Options.ClientId}-(RPH)- <-- Received PubRel id={pubRelPacket.PacketIdentifier} reason={pubRelPacket.ReasonCode}");
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

            // The Application Message Event (only when not manual ack; with manual ack it was already fired on receive)
            if (!this.Client.Options.ManualAckEnabled)
            {
                this.Client.OnMessageReceivedEventLauncher(publishPacket);
            }
        }
        else
        {
            // FIXME: Send an appropriate disconnect packet?
            Logger.Warn($"QoS1: Couldn't remove PubAck --> Publish QoS1 Chain for packet identifier {pubAckPacket.PacketIdentifier}.");
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
        Logger.Trace($"{this.Client.Options.ClientId}-(RPH)- <-- Sent PubComp id={pubCompPacket.PacketIdentifier} reason={pubCompPacket.ReasonCode}");

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

                // Trigger the application message event (only when not manual ack; with manual ack it was already fired on receive)
                if (!this.Client.Options.ManualAckEnabled)
                {
                    this.Client.OnMessageReceivedEventLauncher(originalPublishPacket);
                }
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
        Logger.Trace($"{this.Client.Options.ClientId}-(RPH)- <-- Received PubComp id={pubCompPacket.PacketIdentifier} reason={pubCompPacket.ReasonCode}");
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
            Logger.Warn($"QoS2: Received PubComp with an unknown packet identifier {pubCompPacket.PacketIdentifier}. Discarded.");
        }

        // QoS2 transaction is done.  Release the packet identifier
        await this.PacketIDManager.MarkPacketIDAsAvailableAsync(pubCompPacket.PacketIdentifier).ConfigureAwait(false);
    }

    /// <summary>
    /// Acknowledge a received QoS 1 or QoS 2 publish by sending PubAck or PubRec to the broker.
    /// Only valid when ManualAckEnabled is true and the packet identifier refers to a pending incoming publish.
    /// </summary>
    /// <param name="packetIdentifier">The packet identifier of the received publish to acknowledge.</param>
    /// <exception cref="HiveMQttClientException">Thrown when no pending incoming publish exists for the packet identifier, or when manual ack is not enabled.</exception>
    internal void AckIncomingPublish(ushort packetIdentifier)
    {
        if (!this.Client.Options.ManualAckEnabled)
        {
            throw new HiveMQttClientException("Manual acknowledgement is not enabled. Set ManualAckEnabled to true when connecting.");
        }

        if (!this.IPubTransactionQueue.TryGetValue(packetIdentifier, out var chain))
        {
            throw new HiveMQttClientException($"No pending incoming publish for packet identifier {packetIdentifier}. It may already be acknowledged or invalid.");
        }

        if (chain.Count != 1 || chain[0] is not PublishPacket publishPacket)
        {
            throw new HiveMQttClientException($"Invalid transaction state for packet identifier {packetIdentifier}.");
        }

        if (publishPacket.Message.QoS == QualityOfService.AtLeastOnceDelivery)
        {
            var pubAckResponse = new PubAckPacket(packetIdentifier, PubAckReasonCode.Success);
            chain.Add(pubAckResponse);
            if (!this.IPubTransactionQueue.TryUpdate(packetIdentifier, chain, chain))
            {
                throw new HiveMQttClientException($"Could not update transaction for packet identifier {packetIdentifier}. It may have been acknowledged already.");
            }

            this.SendQueue.Enqueue(pubAckResponse);
            Logger.Trace($"{this.Client.Options.ClientId}-(RPH)- Manual ack: enqueued PubAck for id={packetIdentifier}");
        }
        else if (publishPacket.Message.QoS == QualityOfService.ExactlyOnceDelivery)
        {
            var pubRecResponse = new PubRecPacket(packetIdentifier, PubRecReasonCode.Success);
            chain.Add(pubRecResponse);
            if (!this.IPubTransactionQueue.TryUpdate(packetIdentifier, chain, chain))
            {
                throw new HiveMQttClientException($"Could not update transaction for packet identifier {packetIdentifier}. It may have been acknowledged already.");
            }

            this.SendQueue.Enqueue(pubRecResponse);
            Logger.Trace($"{this.Client.Options.ClientId}-(RPH)- Manual ack: enqueued PubRec for id={packetIdentifier}");
        }
        else
        {
            throw new HiveMQttClientException($"Packet identifier {packetIdentifier} is not a QoS 1 or QoS 2 incoming publish.");
        }
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
            Logger.Trace("HandleDisconnection: Already disconnected.");
            return false;
        }

        Logger.Debug($"HandleDisconnection: Handling disconnection. clean={clean}.");

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
                Logger.Warn($"HandleDisconnection: Send queue not empty. {this.SendQueue.Count} packets pending but we are disconnecting.");
            }

            if (!this.OutgoingPublishQueue.IsEmpty)
            {
                Logger.Warn($"HandleDisconnection: Outgoing publish queue not empty. {this.OutgoingPublishQueue.Count} packets pending but we are disconnecting.");
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
