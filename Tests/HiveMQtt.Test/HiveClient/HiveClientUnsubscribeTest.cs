namespace HiveMQtt.Test.HiveClient;

using System.Threading.Tasks;
using HiveMQtt.Client;
using HiveMQtt.Client.Events;
using HiveMQtt.MQTT5.ReasonCodes;
using Xunit;

public class HiveClientUnsubscribeTest
{
    [Fact]
    public async Task MostBasicUnsubscribeAsync()
    {
        var subClient = new HiveClient();
        var connectResult = await subClient.ConnectAsync().ConfigureAwait(false);
        Assert.True(connectResult.ReasonCode == ConnAckReasonCode.Success);

        var subResult = await subClient.SubscribeAsync("data/topic").ConfigureAwait(false);

        Assert.NotEmpty(subResult.Subscriptions);
        Assert.True(subClient.Subscriptions.Count == 1);
        Assert.Equal(SubAckReasonCode.GrantedQoS0, subResult.Subscriptions[0].SubscribeReasonCode);

        var unsubResult = await subClient.UnsubscribeAsync("data/topic").ConfigureAwait(false);

        Assert.NotEmpty(unsubResult.Subscriptions);
        Assert.Equal(UnsubAckReasonCode.Success, unsubResult.Subscriptions[0].UnsubscribeReasonCode);
        Assert.True(subClient.Subscriptions.Count == 0);

        var disconnectResult = await subClient.DisconnectAsync().ConfigureAwait(false);
        Assert.True(disconnectResult);
    }

    [Fact]
    public async Task Test_Unsubscribe_Events_Async()
    {
        var client = new HiveClient();

        // Client Events
        client.BeforeUnsubscribe += BeforeUnsubscribeHandler;
        client.AfterUnsubscribe += AfterUnsubscribeHandler;

        // Packet Events
        client.OnUnsubscribeSent += OnUnsubscribeSentHandler;
        client.OnUnsubAckReceived += OnUnsubAckReceivedHandler;

        var result = await client.ConnectAsync().ConfigureAwait(false);
        Assert.Equal(ConnAckReasonCode.Success, result.ReasonCode);

        var subscribeResult = client.UnsubscribeAsync("data/topic").ConfigureAwait(false);

        // Wait for event handlers to finish
        await Task.Delay(1000).ConfigureAwait(false);

        // Assert that all Events were called
        Assert.True(client.LocalStore.ContainsKey("BeforeUnsubscribeHandlerCalled"));
        Assert.True(client.LocalStore.ContainsKey("AfterUnsubscribeHandlerCalled"));

        Assert.True(client.LocalStore.ContainsKey("OnUnsubscribeSentHandlerCalled"));
        Assert.True(client.LocalStore.ContainsKey("OnSubAckReceivedHandlerCalled"));

        // Remove event handlers
        client.BeforeUnsubscribe -= BeforeUnsubscribeHandler;
        client.AfterUnsubscribe -= AfterUnsubscribeHandler;

        client.OnUnsubscribeSent -= OnUnsubscribeSentHandler;
        client.OnUnsubAckReceived -= OnUnsubAckReceivedHandler;
    }

    private static void BeforeUnsubscribeHandler(object? sender, BeforeUnsubscribeEventArgs eventArgs)
    {
        Assert.NotNull(sender);
        if (sender is not null)
        {
            var client = (HiveClient)sender;
            client.LocalStore.Add("BeforeUnsubscribeHandlerCalled", "true");
        }

        Assert.NotNull(eventArgs.Subscriptions);
    }

    private static void OnUnsubscribeSentHandler(object? sender, OnUnsubscribeSentEventArgs eventArgs)
    {
        Assert.NotNull(sender);
        if (sender is not null)
        {
            var client = (HiveClient)sender;
            client.LocalStore.Add("OnUnsubscribeSentHandlerCalled", "true");
        }

        Assert.NotNull(eventArgs.UnsubscribePacket);
    }

    private static void OnUnsubAckReceivedHandler(object? sender, OnUnsubAckReceivedEventArgs eventArgs)
    {
        Assert.NotNull(sender);
        if (sender is not null)
        {
            var client = (HiveClient)sender;
            client.LocalStore.Add("OnUnsubAckReceivedHandlerCalled", "true");
        }

        Assert.NotNull(eventArgs.UnsubAckPacket);
    }

    private static void AfterUnsubscribeHandler(object? sender, AfterUnsubscribeEventArgs eventArgs)
    {
        Assert.NotNull(sender);
        if (sender is not null)
        {
            var client = (HiveClient)sender;
            client.LocalStore.Add("AfterUnsubscribeHandlerCalled", "true");
        }

        Assert.NotNull(eventArgs.UnsubscribeResult);
    }
}
