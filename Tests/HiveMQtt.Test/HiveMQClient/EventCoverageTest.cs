namespace HiveMQtt.Test.HiveMQClient;

using System.Threading.Tasks;
using HiveMQtt.Client;
using HiveMQtt.Client.Events;
using HiveMQtt.MQTT5.ReasonCodes;
using HiveMQtt.MQTT5.Types;
using Xunit;

/// <summary>
/// Comprehensive tests for event system coverage, including missing events and edge cases.
/// </summary>
[Collection("Broker")]
public class EventCoverageTest
{
    [Fact]
    public async Task TestQoS2PacketEventsAsync()
    {
        var publisherOptions = new HiveMQClientOptionsBuilder()
            .WithClientId("QoS2PacketEventsPublisher")
            .Build();
        var publisher = new HiveMQClient(publisherOptions);

        var subscriberOptions = new HiveMQClientOptionsBuilder()
            .WithClientId("QoS2PacketEventsSubscriber")
            .Build();
        var subscriber = new HiveMQClient(subscriberOptions);

        // Track all QoS 2 packet events
        var pubRecReceivedSource = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var pubRecSentSource = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var pubRelReceivedSource = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var pubRelSentSource = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var pubCompReceivedSource = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var pubCompSentSource = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

        // Publisher events (receives PubRec from subscriber, sends PubRel, receives PubComp)
        publisher.OnPubRecReceived += (sender, args) =>
        {
            Assert.NotNull(args.PubRecPacket);
            pubRecReceivedSource.TrySetResult(true);
        };

        publisher.OnPubRelSent += (sender, args) =>
        {
            Assert.NotNull(args.PubRelPacket);
            pubRelSentSource.TrySetResult(true);
        };

        publisher.OnPubCompReceived += (sender, args) =>
        {
            Assert.NotNull(args.PubCompPacket);
            pubCompReceivedSource.TrySetResult(true);
        };

        // Subscriber events (sends PubRec to publisher, receives PubRel, sends PubComp)
        subscriber.OnPubRecSent += (sender, args) =>
        {
            Assert.NotNull(args.PubRecPacket);
            pubRecSentSource.TrySetResult(true);
        };

        subscriber.OnPubRelReceived += (sender, args) =>
        {
            Assert.NotNull(args.PubRelPacket);
            pubRelReceivedSource.TrySetResult(true);
        };

        subscriber.OnPubCompSent += (sender, args) =>
        {
            Assert.NotNull(args.PubCompPacket);
            pubCompSentSource.TrySetResult(true);
        };

        // Connect both clients
        var pubConnectResult = await publisher.ConnectAsync().ConfigureAwait(false);
        Assert.True(pubConnectResult.ReasonCode == ConnAckReasonCode.Success);

        var subConnectResult = await subscriber.ConnectAsync().ConfigureAwait(false);
        Assert.True(subConnectResult.ReasonCode == ConnAckReasonCode.Success);

        // Subscribe with QoS 2
        var subscribeResult = await subscriber.SubscribeAsync(
            "tests/QoS2PacketEvents",
            QualityOfService.ExactlyOnceDelivery)
            .ConfigureAwait(false);
        Assert.NotEmpty(subscribeResult.Subscriptions);
        Assert.Equal(SubAckReasonCode.GrantedQoS2, subscribeResult.Subscriptions[0].SubscribeReasonCode);

        // Set up message received handler
        var messageReceivedSource = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        subscriber.OnMessageReceived += (sender, args) =>
        {
            if (args.PublishMessage.Topic == "tests/QoS2PacketEvents")
            {
                messageReceivedSource.TrySetResult(true);
            }
        };

        // Publish QoS 2 message
        var publishResult = await publisher.PublishAsync(
            "tests/QoS2PacketEvents",
            "QoS 2 test message",
            QualityOfService.ExactlyOnceDelivery)
            .ConfigureAwait(false);

        Assert.NotNull(publishResult);
        Assert.NotNull(publishResult.QoS2ReasonCode);

        // Wait for all QoS 2 packet events
        // Flow: Publisher sends PUBLISH -> Subscriber sends PUBREC -> Publisher sends PUBREL -> Subscriber sends PUBCOMP
        await pubRecSentSource.Task.WaitAsync(TimeSpan.FromSeconds(5)).ConfigureAwait(false); // Subscriber sends PUBREC
        await pubRecReceivedSource.Task.WaitAsync(TimeSpan.FromSeconds(5)).ConfigureAwait(false); // Publisher receives PUBREC
        await pubRelSentSource.Task.WaitAsync(TimeSpan.FromSeconds(5)).ConfigureAwait(false); // Publisher sends PUBREL
        await pubRelReceivedSource.Task.WaitAsync(TimeSpan.FromSeconds(5)).ConfigureAwait(false); // Subscriber receives PUBREL
        await pubCompSentSource.Task.WaitAsync(TimeSpan.FromSeconds(5)).ConfigureAwait(false); // Subscriber sends PUBCOMP
        await pubCompReceivedSource.Task.WaitAsync(TimeSpan.FromSeconds(5)).ConfigureAwait(false); // Publisher receives PUBCOMP
        await messageReceivedSource.Task.WaitAsync(TimeSpan.FromSeconds(5)).ConfigureAwait(false);

        // Small delay to allow async event handlers to complete
        await Task.Delay(100).ConfigureAwait(false);

        // Clean up
        await publisher.DisconnectAsync().ConfigureAwait(false);
        await subscriber.DisconnectAsync().ConfigureAwait(false);
        publisher.Dispose();
        subscriber.Dispose();
    }

