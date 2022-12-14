namespace HiveMQtt.Test.HiveClient;

using System.Threading.Tasks;
using HiveMQtt.Client;
using HiveMQtt.Client.Options;
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

    // FIXME: Add Connect failure tests: non existent brokers, bad SSL etc...
}
