namespace HiveMQtt.Test.HiveMQClient;

using System.Text;
using System.Threading.Tasks;
using HiveMQtt.Client;
using HiveMQtt.MQTT5.ReasonCodes;
using HiveMQtt.MQTT5.Types;
using Xunit;

public class PublishTest
{
    [Fact]
    public async Task MostBasicPublishAsync()
    {
        var client = new HiveMQClient();
        var connectResult = await client.ConnectAsync().ConfigureAwait(false);
        Assert.True(connectResult.ReasonCode == ConnAckReasonCode.Success);

        var msg = new string(/*lang=json,strict*/ "{\"interference\": \"1029384\"}");
        var result = await client.PublishAsync("tests/MostBasicPublishAsync", msg).ConfigureAwait(false);

        var disconnectResult = await client.DisconnectAsync().ConfigureAwait(false);
        Assert.True(disconnectResult);
    }

    [Fact]
    public async Task MostBasicPublishWithQoS0Async()
    {
        var client = new HiveMQClient();
        var connectResult = await client.ConnectAsync().ConfigureAwait(false);
        Assert.True(connectResult.ReasonCode == ConnAckReasonCode.Success);

        var msg = new string(/*lang=json,strict*/ "{\"interference\": \"1029384\"}");
        var result = await client.PublishAsync("tests/MostBasicPublishWithQoS0Async", msg, MQTT5.Types.QualityOfService.AtMostOnceDelivery).ConfigureAwait(false);

        var disconnectResult = await client.DisconnectAsync().ConfigureAwait(false);
        Assert.True(disconnectResult);
    }

    [Fact]
    public async Task MostBasicPublishWithQoS1Async()
    {
        var client = new HiveMQClient();
        var connectResult = await client.ConnectAsync().ConfigureAwait(false);
        Assert.True(connectResult.ReasonCode == ConnAckReasonCode.Success);

        var msg = new string(/*lang=json,strict*/ "{\"interference\": \"1029384\"}");
        var result = await client.PublishAsync("tests/MostBasicPublishWithQoS1Async", msg, MQTT5.Types.QualityOfService.AtLeastOnceDelivery).ConfigureAwait(false);
        Assert.NotNull(result.QoS1ReasonCode);
        Assert.Equal(PubAckReasonCode.NoMatchingSubscribers, result?.QoS1ReasonCode);

        var disconnectResult = await client.DisconnectAsync().ConfigureAwait(false);
        Assert.True(disconnectResult);
    }

    [Fact]
    public async Task MostBasicPublishWithQoS2Async()
    {
        var client = new HiveMQClient();
        var connectResult = await client.ConnectAsync().ConfigureAwait(false);
        Assert.True(connectResult.ReasonCode == ConnAckReasonCode.Success);

        var msg = new string(/*lang=json,strict*/ "{\"interference\": \"1029384\"}");
        var result = await client.PublishAsync("tests/MostBasicPublishWithQoS2Async", msg, MQTT5.Types.QualityOfService.ExactlyOnceDelivery).ConfigureAwait(false);
        Assert.NotNull(result.QoS2ReasonCode);
        Assert.Equal(PubRecReasonCode.NoMatchingSubscribers, result?.QoS2ReasonCode);

        var disconnectResult = await client.DisconnectAsync().ConfigureAwait(false);
        Assert.True(disconnectResult);
    }

    [Fact]
    public async Task MultiPublishWithQoS0Async()
    {
        var client = new HiveMQClient();
        var connectResult = await client.ConnectAsync().ConfigureAwait(false);
        Assert.True(connectResult.ReasonCode == ConnAckReasonCode.Success);

        var msg = new string(/*lang=json,strict*/ "{\"interference\": \"1029384\"}");
        Client.Results.PublishResult result;

        for (var i = 1; i <= 10; i++)
        {
            result = await client.PublishAsync("tests/MultiPublishWithQoS0Async", msg, MQTT5.Types.QualityOfService.AtMostOnceDelivery).ConfigureAwait(false);
            Assert.IsType<Client.Results.PublishResult>(result);
            Assert.Null(result.QoS1ReasonCode);
            Assert.Null(result.QoS2ReasonCode);
            Assert.Equal(MQTT5.Types.QualityOfService.AtMostOnceDelivery, result.Message.QoS);
        }

        var disconnectResult = await client.DisconnectAsync().ConfigureAwait(false);
        Assert.True(disconnectResult);
    }

