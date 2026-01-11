namespace HiveMQtt.Test.RawClient;

using System.Threading.Tasks;
using HiveMQtt.Client;
using HiveMQtt.Client.Options;
using HiveMQtt.MQTT5.ReasonCodes;
using Xunit;

public class RawClientConnectDisconnectTest
{
    [Fact]
    public async Task ConnectWithConnectOptionsOverrideAsync()
    {
        var options = new HiveMQClientOptionsBuilder()
            .WithClientId("RawClientConnectWithConnectOptionsOverrideAsync")
            .WithKeepAlive(60)
            .WithSessionExpiryInterval(300)
            .WithCleanStart(true)
            .Build();

        var client = new RawClient(options);

        // Override with ConnectOptions
        var connectOptions = new ConnectOptions
        {
            KeepAlive = 120,
            SessionExpiryInterval = 600,
            CleanStart = false,
        };

        var connectResult = await client.ConnectAsync(connectOptions).ConfigureAwait(false);
        Assert.True(connectResult.ReasonCode == ConnAckReasonCode.Success);

        // Verify options were overridden
        Assert.Equal(120, client.Options.KeepAlive);
        Assert.Equal(600, client.Options.SessionExpiryInterval);
        Assert.False(client.Options.CleanStart);

        await client.DisconnectAsync().ConfigureAwait(false);
        client.Dispose();
    }

    [Fact]
    public async Task ConnectWithPartialConnectOptionsOverrideAsync()
    {
        var options = new HiveMQClientOptionsBuilder()
            .WithClientId("RawClientConnectWithPartialConnectOptionsOverrideAsync")
            .WithKeepAlive(60)
            .WithSessionExpiryInterval(300)
            .Build();

        var client = new RawClient(options);

        // Override only KeepAlive
        var connectOptions = new ConnectOptions
        {
            KeepAlive = 90,
        };

        var connectResult = await client.ConnectAsync(connectOptions).ConfigureAwait(false);
        Assert.True(connectResult.ReasonCode == ConnAckReasonCode.Success);

        // Verify only KeepAlive was overridden
        Assert.Equal(90, client.Options.KeepAlive);
        Assert.Equal(300, client.Options.SessionExpiryInterval); // Should remain unchanged

        await client.DisconnectAsync().ConfigureAwait(false);
        client.Dispose();
    }

    [Fact]
    public async Task DisconnectWithDisconnectOptionsAsync()
    {
        var options = new HiveMQClientOptionsBuilder().WithClientId("RawClientDisconnectWithDisconnectOptionsAsync").Build();
        var client = new RawClient(options);

        var connectResult = await client.ConnectAsync().ConfigureAwait(false);
        Assert.True(connectResult.ReasonCode == ConnAckReasonCode.Success);

        var disconnectOptions = new DisconnectOptions
        {
            ReasonCode = DisconnectReasonCode.DisconnectWithWillMessage,
        };

        var disconnectResult = await client.DisconnectAsync(disconnectOptions).ConfigureAwait(false);
        Assert.True(disconnectResult);
        Assert.False(client.IsConnected());

        client.Dispose();
    }

    [Fact]
    public async Task DisconnectWhenNotConnectedAsync()
    {
        var options = new HiveMQClientOptionsBuilder().WithClientId("RawClientDisconnectWhenNotConnectedAsync").Build();
        var client = new RawClient(options);

        // Try to disconnect when not connected
        var disconnectResult = await client.DisconnectAsync().ConfigureAwait(false);
        Assert.False(disconnectResult); // Should return false, not throw

        client.Dispose();
    }

    [Fact]
    public async Task DisconnectWhenAlreadyDisconnectingAsync()
    {
        var options = new HiveMQClientOptionsBuilder().WithClientId("RawClientDisconnectWhenAlreadyDisconnectingAsync").Build();
        var client = new RawClient(options);

        var connectResult = await client.ConnectAsync().ConfigureAwait(false);
        Assert.True(connectResult.ReasonCode == ConnAckReasonCode.Success);

        // Start two disconnect operations concurrently
        var disconnectTask1 = client.DisconnectAsync();
        var disconnectTask2 = client.DisconnectAsync();

        var results = await Task.WhenAll(disconnectTask1, disconnectTask2).ConfigureAwait(false);

        // Both should complete successfully (second one should return true immediately)
        Assert.All(results, result => Assert.True(result));

        client.Dispose();
    }

    [Fact]
    public async Task IsConnectedInVariousStatesAsync()
    {
        var options = new HiveMQClientOptionsBuilder().WithClientId("RawClientIsConnectedInVariousStatesAsync").Build();
        var client = new RawClient(options);

        // Initially not connected
        Assert.False(client.IsConnected());

        // After connect
        var connectResult = await client.ConnectAsync().ConfigureAwait(false);
        Assert.True(connectResult.ReasonCode == ConnAckReasonCode.Success);
        Assert.True(client.IsConnected());

        // After disconnect
        await client.DisconnectAsync().ConfigureAwait(false);
        Assert.False(client.IsConnected());

        client.Dispose();
    }

    [Fact]
    public async Task MultipleConnectDisconnectCyclesAsync()
    {
        var options = new HiveMQClientOptionsBuilder().WithClientId("RawClientMultipleConnectDisconnectCyclesAsync").Build();
        var client = new RawClient(options);

        // Perform multiple connect/disconnect cycles
        for (var i = 0; i < 3; i++)
        {
            var connectResult = await client.ConnectAsync().ConfigureAwait(false);
            Assert.True(connectResult.ReasonCode == ConnAckReasonCode.Success);
            Assert.True(client.IsConnected());

            var disconnectResult = await client.DisconnectAsync().ConfigureAwait(false);
            Assert.True(disconnectResult);
            Assert.False(client.IsConnected());
        }

        client.Dispose();
    }
}
