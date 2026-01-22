namespace HiveMQtt.Test.RawClient;

using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HiveMQtt.Client;
using HiveMQtt.Client.Exceptions;
using HiveMQtt.MQTT5.ReasonCodes;
using HiveMQtt.MQTT5.Types;
using Xunit;

public class RawClientPublishTest
{
    [Fact]
    public async Task PublishWithMQTT5PublishMessageAsync()
    {
        var testTopic = "tests/RawClientPublishWithMQTT5PublishMessageAsync";
        var testPayload = Encoding.UTF8.GetBytes("test payload");
        var options = new HiveMQClientOptionsBuilder().WithClientId("RawClientPublishWithMQTT5PublishMessageAsync").Build();
        var client = new RawClient(options);

        var connectResult = await client.ConnectAsync().ConfigureAwait(false);
        Assert.True(connectResult.ReasonCode == ConnAckReasonCode.Success);

        var message = new MQTT5PublishMessage
        {
            Topic = testTopic,
            Payload = testPayload,
            QoS = QualityOfService.AtMostOnceDelivery,
        };

        var result = await client.PublishAsync(message).ConfigureAwait(false);
        Assert.NotNull(result);
        Assert.NotNull(result.Message);
        Assert.Equal(testTopic, result.Message.Topic);
        Assert.Equal(testPayload, result.Message.Payload);

        await client.DisconnectAsync().ConfigureAwait(false);
        client.Dispose();
    }

    [Fact]
    public async Task PublishWithCancellationTokenAsync()
    {
        var testTopic = "tests/RawClientPublishWithCancellationTokenAsync";
        var testPayload = "test payload";
        var options = new HiveMQClientOptionsBuilder().WithClientId("RawClientPublishWithCancellationTokenAsync").Build();
        var client = new RawClient(options);

        var connectResult = await client.ConnectAsync().ConfigureAwait(false);
        Assert.True(connectResult.ReasonCode == ConnAckReasonCode.Success);

        var message = new MQTT5PublishMessage
        {
            Topic = testTopic,
            Payload = Encoding.UTF8.GetBytes(testPayload),
            QoS = QualityOfService.AtLeastOnceDelivery, // QoS 1 to test cancellation during handshake
        };

        var cts = new CancellationTokenSource();
        cts.Cancel();

        // TaskCanceledException derives from OperationCanceledException, but xUnit's Assert.ThrowsAsync
        // is strict about the exact type. We'll catch the actual exception type.
        var exception = await Assert.ThrowsAnyAsync<OperationCanceledException>(() => client.PublishAsync(message, cts.Token)).ConfigureAwait(false);
        Assert.NotNull(exception);

        await client.DisconnectAsync().ConfigureAwait(false);
        client.Dispose();
    }

    [Fact]
    public async Task PublishWithRetainFlagAsync()
    {
        var testTopic = "tests/RawClientPublishWithRetainFlagAsync";
        var testPayload = "retained message";
        var options = new HiveMQClientOptionsBuilder().WithClientId("RawClientPublishWithRetainFlagAsync").Build();
        var client = new RawClient(options);

        var connectResult = await client.ConnectAsync().ConfigureAwait(false);
        Assert.True(connectResult.ReasonCode == ConnAckReasonCode.Success);

        var message = new MQTT5PublishMessage
        {
            Topic = testTopic,
            Payload = Encoding.UTF8.GetBytes(testPayload),
            QoS = QualityOfService.AtMostOnceDelivery,
            Retain = true,
        };

        var result = await client.PublishAsync(message).ConfigureAwait(false);
        Assert.NotNull(result);
        Assert.True(result.Message.Retain);

        await client.DisconnectAsync().ConfigureAwait(false);
        client.Dispose();
    }

    [Fact]
    public async Task PublishWhenNotConnectedAsync()
    {
        var options = new HiveMQClientOptionsBuilder().WithClientId("RawClientPublishWhenNotConnectedAsync").Build();
        var client = new RawClient(options);

        var message = new MQTT5PublishMessage
        {
            Topic = "test/topic",
            Payload = Encoding.UTF8.GetBytes("test"),
            QoS = QualityOfService.AtLeastOnceDelivery,
        };

        // For QoS 0, it might queue, but for QoS 1/2 it should throw
        await Assert.ThrowsAsync<HiveMQttClientException>(() => client.PublishAsync(message)).ConfigureAwait(false);

        client.Dispose();
    }

