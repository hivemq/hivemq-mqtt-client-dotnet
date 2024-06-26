namespace HiveMQtt.Test.HiveMQClient;

using System.Text.RegularExpressions;
using System.Threading.Tasks;
using HiveMQtt.Client;
using HiveMQtt.Client.Events;
using HiveMQtt.MQTT5.ReasonCodes;
using Xunit;

public class SubscribeBuilderTest
{
    [Fact]
    public async Task MultiSubscribeAsync()
    {
        var options = new HiveMQClientOptionsBuilder().WithClientId("MultiSubscribeAsync").Build();
        var subClient = new HiveMQClient(options);
        var connectResult = await subClient.ConnectAsync().ConfigureAwait(false);
        Assert.True(connectResult.ReasonCode == ConnAckReasonCode.Success);

        var subscribeOptions = new SubscribeOptionsBuilder()
            .WithSubscription("tests/MultiSubscribeAsync", MQTT5.Types.QualityOfService.AtLeastOnceDelivery)
            .WithSubscription("tests/MultiSubscribeAsync2", MQTT5.Types.QualityOfService.AtLeastOnceDelivery)
            .WithUserProperty("test", "test")
            .Build();

        var subResult = await subClient.SubscribeAsync(subscribeOptions).ConfigureAwait(false);

        Assert.NotEmpty(subResult.Subscriptions);
        Assert.Equal(SubAckReasonCode.GrantedQoS1, subResult.Subscriptions[0].SubscribeReasonCode);
        Assert.Equal(SubAckReasonCode.GrantedQoS1, subResult.Subscriptions[1].SubscribeReasonCode);

        var disconnectResult = await subClient.DisconnectAsync().ConfigureAwait(false);
        Assert.True(disconnectResult);
    }

    [Fact]
    public async Task MultiSubscribeExtendedOptionsAsync()
    {
        var options = new HiveMQClientOptionsBuilder().WithClientId("MultiSubscribeExtendedOptionsAsync").Build();
        var subClient = new HiveMQClient(options);
        var connectResult = await subClient.ConnectAsync().ConfigureAwait(false);
        Assert.True(connectResult.ReasonCode == ConnAckReasonCode.Success);

        var subscribeOptions = new SubscribeOptionsBuilder()
            .WithSubscription("tests/MultiSubscribeAsync", MQTT5.Types.QualityOfService.AtLeastOnceDelivery, true, true, MQTT5.Types.RetainHandling.SendAtSubscribe)
            .WithSubscription("tests/MultiSubscribeAsync2", MQTT5.Types.QualityOfService.AtLeastOnceDelivery)
            .WithUserProperty("test", "test")
            .Build();

        var subResult = await subClient.SubscribeAsync(subscribeOptions).ConfigureAwait(false);

        Assert.NotEmpty(subResult.Subscriptions);
        Assert.Equal(SubAckReasonCode.GrantedQoS1, subResult.Subscriptions[0].SubscribeReasonCode);
        Assert.Equal(true, subResult.Subscriptions[0].TopicFilter.NoLocal);
        Assert.Equal(true, subResult.Subscriptions[0].TopicFilter.RetainAsPublished);
        Assert.Equal(MQTT5.Types.RetainHandling.SendAtSubscribe, subResult.Subscriptions[0].TopicFilter.RetainHandling);
        Assert.Equal(SubAckReasonCode.GrantedQoS1, subResult.Subscriptions[1].SubscribeReasonCode);

        var disconnectResult = await subClient.DisconnectAsync().ConfigureAwait(false);
        Assert.True(disconnectResult);
    }

