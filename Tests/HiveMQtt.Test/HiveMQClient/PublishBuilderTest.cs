namespace HiveMQtt.Test.HiveMQClient;

using System.Threading.Tasks;
using HiveMQtt.Client;
using HiveMQtt.MQTT5.ReasonCodes;
using HiveMQtt.MQTT5.Types;
using Xunit;

public class PublishBuilderTest
{

    private readonly byte[] payload = { 0x01, 0x02, 0x03 };

    [Fact]
    public async Task MostBasicPublishWithQoS0Async()
    {
        var options = new HiveMQClientOptionsBuilder().WithClientId("MostBasicPublishWithQoS0Async").Build();
        var client = new HiveMQClient(options);
        var connectResult = await client.ConnectAsync().ConfigureAwait(false);
        Assert.True(connectResult.ReasonCode == ConnAckReasonCode.Success);

        var message = new PublishMessageBuilder()
            .WithTopic("tests/MostBasicPublishWithQoS0Async")
            .WithPayload(this.payload)
            .WithQualityOfService(QualityOfService.AtMostOnceDelivery)
            .Build();

        var result = await client.PublishAsync(message).ConfigureAwait(false);
        Assert.IsType<Client.Results.PublishResult>(result);
        Assert.Null(result.QoS1ReasonCode);
        Assert.Null(result.QoS2ReasonCode);
        Assert.Equal(QualityOfService.AtMostOnceDelivery, result.Message.QoS);

        var disconnectResult = await client.DisconnectAsync().ConfigureAwait(false);
        Assert.True(disconnectResult);

        client.Dispose();
    }

    [Fact]
    public async Task MostBasicPublishWithQoS1Async()
    {
        var options = new HiveMQClientOptionsBuilder().WithClientId("MostBasicPublishWithQoS1Async").Build();
        var client = new HiveMQClient(options);
        var connectResult = await client.ConnectAsync().ConfigureAwait(false);
        Assert.True(connectResult.ReasonCode == ConnAckReasonCode.Success);

        var message = new PublishMessageBuilder()
            .WithTopic("tests/MostBasicPublishWithQoS1Async")
            .WithPayload(this.payload)
            .WithQualityOfService(QualityOfService.AtLeastOnceDelivery)
            .Build();
        var result = await client.PublishAsync(message).ConfigureAwait(false);
        Assert.IsType<Client.Results.PublishResult>(result);
        Assert.NotNull(result.QoS1ReasonCode);
        Assert.Null(result.QoS2ReasonCode);
        Assert.Equal(QualityOfService.AtLeastOnceDelivery, result.Message.QoS);

        var disconnectResult = await client.DisconnectAsync().ConfigureAwait(false);
        Assert.True(disconnectResult);

        client.Dispose();
    }

    [Fact]
    public async Task MostBasicPublishWithQoS2Async()
    {
        var options = new HiveMQClientOptionsBuilder().WithClientId("MostBasicPublishWithQoS2Async").Build();
        var client = new HiveMQClient(options);
        var connectResult = await client.ConnectAsync().ConfigureAwait(false);
        Assert.True(connectResult.ReasonCode == ConnAckReasonCode.Success);

        var message = new PublishMessageBuilder()
            .WithTopic("tests/MostBasicPublishWithQoS1Async")
            .WithPayload(this.payload)
            .WithQualityOfService(QualityOfService.ExactlyOnceDelivery)
            .Build();
        var result = await client.PublishAsync(message).ConfigureAwait(false);
        Assert.IsType<Client.Results.PublishResult>(result);
        Assert.NotNull(result.QoS2ReasonCode);
        Assert.Null(result.QoS1ReasonCode);
        Assert.Equal(QualityOfService.ExactlyOnceDelivery, result.Message.QoS);

        var disconnectResult = await client.DisconnectAsync().ConfigureAwait(false);
        Assert.True(disconnectResult);

        client.Dispose();
    }

    [Fact]
    public async Task MultiPublishWithQoS0Async()
    {
        var options = new HiveMQClientOptionsBuilder().WithClientId("MultiPublishWithQoS0Async").Build();
        var client = new HiveMQClient(options);
        var connectResult = await client.ConnectAsync().ConfigureAwait(false);
        Assert.True(connectResult.ReasonCode == ConnAckReasonCode.Success);

        var message = new PublishMessageBuilder()
            .WithTopic("tests/MultiPublishWithQoS0Async")
            .WithPayload(this.payload)
            .WithQualityOfService(QualityOfService.AtMostOnceDelivery)
            .Build();

        Client.Results.PublishResult result;

        for (var i = 1; i <= 10; i++)
        {
            result = await client.PublishAsync(message).ConfigureAwait(false);
            Assert.IsType<Client.Results.PublishResult>(result);
            Assert.Null(result.QoS1ReasonCode);
            Assert.Null(result.QoS2ReasonCode);
            Assert.Equal(QualityOfService.AtMostOnceDelivery, result.Message.QoS);
        }

        var disconnectResult = await client.DisconnectAsync().ConfigureAwait(false);
        Assert.True(disconnectResult);

        client.Dispose();
    }

