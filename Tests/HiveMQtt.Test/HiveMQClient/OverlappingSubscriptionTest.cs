namespace HiveMQtt.Test.HiveMQClient;

using System.Threading.Tasks;
using HiveMQtt.Client;
using HiveMQtt.Client.Events;
using HiveMQtt.Client.Options;
using HiveMQtt.MQTT5.ReasonCodes;
using HiveMQtt.MQTT5.Types;
using Xunit;

/// <summary>
/// Tests for overlapping subscription behavior (GitHub Issue #329).
/// </summary>
[Collection("Broker")]
public class OverlappingSubscriptionTest
{
    /// <summary>
    /// Test that FireAllMatchingHandlers (default) fires all matching subscription handlers.
    /// </summary>
    [Fact]
    public async Task FireAllMatchingHandlers_Behavior_FiresAllHandlers()
    {
        var options = new HiveMQClientOptionsBuilder()
            .WithClientId("FireAllMatchingHandlers_FiresAll")
            .WithOverlappingSubscriptionBehavior(OverlappingSubscriptionBehavior.FireAllMatchingHandlers)
            .Build();

        var client = new HiveMQClient(options);

        var connectResult = await client.ConnectAsync().ConfigureAwait(false);
        Assert.Equal(ConnAckReasonCode.Success, connectResult.ReasonCode);

        var prefix = $"tests/overlapping/all/{Guid.NewGuid()}";

        var handler1Count = 0;
        var handler2Count = 0;
        var handler3Count = 0;
        var messageReceived = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

        // Subscribe to overlapping topics with individual handlers
        var opts1 = new SubscribeOptions();
        opts1.TopicFilters.Add(new TopicFilter($"{prefix}/room/a/b/c/#", QualityOfService.AtLeastOnceDelivery));
        opts1.Handlers[$"{prefix}/room/a/b/c/#"] = (s, e) =>
        {
            Interlocked.Increment(ref handler1Count);
            messageReceived.TrySetResult(true);
        };
        await client.SubscribeAsync(opts1).ConfigureAwait(false);

        var opts2 = new SubscribeOptions();
        opts2.TopicFilters.Add(new TopicFilter($"{prefix}/room/a/b/#", QualityOfService.AtLeastOnceDelivery));
        opts2.Handlers[$"{prefix}/room/a/b/#"] = (s, e) =>
        {
            Interlocked.Increment(ref handler2Count);
        };
        await client.SubscribeAsync(opts2).ConfigureAwait(false);

        var opts3 = new SubscribeOptions();
        opts3.TopicFilters.Add(new TopicFilter($"{prefix}/room/a/#", QualityOfService.AtLeastOnceDelivery));
        opts3.Handlers[$"{prefix}/room/a/#"] = (s, e) =>
        {
            Interlocked.Increment(ref handler3Count);
        };
        await client.SubscribeAsync(opts3).ConfigureAwait(false);

        // Publish a message that matches all three subscriptions
        await client.PublishAsync($"{prefix}/room/a/b/c/whatever", "test", QualityOfService.AtLeastOnceDelivery).ConfigureAwait(false);

        // Wait for message to be received with timeout
        await messageReceived.Task.WaitAsync(TimeSpan.FromSeconds(5)).ConfigureAwait(false);
        await Task.Delay(500).ConfigureAwait(false); // Give time for all handlers to fire

        // With FireAllMatchingHandlers, all 3 handlers should fire
        Assert.True(handler1Count >= 1, $"Handler 1 should have fired at least once, but fired {handler1Count} times");
        Assert.True(handler2Count >= 1, $"Handler 2 should have fired at least once, but fired {handler2Count} times");
        Assert.True(handler3Count >= 1, $"Handler 3 should have fired at least once, but fired {handler3Count} times");

        await client.DisconnectAsync().ConfigureAwait(false);
        client.Dispose();
    }