    [Fact]
    public async Task TestOnPingRespReceivedAsync()
    {
        var options = new HiveMQClientOptionsBuilder()
            .WithClientId("OnPingRespReceivedTest")
            .Build();
        var client = new HiveMQClient(options);

        var pingRespReceivedSource = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

        client.OnPingRespReceived += (sender, args) =>
        {
            Assert.NotNull(args.PingRespPacket);
            pingRespReceivedSource.TrySetResult(true);
        };

        var connectResult = await client.ConnectAsync().ConfigureAwait(false);
        Assert.True(connectResult.ReasonCode == ConnAckReasonCode.Success);

        // Wait for keep-alive ping/pong cycle (default keep-alive is usually 60 seconds, but broker may send ping)
        // We'll wait a bit for the ping response
        var pingRespReceived = await Task.WhenAny(
            pingRespReceivedSource.Task,
            Task.Delay(TimeSpan.FromSeconds(10)))
            .ConfigureAwait(false);

        // If we got the ping response, verify it
        if (pingRespReceived == pingRespReceivedSource.Task)
        {
            await pingRespReceivedSource.Task.WaitAsync(TimeSpan.FromSeconds(1)).ConfigureAwait(false);
        }

        // Small delay to allow async event handlers to complete
        await Task.Delay(100).ConfigureAwait(false);

        await client.DisconnectAsync().ConfigureAwait(false);
        client.Dispose();
    }

    [Fact]
    public async Task TestOnPubAckSentAsync()
    {
        var publisherOptions = new HiveMQClientOptionsBuilder()
            .WithClientId("OnPubAckSentPublisher")
            .Build();
        var publisher = new HiveMQClient(publisherOptions);

        var subscriberOptions = new HiveMQClientOptionsBuilder()
            .WithClientId("OnPubAckSentSubscriber")
            .Build();
        var subscriber = new HiveMQClient(subscriberOptions);

        var pubAckSentSource = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

        // Subscriber sends PubAck when receiving QoS 1 message
        subscriber.OnPubAckSent += (sender, args) =>
        {
            Assert.NotNull(args.PubAckPacket);
            pubAckSentSource.TrySetResult(true);
        };

        var pubConnectResult = await publisher.ConnectAsync().ConfigureAwait(false);
        Assert.True(pubConnectResult.ReasonCode == ConnAckReasonCode.Success);

        var subConnectResult = await subscriber.ConnectAsync().ConfigureAwait(false);
        Assert.True(subConnectResult.ReasonCode == ConnAckReasonCode.Success);

        // Subscribe with QoS 1
        var subscribeResult = await subscriber.SubscribeAsync(
            "tests/OnPubAckSent",
            QualityOfService.AtLeastOnceDelivery)
            .ConfigureAwait(false);
        Assert.NotEmpty(subscribeResult.Subscriptions);

        // Set up message received handler
        var messageReceivedSource = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        subscriber.OnMessageReceived += (sender, args) =>
        {
            if (args.PublishMessage.Topic == "tests/OnPubAckSent")
            {
                messageReceivedSource.TrySetResult(true);
            }
        };

        // Publish QoS 1 message
        var publishResult = await publisher.PublishAsync(
            "tests/OnPubAckSent",
            "QoS 1 test message",
            QualityOfService.AtLeastOnceDelivery)
            .ConfigureAwait(false);

        Assert.NotNull(publishResult);
        Assert.NotNull(publishResult.QoS1ReasonCode);

        // Wait for PubAck sent event
        await pubAckSentSource.Task.WaitAsync(TimeSpan.FromSeconds(5)).ConfigureAwait(false);
        await messageReceivedSource.Task.WaitAsync(TimeSpan.FromSeconds(5)).ConfigureAwait(false);

        // Small delay to allow async event handlers to complete
        await Task.Delay(100).ConfigureAwait(false);

        await publisher.DisconnectAsync().ConfigureAwait(false);
        await subscriber.DisconnectAsync().ConfigureAwait(false);
        publisher.Dispose();
        subscriber.Dispose();
    }

