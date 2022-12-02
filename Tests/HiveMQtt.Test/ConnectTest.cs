namespace HiveMQtt.Test;

using System.Threading;
using System.Threading.Tasks;
using HiveMQtt.Client;
using Xunit;

public class ConnectTest
{
    [Fact]
    public async Task Basic_ConnectAsync()
    {
        var client = new HiveClient();
        Assert.NotNull(client);

        await client.ConnectAsync().ConfigureAwait(false);
        Assert.True(client.IsConnected());
        Thread.Sleep(3000);
    }
}
