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
/// The excellent, superb and slightly wonderful HiveMQ MQTT Client.
/// Fully MQTT compliant and compatible with all respectable MQTT Brokers because sharing is caring
/// and MQTT is awesome.
/// </summary>
public partial class HiveMQClient : IDisposable, IHiveMQClient
{
    private ConnectState connectState = ConnectState.Disconnected;

    public HiveMQClient(HiveMQClientOptions? options = null)
    {
        // Trace.Listeners.Add(new TextWriterTraceListener(Console.Out));
        // Trace.AutoFlush = true;
        options ??= new HiveMQClientOptions();
        this.Options = options;
    }

    /// <inheritdoc />
    public Dictionary<string, string> LocalStore { get; } = new();

    public HiveMQClientOptions Options { get; set; }

    public List<Subscription> Subscriptions { get; } = new();

    internal MQTT5Properties? ConnectionProperties { get; }

    public bool IsConnected() => this.connectState == ConnectState.Connected;

    /// <inheritdoc />
    public async Task<ConnectResult> ConnectAsync()
    {
        this.connectState = ConnectState.Connecting;

        // Fire the corresponding event
        this.BeforeConnectEventLauncher(this.Options);

        var socketIsConnected = await this.ConnectSocketAsync().ConfigureAwait(false);

        var taskCompletionSource = new TaskCompletionSource<ConnAckPacket>();

        EventHandler<OnConnAckReceivedEventArgs> eventHandler = (sender, args) =>
        {
            taskCompletionSource.SetResult(args.ConnAckPacket);
        };
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
        catch (System.TimeoutException ex)
        {
            // log.Error(string.Format("Connect timeout.  No response received in time.", ex);
            throw;
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
        connectResult.Properties.SessionExpiryInterval ??= (UInt32)this.Options.SessionExpiryInterval;

        // Fire the corresponding event
        this.AfterConnectEventLauncher(connectResult);

        return connectResult;
    }

    /// <inheritdoc />
    public async Task<bool> DisconnectAsync(DisconnectOptions? options = null)
    {
        if (this.connectState != ConnectState.Connected)
        {
            // log.Error("Disconnect failed.  Client is not connected.");
            return false;
        }

        options ??= new DisconnectOptions();

        var disconnectPacket = new DisconnectPacket
        {
            DisconnectReasonCode = options.ReasonCode,
        };

        // Once this is set, no more incoming packets or outgoing will be accepted
        this.connectState = ConnectState.Disconnecting;

        var taskCompletionSource = new TaskCompletionSource<DisconnectPacket>();
        EventHandler<OnDisconnectSentEventArgs> eventHandler = (sender, args) =>
        {
            taskCompletionSource.SetResult(args.DisconnectPacket);
        };
        this.OnDisconnectSent += eventHandler;

        this.sendQueue.Enqueue(disconnectPacket);

        try
        {
            disconnectPacket = await taskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(5)).ConfigureAwait(false);
        }
        catch (System.TimeoutException ex)
        {
            // Does it matter?  We're disconnecting anyway.
        }
        finally
        {
            // Remove the event handler
            this.OnDisconnectSent -= eventHandler;
        }

        this.connectState = ConnectState.Disconnected;

        // Close the socket
        this.CloseSocket();

        // Clear the queues
        this.sendQueue.Clear();
        this.receiveQueue.Clear();

        return true;
    }

    /// <summary>
    /// Publish a message to an MQTT topic.
    /// </summary>
    /// <param name="message">The <seealso cref="MQTT5PublishMessage"/> for the Publish.</param>
    /// <returns>A <seealso cref="PublishResult"/> representing the result of the publish operation.</returns>
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

            EventHandler<OnPublishQoS1CompleteEventArgs> eventHandler = (sender, args) =>
            {
                taskCompletionSource.SetResult(args.PubAckPacket);
            };
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

            EventHandler<OnPublishQoS2CompleteEventArgs> eventHandler = (sender, args) =>
            {
                taskCompletionSource.SetResult(args.PubRecPacket);
            };
            publishPacket.OnPublishQoS2Complete += eventHandler;

            // Construct the MQTT Connect packet and queue to send
            this.sendQueue.Enqueue(publishPacket);

