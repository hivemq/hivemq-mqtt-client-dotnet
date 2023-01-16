namespace HiveMQtt.Test.HiveClient;

using System.Threading.Tasks;
using HiveMQtt.Client;
using HiveMQtt.Client.Events;
using HiveMQtt.Client.Options;
using HiveMQtt.MQTT5.ReasonCodes;
using Xunit;

public class HiveClientConnectTest
{
    [Fact]
    public async Task Basic_Connect_And_Disconnect_Async()
    {
        var client = new HiveClient();
        Assert.NotNull(client);

        var connectResult = await client.ConnectAsync().ConfigureAwait(false);

        Assert.True(connectResult.ReasonCode == ConnAckReasonCode.Success);
        Assert.True(client.IsConnected());

        var disconnectOptions = new DisconnectOptions();
        Assert.Equal(DisconnectReasonCode.NormalDisconnection, disconnectOptions.ReasonCode);

        var disconnectResult = await client.DisconnectAsync(disconnectOptions).ConfigureAwait(false);
        Assert.True(disconnectResult);
    }

    [Fact]
    public async Task Test_Connect_Events_Async()
    {
        var client = new HiveClient();

        // Client Events
        client.BeforeConnect += BeforeConnectHandler;
        client.OnConnected += OnConnectedHandler;
        client.AfterConnect += AfterConnectHandler;

        // Packet Events
        client.OnConnectSent += OnConnectSentHandler;
        client.OnConnAckReceived += OnConnAckReceivedHandler;

        var result = await client.ConnectAsync().ConfigureAwait(false);
        Assert.NotNull(client);

        // Wait for event handlers to finish
        await Task.Delay(100);

        // Assert that all Events were called
        Assert.True(client.LocalStore.ContainsKey("BeforeConnectHandlerCalled"));
        Assert.True(client.LocalStore.ContainsKey("OnConnectedHandlerCalled"));
        Assert.True(client.LocalStore.ContainsKey("AfterConnectHandlerCalled"));

        Assert.True(client.LocalStore.ContainsKey("OnConnectSentHandlerCalled"));
        Assert.True(client.LocalStore.ContainsKey("OnConnAckReceivedHandlerCalled"));

        // Remove event handlers
        client.BeforeConnect -= BeforeConnectHandler;
        client.OnConnected -= OnConnectedHandler;
        client.AfterConnect -= AfterConnectHandler;

        client.OnConnectSent -= OnConnectSentHandler;
        client.OnConnAckReceived -= OnConnAckReceivedHandler;
    }

    private static void BeforeConnectHandler(object? sender, BeforeConnectEventArgs eventArgs)
    {
        Assert.NotNull(sender);
        if (sender is not null)
        {
            var client = (HiveClient)sender;
            client.LocalStore.Add("BeforeConnectHandlerCalled", "true");
        }

        Assert.NotNull(eventArgs.Options);
    }

    private static void OnConnectedHandler(object? sender, ConnectedEventArgs eventArgs)
    {
        Assert.NotNull(sender);
        if (sender is not null)
        {
            var client = (HiveClient)sender;
            client.LocalStore.Add("OnConnectedHandlerCalled", "true");
        }

        Assert.NotNull(eventArgs.ConnectResult);
    }

    private static void OnConnectSentHandler(object? sender, ConnectSentEventArgs eventArgs)
    {
        Assert.NotNull(sender);
        if (sender is not null)
        {
            var client = (HiveClient)sender;
            client.LocalStore.Add("OnConnectSentHandlerCalled", "true");
        }

        Assert.NotNull(eventArgs.ConnectPacket);
    }

    private static void OnConnAckReceivedHandler(object? sender, ConnAckReceivedEventArgs eventArgs)
    {
        Assert.NotNull(sender);
        if (sender is not null)
        {
            var client = (HiveClient)sender;
            client.LocalStore.Add("OnConnAckReceivedHandlerCalled", "true");
        }

        Assert.NotNull(eventArgs.ConnAckPacket);
    }

    private static void AfterConnectHandler(object? sender, AfterConnectEventArgs eventArgs)
    {
        Assert.NotNull(sender);
        if (sender is not null)
        {
            var client = (HiveClient)sender;
            client.LocalStore.Add("AfterConnectHandlerCalled", "true");
        }
        Assert.NotNull(eventArgs.ConnectResult);
    }
}
