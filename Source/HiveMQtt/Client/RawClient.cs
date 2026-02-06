/*
 * Copyright 2025-present HiveMQ and the HiveMQ Community
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
using System.Text;
using System.Threading.Tasks;
using HiveMQtt.Client.Connection;
using HiveMQtt.Client.Events;
using HiveMQtt.Client.Exceptions;
using HiveMQtt.Client.Internal;
using HiveMQtt.Client.Options;
using HiveMQtt.Client.Results;
using HiveMQtt.MQTT5;
using HiveMQtt.MQTT5.Packets;
using HiveMQtt.MQTT5.ReasonCodes;
using HiveMQtt.MQTT5.Types;

/// <summary>
/// A low-level MQTT client that provides direct access to MQTT v5 protocol
/// without subscription management features. This client is performance-oriented and
/// does not maintain subscription state.
/// </summary>
public partial class RawClient : IDisposable, IRawClient, IBaseMQTTClient
{
    private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

    internal ConnectionManager Connection { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="RawClient"/> class.
    /// </summary>
    /// <param name="options">The client options. If null, default options will be used.</param>
    public RawClient(HiveMQClientOptions? options = null)
    {
        options ??= new HiveMQClientOptions();
        options.Validate();

        Logger.Trace($"New RawClient created: Client ID: {options.ClientId}");

        this.Options = options;

        // Initialize the connection manager
        this.Connection = new ConnectionManager(this);
    }

    /// <inheritdoc />
    public Dictionary<string, string> LocalStore { get; } = new();

    /// <inheritdoc />
    public HiveMQClientOptions Options { get; set; }

    private readonly object _ackLock = new();

    // Cached connection properties for fast access during publish operations
    // These are updated when ConnectionProperties change to avoid repeated property access
    private int cachedTopicAliasMaximum;
    private bool cachedRetainAvailable = true; // Default to true per MQTT spec
    private int? cachedMaximumQoS;
    private bool connectionPropertiesCached;

    /// <summary>
    /// Updates the cached connection properties for fast access during publish operations.
    /// This method is called when connection properties are established or changed.
    /// </summary>
    /// <param name="properties">The connection properties to cache.</param>
    void IBaseMQTTClient.UpdateConnectionPropertyCache(MQTT5Properties? properties) => this.UpdateConnectionPropertyCache(properties);

    internal void UpdateConnectionPropertyCache(MQTT5Properties? properties)
    {
        if (properties == null)
        {
            // Reset to defaults when disconnected
            this.cachedTopicAliasMaximum = 0;
            this.cachedRetainAvailable = true;
            this.cachedMaximumQoS = null;
            this.connectionPropertiesCached = false;
            return;
        }

        this.cachedTopicAliasMaximum = properties.TopicAliasMaximum ?? 0;
        this.cachedRetainAvailable = properties.RetainAvailable ?? true;
        this.cachedMaximumQoS = properties.MaximumQoS;
        this.connectionPropertiesCached = true;
    }

    /// <summary>
    /// Clear all tracked subscriptions. This is a no-op for RawClient as it doesn't track subscriptions.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task IBaseMQTTClient.ClearSubscriptionsAsync() => Task.CompletedTask;

    /// <inheritdoc />
    public bool IsConnected() => !this.disposed && this.Connection != null && this.Connection.State == ConnectState.Connected;

    /// <inheritdoc />
    public async Task<ConnectResult> ConnectAsync(ConnectOptions? connectOptions = null)
    {
        this.Connection.State = ConnectState.Connecting;
        this.Connection.SignalNotDisconnected();

        Logger.Info("Connecting to broker at {0}:{1}", this.Options.Host, this.Options.Port);

        // Apply the connect override options if provided
        if (connectOptions != null)
        {
            if (connectOptions.SessionExpiryInterval != null)
            {
                this.Options.SessionExpiryInterval = connectOptions.SessionExpiryInterval.Value;
            }

            if (connectOptions.KeepAlive != null)
            {
                this.Options.KeepAlive = connectOptions.KeepAlive.Value;
            }

            if (connectOptions.CleanStart != null)
            {
                this.Options.CleanStart = connectOptions.CleanStart.Value;
            }
        }

        // Fire the corresponding event
        this.BeforeConnectEventLauncher(this.Options);

        await this.Connection.ConnectAsync().ConfigureAwait(true);

        var taskCompletionSource = new TaskCompletionSource<ConnAckPacket>(TaskCreationOptions.RunContinuationsAsynchronously);
        void TaskHandler(object? sender, OnConnAckReceivedEventArgs args) => taskCompletionSource.SetResult(args.ConnAckPacket);

        EventHandler<OnConnAckReceivedEventArgs> eventHandler = TaskHandler;
        this.OnConnAckReceived += eventHandler;

        // Construct the MQTT Connect packet and queue to send
        var connPacket = new ConnectPacket(this.Options);
        Logger.Trace($"Queuing CONNECT packet for send.");
        this.Connection.SendQueue.Enqueue(connPacket);

        ConnAckPacket connAck;
        ConnectResult connectResult;
        try
        {
            connAck = await taskCompletionSource.Task.WaitAsync(TimeSpan.FromMilliseconds(this.Options.ConnectTimeoutInMs)).ConfigureAwait(false);
        }
        catch (TimeoutException)
        {
            this.Connection.State = ConnectState.Disconnected;
            this.Connection.ResetNotDisconnectedSignal();
            Logger.Error($"Connect timeout.  No response received in {this.Options.ConnectTimeoutInMs} milliseconds.");
            throw new HiveMQttClientException($"Connect timeout.  No response received in {this.Options.ConnectTimeoutInMs} milliseconds.");
        }
        finally
        {
            // Remove the event handler
            this.OnConnAckReceived -= eventHandler;
        }

        if (connAck.ReasonCode == ConnAckReasonCode.Success)
        {
            this.Connection.State = ConnectState.Connected;

            // Ensure connection-ready signal is set for any writers awaiting readiness
            this.Connection.SignalNotDisconnected();
            this.Connection.SignalConnected();
        }
        else
        {
            this.Connection.State = ConnectState.Disconnected;
            this.Connection.ResetNotDisconnectedSignal();
        }

        connectResult = new ConnectResult(connAck.ReasonCode, connAck.SessionPresent, connAck.Properties);

        // Data massage: This class is used for end users.  Let's prep the data so it's easily understandable.
        // If the Session Expiry Interval is absent the value in the CONNECT Packet used.
        connectResult.Properties.SessionExpiryInterval ??= (uint)this.Options.SessionExpiryInterval;

        // Update cached connection properties for fast access during publish operations
        this.UpdateConnectionPropertyCache(connAck.Properties);

        // Fire the corresponding event
        this.AfterConnectEventLauncher(connectResult);

        return connectResult;
    }

    /// <inheritdoc />
    public async Task<bool> DisconnectAsync(DisconnectOptions? options = null)
    {
        if (this.Connection.State == ConnectState.Disconnecting)
        {
            // We're already disconnecting in another task.
            return true;
        }

        if (this.Connection.State != ConnectState.Connected)
        {
            Logger.Warn($"DisconnectAsync called but this client is not connected.  State is {this.Connection.State}.");
            return false;
        }

        options ??= new DisconnectOptions();

        Logger.Info("Disconnecting from broker at {0}:{1}", this.Options.Host, this.Options.Port);

        // Fire the corresponding event
        this.BeforeDisconnectEventLauncher();

        var disconnectPacket = new DisconnectPacket(options)
        {
            DisconnectReasonCode = options.ReasonCode,
        };

        // Once this is set, no more incoming packets or outgoing will be accepted
        this.Connection.State = ConnectState.Disconnecting;
        this.Connection.SignalNotDisconnected();

        var taskCompletionSource = new TaskCompletionSource<DisconnectPacket>(TaskCreationOptions.RunContinuationsAsynchronously);
        void TaskHandler(object? sender, OnDisconnectSentEventArgs args) => taskCompletionSource.SetResult(args.DisconnectPacket);
        EventHandler<OnDisconnectSentEventArgs> eventHandler = TaskHandler;
        this.OnDisconnectSent += eventHandler;

        Logger.Trace($"Queuing DISCONNECT packet for send.");
        this.Connection.SendQueue.Enqueue(disconnectPacket);

        try
        {
            disconnectPacket = await taskCompletionSource.Task
                                                .WaitAsync(TimeSpan.FromMilliseconds(this.Options.ResponseTimeoutInMs))
                                                .ConfigureAwait(false);
        }
        catch (TimeoutException)
        {
            // Does it matter?  We're disconnecting anyway.
        }
        finally
        {
            // Remove the event handler
            this.OnDisconnectSent -= eventHandler;
        }

        return await this.Connection.HandleDisconnectionAsync().ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<PublishResult> PublishAsync(MQTT5PublishMessage message, CancellationToken cancellationToken = default)
    {
        // Fast path for simple QoS 0 messages (most common case)
        // Skip validation overhead for messages that don't need it
        if (message.QoS == QualityOfService.AtMostOnceDelivery &&
            !message.TopicAlias.HasValue &&
            !message.Retain &&
            this.Connection?.State == ConnectState.Connected &&
            this.connectionPropertiesCached)
        {
            var publishPacket = new PublishPacket(message, 0);
            this.Connection.OutgoingPublishQueue.Enqueue(publishPacket);
            return new PublishResult(publishPacket.Message);
        }

        // Full validation path for complex cases
        message.Validate();

        // Check if topic alias is used but not supported by broker
        var topicAliasMaximum = this.connectionPropertiesCached
            ? this.cachedTopicAliasMaximum
            : (this.Connection?.ConnectionProperties?.TopicAliasMaximum ?? 0);

        if (message.TopicAlias.HasValue)
        {
            if (topicAliasMaximum == 0)
            {
                throw new HiveMQttClientException("Topic aliases are not supported by the broker");
            }

            if (message.TopicAlias.Value > topicAliasMaximum)
            {
                throw new HiveMQttClientException($"Topic alias exceeds broker's maximum allowed value of {topicAliasMaximum}");
            }
        }

        // Check if retain is used but not supported by broker
        var retainSupported = this.connectionPropertiesCached
            ? this.cachedRetainAvailable
            : (this.Connection?.ConnectionProperties?.RetainAvailable ?? true);

        if (!retainSupported && message.Retain)
        {
            throw new HiveMQttClientException("Retained messages are not supported by the broker");
        }

        // Check QoS maximum
        var maximumQoS = this.connectionPropertiesCached
            ? this.cachedMaximumQoS
            : this.Connection?.ConnectionProperties?.MaximumQoS;

        if (maximumQoS.HasValue && message.QoS.HasValue && (ushort)message.QoS.Value > maximumQoS.Value)
        {
            if (this.Connection == null)
            {
                throw new HiveMQttClientException("Connection is not available");
            }

            Logger.Debug($"Reducing message QoS from {message.QoS} to broker enforced maximum of {maximumQoS}");
            message.QoS = (QualityOfService)maximumQoS.Value;
        }

        // QoS 0: Fast Service
        if (message.QoS == QualityOfService.AtMostOnceDelivery)
        {
            var publishPacket = new PublishPacket(message, 0);
            Logger.Trace($"Queuing QoS 0 publish packet for send: {publishPacket.GetType().Name}");

            this.Connection?.OutgoingPublishQueue.Enqueue(publishPacket);
            return new PublishResult(publishPacket.Message);
        }
        else if (message.QoS == QualityOfService.AtLeastOnceDelivery)
        {
            // QoS 1: Acknowledged Delivery
            if (this.Connection == null)
            {
                throw new HiveMQttClientException("Connection is not available");
            }

            if (this.Connection.State != ConnectState.Connected)
            {
                throw new HiveMQttClientException("Client is not connected");
            }

            var packetIdentifier = await this.Connection.PacketIDManager.GetAvailablePacketIDAsync().ConfigureAwait(false);
            var publishPacket = new PublishPacket(message, (ushort)packetIdentifier);
            PubAckPacket pubAckPacket;

            Logger.Trace($"Queuing QoS 1 publish packet for send: {publishPacket.GetType().Name} id={publishPacket.PacketIdentifier}");
            this.Connection.OutgoingPublishQueue.Enqueue(publishPacket);

            try
            {
                // Wait on the QoS 1 handshake
                pubAckPacket = await publishPacket.OnPublishQoS1CompleteTCS.Task
                                                                .WaitAsync(cancellationToken)
                                                                .ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                Logger.Debug("PublishAsync: Operation cancelled by user.");
                throw;
            }

            return new PublishResult(publishPacket.Message, pubAckPacket);
        }
        else if (message.QoS == QualityOfService.ExactlyOnceDelivery)
        {
            // QoS 2: Assured Delivery
            if (this.Connection == null)
            {
                throw new HiveMQttClientException("Connection is not available");
            }

            if (this.Connection.State != ConnectState.Connected)
            {
                throw new HiveMQttClientException("Client is not connected");
            }

            var packetIdentifier = await this.Connection.PacketIDManager.GetAvailablePacketIDAsync().ConfigureAwait(false);
            var publishPacket = new PublishPacket(message, (ushort)packetIdentifier);
            var publishResult = new PublishResult(publishPacket.Message);

            Logger.Trace($"Queuing QoS 2 publish packet for send: {publishPacket.GetType().Name} id={publishPacket.PacketIdentifier}");
            this.Connection.OutgoingPublishQueue.Enqueue(publishPacket);

            List<ControlPacket> packetList;
            try
            {
                // Wait on the QoS 2 handshake
                packetList = await publishPacket.OnPublishQoS2CompleteTCS.Task
                                                        .WaitAsync(cancellationToken)
                                                        .ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                Logger.Debug("PublishAsync: Operation cancelled by user.");
                throw;
            }

            foreach (var packet in packetList)
            {
                if (packet is PubRecPacket pubRecPacket)
                {
                    publishResult = new PublishResult(publishPacket.Message, pubRecPacket);
                }
            }

            return publishResult;
        }

        throw new HiveMQttClientException("Invalid QoS value.");
    }

    /// <inheritdoc />
    public async Task<PublishResult> PublishAsync(string topic, string payload, QualityOfService qos = QualityOfService.AtMostOnceDelivery)
    {
        var message = new MQTT5PublishMessage
        {
            Topic = topic,
            Payload = Encoding.ASCII.GetBytes(payload),
            QoS = qos,
        };

        return await this.PublishAsync(message).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<PublishResult> PublishAsync(string topic, byte[] payload, QualityOfService qos = QualityOfService.AtMostOnceDelivery)
    {
        var message = new MQTT5PublishMessage
        {
            Topic = topic,
            Payload = payload,
            QoS = qos,
        };

        return await this.PublishAsync(message).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<SubscribeResult> SubscribeAsync(string topic, QualityOfService qos = QualityOfService.AtMostOnceDelivery, bool noLocal = false, bool retainAsPublished = false, RetainHandling retainHandling = RetainHandling.SendAtSubscribe)
    {
        var options = new SubscribeOptions();

        var tf = new TopicFilter(topic, qos, noLocal, retainAsPublished, retainHandling);
        options.TopicFilters.Add(tf);

        return await this.SubscribeAsync(options).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<SubscribeResult> SubscribeAsync(SubscribeOptions options)
    {
        // Check if subscription identifiers are used but not supported by broker
        var subscriptionIdentifiersSupported = this.Connection?.ConnectionProperties?.SubscriptionIdentifiersAvailable ?? true;
        if (!subscriptionIdentifiersSupported && options.SubscriptionIdentifier.HasValue)
        {
            throw new HiveMQttClientException("Subscription identifiers are not supported by the broker");
        }

        // Check if retain is used but not supported by broker
        var retainSupported = this.Connection?.ConnectionProperties?.RetainAvailable ?? true;
        if (!retainSupported)
        {
            // Check if any topic filter has retainAsPublished set to true
            foreach (var topicFilter in options.TopicFilters)
            {
                if (topicFilter.RetainAsPublished is true)
                {
                    throw new HiveMQttClientException("Retained messages are not supported by the broker");
                }
            }
        }

        // Check if shared subscriptions are used but not supported by broker
        var sharedSubscriptionSupported = this.Connection?.ConnectionProperties?.SharedSubscriptionAvailable ?? true;
        if (!sharedSubscriptionSupported)
        {
            // Check if any topic filter contains shared subscription prefix ($share/)
            foreach (var topicFilter in options.TopicFilters)
            {
                if (topicFilter.Topic.StartsWith("$share/", StringComparison.Ordinal))
                {
                    throw new HiveMQttClientException("Shared subscriptions are not supported by the broker");
                }
            }
        }

        // Check if wildcards are used but not supported by broker
        var wildcardSupported = this.Connection?.ConnectionProperties?.WildcardSubscriptionAvailable ?? true;
        if (!wildcardSupported)
        {
            // Check if any topic filter contains wildcards (+ or #)
            foreach (var topicFilter in options.TopicFilters)
            {
                if (topicFilter.Topic.Contains('+') || topicFilter.Topic.Contains('#'))
                {
                    throw new HiveMQttClientException("Wildcard subscriptions are not supported by the broker");
                }
            }
        }

        // Fire the corresponding event
        this.BeforeSubscribeEventLauncher(options);

        if (this.Connection == null)
        {
            throw new HiveMQttClientException("Connection is not available");
        }

        if (this.Connection.State != ConnectState.Connected)
        {
            throw new HiveMQttClientException("Client is not connected");
        }

        // Construct the MQTT Subscribe packet
        var packetIdentifier = await this.Connection.PacketIDManager.GetAvailablePacketIDAsync().ConfigureAwait(false);
        var subscribePacket = new SubscribePacket(options, (ushort)packetIdentifier);

        // Setup the task completion source to wait for the SUBACK
        var taskCompletionSource = new TaskCompletionSource<SubAckPacket>(TaskCreationOptions.RunContinuationsAsynchronously);
        void TaskHandler(object? sender, OnSubAckReceivedEventArgs args)
        {
            if (args.SubAckPacket.PacketIdentifier == subscribePacket.PacketIdentifier)
            {
                taskCompletionSource.SetResult(args.SubAckPacket);
            }
        }

        EventHandler<OnSubAckReceivedEventArgs> eventHandler = TaskHandler;
        this.OnSubAckReceived += eventHandler;

        // Queue the constructed packet to be sent on the wire
        this.Connection.SendQueue.Enqueue(subscribePacket);

        SubAckPacket subAck;
        SubscribeResult subscribeResult;
        try
        {
            subAck = await taskCompletionSource.Task
                                .WaitAsync(TimeSpan.FromMilliseconds(this.Options.ResponseTimeoutInMs))
                                .ConfigureAwait(false);
        }
        catch (TimeoutException)
        {
            Logger.Error("Subscribe timeout.  No SUBACK response received in time.");
            throw;
        }
        finally
        {
            // Remove the event handler
            this.OnSubAckReceived -= eventHandler;
        }

        subscribeResult = new SubscribeResult(options, subAck);

        // Note: RawClient does NOT maintain subscription state
        // The subscription result is returned but not tracked

        // Fire the corresponding event
        this.AfterSubscribeEventLauncher(subscribeResult);

        return subscribeResult;
    }

    /// <inheritdoc />
    public async Task<UnsubscribeResult> UnsubscribeAsync(UnsubscribeOptions options)
    {
        // Fire the corresponding event
        this.BeforeUnsubscribeEventLauncher(options.Subscriptions);

        if (this.Connection == null)
        {
            throw new HiveMQttClientException("Connection is not available");
        }

        if (this.Connection.State != ConnectState.Connected)
        {
            throw new HiveMQttClientException("Client is not connected");
        }

        var packetIdentifier = await this.Connection.PacketIDManager.GetAvailablePacketIDAsync().ConfigureAwait(false);
        var unsubscribePacket = new UnsubscribePacket(options, (ushort)packetIdentifier);

        var taskCompletionSource = new TaskCompletionSource<UnsubAckPacket>(TaskCreationOptions.RunContinuationsAsynchronously);
        void TaskHandler(object? sender, OnUnsubAckReceivedEventArgs args)
        {
            if (args.UnsubAckPacket.PacketIdentifier == unsubscribePacket.PacketIdentifier)
            {
                taskCompletionSource.SetResult(args.UnsubAckPacket);
            }
        }

        EventHandler<OnUnsubAckReceivedEventArgs> eventHandler = TaskHandler;
        this.OnUnsubAckReceived += eventHandler;

        this.Connection.SendQueue.Enqueue(unsubscribePacket);

        UnsubAckPacket unsubAck;
        UnsubscribeResult unsubscribeResult;
        try
        {
            unsubAck = await taskCompletionSource.Task
                                            .WaitAsync(TimeSpan.FromMilliseconds(this.Options.ResponseTimeoutInMs))
                                            .ConfigureAwait(false);
        }
        catch (TimeoutException)
        {
            throw;
        }
        finally
        {
            // Remove the event handler
            this.OnUnsubAckReceived -= eventHandler;
        }

        // Prepare the result
        unsubscribeResult = new UnsubscribeResult
        {
            Subscriptions = unsubscribePacket.Subscriptions,
        };

        var counter = 0;
        foreach (var reasonCode in unsubAck.ReasonCodes)
        {
            unsubscribeResult.Subscriptions[counter].UnsubscribeReasonCode = reasonCode;
            counter++;
        }

        // Note: RawClient does NOT maintain subscription state
        // The unsubscribe result is returned but no subscription tracking is performed

        // Fire the corresponding event and return
        this.AfterUnsubscribeEventLauncher(unsubscribeResult);
        return unsubscribeResult;
    }

    /// <inheritdoc />
    public Task AckAsync(ushort packetIdentifier)
    {
        if (this.Connection == null || this.Connection.State != ConnectState.Connected)
        {
            throw new HiveMQttClientException("Client is not connected.");
        }

        lock (this._ackLock)
        {
            this.Connection.AckIncomingPublish(packetIdentifier);
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task AckAsync(OnMessageReceivedEventArgs eventArgs)
    {
        ArgumentNullException.ThrowIfNull(eventArgs);

        if (!eventArgs.PacketIdentifier.HasValue)
        {
            return Task.CompletedTask;
        }

        return this.AckAsync(eventArgs.PacketIdentifier.Value);
    }

    private bool disposed;

    /// <summary>
    /// Disposes the client and releases all resources.
    /// </summary>
    public void Dispose()
    {
        this.Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Disposes the client and releases all resources.
    /// </summary>
    /// <param name="disposing">True if called from user code.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!this.disposed)
        {
            if (disposing)
            {
                this.Connection?.Dispose();
            }

            this.disposed = true;
        }
    }
}
