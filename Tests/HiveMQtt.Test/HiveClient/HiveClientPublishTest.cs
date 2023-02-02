namespace HiveMQtt.Test.HiveClient;

using System.Threading.Tasks;
using HiveMQtt.Client;
using HiveMQtt.Client.ReasonCodes;
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

        var msg = new string(/*lang=json,strict*/ "{\"interference\": \"1029384\"}");
        var result = await client.PublishAsync("data/topic", msg).ConfigureAwait(false);

        var disconnectResult = await client.DisconnectAsync().ConfigureAwait(false);
        Assert.True(disconnectResult);
    }

    [Fact]
    public async Task MostBasicPublishWithQoS0Async()
    {
        var client = new HiveClient();
        var connectResult = await client.ConnectAsync().ConfigureAwait(false);
        Assert.True(connectResult.ReasonCode == ConnAckReasonCode.Success);

        var msg = new string(/*lang=json,strict*/ "{\"interference\": \"1029384\"}");
        var result = await client.PublishAsync("data/topic", msg, MQTT5.Types.QualityOfService.AtMostOnceDelivery).ConfigureAwait(false);

        var disconnectResult = await client.DisconnectAsync().ConfigureAwait(false);
        Assert.True(disconnectResult);
    }

    [Fact]
    public async Task MostBasicPublishWithQoS1Async()
    {
        var client = new HiveClient();
        var connectResult = await client.ConnectAsync().ConfigureAwait(false);
        Assert.True(connectResult.ReasonCode == ConnAckReasonCode.Success);

        var msg = new string(/*lang=json,strict*/ "{\"interference\": \"1029384\"}");
        var result = await client.PublishAsync("data/topic", msg, MQTT5.Types.QualityOfService.AtLeastOnceDelivery).ConfigureAwait(false);
        Assert.Equal((int)QoS1ReasonCode.Success, (int)result.QoS1ReasonCode);

        var disconnectResult = await client.DisconnectAsync().ConfigureAwait(false);
        Assert.True(disconnectResult);
    }

    [Fact]
    public async Task MostBasicPublishWithQoS2Async()
    {
        var client = new HiveClient();
        var connectResult = await client.ConnectAsync().ConfigureAwait(false);
        Assert.True(connectResult.ReasonCode == ConnAckReasonCode.Success);

        var msg = new string(/*lang=json,strict*/ "{\"interference\": \"1029384\"}");
        var result = await client.PublishAsync("data/topic", msg, MQTT5.Types.QualityOfService.ExactlyOnceDelivery).ConfigureAwait(false);
        Assert.Equal((int)QoS2ReasonCode.Success, (int)result.QoS2ReasonCode);

        var disconnectResult = await client.DisconnectAsync().ConfigureAwait(false);
        Assert.True(disconnectResult);
    }

    [Fact]
    public async Task PublishWithOptionsAsync()
    {
        var client = new HiveClient();
        var connectResult = await client.ConnectAsync().ConfigureAwait(false);
        Assert.True(connectResult.ReasonCode == ConnAckReasonCode.Success);

        var msg = new string(/*lang=json,strict*/ "{\"interference\": \"1029384\"}");
        var result = await client.PublishAsync("data/topic", msg).ConfigureAwait(false);

        var disconnectResult = await client.DisconnectAsync().ConfigureAwait(false);
        Assert.True(disconnectResult);
    }
}
