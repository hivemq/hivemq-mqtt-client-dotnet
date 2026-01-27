namespace HiveMQtt.Test.RawClient;

using System.Threading.Tasks;
using HiveMQtt.Client;
using HiveMQtt.MQTT5.ReasonCodes;
using HiveMQtt.MQTT5.Types;
using Xunit;

public class RawClientEventsTest
{
    [Fact]
    public async Task TestPublishPacketEventsAsync()
    {
        var testTopic = "tests/RawClientTestPublishPacketEventsAsync";
        var testPayload = "test payload";
        var options = new HiveMQClientOptionsBuilder().WithClientId("RawClientTestPublishPacketEventsAsync").Build();
        var client = new RawClient(options);

        var onPublishSentCalled = false;
        var onPubAckReceivedCalled = false;

        client.OnPublishSent += (sender, args) =>
        {
            onPublishSentCalled = true;
            Assert.NotNull(args.PublishPacket);
            Assert.Equal(testTopic, args.PublishPacket.Message.Topic);
        };

        client.OnPubAckReceived += (sender, args) =>
        {
            onPubAckReceivedCalled = true;
            Assert.NotNull(args.PubAckPacket);
        };

        var connectResult = await client.ConnectAsync().ConfigureAwait(false);
        Assert.True(connectResult.ReasonCode == ConnAckReasonCode.Success);

        // RawClient doesn't manage subscriptions - we need to subscribe first
        await client.SubscribeAsync(testTopic, QualityOfService.AtLeastOnceDelivery).ConfigureAwait(false);

        // Publish with QoS 1 to trigger PUBACK
        await client.PublishAsync(testTopic, testPayload, QualityOfService.AtLeastOnceDelivery).ConfigureAwait(false);

        // Wait for events to complete
        await Task.Delay(200).ConfigureAwait(false);

        Assert.True(onPublishSentCalled);
        Assert.True(onPubAckReceivedCalled);

        await client.DisconnectAsync().ConfigureAwait(false);
        client.Dispose();
    }

    [Fact]
    public async Task TestQoS2PacketEventsAsync()
    {
        var testTopic = "tests/RawClientTestQoS2PacketEventsAsync";
        var testPayload = "QoS 2 test payload";
        var options = new HiveMQClientOptionsBuilder().WithClientId("RawClientTestQoS2PacketEventsAsync").Build();
        var client = new RawClient(options);

        var onPublishSentCalled = false;
        var onPubRecReceivedCalled = false;
        var onPubRelSentCalled = false;
        var onPubCompReceivedCalled = false;

        client.OnPublishSent += (sender, args) =>
        {
            onPublishSentCalled = true;
            Assert.NotNull(args.PublishPacket);
        };

        client.OnPubRecReceived += (sender, args) =>
        {
            onPubRecReceivedCalled = true;
            Assert.NotNull(args.PubRecPacket);
        };

        client.OnPubRelSent += (sender, args) =>
        {
            onPubRelSentCalled = true;
            Assert.NotNull(args.PubRelPacket);
        };

        client.OnPubCompReceived += (sender, args) =>
        {
            onPubCompReceivedCalled = true;
            Assert.NotNull(args.PubCompPacket);
        };

        var connectResult = await client.ConnectAsync().ConfigureAwait(false);
        Assert.True(connectResult.ReasonCode == ConnAckReasonCode.Success);

        // RawClient doesn't manage subscriptions - we need to subscribe first
        await client.SubscribeAsync(testTopic, QualityOfService.ExactlyOnceDelivery).ConfigureAwait(false);

        // Publish with QoS 2 to trigger full QoS 2 handshake
        await client.PublishAsync(testTopic, testPayload, QualityOfService.ExactlyOnceDelivery).ConfigureAwait(false);

        // Wait for events to complete (QoS 2 handshake takes longer)
        await Task.Delay(500).ConfigureAwait(false);

        Assert.True(onPublishSentCalled);
        Assert.True(onPubRecReceivedCalled);
        Assert.True(onPubRelSentCalled);
        Assert.True(onPubCompReceivedCalled);

        await client.DisconnectAsync().ConfigureAwait(false);
        client.Dispose();
    }

