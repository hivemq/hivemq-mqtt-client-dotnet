namespace HiveMQtt.Test.HiveClient;

using System.Threading.Tasks;
using HiveMQtt.Client;
using HiveMQtt.Client.Events;
using HiveMQtt.MQTT5.ReasonCodes;
using Xunit;

// public event EventHandler<MqttApplicationMessageReceivedEventArgs> ApplicationMessageReceived;

public class HiveClientSubscribeTest
{
    [Fact]
    public async Task MostBasicSubscribe()
    {
        var subClient = new HiveClient();
        var connectResult = await subClient.ConnectAsync().ConfigureAwait(false);
        Assert.True(connectResult.ReasonCode == ConnAckReasonCode.Success);

        var subResult = await subClient.SubscribeAsync("data/topic").ConfigureAwait(false);

        Assert.NotEmpty(subResult.Subscriptions);
        Assert.Equal(SubAckReasonCode.GrantedQoS0, subResult.Subscriptions[0].SubscribeReasonCode);

        var disconnectResult = await subClient.DisconnectAsync().ConfigureAwait(false);
        Assert.True(disconnectResult);
    }

    [Fact]
    public async Task SubscribeQoS1()
    {
        var subClient = new HiveClient();
        var connectResult = await subClient.ConnectAsync().ConfigureAwait(false);
        Assert.True(connectResult.ReasonCode == ConnAckReasonCode.Success);

        var subResult = await subClient.SubscribeAsync("data/topic", MQTT5.Types.QualityOfService.AtLeastOnceDelivery).ConfigureAwait(false);

        Assert.NotEmpty(subResult.Subscriptions);
        Assert.Equal(SubAckReasonCode.GrantedQoS1, subResult.Subscriptions[0].SubscribeReasonCode);

        var disconnectResult = await subClient.DisconnectAsync().ConfigureAwait(false);
        Assert.True(disconnectResult);
    }

    [Fact]
    public async Task SubscribeQoS2()
    {
        var subClient = new HiveClient();
        var connectResult = await subClient.ConnectAsync().ConfigureAwait(false);
        Assert.True(connectResult.ReasonCode == ConnAckReasonCode.Success);

        var subResult = await subClient.SubscribeAsync("data/topic", MQTT5.Types.QualityOfService.AtLeastOnceDelivery).ConfigureAwait(false);

        Assert.NotEmpty(subResult.Subscriptions);
        Assert.Equal(SubAckReasonCode.GrantedQoS2, subResult.Subscriptions[0].SubscribeReasonCode);

        var disconnectResult = await subClient.DisconnectAsync().ConfigureAwait(false);
        Assert.True(disconnectResult);
    }

    [Fact]
    public async Task Test_Subscribe_Events_Async()
    {
        var client = new HiveClient();

        // Client Events
        client.BeforeSubscribe += BeforeSubscribeHandler;
        client.AfterSubscribe += AfterSubscribeHandler;

        // Packet Events
        client.OnSubscribeSent += OnSubscribeSentHandler;
        client.OnSubAckReceived += OnSubAckReceivedHandler;

        var result = await client.ConnectAsync().ConfigureAwait(false);

        var subscribeResult = client.SubscribeAsync("data/topic").ConfigureAwait(false);

        // Wait for event handlers to finish
        await Task.Delay(100);

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
            var client = (HiveClient)sender;
            client.LocalStore.Add("BeforeSubscribeHandlerCalled", "true");
        }

        Assert.NotNull(eventArgs.SubscribeOptions);
    }

    private static void OnSubscribeSentHandler(object? sender, SubscribeSentEventArgs eventArgs)
    {
        Assert.NotNull(sender);
        if (sender is not null)
        {
            var client = (HiveClient)sender;
            client.LocalStore.Add("OnSubscribeSentHandlerCalled", "true");
        }

        Assert.NotNull(eventArgs.SubscribePacket);
    }

    private static void OnSubAckReceivedHandler(object? sender, SubAckReceivedEventArgs eventArgs)
    {
        Assert.NotNull(sender);
        if (sender is not null)
        {
            var client = (HiveClient)sender;
            client.LocalStore.Add("OnSubAckReceivedHandlerCalled", "true");
        }

        Assert.NotNull(eventArgs.SubAckPacket);
    }

    private static void AfterSubscribeHandler(object? sender, AfterSubscribeEventArgs eventArgs)
    {
        Assert.NotNull(sender);
        if (sender is not null)
        {
            var client = (HiveClient)sender;
            client.LocalStore.Add("AfterSubscribeHandlerCalled", "true");
        }
        Assert.NotNull(eventArgs.SubscribeResult);
    }
}