    [Fact]
    public async Task MultiPublishWithQoS1Async()
    {
        var options = new HiveMQClientOptionsBuilder().WithClientId("MultiPublishWithQoS1Async").Build();
        var client = new HiveMQClient(options);
        var connectResult = await client.ConnectAsync().ConfigureAwait(false);
        Assert.True(connectResult.ReasonCode == ConnAckReasonCode.Success);

        var message = new PublishMessageBuilder()
            .WithTopic("tests/MultiPublishWithQoS1Async")
            .WithPayload(this.payload)
            .WithQualityOfService(QualityOfService.AtLeastOnceDelivery)
            .Build();
        Client.Results.PublishResult result;

        for (var i = 1; i <= 10; i++)
        {
            result = await client.PublishAsync(message).ConfigureAwait(false);
            Assert.IsType<Client.Results.PublishResult>(result);
            Assert.NotNull(result.QoS1ReasonCode);
            Assert.Equal(PubAckReasonCode.NoMatchingSubscribers, result?.QoS1ReasonCode);
        }

        var disconnectResult = await client.DisconnectAsync().ConfigureAwait(false);
        Assert.True(disconnectResult);

        client.Dispose();
    }

    [Fact]
    public async Task MultiPublishWithQoS2Async()
    {
        var options = new HiveMQClientOptionsBuilder().WithClientId("MultiPublishWithQoS2Async").Build();
        var client = new HiveMQClient(options);
        var connectResult = await client.ConnectAsync().ConfigureAwait(false);
        Assert.True(connectResult.ReasonCode == ConnAckReasonCode.Success);

        var message = new PublishMessageBuilder()
            .WithTopic("tests/MultiPublishWithQoS2Async")
            .WithPayload(this.payload)
            .WithQualityOfService(QualityOfService.ExactlyOnceDelivery)
            .Build();
        Client.Results.PublishResult result;

        for (var i = 1; i <= 10; i++)
        {
            result = await client.PublishAsync(message).ConfigureAwait(false);
            Assert.NotNull(result.QoS2ReasonCode);
            Assert.Equal(PubRecReasonCode.NoMatchingSubscribers, result?.QoS2ReasonCode);
        }

        var disconnectResult = await client.DisconnectAsync().ConfigureAwait(false);
        Assert.True(disconnectResult);

        client.Dispose();
    }

    [Fact]
    public async Task PublishWithOptionsAsync()
    {
        var options = new HiveMQClientOptionsBuilder().WithClientId("PublishWithOptionsAsync").Build();
        var client = new HiveMQClient(options);
        var connectResult = await client.ConnectAsync().ConfigureAwait(false);
        Assert.True(connectResult.ReasonCode == ConnAckReasonCode.Success);

        var message = new PublishMessageBuilder()
            .WithTopic("tests/PublishWithOptionsAsync")
            .WithPayload(this.payload)
            .WithQualityOfService(QualityOfService.AtMostOnceDelivery)
            .WithUserProperty("test", "test")
            .Build();

        var publishResult = await client.PublishAsync(message).ConfigureAwait(false);
        Assert.IsType<Client.Results.PublishResult>(publishResult);
        Assert.Null(publishResult.QoS1ReasonCode);
        Assert.Null(publishResult.QoS2ReasonCode);
        Assert.Equal(MQTT5.Types.QualityOfService.AtMostOnceDelivery, publishResult.Message.QoS);
        Assert.Equal("test", publishResult.Message.UserProperties["test"]);

        var disconnectResult = await client.DisconnectAsync().ConfigureAwait(false);
        Assert.True(disconnectResult);

        client.Dispose();
    }

    [Fact]
    public async Task PublishPayloadFormatIndicatorAsync()
    {
        var options = new HiveMQClientOptionsBuilder().WithClientId("PublishPayloadFormatIndicatorAsync").Build();
        var client = new HiveMQClient(options);
        var connectResult = await client.ConnectAsync().ConfigureAwait(false);
        Assert.True(connectResult.ReasonCode == ConnAckReasonCode.Success);

        var message = new PublishMessageBuilder()
            .WithTopic("tests/PublishPayloadFormatIndicatorAsync")
            .WithPayload(this.payload)
            .WithQualityOfService(QualityOfService.AtMostOnceDelivery)
            .WithPayloadFormatIndicator(MQTT5PayloadFormatIndicator.UTF8Encoded)
            .Build();

        var taskCompletionSource = new TaskCompletionSource<bool>();
        client.OnPublishSent += (sender, args) =>
        {
            Assert.Equal(MQTT5PayloadFormatIndicator.UTF8Encoded, args.PublishPacket.Message.PayloadFormatIndicator);
            taskCompletionSource.SetResult(true);
        };

        var result = await client.PublishAsync(message).ConfigureAwait(false);
        var eventResult = await taskCompletionSource.Task.ConfigureAwait(false);

        var disconnectResult = await client.DisconnectAsync().ConfigureAwait(false);
        Assert.True(disconnectResult);

        client.Dispose();
    }
}
