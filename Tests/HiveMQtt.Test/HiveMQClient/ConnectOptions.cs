namespace HiveMQtt.Test.HiveMQClient;

using System.Threading.Tasks;
using HiveMQtt.Client;
using HiveMQtt.Client.Options;
using HiveMQtt.MQTT5.ReasonCodes;
using Xunit;

public class ConnectOptionsTest
{
    [Fact]
    public async Task Clean_Start_Async()
    {
        var options = new HiveMQClientOptions
        {
            CleanStart = false,
        };

        var client = new HiveMQClient(options);
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
        var options = new HiveMQClientOptions
        {
            KeepAlive = 93,
        };

        var client = new HiveMQClient(options);
        Assert.Equal(93, client.Options.KeepAlive);

        var connectResult = await client.ConnectAsync().ConfigureAwait(false);

        Assert.True(connectResult.ReasonCode == ConnAckReasonCode.Success);

        var disconnectResult = await client.DisconnectAsync().ConfigureAwait(false);
        Assert.True(disconnectResult);
    }

    [Fact]
    public async Task Custom_Session_Expiry_Async()
    {
        var options = new HiveMQClientOptions
        {
            SessionExpiryInterval = 89,
        };

        var client = new HiveMQClient(options);
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
        var options = new HiveMQClientOptions
        {
            ClientReceiveMaximum = 5,
        };

        var client = new HiveMQClient(options);
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
        var options = new HiveMQClientOptions
        {
            ClientMaximumPacketSize = 1500,
        };

        var client = new HiveMQClient(options);
        Assert.Equal(1500, client.Options.ClientMaximumPacketSize);

        var connectResult = await client.ConnectAsync().ConfigureAwait(false);

        Assert.True(connectResult.ReasonCode == ConnAckReasonCode.Success);

        var disconnectResult = await client.DisconnectAsync().ConfigureAwait(false);
        Assert.True(disconnectResult);
    }

    [Fact]
    public async Task Custom_Topic_Alias_Maximum_Async()
    {
        var options = new HiveMQClientOptions
        {
            ClientTopicAliasMaximum = 5,
        };

        var client = new HiveMQClient(options);
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
        var options = new HiveMQClientOptions
        {
            RequestResponseInformation = true,
        };

        var client = new HiveMQClient(options);
        Assert.Equal(true, client.Options.RequestResponseInformation);

        var connectResult = await client.ConnectAsync().ConfigureAwait(false);
        Assert.True(connectResult.ReasonCode == ConnAckReasonCode.Success);

        // FIXME: Broker doesn't return ResponseInformation for successful connections
        // Make a better test with a failure scenario
        Assert.Null(connectResult.ResponseInformation);

        var disconnectResult = await client.DisconnectAsync().ConfigureAwait(false);
        Assert.True(disconnectResult);
    }
}
