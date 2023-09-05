namespace HiveMQtt.Test.HiveMQClient;

using System.Threading.Tasks;
using HiveMQtt.Client;
using HiveMQtt.Client.Options;
using HiveMQtt.MQTT5.ReasonCodes;
using HiveMQtt.MQTT5.Types;
using Xunit;

public class HiveMQClientLWTTest
{
    [Fact]
    public async Task Basic_Last_Will_Async()
    {
        var options = new HiveMQClientOptions
        {
            LastWillAndTestament = new LastWillAndTestament("last/will", QualityOfService.AtLeastOnceDelivery, "last will message"),
        };

        var client = new HiveMQClient(options);

        var connectResult = await client.ConnectAsync().ConfigureAwait(false);
        Assert.True(connectResult.ReasonCode == ConnAckReasonCode.Success);
        Assert.True(client.IsConnected());
    }

    [Fact]
    public async Task Last_Will_With_Properties_Async()
    {
        var options = new HiveMQClientOptions
        {
            LastWillAndTestament = new LastWillAndTestament("last/will", QualityOfService.AtLeastOnceDelivery, "last will message"),
        };

        options.LastWillAndTestament.WillDelayInterval = 1;
        options.LastWillAndTestament.PayloadFormatIndicator = 1;
        options.LastWillAndTestament.MessageExpiryInterval = 100;
        options.LastWillAndTestament.ContentType = "application/json";
        options.LastWillAndTestament.ResponseTopic = "response/topic";
        options.LastWillAndTestament.CorrelationData = new byte[] { 1, 2, 3, 4, 5 };
        options.LastWillAndTestament.UserProperties.Add("userProperty", "userProperty");

        // Connect a client to listen for LWT
        var listenerClient = new HiveMQClient();
        var connectResult = await listenerClient.ConnectAsync().ConfigureAwait(false);
        Assert.True(connectResult.ReasonCode == ConnAckReasonCode.Success);
        Assert.True(listenerClient.IsConnected());

        var result = await listenerClient.SubscribeAsync("last/will", QualityOfService.AtLeastOnceDelivery).ConfigureAwait(false);
        // Assert.True(result.ReasonCode == SubAckReasonCode.GrantedQoS1);

        // Connect the client with LWT
        var client = new HiveMQClient(options);
        connectResult = await client.ConnectAsync().ConfigureAwait(false);
        Assert.True(connectResult.ReasonCode == ConnAckReasonCode.Success);
        Assert.True(client.IsConnected());
    }
}