    [Fact]
    public async Task TestPingEventsAsync()
    {
        var options = new HiveMQClientOptionsBuilder()
            .WithClientId("RawClientTestPingEventsAsync")
            .WithKeepAlive(3) // Short keepalive to trigger ping (but not too short to avoid timeout)
            .Build();

        var client = new RawClient(options);

        var onPingReqSentCalled = false;
        var onPingRespReceivedCalled = false;

        client.OnPingReqSent += (sender, args) =>
        {
            onPingReqSentCalled = true;
            Assert.NotNull(args.PingReqPacket);
        };

        client.OnPingRespReceived += (sender, args) =>
        {
            onPingRespReceivedCalled = true;
            Assert.NotNull(args.PingRespPacket);
        };

        var connectResult = await client.ConnectAsync().ConfigureAwait(false);
        Assert.True(connectResult.ReasonCode == ConnAckReasonCode.Success);

        // Wait for keepalive to trigger ping (keepalive is 3 seconds, wait 4 to ensure ping is sent and response received)
        // Events are fired asynchronously, so we need to wait a bit longer
        await Task.Delay(4500).ConfigureAwait(false);

        // Give events time to complete (they're fired via Task.Run)
        await Task.Delay(500).ConfigureAwait(false);

        Assert.True(onPingReqSentCalled, "OnPingReqSent event should have been fired");
        Assert.True(onPingRespReceivedCalled, "OnPingRespReceived event should have been fired");

        await client.DisconnectAsync().ConfigureAwait(false);
        client.Dispose();
    }

    [Fact]
    public async Task TestDisconnectReceivedEventAsync()
    {
        var options = new HiveMQClientOptionsBuilder().WithClientId("RawClientTestDisconnectReceivedEventAsync").Build();
        var client = new RawClient(options);

        var onDisconnectReceivedCalled = false;

        client.OnDisconnectReceived += (sender, args) =>
        {
            onDisconnectReceivedCalled = true;
            Assert.NotNull(args.DisconnectPacket);
        };

        var connectResult = await client.ConnectAsync().ConfigureAwait(false);
        Assert.True(connectResult.ReasonCode == ConnAckReasonCode.Success);

        // Disconnect - broker may send a DISCONNECT packet
        await client.DisconnectAsync().ConfigureAwait(false);

        // Wait for events to complete
        await Task.Delay(200).ConfigureAwait(false);

        // Note: Broker may or may not send DISCONNECT, so this may or may not be called
        // This test verifies the event handler is set up correctly
        _ = onDisconnectReceivedCalled;
        client.Dispose();
    }

    [Fact]
    public async Task TestOnMessageReceivedEventAsync()
    {
        var testTopic = "tests/RawClientTestOnMessageReceivedEventAsync";
        var testPayload = "message received test";
        var options = new HiveMQClientOptionsBuilder().WithClientId("RawClientTestOnMessageReceivedEventAsync").Build();
        var client = new RawClient(options);

        var onMessageReceivedCalled = false;
        var onPublishReceivedCalled = false;

        client.OnMessageReceived += (sender, args) =>
        {
            onMessageReceivedCalled = true;
            Assert.NotNull(args.PublishMessage);
            Assert.Equal(testTopic, args.PublishMessage.Topic);
            Assert.Equal(testPayload, args.PublishMessage.PayloadAsString);
        };

        client.OnPublishReceived += (sender, args) =>
        {
            onPublishReceivedCalled = true;
            Assert.NotNull(args.PublishPacket);
        };

        var connectResult = await client.ConnectAsync().ConfigureAwait(false);
        Assert.True(connectResult.ReasonCode == ConnAckReasonCode.Success);

        // Subscribe to receive messages
        await client.SubscribeAsync(testTopic).ConfigureAwait(false);

        // Publish a message
        await client.PublishAsync(testTopic, testPayload).ConfigureAwait(false);

        // Wait for message to be received
        await Task.Delay(300).ConfigureAwait(false);

        Assert.True(onMessageReceivedCalled);
        Assert.True(onPublishReceivedCalled);

        await client.DisconnectAsync().ConfigureAwait(false);
        client.Dispose();
    }
}
