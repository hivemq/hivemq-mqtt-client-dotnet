namespace HiveMQtt.Test.HiveMQClient;

using System.Threading.Tasks;
using HiveMQtt.Client;
using HiveMQtt.Client.Options;
using HiveMQtt.MQTT5.ReasonCodes;
using Xunit;

public class TLSTest
{
    [Fact]
    public async Task Public_Broker_TLS_Async()
    {
        var options = new HiveMQClientOptions
        {
            Host = "broker.hivemq.com",
            Port = 8883,
        };

        var client = new HiveMQClient(options);

        var connectResult = await client.ConnectAsync().ConfigureAwait(false);

        Assert.Equal(ConnAckReasonCode.Success, connectResult.ReasonCode);

        var disconnectResult = await client.DisconnectAsync().ConfigureAwait(false);
        Assert.True(disconnectResult);
    }
}
