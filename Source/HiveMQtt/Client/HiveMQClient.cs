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
using HiveMQtt.MQTT5.Packets;
using HiveMQtt.MQTT5.ReasonCodes;
using HiveMQtt.MQTT5.Types;

/// <summary>
/// The excellent, superb and slightly wonderful HiveMQ C# MQTT Client.
/// </summary>
public partial class HiveMQClient : IDisposable, IHiveMQClient
{
    private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

    private ConnectState connectState = ConnectState.Disconnected;

    public HiveMQClient(HiveMQClientOptions? options = null)
    {
        options ??= new HiveMQClientOptions();
        this.Options = options;
    }

    /// <inheritdoc />
    public Dictionary<string, string> LocalStore { get; } = new();

    /// <inheritdoc />
    public HiveMQClientOptions Options { get; set; }

    /// <inheritdoc />
    public List<Subscription> Subscriptions { get; } = new();

    /// <inheritdoc />
    internal MQTT5Properties? ConnectionProperties { get; }

    /// <inheritdoc />
    public bool IsConnected() => this.connectState == ConnectState.Connected;

    /// <inheritdoc />
    public async Task<ConnectResult> ConnectAsync()
    {
        this.connectState = ConnectState.Connecting;

        // Fire the corresponding event
        this.BeforeConnectEventLauncher(this.Options);

        var socketIsConnected = await this.ConnectSocketAsync().ConfigureAwait(false);

        var taskCompletionSource = new TaskCompletionSource<ConnAckPacket>();
        void TaskHandler(object? sender, OnConnAckReceivedEventArgs args) => taskCompletionSource.SetResult(args.ConnAckPacket);

        EventHandler<OnConnAckReceivedEventArgs> eventHandler = TaskHandler;
        this.OnConnAckReceived += eventHandler;

        // Construct the MQTT Connect packet and queue to send
        var connPacket = new ConnectPacket(this.Options);
        this.sendQueue.Enqueue(connPacket);

        // FIXME: Cancellation token and better timeout value
        ConnAckPacket connAck;
        ConnectResult connectResult;
        try
        {
            connAck = await taskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(5)).ConfigureAwait(false);
        }
        catch (TimeoutException)
        {
            this.connectState = ConnectState.Disconnected;
            throw new HiveMQttClientException("Connect timeout.  No response received in time.");
        }
        finally
        {
            // Remove the event handler
            this.OnConnAckReceived -= eventHandler;
        }

        if (connAck.ReasonCode == ConnAckReasonCode.Success)
        {
            this.connectState = ConnectState.Connected;
        }
        else
        {
            this.connectState = ConnectState.Disconnected;
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
        if (this.connectState != ConnectState.Connected)
        {
            return false;
        }

        options ??= new DisconnectOptions();

        // Fire the corresponding event
        this.BeforeDisconnectEventLauncher();

        var disconnectPacket = new DisconnectPacket
        {
            DisconnectReasonCode = options.ReasonCode,
        };

        // Once this is set, no more incoming packets or outgoing will be accepted
        this.connectState = ConnectState.Disconnecting;

        var taskCompletionSource = new TaskCompletionSource<DisconnectPacket>();
        void TaskHandler(object? sender, OnDisconnectSentEventArgs args) => taskCompletionSource.SetResult(args.DisconnectPacket);
        EventHandler<OnDisconnectSentEventArgs> eventHandler = TaskHandler;
        this.OnDisconnectSent += eventHandler;

        this.sendQueue.Enqueue(disconnectPacket);

        try
        {
            disconnectPacket = await taskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(5)).ConfigureAwait(false);
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

        // Close the socket
        this.CloseSocket();

        // Fire the corresponding event
        this.AfterDisconnectEventLauncher(true);

        this.connectState = ConnectState.Disconnected;

        // Clear the send queue
        this.sendQueue.Clear();
        return true;
    }

    /// <inheritdoc />
    public async Task<PublishResult> PublishAsync(MQTT5PublishMessage message)
    {
        message.Validate();

        var packetIdentifier = this.GeneratePacketIdentifier();
        var publishPacket = new PublishPacket(message, (ushort)packetIdentifier);

        // QoS 0: Fast Service
        if (message.QoS == QualityOfService.AtMostOnceDelivery)
        {
            this.sendQueue.Enqueue(publishPacket);
            return new PublishResult(publishPacket.Message);
        }
        else if (message.QoS == QualityOfService.AtLeastOnceDelivery)
        {
            // QoS 1: Acknowledged Delivery
            var taskCompletionSource = new TaskCompletionSource<PubAckPacket>();
            void TaskHandler(object? sender, OnPublishQoS1CompleteEventArgs args) => taskCompletionSource.SetResult(args.PubAckPacket);
            EventHandler<OnPublishQoS1CompleteEventArgs> eventHandler = TaskHandler;
            publishPacket.OnPublishQoS1Complete += eventHandler;

            // Construct the MQTT Connect packet and queue to send
            this.sendQueue.Enqueue(publishPacket);

            var pubAckPacket = await taskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(5)).ConfigureAwait(false);

            publishPacket.OnPublishQoS1Complete -= eventHandler;
            return new PublishResult(publishPacket.Message, pubAckPacket);
        }
        else if (message.QoS == QualityOfService.ExactlyOnceDelivery)
        {
            // QoS 2: Assured Delivery
            var taskCompletionSource = new TaskCompletionSource<PubRecPacket>();
            void TaskHandler(object? sender, OnPublishQoS2CompleteEventArgs args) => taskCompletionSource.SetResult(args.PubRecPacket);
            EventHandler<OnPublishQoS2CompleteEventArgs> eventHandler = TaskHandler;
            publishPacket.OnPublishQoS2Complete += eventHandler;

            // Construct the MQTT Connect packet and queue to send
            this.sendQueue.Enqueue(publishPacket);

            var pubRecPacket = await taskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(5)).ConfigureAwait(false);

