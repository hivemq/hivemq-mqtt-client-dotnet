namespace HiveMQtt.Test.HiveMQClient;

using System.Threading.Tasks;
using HiveMQtt.Client;
using HiveMQtt.Client.Events;
using HiveMQtt.Client.Options;
using HiveMQtt.MQTT5.ReasonCodes;
using HiveMQtt.MQTT5.Types;
using Xunit;

public class SubscribeTest
{
    [Fact]
    public async Task MostBasicSubscribeAsync()
    {
        var subClient = new HiveMQClient();
        var connectResult = await subClient.ConnectAsync().ConfigureAwait(false);
        Assert.True(connectResult.ReasonCode == ConnAckReasonCode.Success);

        var subResult = await subClient.SubscribeAsync("tests/MostBasicSubscribeAsync").ConfigureAwait(false);

        Assert.NotEmpty(subResult.Subscriptions);
        Assert.Equal(SubAckReasonCode.GrantedQoS0, subResult.Subscriptions[0].SubscribeReasonCode);
        Assert.Single(subClient.Subscriptions);

        var disconnectResult = await subClient.DisconnectAsync().ConfigureAwait(false);
        Assert.True(disconnectResult);
    }

    [Fact]
    public async Task MultiSubscribeAsync()
    {
        var subClient = new HiveMQClient();
        var connectResult = await subClient.ConnectAsync().ConfigureAwait(false);
        Assert.True(connectResult.ReasonCode == ConnAckReasonCode.Success);

        var subscribeOptions = new SubscribeOptions();
        subscribeOptions.TopicFilters.Add(new TopicFilter("tests/MultiSubscribeAsync", MQTT5.Types.QualityOfService.AtLeastOnceDelivery));
        subscribeOptions.TopicFilters.Add(new TopicFilter("tests/MultiSubscribeAsync2", MQTT5.Types.QualityOfService.AtLeastOnceDelivery));
        subscribeOptions.UserProperties.Add("test", "test");

        var subResult = await subClient.SubscribeAsync(subscribeOptions).ConfigureAwait(false);

        Assert.NotEmpty(subResult.Subscriptions);
        Assert.Equal(SubAckReasonCode.GrantedQoS1, subResult.Subscriptions[0].SubscribeReasonCode);
        Assert.Equal(SubAckReasonCode.GrantedQoS1, subResult.Subscriptions[1].SubscribeReasonCode);
        Assert.Equal(2, subResult.Subscriptions.Count);
        Assert.Equal(2, subClient.Subscriptions.Count);

        var disconnectResult = await subClient.DisconnectAsync().ConfigureAwait(false);
        Assert.True(disconnectResult);
    }

