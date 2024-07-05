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
using System.Text;
using System.Threading.Tasks;

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
/// The excellent, superb and slightly wonderful HiveMQ C# MQTT Client.
/// </summary>
public partial class HiveMQClient : IDisposable, IHiveMQClient
{
    private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

    internal ConnectState ConnectState { get; set; }

    public HiveMQClient(HiveMQClientOptions? options = null)
    {
        this.ConnectState = ConnectState.Disconnected;

        options ??= new HiveMQClientOptions();
        options.Validate();

        Logger.Trace($"New client created: Client ID: {options.ClientId}");

        Logger.Trace("Trace Level Logging Legend:");
        Logger.Trace("    -(W)-   == ConnectionWriter");
        Logger.Trace("    -(PW)-  == ConnectionPublishWriter");
        Logger.Trace("    -(R)-   == ConnectionReader");
        Logger.Trace("    -(CM)-  == ConnectionMonitor");
        Logger.Trace("    -(RPH)- == ReceivedPacketsHandler");

        this.Options = options;
        this.cancellationTokenSource = new CancellationTokenSource();

        if (this.Options.AutomaticReconnect)
        {
            this.AfterDisconnect += AutomaticReconnectHandler;
        }

        // In-flight transaction queues
        this.IPubTransactionQueue = new BoundedDictionaryX<int, List<ControlPacket>>(this.Options.ClientReceiveMaximum);

        // Set protocol default until ConnAck is received
        this.OPubTransactionQueue = new BoundedDictionaryX<int, List<ControlPacket>>(65535);
    }

    /// <inheritdoc />
    public Dictionary<string, string> LocalStore { get; } = new();

    /// <inheritdoc />
    public HiveMQClientOptions Options { get; set; }

    /// <inheritdoc />
    public List<Subscription> Subscriptions { get; } = new();

    /// <inheritdoc />
    public bool IsConnected() => this.ConnectState == ConnectState.Connected;

    /// <inheritdoc />
    public async Task<ConnectResult> ConnectAsync()
    {
        this.ConnectState = ConnectState.Connecting;

        Logger.Info("Connecting to broker at {0}:{1}", this.Options.Host, this.Options.Port);

        // Fire the corresponding event
        this.BeforeConnectEventLauncher(this.Options);

        var socketIsConnected = await this.ConnectSocketAsync().ConfigureAwait(false);

        var taskCompletionSource = new TaskCompletionSource<ConnAckPacket>();
        void TaskHandler(object? sender, OnConnAckReceivedEventArgs args) => taskCompletionSource.SetResult(args.ConnAckPacket);

        EventHandler<OnConnAckReceivedEventArgs> eventHandler = TaskHandler;
        this.OnConnAckReceived += eventHandler;

        // Construct the MQTT Connect packet and queue to send
        var connPacket = new ConnectPacket(this.Options);
        Logger.Trace($"Queuing CONNECT packet for send.");
        this.SendQueue.Enqueue(connPacket);

        ConnAckPacket connAck;
        ConnectResult connectResult;
        try
        {
            connAck = await taskCompletionSource.Task.WaitAsync(TimeSpan.FromMilliseconds(this.Options.ConnectTimeoutInMs)).ConfigureAwait(false);
        }
        catch (TimeoutException)
        {
            this.ConnectState = ConnectState.Disconnected;
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
            this.ConnectState = ConnectState.Connected;
        }
        else
        {
            this.ConnectState = ConnectState.Disconnected;
        }

        connectResult = new ConnectResult(connAck.ReasonCode, connAck.SessionPresent, connAck.Properties);

        // Data massage: This class is used for end users.  Let's prep the data so it's easily understandable.
        // If the Session Expiry Interval is absent the value in the CONNECT Packet used.
        connectResult.Properties.SessionExpiryInterval ??= (uint)this.Options.SessionExpiryInterval;

        // Fire the corresponding event
        this.AfterConnectEventLauncher(connectResult);

        return connectResult;
    }

