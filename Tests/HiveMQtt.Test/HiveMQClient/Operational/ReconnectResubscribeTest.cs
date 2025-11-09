namespace HiveMQtt.Test.HiveMQClient.Operational;

using System.Threading.Tasks;
using HiveMQtt.Client;
using HiveMQtt.Client.Events;
using HiveMQtt.Client.Options;
using HiveMQtt.MQTT5.ReasonCodes;
using HiveMQtt.MQTT5.Types;
using Xunit;

public class ReconnectResubscribeTest
{
    [Fact]
    public async Task ManualResubscribe_AfterReconnect_DoesNotDuplicateClientSubscriptionsAsync()
    {
        var options = new HiveMQClientOptionsBuilder()
            .WithClientId("ManualResubscribe_Duplicates_Subscriptions")
            .WithAutomaticReconnect(true)
            .Build();

        var client = new HiveMQClient(options);

        var connectResult = await client.ConnectAsync().ConfigureAwait(false);
        Assert.Equal(ConnAckReasonCode.Success, connectResult.ReasonCode);

        const string topic = "tests/reconnect/duplicate-subscriptions";
        var subscribeResult = await client.SubscribeAsync(topic, QualityOfService.AtLeastOnceDelivery).ConfigureAwait(false);
        Assert.Single(client.Subscriptions);
        Assert.Equal(SubAckReasonCode.GrantedQoS1, subscribeResult.Subscriptions[0].SubscribeReasonCode);

        // Simulate a network drop and let AutomaticReconnect restore the connection
        _ = await client.Connection.HandleDisconnectionAsync(false).ConfigureAwait(false);
        await WaitForReconnectAsync(client).ConfigureAwait(false);

        // Manually resubscribe to the same topic after reconnect (typical application behavior)
        var subscribeResult2 = await client.SubscribeAsync(topic, QualityOfService.AtLeastOnceDelivery).ConfigureAwait(false);
        Assert.Equal(SubAckReasonCode.GrantedQoS1, subscribeResult2.Subscriptions[0].SubscribeReasonCode);

        // Expect no duplicate entries in client.Subscriptions
        Assert.Single(client.Subscriptions);
        Assert.Equal(topic, client.Subscriptions[0].TopicFilter.Topic);

        var disconnectResult = await client.DisconnectAsync().ConfigureAwait(false);
        Assert.True(disconnectResult);
        client.Dispose();
    }

    [Fact]
    public async Task ManualResubscribe_AfterReconnect_DoesNotInvokeHandlerTwiceAsync()
    {
        var options = new HiveMQClientOptionsBuilder()
            .WithClientId("ManualResubscribe_Invokes_Handler_Twice")
            .WithAutomaticReconnect(true)
            .Build();

        var client = new HiveMQClient(options);

        var connectResult = await client.ConnectAsync().ConfigureAwait(false);
        Assert.Equal(ConnAckReasonCode.Success, connectResult.ReasonCode);

        const string topic = "tests/reconnect/duplicate-handlers";

        var handlerInvokeCount = 0;
        var messageReceived = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        void OnMessage(object? sender, OnMessageReceivedEventArgs e)
        {
            if (e.PublishMessage.Topic == topic)
            {
                var count = Interlocked.Increment(ref handlerInvokeCount);
                if (count == 1)
                {
                    messageReceived.TrySetResult(true);
                }
            }
        }

        var subscribeOptions = new SubscribeOptions();
        subscribeOptions.TopicFilters.Add(new TopicFilter(topic, QualityOfService.AtLeastOnceDelivery));
        subscribeOptions.Handlers[topic] = OnMessage;

        _ = await client.SubscribeAsync(subscribeOptions).ConfigureAwait(false);
        Assert.Single(client.Subscriptions);

        // Force disconnect and wait for automatic reconnect
        _ = await client.Connection.HandleDisconnectionAsync(false).ConfigureAwait(false);
        await WaitForReconnectAsync(client).ConfigureAwait(false);

        // Resubscribe with the same handler mapping
        var subscribeOptions2 = new SubscribeOptions();
        subscribeOptions2.TopicFilters.Add(new TopicFilter(topic, QualityOfService.AtLeastOnceDelivery));
        subscribeOptions2.Handlers[topic] = OnMessage;
        _ = await client.SubscribeAsync(subscribeOptions2).ConfigureAwait(false);

        // Publish a single message; handler should only fire once if no duplicates exist
        var publishResult = await client.PublishAsync(topic, "hello", QualityOfService.AtLeastOnceDelivery).ConfigureAwait(false);
        Assert.NotNull(publishResult.QoS1ReasonCode);

        // Wait for message to be received with timeout instead of fixed delay
        await messageReceived.Task.WaitAsync(TimeSpan.FromSeconds(5)).ConfigureAwait(false);
        Assert.Equal(1, handlerInvokeCount);

        var disconnectResult = await client.DisconnectAsync().ConfigureAwait(false);
        Assert.True(disconnectResult);
        client.Dispose();
    }

