namespace HiveMQtt.Test.HiveClient;

using HiveMQtt.Client;
using Xunit;

public class HiveClientTest
{
    [Fact]
    public void Client_Initializes_With_Defaults()
    {
        // var clientOptions = new HiveClientOptions();
        var client = new HiveClient();

        Assert.NotNull(client.Options.ClientId);
        Assert.True(client.Options.ClientId.Length < 24);
        Assert.Equal("127.0.0.1", client.Options.Host);
        Assert.Equal(1883, client.Options.Port);
        Assert.Equal(60, client.Options.KeepAlive);
        Assert.True(client.Options.CleanStart);
        Assert.Null(client.Options.UserName);
        Assert.Null(client.Options.Password);

        Assert.NotNull(client);

        client.Dispose();
    }

    [Fact]
    public async Task Client_Has_Default_Connect_Async()
    {
        var client = new HiveClient();
        var connectResult = await client.ConnectAsync().ConfigureAwait(false);

        Assert.NotNull(connectResult);
        Assert.True(connectResult.ReasonCode == MQTT5.ReasonCodes.ConnAckReasonCode.Success);

        var result = await client.DisconnectAsync().ConfigureAwait(false);

        Assert.True(result);
    }
}