            publishPacket.OnPublishQoS2Complete -= eventHandler;
            return new PublishResult(publishPacket.Message, pubRecPacket);
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
    public async Task<SubscribeResult> SubscribeAsync(string topic, QualityOfService qos = QualityOfService.AtMostOnceDelivery)
    {
        var options = new SubscribeOptions();

        var tf = new TopicFilter(topic, qos);
        options.TopicFilters.Add(tf);

        return await this.SubscribeAsync(options).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<SubscribeResult> SubscribeAsync(SubscribeOptions options)
    {
        // Fire the corresponding event
        this.BeforeSubscribeEventLauncher(options);

        var packetIdentifier = this.GeneratePacketIdentifier();
        var subscribePacket = new SubscribePacket(options, (ushort)packetIdentifier);

        var taskCompletionSource = new TaskCompletionSource<SubAckPacket>();
        void TaskHandler(object? sender, OnSubAckReceivedEventArgs args) => taskCompletionSource.SetResult(args.SubAckPacket);

        // FIXME: We should only ever have one subscribe in flight at any time (for now)
        EventHandler<OnSubAckReceivedEventArgs> eventHandler = TaskHandler;
        this.OnSubAckReceived += eventHandler;

        // Construct the MQTT Connect packet and queue to send
        this.sendQueue.Enqueue(subscribePacket);

        // FIXME: Cancellation token and better timeout value
        SubAckPacket subAck;
        SubscribeResult subscribeResult;
        try
        {
            subAck = await taskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(5)).ConfigureAwait(false);

            // FIXME: Validate that the packet identifier matches
        }
        catch (TimeoutException ex)
        {
            // log.Error(string.Format("Connect timeout.  No response received in time.", ex);
            throw ex;
        }
        finally
        {
            // Remove the event handler
            this.OnSubAckReceived -= eventHandler;
        }

        subscribeResult = new SubscribeResult(options, subAck);

        // Add the subscriptions to the client
        this.Subscriptions.AddRange(subscribeResult.Subscriptions);

        // Fire the corresponding event
        this.AfterSubscribeEventLauncher(subscribeResult);

        return subscribeResult;
    }

    /// <inheritdoc />
    public async Task<UnsubscribeResult> UnsubscribeAsync(string topic)
    {
        foreach (var subscription in this.Subscriptions)
        {
            if (subscription.TopicFilter.Topic == topic)
            {
                return await this.UnsubscribeAsync(subscription).ConfigureAwait(false);
            }
        }

        throw new HiveMQttClientException("No subscription found for topic: " + topic);
    }

    /// <inheritdoc />
    public async Task<UnsubscribeResult> UnsubscribeAsync(Subscription subscription)
    {
        if (!this.Subscriptions.Contains(subscription))
        {
            throw new HiveMQttClientException("No such subscription found.  Make sure to take individual subscription from client.Subscriptions.");
        }

        var subscriptions = new List<Subscription> { subscription };
        return await this.UnsubscribeAsync(subscriptions).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<UnsubscribeResult> UnsubscribeAsync(List<Subscription> subscriptions)
    {
        // Fire the corresponding event
        this.BeforeUnsubscribeEventLauncher(subscriptions);

        var packetIdentifier = this.GeneratePacketIdentifier();
        var unsubscribePacket = new UnsubscribePacket(subscriptions, (ushort)packetIdentifier);

        var taskCompletionSource = new TaskCompletionSource<UnsubAckPacket>();

        void TaskHandler(object? sender, OnUnsubAckReceivedEventArgs args) => taskCompletionSource.SetResult(args.UnsubAckPacket);
        EventHandler<OnUnsubAckReceivedEventArgs> eventHandler = TaskHandler;
        this.OnUnsubAckReceived += eventHandler;

        this.sendQueue.Enqueue(unsubscribePacket);

        // FIXME: Cancellation token and better timeout value
        UnsubAckPacket unsubAck;
        UnsubscribeResult unsubscribeResult;
        try
        {
            unsubAck = await taskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(5)).ConfigureAwait(false);

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

        // Fire the corresponding event
        this.AfterUnsubscribeEventLauncher(unsubscribeResult);

        return unsubscribeResult;
    }
}