    [Fact]
    public async Task SubscribeAsync_WithExistingTopic_ReplacesSubscriptionAsync()
    {
        var options = new HiveMQClientOptionsBuilder()
            .WithClientId("SubscribeAsync_Replaces_Subscription")
            .Build();

        var client = new HiveMQClient(options);
        var connectResult = await client.ConnectAsync().ConfigureAwait(false);
        Assert.Equal(ConnAckReasonCode.Success, connectResult.ReasonCode);

        const string topic = "tests/deduplication/replace-subscription";

        // First subscription with QoS 0
        var subscribeResult1 = await client.SubscribeAsync(topic, QualityOfService.AtMostOnceDelivery).ConfigureAwait(false);
        Assert.Single(client.Subscriptions);
        Assert.Equal(QualityOfService.AtMostOnceDelivery, client.Subscriptions[0].TopicFilter.QoS);

        // Second subscription with same topic but different QoS - should replace the first
        var subscribeResult2 = await client.SubscribeAsync(topic, QualityOfService.AtLeastOnceDelivery).ConfigureAwait(false);
        Assert.Single(client.Subscriptions);
        Assert.Equal(QualityOfService.AtLeastOnceDelivery, client.Subscriptions[0].TopicFilter.QoS);

        var disconnectResult = await client.DisconnectAsync().ConfigureAwait(false);
        Assert.True(disconnectResult);
        client.Dispose();
    }

    [Fact]
    public async Task SubscribeAsync_WithMultipleTopics_ReplacesOnlyMatchingTopicsAsync()
    {
        var options = new HiveMQClientOptionsBuilder()
            .WithClientId("SubscribeAsync_Replaces_Matching_Topics")
            .Build();

        var client = new HiveMQClient(options);
        var connectResult = await client.ConnectAsync().ConfigureAwait(false);
        Assert.Equal(ConnAckReasonCode.Success, connectResult.ReasonCode);

        const string topic1 = "tests/deduplication/topic1";
        const string topic2 = "tests/deduplication/topic2";
        const string topic3 = "tests/deduplication/topic3";

        // Initial subscriptions
        var subscribeOptions1 = new SubscribeOptions();
        subscribeOptions1.TopicFilters.Add(new TopicFilter(topic1, QualityOfService.AtMostOnceDelivery));
        subscribeOptions1.TopicFilters.Add(new TopicFilter(topic2, QualityOfService.AtMostOnceDelivery));
        subscribeOptions1.TopicFilters.Add(new TopicFilter(topic3, QualityOfService.AtMostOnceDelivery));

        _ = await client.SubscribeAsync(subscribeOptions1).ConfigureAwait(false);
        Assert.Equal(3, client.Subscriptions.Count);

        // Resubscribe with topic1 and topic2 having different QoS, topic3 unchanged
        var subscribeOptions2 = new SubscribeOptions();
        subscribeOptions2.TopicFilters.Add(new TopicFilter(topic1, QualityOfService.AtLeastOnceDelivery));
        subscribeOptions2.TopicFilters.Add(new TopicFilter(topic2, QualityOfService.ExactlyOnceDelivery));
        subscribeOptions2.TopicFilters.Add(new TopicFilter(topic3, QualityOfService.AtMostOnceDelivery));

        _ = await client.SubscribeAsync(subscribeOptions2).ConfigureAwait(false);
        Assert.Equal(3, client.Subscriptions.Count);

        // Verify topic1 was replaced with QoS 1
        var topic1Sub = client.Subscriptions.FirstOrDefault(s => s.TopicFilter.Topic == topic1);
        Assert.NotNull(topic1Sub);
        Assert.Equal(QualityOfService.AtLeastOnceDelivery, topic1Sub.TopicFilter.QoS);

        // Verify topic2 was replaced with QoS 2
        var topic2Sub = client.Subscriptions.FirstOrDefault(s => s.TopicFilter.Topic == topic2);
        Assert.NotNull(topic2Sub);
        Assert.Equal(QualityOfService.ExactlyOnceDelivery, topic2Sub.TopicFilter.QoS);

        // Verify topic3 remains unchanged
        var topic3Sub = client.Subscriptions.FirstOrDefault(s => s.TopicFilter.Topic == topic3);
        Assert.NotNull(topic3Sub);
        Assert.Equal(QualityOfService.AtMostOnceDelivery, topic3Sub.TopicFilter.QoS);

        var disconnectResult = await client.DisconnectAsync().ConfigureAwait(false);
        Assert.True(disconnectResult);
        client.Dispose();
    }