    /// <inheritdoc />
    public async Task<bool> DisconnectAsync(DisconnectOptions? options = null)
    {
        if (this.ConnectState == ConnectState.Disconnecting)
        {
            // We're already disconnecting in another task.
            return true;
        }

        if (this.ConnectState != ConnectState.Connected)
        {
            Logger.Warn($"DisconnectAsync called but this client is not connected.  State is {this.ConnectState}.");
            return false;
        }

        options ??= new DisconnectOptions();

        Logger.Info("Disconnecting from broker at {0}:{1}", this.Options.Host, this.Options.Port);

        // Fire the corresponding event
        this.BeforeDisconnectEventLauncher();

        var disconnectPacket = new DisconnectPacket
        {
            DisconnectReasonCode = options.ReasonCode,
        };

        // Once this is set, no more incoming packets or outgoing will be accepted
        this.ConnectState = ConnectState.Disconnecting;

        var taskCompletionSource = new TaskCompletionSource<DisconnectPacket>();
        void TaskHandler(object? sender, OnDisconnectSentEventArgs args) => taskCompletionSource.SetResult(args.DisconnectPacket);
        EventHandler<OnDisconnectSentEventArgs> eventHandler = TaskHandler;
        this.OnDisconnectSent += eventHandler;

        Logger.Trace($"Queuing DISCONNECT packet for send.");
        this.SendQueue.Enqueue(disconnectPacket);

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

        return await this.HandleDisconnectionAsync().ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<PublishResult> PublishAsync(MQTT5PublishMessage message, CancellationToken cancellationToken = default)
    {
        message.Validate();

        // QoS 0: Fast Service
        if (message.QoS == QualityOfService.AtMostOnceDelivery)
        {
            var publishPacket = new PublishPacket(message, 0);
            Logger.Trace($"Queuing QoS 0 publish packet for send: {publishPacket.GetType().Name}");

            this.OutgoingPublishQueue.Enqueue(publishPacket);
            return new PublishResult(publishPacket.Message);
        }
        else if (message.QoS == QualityOfService.AtLeastOnceDelivery)
        {
            // QoS 1: Acknowledged Delivery
            var packetIdentifier = await this.PacketIDManager.GetAvailablePacketIDAsync().ConfigureAwait(false);
            var publishPacket = new PublishPacket(message, (ushort)packetIdentifier);
            PubAckPacket pubAckPacket;

            Logger.Trace($"Queuing QoS 1 publish packet for send: {publishPacket.GetType().Name} id={publishPacket.PacketIdentifier}");
            this.OutgoingPublishQueue.Enqueue(publishPacket);

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
            var packetIdentifier = await this.PacketIDManager.GetAvailablePacketIDAsync().ConfigureAwait(false);
            var publishPacket = new PublishPacket(message, (ushort)packetIdentifier);
            PublishResult? publishResult = null;

            Logger.Trace($"Queuing QoS 2 publish packet for send: {publishPacket.GetType().Name} id={publishPacket.PacketIdentifier}");
            this.OutgoingPublishQueue.Enqueue(publishPacket);

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
        // Note: Should we validate encoding here?
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
        // Fire the corresponding event
        this.BeforeSubscribeEventLauncher(options);

        // FIXME: We should only ever have one subscribe in flight at any time (for now)
        // Construct the MQTT Subscribe packet
        var packetIdentifier = await this.PacketIDManager.GetAvailablePacketIDAsync().ConfigureAwait(false);
        var subscribePacket = new SubscribePacket(options, (ushort)packetIdentifier);

        // Setup the task completion source to wait for the SUBACK
        var taskCompletionSource = new TaskCompletionSource<SubAckPacket>();
        void TaskHandler(object? sender, OnSubAckReceivedEventArgs args) => taskCompletionSource.SetResult(args.SubAckPacket);
        EventHandler<OnSubAckReceivedEventArgs> eventHandler = TaskHandler;
        this.OnSubAckReceived += eventHandler;

        // Queue the constructed packet to be sent on the wire
        this.SendQueue.Enqueue(subscribePacket);

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

        // Add the subscriptions to the client
        foreach (var subscription in subscribeResult.Subscriptions)
        {
            // If the user has registered a handler for this topic, add it to the subscription
            foreach (var handler in options.Handlers)
            {
                if (handler.Key == subscription.TopicFilter.Topic)
                {
                    subscription.MessageReceivedHandler = handler.Value;
                }
            }

            this.Subscriptions.Add(subscription);
        }

        // Fire the corresponding event
        this.AfterSubscribeEventLauncher(subscribeResult);

        return subscribeResult;
    }

    /// <inheritdoc />
    public async Task<UnsubscribeResult> UnsubscribeAsync(string topic)
    {
        var subscription = this.GetSubscriptionByTopic(topic);
        if (subscription is not null)
        {
            var unsubOptions = new UnsubscribeOptionsBuilder()
                .WithSubscription(subscription)
                .Build();

            return await this.UnsubscribeAsync(unsubOptions).ConfigureAwait(false);
        }

        throw new HiveMQttClientException($"No subscription found for topic: {topic}.  Make sure to refer to existing subscription in client.Subscriptions.");
    }

    /// <inheritdoc />
    public async Task<UnsubscribeResult> UnsubscribeAsync(Subscription subscription)
    {
        if (!this.Subscriptions.Contains(subscription))
        {
            throw new HiveMQttClientException("No such subscription found.  Make sure to take subscription(s) from HiveMQClient.Subscriptions[] or HiveMQClient.GetSubscriptionByTopic().");
        }

        var unsubOptions = new UnsubscribeOptionsBuilder()
            .WithSubscription(subscription)
            .Build();

        return await this.UnsubscribeAsync(unsubOptions).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<UnsubscribeResult> UnsubscribeAsync(List<Subscription> subscriptions)
    {
        for (var i = 0; i < subscriptions.Count; i++)
        {
            if (!this.Subscriptions.Contains(subscriptions[i]))
            {
                throw new HiveMQttClientException("No such subscription found.  Make sure to take subscription(s) from HiveMQClient.Subscriptions[] or HiveMQClient.GetSubscriptionByTopic().");
            }
        }

        var unsubOptions = new UnsubscribeOptionsBuilder()
            .WithSubscriptions(subscriptions)
            .Build();

        return await this.UnsubscribeAsync(unsubOptions).ConfigureAwait(false);
    }

    public async Task<UnsubscribeResult> UnsubscribeAsync(UnsubscribeOptions unsubOptions)
    {
        // Fire the corresponding event
        this.BeforeUnsubscribeEventLauncher(unsubOptions.Subscriptions);

        var packetIdentifier = await this.PacketIDManager.GetAvailablePacketIDAsync().ConfigureAwait(false);
        var unsubscribePacket = new UnsubscribePacket(unsubOptions, (ushort)packetIdentifier);

        var taskCompletionSource = new TaskCompletionSource<UnsubAckPacket>();
        void TaskHandler(object? sender, OnUnsubAckReceivedEventArgs args) => taskCompletionSource.SetResult(args.UnsubAckPacket);
        EventHandler<OnUnsubAckReceivedEventArgs> eventHandler = TaskHandler;
        this.OnUnsubAckReceived += eventHandler;

        this.SendQueue.Enqueue(unsubscribePacket);

        // FIXME: Cancellation token and better timeout value
        UnsubAckPacket unsubAck;
        UnsubscribeResult unsubscribeResult;
        try
        {
            unsubAck = await taskCompletionSource.Task
                                            .WaitAsync(TimeSpan.FromMilliseconds(this.Options.ResponseTimeoutInMs))
                                            .ConfigureAwait(false);

            // FIXME: Validate that the packet identifier matches
        }
        catch (TimeoutException)
        {
            // log.Error(string.Format("Connect timeout.  No response received in time.", ex);
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
            if (reasonCode == UnsubAckReasonCode.Success)
            {
                // Remove the subscription from the client
                this.Subscriptions.Remove(unsubscribeResult.Subscriptions[counter]);
            }
        }

        // Fire the corresponding event and return
        this.AfterUnsubscribeEventLauncher(unsubscribeResult);
        return unsubscribeResult;
    }
}
