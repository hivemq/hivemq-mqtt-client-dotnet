namespace HiveMQtt.Test.RawClient;

using System.Threading.Tasks;
using HiveMQtt.Client;
using HiveMQtt.Client.Events;
using HiveMQtt.Client.Exceptions;
using HiveMQtt.Client.Options;
using HiveMQtt.MQTT5.ReasonCodes;
using Xunit;

[Collection("Broker")]
public class RawClientTest
{
    [Fact]
    public async Task BasicConnectAndDisconnectAsync()
    {
        var options = new HiveMQClientOptionsBuilder().WithClientId("RawClientBasicConnectAndDisconnectAsync").Build();
        var client = new RawClient(options);
        Assert.NotNull(client);

        var connectResult = await client.ConnectAsync().ConfigureAwait(false);

        Assert.True(connectResult.ReasonCode == ConnAckReasonCode.Success);
        Assert.True(client.IsConnected());

        var disconnectOptions = new DisconnectOptions();
        Assert.Equal(DisconnectReasonCode.NormalDisconnection, disconnectOptions.ReasonCode);

        var disconnectResult = await client.DisconnectAsync(disconnectOptions).ConfigureAwait(false);
        Assert.True(disconnectResult);
        Assert.False(client.IsConnected());

        client.Dispose();
    }

    [Fact]
    public async Task RaiseOnFailureToConnectAsync()
    {
        // Bad port number
        var clientOptions = new HiveMQClientOptionsBuilder().WithPort(0).WithClientId("RawClientRaiseOnFailureToConnectAsync").Build();
        var client = new RawClient(clientOptions);

        await Assert.ThrowsAsync<HiveMQttClientException>(() => client.ConnectAsync()).ConfigureAwait(false);

        client.Dispose();
    }

    [Fact]
    public async Task TestConnectEventsAsync()
    {
        var options = new HiveMQClientOptionsBuilder().WithClientId("RawClientTestConnectEventsAsync").Build();
        var client = new RawClient(options);

        // Client Events
        client.BeforeConnect += BeforeConnectHandler;
        client.AfterConnect += AfterConnectHandler;
        client.BeforeDisconnect += BeforeDisconnectHandler;
        client.AfterDisconnect += AfterDisconnectHandler;

        // Packet Events
        client.OnConnectSent += OnConnectSentHandler;
        client.OnConnAckReceived += OnConnAckReceivedHandler;

        // Set up TaskCompletionSource to wait for event handlers to finish
        var disconnectSentSource = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var afterDisconnectSource = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

        client.OnDisconnectSent += (sender, args) => disconnectSentSource.TrySetResult(true);
        client.AfterDisconnect += (sender, args) => afterDisconnectSource.TrySetResult(true);

        // Connect and Disconnect
        var result = await client.ConnectAsync().ConfigureAwait(false);
        await client.DisconnectAsync().ConfigureAwait(false);

        // Wait for both disconnect events to complete instead of fixed delay
        await disconnectSentSource.Task.WaitAsync(TimeSpan.FromSeconds(5)).ConfigureAwait(false);
        await afterDisconnectSource.Task.WaitAsync(TimeSpan.FromSeconds(6)).ConfigureAwait(false);

        // Small delay to allow async event handlers to complete (they run via Task.Run)
        await Task.Delay(100).ConfigureAwait(false);

        // Assert that all Events were called
        Assert.True(client.LocalStore.TryGetValue("BeforeConnectHandlerCalled", out _));
        Assert.True(client.LocalStore.TryGetValue("AfterConnectHandlerCalled", out _));

        Assert.True(client.LocalStore.TryGetValue("BeforeDisconnectHandlerCalled", out _));
        Assert.True(client.LocalStore.TryGetValue("AfterDisconnectHandlerCalled", out _));

        Assert.True(client.LocalStore.TryGetValue("OnConnectSentHandlerCalled", out _));
        Assert.True(client.LocalStore.TryGetValue("OnConnAckReceivedHandlerCalled", out _));

        // Remove event handlers
        client.BeforeConnect -= BeforeConnectHandler;
        client.AfterConnect -= AfterConnectHandler;
        client.BeforeDisconnect -= BeforeDisconnectHandler;
        client.AfterDisconnect -= AfterDisconnectHandler;

        client.OnConnectSent -= OnConnectSentHandler;
        client.OnConnAckReceived -= OnConnAckReceivedHandler;

        client.Dispose();
    }

