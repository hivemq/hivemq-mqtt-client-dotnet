namespace HiveMQtt.Test.HiveMQClient;

using System.Threading.Tasks;
using HiveMQtt.Client;
using HiveMQtt.Client.Options;
using HiveMQtt.Client.Results;
using HiveMQtt.MQTT5.ReasonCodes;
using HiveMQtt.MQTT5.Types;
using Xunit;

public class UnsubscribeBuilderTest
{
    [Fact]
    public async Task BasicUnsubscribeAsync()
    {
        var subClient = new HiveMQClient();
        var connectResult = await subClient.ConnectAsync().ConfigureAwait(false);
        Assert.True(connectResult.ReasonCode == ConnAckReasonCode.Success);

        var subResult = await subClient.SubscribeAsync("tests/MostBasicUnsubscribeAsync").ConfigureAwait(false);

        Assert.NotEmpty(subResult.Subscriptions);
        Assert.Single(subClient.Subscriptions);
        Assert.Equal(SubAckReasonCode.GrantedQoS0, subResult.Subscriptions[0].SubscribeReasonCode);

        var subscription = subClient.GetSubscriptionByTopic("tests/MostBasicUnsubscribeAsync");
        Assert.NotNull(subscription);

        UnsubscribeOptions unsubOptions;
        UnsubscribeResult unsubResult;
        if (subscription is not null)
        {
            unsubOptions = new UnsubscribeOptionsBuilder()
                .WithSubscription(subscription)
                .Build();

            unsubResult = await subClient.UnsubscribeAsync(unsubOptions).ConfigureAwait(false);
            Assert.NotEmpty(unsubResult.Subscriptions);
            Assert.Equal(UnsubAckReasonCode.Success, unsubResult.Subscriptions[0].UnsubscribeReasonCode);
        }

        Assert.Empty(subClient.Subscriptions);

        var disconnectResult = await subClient.DisconnectAsync().ConfigureAwait(false);
        Assert.True(disconnectResult);
    }

    [Fact]
    public async Task WithSubscriptionAsync()
    {
        var subClient = new HiveMQClient();
        var connectResult = await subClient.ConnectAsync().ConfigureAwait(false);
        Assert.True(connectResult.ReasonCode == ConnAckReasonCode.Success);

        var subResult = await subClient.SubscribeAsync("tests/MostBasicUnsubscribeAsync").ConfigureAwait(false);

        Assert.Single(subResult.Subscriptions);
        Assert.Single(subClient.Subscriptions);
        Assert.Equal(SubAckReasonCode.GrantedQoS0, subResult.Subscriptions[0].SubscribeReasonCode);

        var unsubOptions = new UnsubscribeOptionsBuilder()
            .WithSubscription(subResult.Subscriptions[0])
            .Build();

        var unsubResult = await subClient.UnsubscribeAsync(unsubOptions).ConfigureAwait(false);

        Assert.NotEmpty(unsubResult.Subscriptions);
        Assert.Equal(UnsubAckReasonCode.Success, unsubResult.Subscriptions[0].UnsubscribeReasonCode);
        Assert.True(subClient.Subscriptions.Count == 0);

        var disconnectResult = await subClient.DisconnectAsync().ConfigureAwait(false);
        Assert.True(disconnectResult);
    }

    [Fact]
    public async Task InvalidUnsubscribeStringAsync()
    {
        var subClient = new HiveMQClient();
        var connectResult = await subClient.ConnectAsync().ConfigureAwait(false);
        Assert.True(connectResult.ReasonCode == ConnAckReasonCode.Success);

        Assert.Empty(subClient.Subscriptions);

        // Generate a Random Topic name
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        var random = new Random();
        var fakeTopicName = new string(Enumerable.Repeat(chars, 15).Select(s => s[random.Next(s.Length)]).ToArray());

        var fakeTopicFilter = new TopicFilter($"tests3/{fakeTopicName}", QualityOfService.AtLeastOnceDelivery);
        var fakeSubscription = new Subscription(fakeTopicFilter);
        var unsubOptions = new UnsubscribeOptionsBuilder()
            .WithSubscription(fakeSubscription)
            .Build();

        var unsubscribeResult = await subClient.UnsubscribeAsync(unsubOptions).ConfigureAwait(false);
        Assert.Single(unsubscribeResult.Subscriptions);
        Assert.True(subClient.Subscriptions.Count == 0);

        // Note: The broker always returns Success - even for topics not subscribed to
        // Assert.Equal(UnsubAckReasonCode.NoSubscriptionExisted, unsubscribeResult.Subscriptions[0].UnsubscribeReasonCode);
        Assert.Equal(UnsubAckReasonCode.Success, unsubscribeResult.Subscriptions[0].UnsubscribeReasonCode);

        var disconnectResult = await subClient.DisconnectAsync().ConfigureAwait(false);
        Assert.True(disconnectResult);
    }
}