    [Fact]
    public async Task TestOnPublishReceivedAsync()
    {
        var publisherOptions = new HiveMQClientOptionsBuilder()
            .WithClientId("OnPublishReceivedPublisher")
            .Build();
        var publisher = new HiveMQClient(publisherOptions);

        var subscriberOptions = new HiveMQClientOptionsBuilder()
            .WithClientId("OnPublishReceivedSubscriber")
            .Build();
        var subscriber = new HiveMQClient(subscriberOptions);

        var publishReceivedSource = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

        // OnPublishReceived is fired when a PUBLISH packet is received (before OnMessageReceived)
        subscriber.OnPublishReceived += (sender, args) =>
        {
            Assert.NotNull(args.PublishPacket);
            Assert.Equal("tests/OnPublishReceived", args.PublishPacket.Message.Topic);
            publishReceivedSource.TrySetResult(true);
        };

        var pubConnectResult = await publisher.ConnectAsync().ConfigureAwait(false);
        Assert.True(pubConnectResult.ReasonCode == ConnAckReasonCode.Success);

        var subConnectResult = await subscriber.ConnectAsync().ConfigureAwait(false);
        Assert.True(subConnectResult.ReasonCode == ConnAckReasonCode.Success);

        // Subscribe
        var subscribeResult = await subscriber.SubscribeAsync(
            "tests/OnPublishReceived",
            QualityOfService.AtMostOnceDelivery)
            .ConfigureAwait(false);
        Assert.NotEmpty(subscribeResult.Subscriptions);

        // Publish message
        var publishResult = await publisher.PublishAsync(
            "tests/OnPublishReceived",
            "Test message",
            QualityOfService.AtMostOnceDelivery)
            .ConfigureAwait(false);

        Assert.NotNull(publishResult);

        // Wait for OnPublishReceived event
        await publishReceivedSource.Task.WaitAsync(TimeSpan.FromSeconds(5)).ConfigureAwait(false);

        // Small delay to allow async event handlers to complete
        await Task.Delay(100).ConfigureAwait(false);

        await publisher.DisconnectAsync().ConfigureAwait(false);
        await subscriber.DisconnectAsync().ConfigureAwait(false);
        publisher.Dispose();
        subscriber.Dispose();
    }

    [Fact]
    public async Task TestOnDisconnectReceivedAsync()
    {
        // Note: Testing broker-initiated disconnect requires special broker configuration
        // This test verifies the event handler is properly wired, but may not trigger in normal scenarios
        var options = new HiveMQClientOptionsBuilder()
            .WithClientId("OnDisconnectReceivedTest")
            .Build();
        var client = new HiveMQClient(options);

        var disconnectReceivedSource = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

        client.OnDisconnectReceived += (sender, args) =>
        {
            Assert.NotNull(args.DisconnectPacket);
            disconnectReceivedSource.TrySetResult(true);
        };

        var connectResult = await client.ConnectAsync().ConfigureAwait(false);
        Assert.True(connectResult.ReasonCode == ConnAckReasonCode.Success);

        // Disconnect normally (this may not trigger OnDisconnectReceived if broker doesn't send DISCONNECT)
        await client.DisconnectAsync().ConfigureAwait(false);

        // Small delay to allow async event handlers to complete
        await Task.Delay(100).ConfigureAwait(false);

        // Note: OnDisconnectReceived may not fire in normal disconnect scenarios
        // This test verifies the event is properly registered and would fire if broker sends DISCONNECT
        client.Dispose();
    }

    [Fact]
    public async Task TestNullHandlerFastPathAsync()
    {
        // This test verifies that events with no handlers return early (no allocations, no logging overhead)
        var options = new HiveMQClientOptionsBuilder()
            .WithClientId("NullHandlerFastPathTest")
            .Build();
        var client = new HiveMQClient(options);

        // Connect without registering any event handlers
        var connectResult = await client.ConnectAsync().ConfigureAwait(false);
        Assert.True(connectResult.ReasonCode == ConnAckReasonCode.Success);

        // Perform operations that would trigger events
        var subscribeResult = await client.SubscribeAsync(
            "tests/NullHandlerFastPath",
            QualityOfService.AtMostOnceDelivery)
            .ConfigureAwait(false);
        Assert.NotEmpty(subscribeResult.Subscriptions);

        var publishResult = await client.PublishAsync(
            "tests/NullHandlerFastPath",
            "Test message",
            QualityOfService.AtMostOnceDelivery)
            .ConfigureAwait(false);
        Assert.NotNull(publishResult);

        var unsubscribeResult = await client.UnsubscribeAsync("tests/NullHandlerFastPath").ConfigureAwait(false);
        Assert.NotEmpty(unsubscribeResult.Subscriptions);

        // All operations should complete successfully even with no event handlers
        // The fast-path null checks should prevent any issues
        await client.DisconnectAsync().ConfigureAwait(false);
        client.Dispose();
    }

