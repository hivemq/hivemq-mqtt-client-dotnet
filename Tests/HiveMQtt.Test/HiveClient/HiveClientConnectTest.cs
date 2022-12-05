namespace HiveMQtt.Test.HiveClient;

using System.Threading.Tasks;
using HiveMQtt.Client;
using HiveMQtt.Client.Disconnect;
using HiveMQtt.MQTT5;
using HiveMQtt.MQTT5.Connect;
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
    public async Task Connect_With_Clean_Start_Async()
    {
        var options = new HiveClientOptions
        {
            CleanStart = false,
        };

        var client = new HiveClient(options);
        Assert.NotNull(client);

        var connectResult = await client.ConnectAsync().ConfigureAwait(false);
        Assert.True(client.IsConnected());
        Assert.True(connectResult.ReasonCode == ConnAckReasonCode.Success);

        // We have no previous session...
        Assert.False(connectResult.SessionPresent);

        var disconnectResult = await client.DisconnectAsync().ConfigureAwait(false);
        Assert.True(disconnectResult);
    }

    [Fact]
    public async Task Connect_With_Custom_Keep_Alive_Async()
    {
        var options = new HiveClientOptions
        {
            KeepAlive = 93,
        };

        var client = new HiveClient(options);
        Assert.NotNull(client);

        var connectResult = await client.ConnectAsync().ConfigureAwait(false);
        Assert.True(client.IsConnected());
        Assert.True(connectResult.ReasonCode == ConnAckReasonCode.Success);

        Assert.True(connectResult.Properties.ServerKeepAlive == 93);

        var disconnectResult = await client.DisconnectAsync().ConfigureAwait(false);
        Assert.True(disconnectResult);
    }
}
