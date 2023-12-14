namespace HiveMQtt.Test.HiveMQClient;

using HiveMQtt.Client;
using Xunit;

public class ClientOptionsTest
{
    [Fact]
    public void Client_ID()
    {
        // var clientOptions = new HiveMQClientOptions();
        var client = new HiveMQClient();

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
}
