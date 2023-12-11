namespace HiveMQtt.Test.HiveMQClient;

using System.Threading.Tasks;
using HiveMQtt.Client;
using HiveMQtt.Client.Options;
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
            .WithUserProperties("test", "test")
            .Build();

        var subResult = await subClient.SubscribeAsync(subscribeOptions).ConfigureAwait(false);

        Assert.NotEmpty(subResult.Subscriptions);
        Assert.Equal(SubAckReasonCode.GrantedQoS0, subResult.Subscriptions[0].SubscribeReasonCode);
        Assert.Equal(SubAckReasonCode.GrantedQoS0, subResult.Subscriptions[1].SubscribeReasonCode);

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
            .WithUserProperties("test", "test")
            .Build();

        var subResult = await subClient.SubscribeAsync(subscribeOptions).ConfigureAwait(false);

        Assert.NotEmpty(subResult.Subscriptions);
        Assert.Equal(SubAckReasonCode.GrantedQoS0, subResult.Subscriptions[0].SubscribeReasonCode);
        Assert.Equal(true, subResult.Subscriptions[0].TopicFilter.NoLocal);
        Assert.Equal(true, subResult.Subscriptions[0].TopicFilter.RetainAsPublished);
        Assert.Equal(MQTT5.Types.RetainHandling.SendAtSubscribe, subResult.Subscriptions[0].TopicFilter.RetainHandling);
        Assert.Equal(SubAckReasonCode.GrantedQoS0, subResult.Subscriptions[1].SubscribeReasonCode);

        var disconnectResult = await subClient.DisconnectAsync().ConfigureAwait(false);
        Assert.True(disconnectResult);
    }
}