    [Fact]
    public async Task PublishQoS0FastPathAsync()
    {
        // Test the fast path for simple QoS 0 messages (lines 260-271 in RawClient.cs)
        var testTopic = "tests/RawClientPublishQoS0FastPathAsync";
        var testPayload = "fast path test";
        var options = new HiveMQClientOptionsBuilder().WithClientId("RawClientPublishQoS0FastPathAsync").Build();
        var client = new RawClient(options);

        var connectResult = await client.ConnectAsync().ConfigureAwait(false);
        Assert.True(connectResult.ReasonCode == ConnAckReasonCode.Success);

        // Wait a bit for connection properties to be cached
        await Task.Delay(100).ConfigureAwait(false);

        var message = new MQTT5PublishMessage
        {
            Topic = testTopic,
            Payload = Encoding.UTF8.GetBytes(testPayload),
            QoS = QualityOfService.AtMostOnceDelivery,
            Retain = false, // No retain, no topic alias - should use fast path
        };

        var result = await client.PublishAsync(message).ConfigureAwait(false);
        Assert.NotNull(result);
        Assert.Equal(QualityOfService.AtMostOnceDelivery, result.Message.QoS);

        await client.DisconnectAsync().ConfigureAwait(false);
        client.Dispose();
    }

    [Fact]
    public async Task PublishQoS1WithPubAckAsync()
    {
        var testTopic = "tests/RawClientPublishQoS1WithPubAckAsync";
        var testPayload = "QoS 1 test";
        var options = new HiveMQClientOptionsBuilder().WithClientId("RawClientPublishQoS1WithPubAckAsync").Build();
        var client = new RawClient(options);

        var connectResult = await client.ConnectAsync().ConfigureAwait(false);
        Assert.True(connectResult.ReasonCode == ConnAckReasonCode.Success);

        // RawClient doesn't manage subscriptions - we need to subscribe first
        var subResult = await client.SubscribeAsync(testTopic, QualityOfService.AtLeastOnceDelivery).ConfigureAwait(false);
        Assert.NotEmpty(subResult.Subscriptions);
        Assert.Equal(SubAckReasonCode.GrantedQoS1, subResult.Subscriptions[0].SubscribeReasonCode);

        var message = new MQTT5PublishMessage
        {
            Topic = testTopic,
            Payload = Encoding.UTF8.GetBytes(testPayload),
            QoS = QualityOfService.AtLeastOnceDelivery,
        };

        var result = await client.PublishAsync(message).ConfigureAwait(false);
        Assert.NotNull(result);
        Assert.NotNull(result.QoS1ReasonCode);
        Assert.Equal(PubAckReasonCode.Success, result.QoS1ReasonCode);

        await client.DisconnectAsync().ConfigureAwait(false);
        client.Dispose();
    }

    [Fact]
    public async Task PublishQoS2WithPubRecAsync()
    {
        var testTopic = "tests/RawClientPublishQoS2WithPubRecAsync";
        var testPayload = "QoS 2 test";
        var options = new HiveMQClientOptionsBuilder().WithClientId("RawClientPublishQoS2WithPubRecAsync").Build();
        var client = new RawClient(options);

        var connectResult = await client.ConnectAsync().ConfigureAwait(false);
        Assert.True(connectResult.ReasonCode == ConnAckReasonCode.Success);

        // RawClient doesn't manage subscriptions - we need to subscribe first
        var subResult = await client.SubscribeAsync(testTopic, QualityOfService.ExactlyOnceDelivery).ConfigureAwait(false);
        Assert.NotEmpty(subResult.Subscriptions);
        Assert.Equal(SubAckReasonCode.GrantedQoS2, subResult.Subscriptions[0].SubscribeReasonCode);

        var message = new MQTT5PublishMessage
        {
            Topic = testTopic,
            Payload = Encoding.UTF8.GetBytes(testPayload),
            QoS = QualityOfService.ExactlyOnceDelivery,
        };

        var result = await client.PublishAsync(message).ConfigureAwait(false);
        Assert.NotNull(result);
        Assert.NotNull(result.QoS2ReasonCode);
        Assert.Equal(PubRecReasonCode.Success, result.QoS2ReasonCode);

        await client.DisconnectAsync().ConfigureAwait(false);
        client.Dispose();
    }
}
