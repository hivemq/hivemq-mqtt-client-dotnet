namespace HiveMQtt.Test.HiveMQClient;

using System.Threading.Tasks;
using HiveMQtt.Client;
using HiveMQtt.Client.Exceptions;
using HiveMQtt.Client.Options;
using HiveMQtt.MQTT5.ReasonCodes;
using Xunit;

public class TLSTest
{
    [Fact(Skip = "Manual test for now.  Requires a broker with self-signed certificate.")]
    public async Task Invalid_TLS_Cert_Throws_Async()
    {
        var options = new HiveMQClientOptions
        {
            Host = "localhost",
            Port = 8883,
            AllowInvalidBrokerCertificates = false,
        };

        var client = new HiveMQClient(options);

        var exception = await Assert.ThrowsAsync<HiveMQttClientException>(client.ConnectAsync).ConfigureAwait(false);
        Assert.NotNull(exception);
    }

    [Fact(Skip = "Manual test for now.  Requires a broker with self-signed certificate.")]
    public async Task Allow_Invalid_TLS_Cert_Async()
    {
        var options = new HiveMQClientOptions
        {
            Host = "localhost",
            Port = 8883,
            AllowInvalidBrokerCertificates = true,
        };

        var client = new HiveMQClient(options);
        var connectResult = await client.ConnectAsync().ConfigureAwait(false);

        Assert.Equal(ConnAckReasonCode.Success, connectResult.ReasonCode);

        var disconnectResult = await client.DisconnectAsync().ConfigureAwait(false);
        Assert.True(disconnectResult);
    }

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
