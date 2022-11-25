namespace HiveMQtt.Test;

using HiveMQtt;
using Xunit;

public class ClientTest
{
    [Fact]
    public void Client_Initializes_With_Defaults()
    {
        // var clientOptions = new ClientOptions();
        var client = new Client();

        Assert.NotNull(client.Options.ClientId);
        Assert.True(client.Options.ClientId.Length < 24);
        Assert.Equal("127.0.0.1", client.Options.Host);
        Assert.Equal(1883, client.Options.Port);
        Assert.Equal(60, client.Options.KeepAlive);
        Assert.True(client.Options.CleanStart);
        Assert.Null(client.Options.UserName);
        Assert.Null(client.Options.Password);

        Assert.NotNull(client);
    }

    [Fact]
    public async void ClientDefaultConnect()
    {
        var client = new Client();
        var result = await client.ConnectAsync().ConfigureAwait(false);

        Assert.NotNull(result);
    }
}
