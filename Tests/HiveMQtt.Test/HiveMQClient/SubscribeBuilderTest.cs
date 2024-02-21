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
        var subClient = new HiveMQClient();
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
        var subClient = new HiveMQClient();
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
        var subClient = new HiveMQClient();
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

        var pubClient = new HiveMQClient();
        var pubConnectResult = await pubClient.ConnectAsync().ConfigureAwait(false);
        Assert.True(pubConnectResult.ReasonCode == ConnAckReasonCode.Success);

        var pubResult = await pubClient.PublishAsync("tests/PerSubscriptionHandler", "test").ConfigureAwait(false);

        var handlerResult = await tcs.Task.WaitAsync(TimeSpan.FromSeconds(10)).ConfigureAwait(false);
        Assert.True(handlerResult);

        var disconnectResult = await subClient.DisconnectAsync().ConfigureAwait(false);
        Assert.True(disconnectResult);
    }

    [Fact]
    public async Task PerSubscriptionHandlerAndGlobalHandlerAsync()
    {
        var subClient = new HiveMQClient();
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

        var pubClient = new HiveMQClient();
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
    }

    [Fact]
    public async Task PerSubHandlerWithSingleLevelWildcardAsync()
    {
        // Create a subClient that subscribes to a topic with a single-level wildcard
        var subClient = new HiveMQClient();
        var connectResult = await subClient.ConnectAsync().ConfigureAwait(false);
        Assert.True(connectResult.ReasonCode == ConnAckReasonCode.Success);

        var tcs = new TaskCompletionSource<bool>();
        var messageCount = 0;

        var subscribeOptions = new SubscribeOptionsBuilder()
            .WithSubscription("tests/PerSubHandlerWithSingleLevelWildcard/+/msg", MQTT5.Types.QualityOfService.AtLeastOnceDelivery, messageReceivedHandler: (sender, args) =>
            {
                messageCount++;
                var pattern = @"^tests/PerSubHandlerWithSingleLevelWildcard/[0-2]/msg$";
                var regex = new Regex(pattern);
                Assert.Matches(regex, args.PublishMessage.Topic);

                Assert.Equal("test", args.PublishMessage.PayloadAsString);

                if (messageCount == 3)
                {
                    tcs.SetResult(true);
                }
            })
            .Build();

        var subResult = await subClient.SubscribeAsync(subscribeOptions).ConfigureAwait(false);

        Assert.NotEmpty(subResult.Subscriptions);
        Assert.Equal(SubAckReasonCode.GrantedQoS1, subResult.Subscriptions[0].SubscribeReasonCode);

        var pubClient = new HiveMQClient();
        var pubConnectResult = await pubClient.ConnectAsync().ConfigureAwait(false);
        Assert.True(pubConnectResult.ReasonCode == ConnAckReasonCode.Success);

        // Publish 3 messages that will match the single-level wildcard
        for (var i = 0; i < 3; i++)
        {
            await pubClient.PublishAsync($"tests/PerSubHandlerWithSingleLevelWildcard/{i}/msg", "test").ConfigureAwait(false);
        }

        // Wait for the 3 messages to be received by the per-subscription handler
        var handlerResult = await tcs.Task.WaitAsync(TimeSpan.FromSeconds(10)).ConfigureAwait(false);
        Assert.True(handlerResult);

        var disconnectResult = await subClient.DisconnectAsync().ConfigureAwait(false);
        Assert.True(disconnectResult);
    }
}
