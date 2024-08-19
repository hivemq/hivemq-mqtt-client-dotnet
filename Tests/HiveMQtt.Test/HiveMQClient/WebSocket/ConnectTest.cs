namespace HiveMQtt.Test.HiveMQClient.WebSocket;

using System.Globalization;
using System.Threading.Tasks;
using HiveMQtt.Client;
using HiveMQtt.Client.Events;
using HiveMQtt.Client.Exceptions;
using HiveMQtt.Client.Options;
using HiveMQtt.MQTT5.ReasonCodes;
using Xunit;

public class ConnectTest
{
    [Fact]
    public async Task ConnectAndDisconnectAsync()
    {
        var options = new HiveMQClientOptionsBuilder()
            .WithWebSocketServer("ws://localhost:8080/mqtt")
            .WithClientId("test")
            .Build();

        var client = new HiveMQClient(options);
        await client.ConnectAsync().ConfigureAwait(false);

        Assert.True(client.IsConnected());

        await client.DisconnectAsync().ConfigureAwait(false);
    }
}
