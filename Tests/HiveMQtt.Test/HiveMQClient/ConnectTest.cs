namespace HiveMQtt.Test.HiveMQClient;

using System.Globalization;
using System.Threading.Tasks;
using HiveMQtt.Client;
using HiveMQtt.Client.Events;
using HiveMQtt.Client.Exceptions;
using HiveMQtt.Client.Options;
using HiveMQtt.MQTT5.ReasonCodes;
using Xunit;

public class ConnectTest
{
    [Fact]
    public async Task BasicConnectAndDisconnectAsync()
    {
        var client = new HiveMQClient();
        Assert.NotNull(client);

        var connectResult = await client.ConnectAsync().ConfigureAwait(false);

        Assert.True(connectResult.ReasonCode == ConnAckReasonCode.Success);
        Assert.True(client.IsConnected());

        var disconnectOptions = new DisconnectOptions();
        Assert.Equal(DisconnectReasonCode.NormalDisconnection, disconnectOptions.ReasonCode);

        var disconnectResult = await client.DisconnectAsync(disconnectOptions).ConfigureAwait(false);
        Assert.True(disconnectResult);
        Assert.False(client.IsConnected());
    }

    [Fact]
    public async Task RaiseOnFailureToConnectAsync()
    {
        // Bad port number
        var clientOptions = new HiveMQClientOptionsBuilder().WithPort(0).Build();
        var client = new HiveMQClient(clientOptions);

        await Assert.ThrowsAsync<HiveMQttClientException>(client.ConnectAsync).ConfigureAwait(false);
    }

    [Fact]
    public async Task TestConnectEventsAsync()
    {
        var client = new HiveMQClient();

        // Client Events
        client.BeforeConnect += BeforeConnectHandler;
        client.AfterConnect += AfterConnectHandler;
        client.BeforeDisconnect += BeforeDisconnectHandler;
        client.AfterDisconnect += AfterDisconnectHandler;

        // Packet Events
        client.OnConnectSent += OnConnectSentHandler;
        client.OnConnAckReceived += OnConnAckReceivedHandler;

        // Set up TaskCompletionSource to wait for event handlers to finish
        var taskCompletionSource = new TaskCompletionSource<bool>();
        client.OnDisconnectSent += (sender, args) => taskCompletionSource.SetResult(true);

        // Connect and Disconnect
        var result = await client.ConnectAsync().ConfigureAwait(false);
        await client.DisconnectAsync().ConfigureAwait(false);

        // Wait for event handlers to finish
        await taskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(5)).ConfigureAwait(false);
        await Task.Delay(1000).ConfigureAwait(false);

        // Assert that all Events were called
        Assert.True(client.LocalStore.ContainsKey("BeforeConnectHandlerCalled"));
        Assert.True(client.LocalStore.ContainsKey("AfterConnectHandlerCalled"));

        Assert.True(client.LocalStore.ContainsKey("BeforeDisconnectHandlerCalled"));
        Assert.True(client.LocalStore.ContainsKey("AfterDisconnectHandlerCalled"));

        Assert.True(client.LocalStore.ContainsKey("OnConnectSentHandlerCalled"));
        Assert.True(client.LocalStore.ContainsKey("OnConnAckReceivedHandlerCalled"));

        // Remove event handlers
        client.BeforeConnect -= BeforeConnectHandler;
        client.AfterConnect -= AfterConnectHandler;
        client.BeforeDisconnect -= BeforeDisconnectHandler;
        client.AfterDisconnect -= AfterDisconnectHandler;