    /// <summary>
    /// Test that FireFirstMatchingHandler fires only the first matching subscription handler.
    /// </summary>
    [Fact]
    public async Task FireFirstMatchingHandler_Behavior_FiresOnlyFirstHandler()
    {
        var options = new HiveMQClientOptionsBuilder()
            .WithClientId("FireFirstMatchingHandler_FiresFirst")
            .WithOverlappingSubscriptionBehavior(OverlappingSubscriptionBehavior.FireFirstMatchingHandler)
            .Build();

        var client = new HiveMQClient(options);

        var connectResult = await client.ConnectAsync().ConfigureAwait(false);
        Assert.Equal(ConnAckReasonCode.Success, connectResult.ReasonCode);

        var prefix = $"tests/overlapping/first/{Guid.NewGuid()}";

        var handler1Count = 0;
        var handler2Count = 0;
        var handler3Count = 0;
        var messageReceived = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

        // Subscribe to overlapping topics with individual handlers (order matters!)
        var opts1 = new SubscribeOptions();
        opts1.TopicFilters.Add(new TopicFilter($"{prefix}/room/a/b/c/#", QualityOfService.AtLeastOnceDelivery));
        opts1.Handlers[$"{prefix}/room/a/b/c/#"] = (s, e) =>
        {
            Interlocked.Increment(ref handler1Count);
            messageReceived.TrySetResult(true);
        };
        await client.SubscribeAsync(opts1).ConfigureAwait(false);

        var opts2 = new SubscribeOptions();
        opts2.TopicFilters.Add(new TopicFilter($"{prefix}/room/a/b/#", QualityOfService.AtLeastOnceDelivery));
        opts2.Handlers[$"{prefix}/room/a/b/#"] = (s, e) =>
        {
            Interlocked.Increment(ref handler2Count);
        };
        await client.SubscribeAsync(opts2).ConfigureAwait(false);

        var opts3 = new SubscribeOptions();
        opts3.TopicFilters.Add(new TopicFilter($"{prefix}/room/a/#", QualityOfService.AtLeastOnceDelivery));
        opts3.Handlers[$"{prefix}/room/a/#"] = (s, e) =>
        {
            Interlocked.Increment(ref handler3Count);
        };
        await client.SubscribeAsync(opts3).ConfigureAwait(false);

        // Publish a message that matches all three subscriptions
        await client.PublishAsync($"{prefix}/room/a/b/c/whatever", "test", QualityOfService.AtLeastOnceDelivery).ConfigureAwait(false);

        // Wait for message to be received with timeout
        await messageReceived.Task.WaitAsync(TimeSpan.FromSeconds(5)).ConfigureAwait(false);
        await Task.Delay(500).ConfigureAwait(false); // Give time to ensure other handlers don't fire

        // With FireFirstMatchingHandler, only the first handler should fire
        Assert.True(handler1Count >= 1, $"Handler 1 (first) should have fired at least once, but fired {handler1Count} times");
        Assert.Equal(0, handler2Count);
        Assert.Equal(0, handler3Count);

        await client.DisconnectAsync().ConfigureAwait(false);
        client.Dispose();
    }

    /// <summary>
    /// Test that FireFirstMatchingHandler with global handler fires both global and first subscription handler.
    /// </summary>
    [Fact]
    public async Task FireFirstMatchingHandler_WithGlobalHandler_BothFire()
    {
        var options = new HiveMQClientOptionsBuilder()
            .WithClientId("FireFirst_GlobalAndPerSub")
            .WithOverlappingSubscriptionBehavior(OverlappingSubscriptionBehavior.FireFirstMatchingHandler)
            .Build();

        var client = new HiveMQClient(options);

        var connectResult = await client.ConnectAsync().ConfigureAwait(false);
        Assert.Equal(ConnAckReasonCode.Success, connectResult.ReasonCode);

        var prefix = $"tests/overlapping/global/{Guid.NewGuid()}";

        var globalCount = 0;
        var perSubCount = 0;
        var messageReceived = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

        // Set up global handler
        client.OnMessageReceived += (s, e) =>
        {
            if (e.PublishMessage.Topic!.StartsWith(prefix))
            {
                Interlocked.Increment(ref globalCount);
            }
        };

        // Set up per-subscription handler
        var opts = new SubscribeOptions();
        opts.TopicFilters.Add(new TopicFilter($"{prefix}/#", QualityOfService.AtLeastOnceDelivery));
        opts.Handlers[$"{prefix}/#"] = (s, e) =>
        {
            Interlocked.Increment(ref perSubCount);
            messageReceived.TrySetResult(true);
        };
        await client.SubscribeAsync(opts).ConfigureAwait(false);

        // Publish a message
        await client.PublishAsync($"{prefix}/test", "test", QualityOfService.AtLeastOnceDelivery).ConfigureAwait(false);

        // Wait for message to be received with timeout
        await messageReceived.Task.WaitAsync(TimeSpan.FromSeconds(5)).ConfigureAwait(false);
        await Task.Delay(500).ConfigureAwait(false);

        // Both global and per-subscription handler should fire
        Assert.True(globalCount >= 1, $"Global handler should have fired at least once, but fired {globalCount} times");
        Assert.True(perSubCount >= 1, $"Per-subscription handler should have fired at least once, but fired {perSubCount} times");

        await client.DisconnectAsync().ConfigureAwait(false);
        client.Dispose();
    }