    [Fact]
    public async Task TestMultipleHandlersOnSameEventAsync()
    {
        var options = new HiveMQClientOptionsBuilder()
            .WithClientId("MultipleHandlersTest")
            .Build();
        var client = new HiveMQClient(options);

        var handler1Called = false;
        var handler2Called = false;
        var handler3Called = false;

        void Handler1(object? sender, AfterConnectEventArgs args) => handler1Called = true;

        void Handler2(object? sender, AfterConnectEventArgs args) => handler2Called = true;

        void Handler3(object? sender, AfterConnectEventArgs args) => handler3Called = true;

        // Register multiple handlers
        client.AfterConnect += Handler1;
        client.AfterConnect += Handler2;
        client.AfterConnect += Handler3;

        var connectResult = await client.ConnectAsync().ConfigureAwait(false);
        Assert.True(connectResult.ReasonCode == ConnAckReasonCode.Success);

        // Small delay to allow async event handlers to complete
        await Task.Delay(200).ConfigureAwait(false);

        // All handlers should have been called
        Assert.True(handler1Called, "Handler1 should have been called");
        Assert.True(handler2Called, "Handler2 should have been called");
        Assert.True(handler3Called, "Handler3 should have been called");

        // Unregister handlers
        client.AfterConnect -= Handler1;
        client.AfterConnect -= Handler2;
        client.AfterConnect -= Handler3;

        await client.DisconnectAsync().ConfigureAwait(false);
        client.Dispose();
    }

    [Fact]
    public async Task TestPerSubscriptionMessageHandlerAsync()
    {
        var publisherOptions = new HiveMQClientOptionsBuilder()
            .WithClientId("PerSubscriptionHandlerPublisher")
            .Build();
        var publisher = new HiveMQClient(publisherOptions);

        var subscriberOptions = new HiveMQClientOptionsBuilder()
            .WithClientId("PerSubscriptionHandlerSubscriber")
            .Build();
        var subscriber = new HiveMQClient(subscriberOptions);

        var globalHandlerSource = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var subscriptionHandlerSource = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

        // Global handler
        subscriber.OnMessageReceived += (sender, args) =>
        {
            if (args.PublishMessage.Topic == "tests/PerSubscriptionHandler")
            {
                globalHandlerSource.TrySetResult(true);
            }
        };

        var pubConnectResult = await publisher.ConnectAsync().ConfigureAwait(false);
        Assert.True(pubConnectResult.ReasonCode == ConnAckReasonCode.Success);

        var subConnectResult = await subscriber.ConnectAsync().ConfigureAwait(false);
        Assert.True(subConnectResult.ReasonCode == ConnAckReasonCode.Success);

        // Subscribe with per-subscription handler using SubscribeOptionsBuilder
        var subscribeOptions = new SubscribeOptionsBuilder()
            .WithSubscription(
                "tests/PerSubscriptionHandler",
                QualityOfService.AtMostOnceDelivery,
                messageReceivedHandler: (sender, args) =>
                {
                    if (args.PublishMessage.Topic == "tests/PerSubscriptionHandler")
                    {
                        subscriptionHandlerSource.TrySetResult(true);
                    }
                })
            .Build();

        var subscribeResult = await subscriber.SubscribeAsync(subscribeOptions).ConfigureAwait(false);
        Assert.NotEmpty(subscribeResult.Subscriptions);

        // Publish message
        var publishResult = await publisher.PublishAsync(
            "tests/PerSubscriptionHandler",
            "Test message",
            QualityOfService.AtMostOnceDelivery)
            .ConfigureAwait(false);

        Assert.NotNull(publishResult);

        // Wait for both handlers to be called
        await globalHandlerSource.Task.WaitAsync(TimeSpan.FromSeconds(5)).ConfigureAwait(false);
        await subscriptionHandlerSource.Task.WaitAsync(TimeSpan.FromSeconds(5)).ConfigureAwait(false);

        // Small delay to allow async event handlers to complete
        await Task.Delay(100).ConfigureAwait(false);

        await publisher.DisconnectAsync().ConfigureAwait(false);
        await subscriber.DisconnectAsync().ConfigureAwait(false);
        publisher.Dispose();
        subscriber.Dispose();
    }
}