            var pubRecPacket = await taskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(5)).ConfigureAwait(false);

            publishPacket.OnPublishQoS2Complete -= eventHandler;
            return new PublishResult(publishPacket.Message, pubRecPacket);
        }

        throw new HiveMQttClientException("Invalid QoS value.");
    }

    /// <summary>
    /// Publish a message to an MQTT topic.
    /// <para>
    /// This is a convenience method that routes to <seealso cref="PublishAsync(MQTT5PublishMessage)"/>.
    /// </para>
    /// </summary>
    /// <param name="topic">The string topic to publish to.</param>
    /// <param name="payload">The string message to publish.</param>
    /// <param name="qos">The <seealso cref="QualityOfService"/> to use for the publish.</param>
    /// <returns>A <seealso cref="PublishResult"/> representing the result of the publish operation.</returns>
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

    /// <summary>
    /// Publish a message to an MQTT topic.
    /// <para>
    /// This is a convenience method that routes to <seealso cref="PublishAsync(MQTT5PublishMessage)"/>.
    /// </para>
    /// </summary>
    /// <param name="topic">The string topic to publish to.</param>
    /// <param name="payload">The UTF-8 encoded array of bytes to publish.</param>
    /// <param name="qos">The <seealso cref="QualityOfService"/> to use for the publish.</param>
    /// <returns>A <seealso cref="PublishResult"/> representing the result of the publish operation.</returns>
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

    /// <summary>
    /// Subscribe with a single topic filter on the MQTT broker.
    /// </summary>
    /// <example>
    /// Usage example:
    /// <code>
    /// var result = await client.SubscribeAsync("my/topic", QualityOfService.AtLeastOnceDelivery);
    /// </code>
    /// </example>
    /// <param name="topic">The topic filter to subscribe to.</param>
    /// <param name="qos">The <seealso cref="QualityOfService">QualityOfService</seealso> level to subscribe with.</param>
    /// <returns>SubscribeResult reflecting the result of the operation.</returns>
    public async Task<SubscribeResult> SubscribeAsync(string topic, QualityOfService qos = QualityOfService.AtMostOnceDelivery)
    {
        var options = new SubscribeOptions();

        var tf = new TopicFilter(topic, qos);
        options.TopicFilters.Add(tf);

        return await this.SubscribeAsync(options).ConfigureAwait(false);
    }

    /// <summary>
    /// Subscribe with SubscribeOptions on the MQTT broker.
    /// </summary>
    /// <example>
    /// Usage example:
    /// <code>
    /// var options = new SubscribeOptions();
    /// options.TopicFilters.Add(new TopicFilter { Topic = "foo", QoS = QualityOfService.AtLeastOnceDelivery });
    /// options.TopicFilters.Add(new TopicFilter { Topic = "bar", QoS = QualityOfService.AtMostOnceDelivery });
    /// var result = await client.SubscribeAsync(options);
    /// </code>
    /// </example>
    /// <param name="options">The options for the subscribe request.</param>
    /// <returns>SubscribeResult reflecting the result of the operation.</returns>
    public async Task<SubscribeResult> SubscribeAsync(SubscribeOptions options)
    {
        // Fire the corresponding event
        this.BeforeSubscribeEventLauncher(options);

        var packetIdentifier = this.GeneratePacketIdentifier();
        var subscribePacket = new SubscribePacket(options, (ushort)packetIdentifier);

        var taskCompletionSource = new TaskCompletionSource<SubAckPacket>();

        // FIXME: We should only ever have one subscribe in flight at any time (for now)
        EventHandler<OnSubAckReceivedEventArgs> eventHandler = (sender, args) =>
        {
            taskCompletionSource.SetResult(args.SubAckPacket);
        };
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
        catch (System.TimeoutException ex)
        {
            // log.Error(string.Format("Connect timeout.  No response received in time.", ex);
            throw;
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

    /// <summary>
    /// Unsubscribe from a single topic filter on the MQTT broker.
    /// </summary>
    /// <exception cref="HiveMQttClientException">Thrown if no subscription is found for the topic.</exception>
    /// <param name="topic">The topic filter to unsubscribe from.</param>
    /// <returns>UnsubscribeResult reflecting the result of the operation.</returns>
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

    /// <summary>
    /// Unsubscribe from a single topic filter on the MQTT broker.
    /// </summary>
    /// <exception cref="HiveMQttClientException">Thrown if no subscription is found for the topic.</exception>
    /// <param name="subscription">The subscription from client.Subscriptions to unsubscribe from.</param>
    /// <returns>UnsubscribeResult reflecting the result of the operation.</returns>
    public async Task<UnsubscribeResult> UnsubscribeAsync(Subscription subscription)
    {
        if (!this.Subscriptions.Contains(subscription))
        {
            throw new HiveMQttClientException("No such subscription found.  Make sure to take individual subscription from client.Subscriptions.");
        }

        var subscriptions = new List<Subscription> { subscription };
        return await this.UnsubscribeAsync(subscriptions).ConfigureAwait(false);
    }

    /// <summary>
    /// Unsubscribe from a single topic filter on the MQTT broker.
    /// </summary>
    /// <exception cref="HiveMQttClientException">Thrown if no subscription is found for the topic.</exception>
    /// <param name="subscriptions">The subscriptions from client.Subscriptions to unsubscribe from.</param>
    /// <returns>UnsubscribeResult reflecting the result of the operation.</returns>
    public async Task<UnsubscribeResult> UnsubscribeAsync(List<Subscription> subscriptions)
    {
        // Fire the corresponding event
        this.BeforeUnsubscribeEventLauncher(subscriptions);

        var packetIdentifier = this.GeneratePacketIdentifier();
        var unsubscribePacket = new UnsubscribePacket(subscriptions, (ushort)packetIdentifier);

        var taskCompletionSource = new TaskCompletionSource<UnsubAckPacket>();
        EventHandler<OnUnsubAckReceivedEventArgs> eventHandler = (sender, args) =>
        {
            taskCompletionSource.SetResult(args.UnsubAckPacket);
        };
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
        catch (System.TimeoutException ex)
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