    /// <summary>
    /// Test that the default behavior is FireAllMatchingHandlers.
    /// </summary>
    [Fact]
    public void DefaultBehavior_IsFireAllMatchingHandlers()
    {
        var options = new HiveMQClientOptions();

        Assert.Equal(OverlappingSubscriptionBehavior.FireAllMatchingHandlers, options.OverlappingSubscriptionBehavior);
    }

    /// <summary>
    /// Test that the builder correctly sets the behavior.
    /// </summary>
    [Fact]
    public void Builder_SetsOverlappingSubscriptionBehavior()
    {
        var options = new HiveMQClientOptionsBuilder()
            .WithOverlappingSubscriptionBehavior(OverlappingSubscriptionBehavior.FireFirstMatchingHandler)
            .Build();

        Assert.Equal(OverlappingSubscriptionBehavior.FireFirstMatchingHandler, options.OverlappingSubscriptionBehavior);
    }

    /// <summary>
    /// Test edge case: FireFirstMatchingHandler when first subscription has no handler.
    /// </summary>
    [Fact]
    public async Task FireFirstMatchingHandler_FirstSubscriptionNoHandler_DoesNotFire()
    {
        var options = new HiveMQClientOptionsBuilder()
            .WithClientId("FireFirst_FirstNoHandler")
            .WithOverlappingSubscriptionBehavior(OverlappingSubscriptionBehavior.FireFirstMatchingHandler)
            .Build();

        var client = new HiveMQClient(options);

        var connectResult = await client.ConnectAsync().ConfigureAwait(false);
        Assert.Equal(ConnAckReasonCode.Success, connectResult.ReasonCode);

        var prefix = $"tests/overlapping/nohandler/{Guid.NewGuid()}";

        var globalCount = 0;
        var perSubCount = 0;
        var messageReceived = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

        // Set up global handler
        client.OnMessageReceived += (s, e) =>
        {
            if (e.PublishMessage.Topic!.StartsWith(prefix))
            {
                Interlocked.Increment(ref globalCount);
                messageReceived.TrySetResult(true);
            }
        };

        // First subscription WITHOUT a per-subscription handler
        await client.SubscribeAsync($"{prefix}/#", QualityOfService.AtLeastOnceDelivery).ConfigureAwait(false);

        // Second subscription WITH a per-subscription handler
        var opts = new SubscribeOptions();
        opts.TopicFilters.Add(new TopicFilter($"{prefix}/room/+", QualityOfService.AtLeastOnceDelivery));
        opts.Handlers[$"{prefix}/room/+"] = (s, e) =>
        {
            Interlocked.Increment(ref perSubCount);
        };
        await client.SubscribeAsync(opts).ConfigureAwait(false);

        // Publish a message that matches both
        await client.PublishAsync($"{prefix}/room/1", "test", QualityOfService.AtLeastOnceDelivery).ConfigureAwait(false);

        // Wait for message to be received with timeout
        await messageReceived.Task.WaitAsync(TimeSpan.FromSeconds(5)).ConfigureAwait(false);
        await Task.Delay(500).ConfigureAwait(false);

        // Global handler should fire, but per-subscription handler should NOT fire
        // because the first matching subscription has no handler
        Assert.True(globalCount >= 1, $"Global handler should have fired, but fired {globalCount} times");
        Assert.Equal(0, perSubCount);

        await client.DisconnectAsync().ConfigureAwait(false);
        client.Dispose();
    }
}