    [Fact]
    public async Task SessionPresentFalse_ClearsSubscriptionsAsync()
    {
        var options = new HiveMQClientOptionsBuilder()
            .WithClientId("SessionPresentFalse_Clears_Subscriptions")
            .WithCleanStart(true) // Force Session Present = false
            .Build();

        var client = new HiveMQClient(options);

        // Connect and subscribe
        var connectResult = await client.ConnectAsync().ConfigureAwait(false);
        Assert.Equal(ConnAckReasonCode.Success, connectResult.ReasonCode);
        Assert.False(connectResult.SessionPresent); // Should be false due to CleanStart = true

        const string topic = "tests/session-present-false";
        _ = await client.SubscribeAsync(topic, QualityOfService.AtLeastOnceDelivery).ConfigureAwait(false);
        Assert.Single(client.Subscriptions);

        // Disconnect and reconnect with CleanStart = true again
        var disconnectResult = await client.DisconnectAsync().ConfigureAwait(false);
        Assert.True(disconnectResult);

        // Reconnect - this should clear subscriptions due to Session Present = false
        var connectResult2 = await client.ConnectAsync().ConfigureAwait(false);
        Assert.Equal(ConnAckReasonCode.Success, connectResult2.ReasonCode);
        Assert.False(connectResult2.SessionPresent);

        // Subscriptions should be cleared
        Assert.Empty(client.Subscriptions);

        client.Dispose();
    }

    [Fact]
    public async Task SessionPresentTrue_PreservesSubscriptionsAsync()
    {
        var options = new HiveMQClientOptionsBuilder()
            .WithClientId("SessionPresentTrue_Preserves_Subscriptions")
            .WithCleanStart(false)
            .WithSessionExpiryInterval(300) // 5 minutes
            .Build();

        var client = new HiveMQClient(options);

        // Connect and subscribe
        var connectResult = await client.ConnectAsync().ConfigureAwait(false);
        Assert.Equal(ConnAckReasonCode.Success, connectResult.ReasonCode);

        const string topic = "tests/session-present-true";
        _ = await client.SubscribeAsync(topic, QualityOfService.AtLeastOnceDelivery).ConfigureAwait(false);
        Assert.Single(client.Subscriptions);

        // Disconnect and reconnect quickly (within session expiry)
        var disconnectResult = await client.DisconnectAsync().ConfigureAwait(false);
        Assert.True(disconnectResult);

        // Reconnect - this should preserve subscriptions due to Session Present = true
        var connectResult2 = await client.ConnectAsync().ConfigureAwait(false);
        Assert.Equal(ConnAckReasonCode.Success, connectResult2.ReasonCode);

        // Note: SessionPresent might be false if broker doesn't support persistent sessions
        // The key test is that subscriptions are handled correctly regardless

        // If SessionPresent is true, subscriptions should be preserved
        // If SessionPresent is false, subscriptions should be cleared
        if (connectResult2.SessionPresent)
        {
            Assert.Single(client.Subscriptions);
            Assert.Equal(topic, client.Subscriptions[0].TopicFilter.Topic);
        }
        else
        {
            Assert.Empty(client.Subscriptions);
        }

        client.Dispose();
    }

    private static async Task WaitForReconnectAsync(HiveMQClient client, int timeoutMs = 30000)
    {
        // Use event-based waiting instead of polling
        var reconnectComplete = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

        void OnConnect(object? sender, AfterConnectEventArgs args)
        {
            if (client.IsConnected())
            {
                reconnectComplete.TrySetResult(true);
                client.AfterConnect -= OnConnect;
            }
        }

        client.AfterConnect += OnConnect;

        // If already connected, signal immediately
        if (client.IsConnected())
        {
            reconnectComplete.TrySetResult(true);
            client.AfterConnect -= OnConnect;
        }

        try
        {
            await reconnectComplete.Task.WaitAsync(TimeSpan.FromMilliseconds(timeoutMs)).ConfigureAwait(false);
        }
        catch (TimeoutException)
        {
            client.AfterConnect -= OnConnect;
            throw new TimeoutException($"Reconnect did not complete within {timeoutMs}ms");
        }

        Assert.True(client.IsConnected());
    }
}
