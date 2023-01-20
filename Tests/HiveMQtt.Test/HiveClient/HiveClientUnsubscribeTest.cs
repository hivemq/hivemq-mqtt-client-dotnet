namespace HiveMQtt.Test.HiveClient;

using System.Threading.Tasks;
using HiveMQtt.Client;
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
}