    [Fact]
    public async Task PerSubscriptionHandlerAsync()
    {
        var options = new HiveMQClientOptionsBuilder().WithClientId("PerSubscriptionHandlerAsync1").Build();
        var subClient = new HiveMQClient(options);
        var connectResult = await subClient.ConnectAsync().ConfigureAwait(false);
        Assert.True(connectResult.ReasonCode == ConnAckReasonCode.Success);

        var tcs = new TaskCompletionSource<bool>();

        var subscribeOptions = new SubscribeOptionsBuilder()
            .WithSubscription("tests/PerSubscriptionHandler", MQTT5.Types.QualityOfService.AtLeastOnceDelivery, messageReceivedHandler: (sender, args) =>
            {
                Assert.Equal("tests/PerSubscriptionHandler", args.PublishMessage.Topic);
                Assert.Equal("test", args.PublishMessage.PayloadAsString);
                tcs.SetResult(true);
            })
            .Build();

        var subResult = await subClient.SubscribeAsync(subscribeOptions).ConfigureAwait(false);

        Assert.NotEmpty(subResult.Subscriptions);
        Assert.Equal(SubAckReasonCode.GrantedQoS1, subResult.Subscriptions[0].SubscribeReasonCode);

        options = new HiveMQClientOptionsBuilder().WithClientId("PerSubscriptionHandlerAsync2").Build();
        var pubClient = new HiveMQClient(options);
        var pubConnectResult = await pubClient.ConnectAsync().ConfigureAwait(false);
        Assert.True(pubConnectResult.ReasonCode == ConnAckReasonCode.Success);

        var pubResult = await pubClient.PublishAsync("tests/PerSubscriptionHandler", "test").ConfigureAwait(false);

        var handlerResult = await tcs.Task.WaitAsync(TimeSpan.FromSeconds(10)).ConfigureAwait(false);
        Assert.True(handlerResult);

        var disconnectResult = await subClient.DisconnectAsync().ConfigureAwait(false);
        Assert.True(disconnectResult);

        disconnectResult = await pubClient.DisconnectAsync().ConfigureAwait(false);
        Assert.True(disconnectResult);

        subClient.Dispose();
        pubClient.Dispose();
    }

    [Fact]
    public async Task PerSubscriptionHandlerAndGlobalHandlerAsync()
    {
        var options = new HiveMQClientOptionsBuilder().WithClientId("PerSubscriptionHandlerAndGlobalHandlerAsync1").Build();
        var subClient = new HiveMQClient(options);
        var connectResult = await subClient.ConnectAsync().ConfigureAwait(false);
        Assert.True(connectResult.ReasonCode == ConnAckReasonCode.Success);

        var tcs = new TaskCompletionSource<bool>();
        var gtcs = new TaskCompletionSource<bool>();

        // Setup a per-subscription handler
        var subscribeOptions = new SubscribeOptionsBuilder()
            .WithSubscription("tests/PerSubscriptionHandler", MQTT5.Types.QualityOfService.AtLeastOnceDelivery, messageReceivedHandler: (sender, args) =>
            {
                Assert.Equal("tests/PerSubscriptionHandler", args.PublishMessage.Topic);
                Assert.Equal("test", args.PublishMessage.PayloadAsString);
                tcs.SetResult(true);
            })
            .Build();

        // Setup a global message handler
        void GlobalMessageHandler(object? sender, OnMessageReceivedEventArgs eventArgs)
        {
            Assert.Equal("tests/PerSubscriptionHandler", eventArgs.PublishMessage.Topic);
            Assert.Equal("test", eventArgs.PublishMessage.PayloadAsString);
            gtcs.SetResult(true);
        }

        subClient.OnMessageReceived += GlobalMessageHandler;

        // Both handlers should be called
        var subResult = await subClient.SubscribeAsync(subscribeOptions).ConfigureAwait(false);

        Assert.NotEmpty(subResult.Subscriptions);
        Assert.Equal(SubAckReasonCode.GrantedQoS1, subResult.Subscriptions[0].SubscribeReasonCode);

        options = new HiveMQClientOptionsBuilder().WithClientId("PerSubscriptionHandlerAndGlobalHandlerAsync2").Build();
        var pubClient = new HiveMQClient(options);
        var pubConnectResult = await pubClient.ConnectAsync().ConfigureAwait(false);
        Assert.True(pubConnectResult.ReasonCode == ConnAckReasonCode.Success);

        var pubResult = await pubClient.PublishAsync("tests/PerSubscriptionHandler", "test").ConfigureAwait(false);

        // Wait on both TaskCompletionSource objects - both handlers should get called
        var timeout = TimeSpan.FromSeconds(10);
        var delayTask = Task.Delay(timeout);
        var completedTask = await Task.WhenAny(Task.WhenAll(tcs.Task, gtcs.Task), delayTask).ConfigureAwait(false);

        if (completedTask == delayTask)
        {
            throw new TimeoutException("The operation has timed out.");
        }

        // If we reach here, it means both tasks have completed before the timeout
        var handlerResult = await tcs.Task.ConfigureAwait(false);
        var globalHandlerResult = await gtcs.Task.ConfigureAwait(false);
        Assert.True(handlerResult);
        Assert.True(globalHandlerResult);

        var disconnectResult = await subClient.DisconnectAsync().ConfigureAwait(false);
        Assert.True(disconnectResult);

        disconnectResult = await pubClient.DisconnectAsync().ConfigureAwait(false);
        Assert.True(disconnectResult);
    }