    [Fact]
    public async Task MultiPublishWithQoS1Async()
    {
        var client = new HiveMQClient();
        var connectResult = await client.ConnectAsync().ConfigureAwait(false);
        Assert.True(connectResult.ReasonCode == ConnAckReasonCode.Success);

        var msg = new string(/*lang=json,strict*/ "{\"interference\": \"1029384\"}");
        Client.Results.PublishResult result;

        for (var i = 1; i <= 10; i++)
        {
            result = await client.PublishAsync("tests/MultiPublishWithQoS1Async", msg, MQTT5.Types.QualityOfService.AtLeastOnceDelivery).ConfigureAwait(false);
            Assert.IsType<Client.Results.PublishResult>(result);
            Assert.NotNull(result.QoS1ReasonCode);
            Assert.Equal(PubAckReasonCode.NoMatchingSubscribers, result?.QoS1ReasonCode);
        }

        var disconnectResult = await client.DisconnectAsync().ConfigureAwait(false);
        Assert.True(disconnectResult);
    }

    [Fact]
    public async Task MultiPublishWithQoS2Async()
    {
        var client = new HiveMQClient();
        var connectResult = await client.ConnectAsync().ConfigureAwait(false);
        Assert.True(connectResult.ReasonCode == ConnAckReasonCode.Success);

        var msg = new string(/*lang=json,strict*/ "{\"interference\": \"1029384\"}");
        Client.Results.PublishResult result;

        for (var i = 1; i <= 10; i++)
        {
            result = await client.PublishAsync("tests/MultiPublishWithQoS2Async", msg, MQTT5.Types.QualityOfService.ExactlyOnceDelivery).ConfigureAwait(false);
            Assert.NotNull(result.QoS2ReasonCode);
            Assert.Equal(PubRecReasonCode.NoMatchingSubscribers, result?.QoS2ReasonCode);
        }

        var disconnectResult = await client.DisconnectAsync().ConfigureAwait(false);
        Assert.True(disconnectResult);
    }

    [Fact]
    public async Task PublishWithOptionsAsync()
    {
        var client = new HiveMQClient();
        var connectResult = await client.ConnectAsync().ConfigureAwait(false);
        Assert.True(connectResult.ReasonCode == ConnAckReasonCode.Success);

        var msg = new string(/*lang=json,strict*/ "{\"interference\": \"1029384\"}");
        var result = await client.PublishAsync("tests/PublishWithOptionsAsync", msg).ConfigureAwait(false);

        var disconnectResult = await client.DisconnectAsync().ConfigureAwait(false);
        Assert.True(disconnectResult);
    }

    [Fact]
    public async Task PublishPayloadFormatIndicatorAsync()
    {
        var client = new HiveMQClient();
        var connectResult = await client.ConnectAsync().ConfigureAwait(false);
        Assert.True(connectResult.ReasonCode == ConnAckReasonCode.Success);

        var msg = new MQTT5PublishMessage("tests/PublishPayloadFormatIndicatorAsync", QualityOfService.AtMostOnceDelivery)
        {
            PayloadFormatIndicator = MQTT5PayloadFormatIndicator.UTF8Encoded,
            Payload = Encoding.ASCII.GetBytes("blah"),
        };

        var taskCompletionSource = new TaskCompletionSource<bool>();
        client.OnPublishSent += (sender, args) =>
        {
            Assert.Equal(MQTT5PayloadFormatIndicator.UTF8Encoded, args.PublishPacket.Message.PayloadFormatIndicator);
            taskCompletionSource.SetResult(true);
        };

        var result = await client.PublishAsync(msg).ConfigureAwait(false);
        var eventResult = await taskCompletionSource.Task.ConfigureAwait(false);

        var disconnectResult = await client.DisconnectAsync().ConfigureAwait(false);
        Assert.True(disconnectResult);
    }
}