    [Fact]
    public async Task Test_AfterDisconnectEvent_Async()
    {
        var options = new HiveMQClientOptionsBuilder().WithClientId("RawClientTest_AfterDisconnectEvent_Async").Build();
        var client = new RawClient(options);

        // Client Events
        client.AfterDisconnect += AfterDisconnectHandler;

        // Set up TaskCompletionSource to wait for event handlers to finish
        var disconnectSentSource = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var afterDisconnectSource = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

        client.OnDisconnectSent += (sender, args) => disconnectSentSource.TrySetResult(true);
        client.AfterDisconnect += (sender, args) => afterDisconnectSource.TrySetResult(true);

        // Connect and Disconnect
        var result = await client.ConnectAsync().ConfigureAwait(false);
        await client.DisconnectAsync().ConfigureAwait(false);

        // Wait for both disconnect events to complete instead of fixed delay
        await disconnectSentSource.Task.WaitAsync(TimeSpan.FromSeconds(5)).ConfigureAwait(false);
        await afterDisconnectSource.Task.WaitAsync(TimeSpan.FromSeconds(6)).ConfigureAwait(false);

        // Small delay to allow async event handlers to complete (they run via Task.Run)
        await Task.Delay(100).ConfigureAwait(false);

        // Assert that all Events were called
        Assert.True(client.LocalStore.TryGetValue("AfterDisconnectHandlerCalled", out _));
        Assert.True(client.LocalStore.TryGetValue("AfterDisconnectHandlerCalledCount", out _));
        Assert.Equal("1", client.LocalStore["AfterDisconnectHandlerCalledCount"]);

        // Remove event handlers
        client.AfterDisconnect -= AfterDisconnectHandler;

        client.Dispose();
    }

    private static void BeforeConnectHandler(object? sender, BeforeConnectEventArgs eventArgs)
    {
        Assert.NotNull(sender);
        if (sender is not null)
        {
            var client = (RawClient)sender;
            lock (client.LocalStore)
            {
                client.LocalStore["BeforeConnectHandlerCalled"] = "true";
            }
        }

        Assert.NotNull(eventArgs.Options);
    }

    private static void OnConnectSentHandler(object? sender, OnConnectSentEventArgs eventArgs)
    {
        Assert.NotNull(sender);
        if (sender is not null)
        {
            var client = (RawClient)sender;
            lock (client.LocalStore)
            {
                client.LocalStore["OnConnectSentHandlerCalled"] = "true";
            }
        }

        Assert.NotNull(eventArgs.ConnectPacket);
    }

    private static void OnConnAckReceivedHandler(object? sender, OnConnAckReceivedEventArgs eventArgs)
    {
        Assert.NotNull(sender);
        if (sender is not null)
        {
            var client = (RawClient)sender;
            lock (client.LocalStore)
            {
                client.LocalStore["OnConnAckReceivedHandlerCalled"] = "true";
            }
        }

        Assert.NotNull(eventArgs.ConnAckPacket);
    }

    private static void AfterConnectHandler(object? sender, AfterConnectEventArgs eventArgs)
    {
        Assert.NotNull(sender);
        if (sender is not null)
        {
            var client = (RawClient)sender;
            lock (client.LocalStore)
            {
                client.LocalStore["AfterConnectHandlerCalled"] = "true";
            }
        }

        Assert.NotNull(eventArgs.ConnectResult);
    }

    private static void BeforeDisconnectHandler(object? sender, BeforeDisconnectEventArgs eventArgs)
    {
        Assert.NotNull(sender);
        if (sender is not null)
        {
            var client = (RawClient)sender;
            lock (client.LocalStore)
            {
                client.LocalStore["BeforeDisconnectHandlerCalled"] = "true";
            }
        }
    }

    private static void AfterDisconnectHandler(object? sender, AfterDisconnectEventArgs eventArgs)
    {
        Assert.NotNull(sender);
        if (sender is not null)
        {
            var client = (RawClient)sender;

            lock (client.LocalStore)
            {
                if (client.LocalStore.TryGetValue("AfterDisconnectHandlerCalled", out var value))
                {
                    var count = Convert.ToInt16(value, System.Globalization.CultureInfo.InvariantCulture);
                    count++;
                    client.LocalStore["AfterDisconnectHandlerCalledCount"] = count.ToString(System.Globalization.CultureInfo.InvariantCulture);
                }
                else
                {
                    client.LocalStore["AfterDisconnectHandlerCalled"] = "true";
                    client.LocalStore["AfterDisconnectHandlerCalledCount"] = "1";
                }
            }
        }
    }
}
