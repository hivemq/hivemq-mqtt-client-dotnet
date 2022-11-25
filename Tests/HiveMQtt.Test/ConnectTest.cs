namespace HiveMQtt.Test;

using System.Threading;
using System.Threading.Tasks;
using HiveMQtt;
using Xunit;

public class ConnectTest
{
    [Fact]
    public async Task Basic_ConnectAsync()
    {
        var client = new Client();
        Assert.NotNull(client);

        await client.ConnectAsync().ConfigureAwait(false);
        Assert.True(client.IsConnected());
        Thread.Sleep(3000);
    }
}
