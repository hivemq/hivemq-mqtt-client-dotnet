namespace HiveMQtt.Test.HiveClient;

using System.Threading.Tasks;
using HiveMQtt.Client;
using HiveMQtt.Client.Disconnect;
using HiveMQtt.MQTT5.Connect;
using Xunit;

public class HiveClientConnectOptionsTest
{
    [Fact]
    public async Task Clean_Start_Async()
    {
        var options = new HiveClientOptions
        {
            CleanStart = false,
        };

        var client = new HiveClient(options);
        var connectResult = await client.ConnectAsync().ConfigureAwait(false);

        Assert.True(connectResult.ReasonCode == ConnAckReasonCode.Success);

        // We have no previous session...
        Assert.False(connectResult.SessionPresent);

        var disconnectResult = await client.DisconnectAsync().ConfigureAwait(false);
        Assert.True(disconnectResult);
    }

    [Fact]
    public async Task Custom_Keep_Alive_Async()
    {
        var options = new HiveClientOptions
        {
            KeepAlive = 93,
        };

        var client = new HiveClient(options);
        Assert.Equal(93, client.Options.KeepAlive);

        var connectResult = await client.ConnectAsync().ConfigureAwait(false);

        Assert.True(connectResult.ReasonCode == ConnAckReasonCode.Success);
        Assert.Equal(93, connectResult.ServerKeepAlive);

        var disconnectResult = await client.DisconnectAsync().ConfigureAwait(false);
        Assert.True(disconnectResult);
    }

    [Fact]
    public async Task Custom_Session_Expiry_Async()
    {
        var options = new HiveClientOptions
        {
            SessionExpiryInterval = 89,
        };

        var client = new HiveClient(options);
        Assert.Equal(89, client.Options.SessionExpiryInterval);

        var connectResult = await client.ConnectAsync().ConfigureAwait(false);

        Assert.True(connectResult.ReasonCode == ConnAckReasonCode.Success);
        Assert.Equal(89, connectResult.SessionExpiryInterval);

        var disconnectResult = await client.DisconnectAsync().ConfigureAwait(false);
        Assert.True(disconnectResult);
    }

    [Fact]
    public async Task Custom_Receive_Maximum_Async()
    {
        var options = new HiveClientOptions
        {
            ClientReceiveMaximum = 5,
        };

        var client = new HiveClient(options);
        Assert.Equal(5, client.Options.ClientReceiveMaximum);

        var connectResult = await client.ConnectAsync().ConfigureAwait(false);

        Assert.True(connectResult.ReasonCode == ConnAckReasonCode.Success);
        Assert.Equal(10, connectResult.BrokerReceiveMaximum);

        var disconnectResult = await client.DisconnectAsync().ConfigureAwait(false);
        Assert.True(disconnectResult);
    }

    [Fact]
    public async Task Custom_Maximum_Packet_Size_Async()
    {
        var options = new HiveClientOptions
        {
            ClientMaximumPacketSize = 5,
        };

        var client = new HiveClient(options);
        Assert.Equal(5, client.Options.ClientMaximumPacketSize);

        var connectResult = await client.ConnectAsync().ConfigureAwait(false);

        Assert.True(connectResult.ReasonCode == ConnAckReasonCode.Success);
        Assert.Equal(10, connectResult.BrokerMaximumPacketSize);

        var disconnectResult = await client.DisconnectAsync().ConfigureAwait(false);
        Assert.True(disconnectResult);
    }

    [Fact]
    public async Task Custom_Topic_Alias_Maximum_Async()
    {
        var options = new HiveClientOptions
        {
            ClientTopicAliasMaximum = 5,
        };

        var client = new HiveClient(options);
        Assert.Equal(5, client.Options.ClientTopicAliasMaximum);

        var connectResult = await client.ConnectAsync().ConfigureAwait(false);

        Assert.True(connectResult.ReasonCode == ConnAckReasonCode.Success);
        Assert.Equal(5, connectResult.BrokerTopicAliasMaximum);

        var disconnectResult = await client.DisconnectAsync().ConfigureAwait(false);
        Assert.True(disconnectResult);
    }

    [Fact]
    public async Task Request_Response_Information_Async()
    {
        var options = new HiveClientOptions
        {
            RequestResponseInformation = true,
        };

        var client = new HiveClient(options);
        Assert.Equal(true, client.Options.RequestResponseInformation);

        var connectResult = await client.ConnectAsync().ConfigureAwait(false);
        Assert.True(connectResult.ReasonCode == ConnAckReasonCode.Success);

        // FIXME: Broker doesn't return ResponseInformation for successful connections
        // Make a better test with a failure scenario
        Assert.Null(connectResult.ResponseInformation);

        var disconnectResult = await client.DisconnectAsync().ConfigureAwait(false);
        Assert.True(disconnectResult);
    }

    [Fact]
    public async Task Request_Problem_Information_Async()
    {
        var options = new HiveClientOptions
        {
            RequestProblemInformation = true,
        };

        var client = new HiveClient(options);
        Assert.Equal(true, client.Options.RequestProblemInformation);

        var connectResult = await client.ConnectAsync().ConfigureAwait(false);
        Assert.True(connectResult.ReasonCode == ConnAckReasonCode.Success);

        // FIXME: Broker doesn't return ReasonString for successful connections
        // Make a better test with a failure scenario
        Assert.NotNull(connectResult.ReasonString);

        var disconnectResult = await client.DisconnectAsync().ConfigureAwait(false);
        Assert.True(disconnectResult);
    }

    // FIXME: Add Authentication Tests
}