    [Fact]
    public async Task PerSubHandlerWithSingleLevelWildcardAsync()
    {
        // Create a subscribeClient that subscribes to a topic with a single-level wildcard
        var options = new HiveMQClientOptionsBuilder().WithClientId("PerSubHandlerWithSingleLevelWildcardAsync1").Build();
        var subscribeClient = new HiveMQClient(options);
        var connectResult = await subscribeClient.ConnectAsync().ConfigureAwait(false);
        Assert.True(connectResult.ReasonCode == ConnAckReasonCode.Success);

        // This is the fix for:
        // per-subscription OnMessageReceivedEventLauncher exception: One or more errors occurred. (An attempt was
        // made to transition a task to a final state when it had already completed.)
        var tcs1 = new TaskCompletionSource<bool>();
        var tcs2 = new TaskCompletionSource<bool>();
        var tcs3 = new TaskCompletionSource<bool>();

        var messageCount = 0;

        var subscribeOptions = new SubscribeOptionsBuilder()
            .WithSubscription("tests/PerSubHandlerWithSingleLevelWildcard/+/msg", MQTT5.Types.QualityOfService.AtLeastOnceDelivery, messageReceivedHandler: (sender, args) =>
            {
                var pattern = @"^tests/PerSubHandlerWithSingleLevelWildcard/[0-2]/msg$";
                var regex = new Regex(pattern);
                Assert.Matches(regex, args.PublishMessage.Topic);

                Assert.Equal("test", args.PublishMessage.PayloadAsString);

                Interlocked.Increment(ref messageCount);
                if (messageCount == 3)
                {
                    if (args.PublishMessage.Topic == "tests/PerSubHandlerWithSingleLevelWildcard/0/msg")
                    {
                        if (!tcs1.Task.IsCompleted)
                        {
                            tcs1.SetResult(true);
                        }
                    }
                    else if (args.PublishMessage.Topic == "tests/PerSubHandlerWithSingleLevelWildcard/1/msg")
                    {
                        if (!tcs2.Task.IsCompleted)
                        {
                            tcs2.SetResult(true);
                        }
                    }
                    else if (args.PublishMessage.Topic == "tests/PerSubHandlerWithSingleLevelWildcard/2/msg")
                    {
                        if (!tcs3.Task.IsCompleted)
                        {
                            tcs3.SetResult(true);
                        }
                    }
                }
            })
            .Build();

        var subResult = await subscribeClient.SubscribeAsync(subscribeOptions).ConfigureAwait(false);

        Assert.NotEmpty(subResult.Subscriptions);
        Assert.Equal(SubAckReasonCode.GrantedQoS1, subResult.Subscriptions[0].SubscribeReasonCode);

        options = new HiveMQClientOptionsBuilder().WithClientId("PerSubHandlerWithSingleLevelWildcardAsync2").Build();
        var pubClient = new HiveMQClient(options);
        var pubConnectResult = await pubClient.ConnectAsync().ConfigureAwait(false);
        Assert.True(pubConnectResult.ReasonCode == ConnAckReasonCode.Success);

        // Publish 3 messages that will match the single-level wildcard
        for (var i = 0; i < 3; i++)
        {
            await pubClient.PublishAsync($"tests/PerSubHandlerWithSingleLevelWildcard/{i}/msg", "test").ConfigureAwait(false);
        }

        // Wait for the 3 messages to be received by the per-subscription handler
        var timeout = TimeSpan.FromSeconds(10);
        var delayTask = Task.Delay(timeout);
        var completedTask = await Task.WhenAny(Task.WhenAll(tcs1.Task, tcs2.Task, tcs3.Task), delayTask).ConfigureAwait(false);

        var disconnectResult = await subscribeClient.DisconnectAsync().ConfigureAwait(false);
        Assert.True(disconnectResult);

        disconnectResult = await pubClient.DisconnectAsync().ConfigureAwait(false);
        Assert.True(disconnectResult);

        subscribeClient.Dispose();
        pubClient.Dispose();
    }

