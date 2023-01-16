namespace HiveMQtt.Test.HiveClient;

using System.Threading.Tasks;
using HiveMQtt.Client;
using HiveMQtt.MQTT5.ReasonCodes;
using Xunit;

public class HiveClientPublishTest
{
    [Fact]
    public async Task MostBasicPublishAsync()
    {
        var client = new HiveClient();
        var connectResult = await client.ConnectAsync().ConfigureAwait(false);
        Assert.True(connectResult.ReasonCode == ConnAckReasonCode.Success);

        var msg = new string(/*lang=json,strict*/ "{\"interferance\": \"1029384\"}");
        var result = await client.PublishAsync("data/topic", msg).ConfigureAwait(false);

        // TODO: Add a way to check if the message was received on the topic
        var disconnectResult = await client.DisconnectAsync().ConfigureAwait(false);
        Assert.True(disconnectResult);
    }

    [Fact]
    public async Task MostBasicPublishWithQoS0Async()
    {
        var client = new HiveClient();
        var connectResult = await client.ConnectAsync().ConfigureAwait(false);
        Assert.True(connectResult.ReasonCode == ConnAckReasonCode.Success);

        var msg = new string(/*lang=json,strict*/ "{\"interferance\": \"1029384\"}");
        var result = await client.PublishAsync("data/topic", msg, MQTT5.Types.QualityOfService.AtMostOnceDelivery).ConfigureAwait(false);

        // TODO: Add a way to check if the message was received on the topic
        var disconnectResult = await client.DisconnectAsync().ConfigureAwait(false);
        Assert.True(disconnectResult);
    }

    [Fact]
    public async Task PublishWithOptionsAsync()
    {
        var client = new HiveClient();
        var connectResult = await client.ConnectAsync().ConfigureAwait(false);
        Assert.True(connectResult.ReasonCode == ConnAckReasonCode.Success);

        var msg = new string(/*lang=json,strict*/ "{\"interferance\": \"1029384\"}");
        var result = await client.PublishAsync("data/topic", msg).ConfigureAwait(false);

        // TODO: Add a way to check if the message was received on the topic
        var disconnectResult = await client.DisconnectAsync().ConfigureAwait(false);
        Assert.True(disconnectResult);
    }
}