    [Fact]
    public async Task MultiSubscribeExtendedOptionsAsync()
    {
        var subClient = new HiveMQClient();
        var connectResult = await subClient.ConnectAsync().ConfigureAwait(false);
        Assert.True(connectResult.ReasonCode == ConnAckReasonCode.Success);

        var subscribeOptions = new SubscribeOptions();
        subscribeOptions.TopicFilters.Add(new TopicFilter("tests/MultiSubscribeAsync", MQTT5.Types.QualityOfService.AtLeastOnceDelivery, true, true, MQTT5.Types.RetainHandling.SendAtSubscribe));
        subscribeOptions.TopicFilters.Add(new TopicFilter("tests/MultiSubscribeAsync2", MQTT5.Types.QualityOfService.AtLeastOnceDelivery));
        subscribeOptions.UserProperties.Add("test", "test");

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
    public async Task SubscribeQoS1Async()
    {
        var subClient = new HiveMQClient();
        var connectResult = await subClient.ConnectAsync().ConfigureAwait(false);
        Assert.True(connectResult.ReasonCode == ConnAckReasonCode.Success);

        var subResult = await subClient.SubscribeAsync("tests/SubscribeQoS1Async", MQTT5.Types.QualityOfService.AtLeastOnceDelivery).ConfigureAwait(false);

        Assert.NotEmpty(subResult.Subscriptions);
        Assert.Equal(SubAckReasonCode.GrantedQoS1, subResult.Subscriptions[0].SubscribeReasonCode);

        var disconnectResult = await subClient.DisconnectAsync().ConfigureAwait(false);
        Assert.True(disconnectResult);
    }

    [Fact]
    public async Task SubscribeQoS2Async()
    {
        var subClient = new HiveMQClient();
        var connectResult = await subClient.ConnectAsync().ConfigureAwait(false);
        Assert.True(connectResult.ReasonCode == ConnAckReasonCode.Success);

        var subResult = await subClient.SubscribeAsync("tests/SubscribeQoS2Async", MQTT5.Types.QualityOfService.ExactlyOnceDelivery).ConfigureAwait(false);

        Assert.NotEmpty(subResult.Subscriptions);
        Assert.Equal(SubAckReasonCode.GrantedQoS2, subResult.Subscriptions[0].SubscribeReasonCode);

        var disconnectResult = await subClient.DisconnectAsync().ConfigureAwait(false);
        Assert.True(disconnectResult);
    }

    [Fact]
    public async Task Test_Subscribe_Events_Async()
    {
        var client = new HiveMQClient();

        // Client Events
        client.BeforeSubscribe += BeforeSubscribeHandler;
        client.AfterSubscribe += AfterSubscribeHandler;

        // Packet Events
        client.OnSubscribeSent += OnSubscribeSentHandler;
        client.OnSubAckReceived += OnSubAckReceivedHandler;

        var result = await client.ConnectAsync().ConfigureAwait(false);
        Assert.Equal(ConnAckReasonCode.Success, result.ReasonCode);

        var subscribeResult = client.SubscribeAsync("tests/Test_Subscribe_Events_Async").ConfigureAwait(false);

        // Wait for event handlers to finish
        await Task.Delay(1000).ConfigureAwait(false);

        // Assert that all Events were called
        Assert.True(client.LocalStore.ContainsKey("BeforeSubscribeHandlerCalled"));
        Assert.True(client.LocalStore.ContainsKey("AfterSubscribeHandlerCalled"));

        Assert.True(client.LocalStore.ContainsKey("OnSubscribeSentHandlerCalled"));
        Assert.True(client.LocalStore.ContainsKey("OnSubAckReceivedHandlerCalled"));

        // Remove event handlers
        client.BeforeSubscribe -= BeforeSubscribeHandler;
        client.AfterSubscribe -= AfterSubscribeHandler;

        client.OnSubscribeSent -= OnSubscribeSentHandler;
        client.OnSubAckReceived -= OnSubAckReceivedHandler;
    }

    private static void BeforeSubscribeHandler(object? sender, BeforeSubscribeEventArgs eventArgs)
    {
        Assert.NotNull(sender);
        if (sender is not null)
        {
            var client = (HiveMQClient)sender;
            client.LocalStore.Add("BeforeSubscribeHandlerCalled", "true");
        }

        Assert.NotNull(eventArgs.Options);
    }

    private static void OnSubscribeSentHandler(object? sender, OnSubscribeSentEventArgs eventArgs)
    {
        Assert.NotNull(sender);
        if (sender is not null)
        {
            var client = (HiveMQClient)sender;
            client.LocalStore.Add("OnSubscribeSentHandlerCalled", "true");
        }

        Assert.NotNull(eventArgs.SubscribePacket);
    }

    private static void OnSubAckReceivedHandler(object? sender, OnSubAckReceivedEventArgs eventArgs)
    {
        Assert.NotNull(sender);
        if (sender is not null)
        {
            var client = (HiveMQClient)sender;
            client.LocalStore.Add("OnSubAckReceivedHandlerCalled", "true");
        }

        Assert.NotNull(eventArgs.SubAckPacket);
    }

    private static void AfterSubscribeHandler(object? sender, AfterSubscribeEventArgs eventArgs)
    {
        Assert.NotNull(sender);
        if (sender is not null)
        {
            var client = (HiveMQClient)sender;
            client.LocalStore.Add("AfterSubscribeHandlerCalled", "true");
        }

        Assert.NotNull(eventArgs.SubscribeResult);
    }
}