    [Fact]
    public async Task PerSubHandlerWithMultiLevelWildcardAsync()
    {
        // Create a subscribeClient that subscribes to a topic with a single-level wildcard
        var options = new HiveMQClientOptionsBuilder().WithClientId("PerSubHandlerWithMultiLevelWildcardAsync1").Build();
        var subscribeClient = new HiveMQClient(options);
        var connectResult = await subscribeClient.ConnectAsync().ConfigureAwait(false);
        Assert.True(connectResult.ReasonCode == ConnAckReasonCode.Success);

        var tcs = new TaskCompletionSource<bool>();
        var messageCount = 0;

        var subscribeOptions = new SubscribeOptionsBuilder()
            .WithSubscription(
                "tests/PerSubHandlerWithMultiLevelWildcard/#",
                MQTT5.Types.QualityOfService.AtLeastOnceDelivery,
                messageReceivedHandler: (sender, args) =>
            {
                var pattern = @"\Atests/PerSubHandlerWithMultiLevelWildcard/(/?|.+)\z";
                var regex = new Regex(pattern);
                Assert.Matches(regex, args.PublishMessage.Topic);

                Assert.Equal("test", args.PublishMessage.PayloadAsString);

                Interlocked.Increment(ref messageCount);
                if (messageCount == 3 && !tcs.Task.IsCompleted)
                {
                    tcs.SetResult(true);
                }
            })
            .Build();

        var subResult = await subscribeClient.SubscribeAsync(subscribeOptions).ConfigureAwait(false);

        Assert.NotEmpty(subResult.Subscriptions);
        Assert.Equal(SubAckReasonCode.GrantedQoS1, subResult.Subscriptions[0].SubscribeReasonCode);

        options = new HiveMQClientOptionsBuilder().WithClientId("PerSubHandlerWithMultiLevelWildcardAsync2").Build();
        var pubClient = new HiveMQClient(options);
        var pubConnectResult = await pubClient.ConnectAsync().ConfigureAwait(false);
        Assert.True(pubConnectResult.ReasonCode == ConnAckReasonCode.Success);

        // Publish 3 messages that will match the multi-level wildcard
        await pubClient.PublishAsync($"tests/PerSubHandlerWithMultiLevelWildcard/1", "test").ConfigureAwait(false);
        await pubClient.PublishAsync($"tests/PerSubHandlerWithMultiLevelWildcard/1/2", "test").ConfigureAwait(false);
        await pubClient.PublishAsync($"tests/PerSubHandlerWithMultiLevelWildcard/1/2/3/4/5", "test").ConfigureAwait(false);

        // Wait for the 3 messages to be received by the per-subscription handler
        var handlerResult = await tcs.Task.WaitAsync(TimeSpan.FromSeconds(10)).ConfigureAwait(false);
        Assert.True(handlerResult);

        var disconnectResult = await subscribeClient.DisconnectAsync().ConfigureAwait(false);
        Assert.True(disconnectResult);

        disconnectResult = await pubClient.DisconnectAsync().ConfigureAwait(false);
        Assert.True(disconnectResult);

        subscribeClient.Dispose();
        pubClient.Dispose();
    }
}