        client.OnConnectSent -= OnConnectSentHandler;
        client.OnConnAckReceived -= OnConnAckReceivedHandler;
    }

    [Fact]
    public async Task Test_AfterDisconnectEvent_Async()
    {
        var client = new HiveMQClient();

        // Client Events
        client.AfterDisconnect += AfterDisconnectHandler;

        // Set up TaskCompletionSource to wait for event handlers to finish
        var taskCompletionSource = new TaskCompletionSource<bool>();
        client.OnDisconnectSent += (sender, args) => taskCompletionSource.SetResult(true);

        // Connect and Disconnect
        var result = await client.ConnectAsync().ConfigureAwait(false);
        await client.DisconnectAsync().ConfigureAwait(false);

        // Wait for event handlers to finish
        await taskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(5)).ConfigureAwait(false);
        await Task.Delay(1000).ConfigureAwait(false);

        // Assert that all Events were called
        Assert.True(client.LocalStore.ContainsKey("AfterDisconnectHandlerCalled"));
        Assert.True(client.LocalStore.ContainsKey("AfterDisconnectHandlerCalledCount"));
        Assert.Equal("1", client.LocalStore["AfterDisconnectHandlerCalledCount"]);

        // Remove event handlers
        client.AfterDisconnect -= AfterDisconnectHandler;
    }

    private static void BeforeConnectHandler(object? sender, BeforeConnectEventArgs eventArgs)
    {
        Assert.NotNull(sender);
        if (sender is not null)
        {
            var client = (HiveMQClient)sender;
            client.LocalStore.Add("BeforeConnectHandlerCalled", "true");
        }

        Assert.NotNull(eventArgs.Options);
    }

    private static void OnConnectSentHandler(object? sender, OnConnectSentEventArgs eventArgs)
    {
        Assert.NotNull(sender);
        if (sender is not null)
        {
            var client = (HiveMQClient)sender;
            client.LocalStore.Add("OnConnectSentHandlerCalled", "true");
        }

        Assert.NotNull(eventArgs.ConnectPacket);
    }

    private static void OnConnAckReceivedHandler(object? sender, OnConnAckReceivedEventArgs eventArgs)
    {
        Assert.NotNull(sender);
        if (sender is not null)
        {
            var client = (HiveMQClient)sender;
            client.LocalStore.Add("OnConnAckReceivedHandlerCalled", "true");
        }

        Assert.NotNull(eventArgs.ConnAckPacket);
    }

    private static void AfterConnectHandler(object? sender, AfterConnectEventArgs eventArgs)
    {
        Assert.NotNull(sender);
        if (sender is not null)
        {
            var client = (HiveMQClient)sender;
            client.LocalStore.Add("AfterConnectHandlerCalled", "true");
        }

        Assert.NotNull(eventArgs.ConnectResult);
    }

    private static void BeforeDisconnectHandler(object? sender, BeforeDisconnectEventArgs eventArgs)
    {
        Assert.NotNull(sender);
        if (sender is not null)
        {
            var client = (HiveMQClient)sender;
            client.LocalStore.Add("BeforeDisconnectHandlerCalled", "true");
        }
    }

    private static void AfterDisconnectHandler(object? sender, AfterDisconnectEventArgs eventArgs)
    {
        Assert.NotNull(sender);
        if (sender is not null)
        {
            var client = (HiveMQClient)sender;

            if (client.LocalStore.TryGetValue("AfterDisconnectHandlerCalled", out var value))
            {
                var count = Convert.ToInt16(value, CultureInfo.InvariantCulture);
                count++;
                client.LocalStore.Add("AfterDisconnectHandlerCalledCount", count.ToString(CultureInfo.InvariantCulture));
            }
            else
            {
                client.LocalStore.Add("AfterDisconnectHandlerCalled", "true");
                client.LocalStore.Add("AfterDisconnectHandlerCalledCount", "1");
            }
        }
    }

    private static void OnDisconnectSentHandler(object? sender, OnDisconnectSentEventArgs eventArgs)
    {
        Assert.NotNull(sender);
        if (sender is not null)
        {
            var client = (HiveMQClient)sender;

            if (client.LocalStore.TryGetValue("OnDisconnectSentHandlerCalled", out var value))
            {
                var count = Convert.ToInt16(value, CultureInfo.InvariantCulture);
                count++;
                client.LocalStore.Add("OnDisconnectSentHandlerCalledCount", count.ToString(CultureInfo.InvariantCulture));
            }
            else
            {
                client.LocalStore.Add("OnDisconnectSentHandlerCalled", "true");
                client.LocalStore.Add("OnDisconnectSentHandlerCalledCount", "1");
            }
        }

        Assert.NotNull(eventArgs.DisconnectPacket);
    }

    private static void OnDisconnectReceivedHandler(object? sender, OnDisconnectReceivedEventArgs eventArgs)
    {
        Assert.NotNull(sender);
        if (sender is not null)
        {
            var client = (HiveMQClient)sender;
            client.LocalStore.Add("OnDisconnectReceivedHandlerCalled", "true");
        }

        Assert.NotNull(eventArgs.DisconnectPacket);
    }
}
