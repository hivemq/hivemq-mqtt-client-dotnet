namespace HiveMQtt.Test.HiveClient;

using System.Threading.Tasks;
using HiveMQtt.Client;
using HiveMQtt.MQTT5.Connect;
using Xunit;

public class HiveClientPubSubTest
{
    [Fact]
    public async Task Subscribe()
    {
        var subClient = new HiveClient();
        var connectResult = await subClient.ConnectAsync().ConfigureAwait(false);
        Assert.True(connectResult.ReasonCode == ConnAckReasonCode.Success);

        var subResult = await subClient.SubscribeAsync("data/topic").ConfigureAwait(false);


        var disconnectResult = await subClient.DisconnectAsync().ConfigureAwait(false);
        Assert.True(disconnectResult);
    }
}
