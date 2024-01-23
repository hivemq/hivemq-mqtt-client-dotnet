namespace HiveMQtt.Test.HiveMQClient;

using System.Threading.Tasks;
using HiveMQtt.Client;
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
}
