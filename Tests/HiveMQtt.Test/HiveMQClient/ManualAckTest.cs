namespace HiveMQtt.Test.HiveMQClient;

using System;
using HiveMQtt.Client;
using HiveMQtt.Client.Events;
using HiveMQtt.Client.Exceptions;
using HiveMQtt.MQTT5.ReasonCodes;
using HiveMQtt.MQTT5.Types;
using Xunit;

public class ManualAckTest
{
    [Fact]
    public async Task AckAsync_WhenNotConnected_ThrowsAsync()
    {
        var options = new HiveMQClientOptionsBuilder()
            .WithClientId("ManualAckNotConnected")
            .WithManualAck(true)
            .Build();
        using var client = new HiveMQClient(options);

        var ex = await Assert.ThrowsAsync<HiveMQttClientException>(() => client.AckAsync(1)).ConfigureAwait(false);
        Assert.Contains("not connected", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ManualAck_QoS1_ReceiveThenAckAsync_SuccessAsync()
    {
        var testTopic = "tests/ManualAckQoS1HiveMQClient";
        var testPayload = "Manual ack QoS 1 payload";

        var subscriberOptions = new HiveMQClientOptionsBuilder()
            .WithClientId("ManualAckSubscriberQoS1")
            .WithManualAck(true)
            .Build();
        var publisherOptions = new HiveMQClientOptionsBuilder()
            .WithClientId("ManualAckPublisherQoS1")
            .Build();

        using var subscriber = new HiveMQClient(subscriberOptions);
        using var publisher = new HiveMQClient(publisherOptions);

        var messageReceivedSource = new TaskCompletionSource<(string Topic, string Payload, ushort PacketId)>(TaskCreationOptions.RunContinuationsAsynchronously);
        var pubAckSentSource = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

        subscriber.OnMessageReceived += (sender, args) =>
        {
            if (args.PublishMessage.Topic == testTopic && args.PacketIdentifier.HasValue)
            {
                messageReceivedSource.TrySetResult((
                    args.PublishMessage.Topic ?? string.Empty,
                    args.PublishMessage.PayloadAsString ?? string.Empty,
                    args.PacketIdentifier.Value));
            }
        };

        subscriber.OnPubAckSent += (sender, args) => pubAckSentSource.TrySetResult(true);

        var subConnectResult = await subscriber.ConnectAsync().ConfigureAwait(false);
        Assert.True(subConnectResult.ReasonCode == ConnAckReasonCode.Success);

        var pubConnectResult = await publisher.ConnectAsync().ConfigureAwait(false);
        Assert.True(pubConnectResult.ReasonCode == ConnAckReasonCode.Success);

        await subscriber.SubscribeAsync(testTopic, QualityOfService.AtLeastOnceDelivery).ConfigureAwait(false);

        await publisher.PublishAsync(testTopic, testPayload, QualityOfService.AtLeastOnceDelivery).ConfigureAwait(false);

        var (topic, payload, packetId) = await messageReceivedSource.Task.WaitAsync(TimeSpan.FromSeconds(5)).ConfigureAwait(false);
        Assert.Equal(testTopic, topic);
        Assert.Equal(testPayload, payload);
        Assert.True(packetId > 0);

        await subscriber.AckAsync(packetId).ConfigureAwait(false);

        await pubAckSentSource.Task.WaitAsync(TimeSpan.FromSeconds(5)).ConfigureAwait(false);

        await subscriber.DisconnectAsync().ConfigureAwait(false);
        await publisher.DisconnectAsync().ConfigureAwait(false);
    }

    [Fact]
    public async Task ManualAck_QoS2_ReceiveThenAckAsync_SuccessAsync()
    {
        var testTopic = "tests/ManualAckQoS2HiveMQClient";
        var testPayload = "Manual ack QoS 2 payload";

        var subscriberOptions = new HiveMQClientOptionsBuilder()
            .WithClientId("ManualAckSubscriberQoS2")
            .WithManualAck(true)
            .Build();
        var publisherOptions = new HiveMQClientOptionsBuilder()
            .WithClientId("ManualAckPublisherQoS2")
            .Build();

        using var subscriber = new HiveMQClient(subscriberOptions);
        using var publisher = new HiveMQClient(publisherOptions);

        var messageReceivedSource = new TaskCompletionSource<(string Topic, ushort PacketId)>(TaskCreationOptions.RunContinuationsAsynchronously);
        var pubCompSentSource = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

        subscriber.OnMessageReceived += (sender, args) =>
        {
            if (args.PublishMessage.Topic == testTopic && args.PacketIdentifier.HasValue)
            {
                messageReceivedSource.TrySetResult((args.PublishMessage.Topic ?? string.Empty, args.PacketIdentifier.Value));
            }
        };

        subscriber.OnPubCompSent += (sender, args) => pubCompSentSource.TrySetResult(true);

        var subConnectResult = await subscriber.ConnectAsync().ConfigureAwait(false);
        Assert.True(subConnectResult.ReasonCode == ConnAckReasonCode.Success);

        var pubConnectResult = await publisher.ConnectAsync().ConfigureAwait(false);
        Assert.True(pubConnectResult.ReasonCode == ConnAckReasonCode.Success);

        await subscriber.SubscribeAsync(testTopic, QualityOfService.ExactlyOnceDelivery).ConfigureAwait(false);

        await publisher.PublishAsync(testTopic, testPayload, QualityOfService.ExactlyOnceDelivery).ConfigureAwait(false);

        var (topic, packetId) = await messageReceivedSource.Task.WaitAsync(TimeSpan.FromSeconds(5)).ConfigureAwait(false);
        Assert.Equal(testTopic, topic);
        Assert.True(packetId > 0);

        await subscriber.AckAsync(packetId).ConfigureAwait(false);

        await pubCompSentSource.Task.WaitAsync(TimeSpan.FromSeconds(5)).ConfigureAwait(false);

        await subscriber.DisconnectAsync().ConfigureAwait(false);
        await publisher.DisconnectAsync().ConfigureAwait(false);
    }

    [Fact]
    public async Task AckAsync_WhenManualAckDisabled_ThrowsAsync()
    {
        var testTopic = "tests/ManualAckDisabledThrows";
        var subscriberOptions = new HiveMQClientOptionsBuilder()
            .WithClientId("ManualAckDisabledSubscriber")
            .WithManualAck(false)
            .Build();
        var publisherOptions = new HiveMQClientOptionsBuilder()
            .WithClientId("ManualAckDisabledPublisher")
            .Build();

        using var subscriber = new HiveMQClient(subscriberOptions);
        using var publisher = new HiveMQClient(publisherOptions);

        ushort? receivedPacketId = null;
        var messageReceivedSource = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

        subscriber.OnMessageReceived += (sender, args) =>
        {
            if (args.PublishMessage.Topic == testTopic)
            {
                receivedPacketId = args.PacketIdentifier;
                messageReceivedSource.TrySetResult(true);
            }
        };

        await subscriber.ConnectAsync().ConfigureAwait(false);
        await publisher.ConnectAsync().ConfigureAwait(false);
        await subscriber.SubscribeAsync(testTopic, QualityOfService.AtLeastOnceDelivery).ConfigureAwait(false);
        await publisher.PublishAsync(testTopic, "payload", QualityOfService.AtLeastOnceDelivery).ConfigureAwait(false);

        await messageReceivedSource.Task.WaitAsync(TimeSpan.FromSeconds(5)).ConfigureAwait(false);
        Assert.True(receivedPacketId.HasValue);

        var ex = await Assert.ThrowsAsync<HiveMQttClientException>(() => subscriber.AckAsync(receivedPacketId!.Value)).ConfigureAwait(false);
        Assert.Contains("Manual acknowledgement is not enabled", ex.Message);

        await subscriber.DisconnectAsync().ConfigureAwait(false);
        await publisher.DisconnectAsync().ConfigureAwait(false);
    }

    [Fact]
    public async Task AckAsync_WithInvalidPacketId_ThrowsAsync()
    {
        var subscriberOptions = new HiveMQClientOptionsBuilder()
            .WithClientId("ManualAckInvalidPacketId")
            .WithManualAck(true)
            .Build();

        using var subscriber = new HiveMQClient(subscriberOptions);
        await subscriber.ConnectAsync().ConfigureAwait(false);

        var ex = await Assert.ThrowsAsync<HiveMQttClientException>(() => subscriber.AckAsync((ushort)9999)).ConfigureAwait(false);
        Assert.Contains("No pending incoming publish", ex.Message);

        await subscriber.DisconnectAsync().ConfigureAwait(false);
    }

    [Fact]
    public async Task PacketIdentifier_Populated_ForQoS1Async()
    {
        var testTopic = "tests/PacketIdQoS1HiveMQClient";
        var subscriberOptions = new HiveMQClientOptionsBuilder()
            .WithClientId("PacketIdSubscriberQoS1")
            .WithManualAck(false)
            .Build();
        var publisherOptions = new HiveMQClientOptionsBuilder()
            .WithClientId("PacketIdPublisherQoS1")
            .Build();

        using var subscriber = new HiveMQClient(subscriberOptions);
        using var publisher = new HiveMQClient(publisherOptions);

        ushort? receivedPacketId = null;
        var messageReceivedSource = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

        subscriber.OnMessageReceived += (sender, args) =>
        {
            if (args.PublishMessage.Topic == testTopic)
            {
                receivedPacketId = args.PacketIdentifier;
                messageReceivedSource.TrySetResult(true);
            }
        };

        await subscriber.ConnectAsync().ConfigureAwait(false);
        await publisher.ConnectAsync().ConfigureAwait(false);
        await subscriber.SubscribeAsync(testTopic, QualityOfService.AtLeastOnceDelivery).ConfigureAwait(false);
        await publisher.PublishAsync(testTopic, "payload", QualityOfService.AtLeastOnceDelivery).ConfigureAwait(false);

        await messageReceivedSource.Task.WaitAsync(TimeSpan.FromSeconds(5)).ConfigureAwait(false);
        Assert.True(receivedPacketId.HasValue);
        Assert.True(receivedPacketId!.Value > 0);

        await subscriber.DisconnectAsync().ConfigureAwait(false);
        await publisher.DisconnectAsync().ConfigureAwait(false);
    }

    [Fact]
    public async Task AckAsync_WithEventArgs_Null_ThrowsAsync()
    {
        var options = new HiveMQClientOptionsBuilder()
            .WithClientId("ManualAckEventArgsNull")
            .WithManualAck(true)
            .Build();
        using var client = new HiveMQClient(options);

        await Assert.ThrowsAsync<ArgumentNullException>(() => client.AckAsync(null!)).ConfigureAwait(false);
    }

    [Fact]
    public async Task AckAsync_WithEventArgs_PacketIdentifierNull_CompletesWithoutThrowAsync()
    {
        var options = new HiveMQClientOptionsBuilder()
            .WithClientId("ManualAckEventArgsQoS0")
            .WithManualAck(true)
            .Build();
        using var client = new HiveMQClient(options);
        var message = new MQTT5PublishMessage
        {
            Topic = "test/topic",
            Payload = Array.Empty<byte>(),
            QoS = QualityOfService.AtMostOnceDelivery,
        };
        var args = new OnMessageReceivedEventArgs(message);

        await client.AckAsync(args).ConfigureAwait(false);
    }

    [Fact]
    public async Task AckAsync_WithEventArgs_QoS1_SuccessAsync()
    {
        var testTopic = "tests/ManualAckEventArgsQoS1HiveMQClient";
        var testPayload = "Manual ack via args QoS 1 payload";

        var subscriberOptions = new HiveMQClientOptionsBuilder()
            .WithClientId("ManualAckSubscriberEventArgsQoS1")
            .WithManualAck(true)
            .Build();
        var publisherOptions = new HiveMQClientOptionsBuilder()
            .WithClientId("ManualAckPublisherEventArgsQoS1")
            .Build();

        using var subscriber = new HiveMQClient(subscriberOptions);
        using var publisher = new HiveMQClient(publisherOptions);

        OnMessageReceivedEventArgs? receivedArgs = null;
        var messageReceivedSource = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var pubAckSentSource = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

        subscriber.OnMessageReceived += (sender, args) =>
        {
            if (args.PublishMessage.Topic == testTopic && args.PacketIdentifier.HasValue)
            {
                receivedArgs = args;
                messageReceivedSource.TrySetResult(true);
            }
        };

        subscriber.OnPubAckSent += (sender, args) => pubAckSentSource.TrySetResult(true);

        await subscriber.ConnectAsync().ConfigureAwait(false);
        await publisher.ConnectAsync().ConfigureAwait(false);
        await subscriber.SubscribeAsync(testTopic, QualityOfService.AtLeastOnceDelivery).ConfigureAwait(false);
        await publisher.PublishAsync(testTopic, testPayload, QualityOfService.AtLeastOnceDelivery).ConfigureAwait(false);

        await messageReceivedSource.Task.WaitAsync(TimeSpan.FromSeconds(5)).ConfigureAwait(false);
        Assert.NotNull(receivedArgs);

        await subscriber.AckAsync(receivedArgs!).ConfigureAwait(false);

        await pubAckSentSource.Task.WaitAsync(TimeSpan.FromSeconds(5)).ConfigureAwait(false);

        await subscriber.DisconnectAsync().ConfigureAwait(false);
        await publisher.DisconnectAsync().ConfigureAwait(false);
    }

    [Fact]
    public async Task AckAsync_WithEventArgs_QoS2_SuccessAsync()
    {
        var testTopic = "tests/ManualAckEventArgsQoS2HiveMQClient";
        var testPayload = "Manual ack via args QoS 2 payload";

        var subscriberOptions = new HiveMQClientOptionsBuilder()
            .WithClientId("ManualAckSubscriberEventArgsQoS2")
            .WithManualAck(true)
            .Build();
        var publisherOptions = new HiveMQClientOptionsBuilder()
            .WithClientId("ManualAckPublisherEventArgsQoS2")
            .Build();

        using var subscriber = new HiveMQClient(subscriberOptions);
        using var publisher = new HiveMQClient(publisherOptions);

        OnMessageReceivedEventArgs? receivedArgs = null;
        var messageReceivedSource = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var pubCompSentSource = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

        subscriber.OnMessageReceived += (sender, args) =>
        {
            if (args.PublishMessage.Topic == testTopic && args.PacketIdentifier.HasValue)
            {
                receivedArgs = args;
                messageReceivedSource.TrySetResult(true);
            }
        };

        subscriber.OnPubCompSent += (sender, args) => pubCompSentSource.TrySetResult(true);

        await subscriber.ConnectAsync().ConfigureAwait(false);
        await publisher.ConnectAsync().ConfigureAwait(false);
        await subscriber.SubscribeAsync(testTopic, QualityOfService.ExactlyOnceDelivery).ConfigureAwait(false);
        await publisher.PublishAsync(testTopic, testPayload, QualityOfService.ExactlyOnceDelivery).ConfigureAwait(false);

        await messageReceivedSource.Task.WaitAsync(TimeSpan.FromSeconds(5)).ConfigureAwait(false);
        Assert.NotNull(receivedArgs);

        await subscriber.AckAsync(receivedArgs!).ConfigureAwait(false);

        await pubCompSentSource.Task.WaitAsync(TimeSpan.FromSeconds(5)).ConfigureAwait(false);

        await subscriber.DisconnectAsync().ConfigureAwait(false);
        await publisher.DisconnectAsync().ConfigureAwait(false);
    }

    [Fact]
    public async Task AckAsync_WithEventArgs_WhenManualAckDisabled_ThrowsAsync()
    {
        var testTopic = "tests/ManualAckEventArgsDisabledThrows";
        var subscriberOptions = new HiveMQClientOptionsBuilder()
            .WithClientId("ManualAckDisabledEventArgsSubscriber")
            .WithManualAck(false)
            .Build();
        var publisherOptions = new HiveMQClientOptionsBuilder()
            .WithClientId("ManualAckDisabledEventArgsPublisher")
            .Build();

        using var subscriber = new HiveMQClient(subscriberOptions);
        using var publisher = new HiveMQClient(publisherOptions);

        OnMessageReceivedEventArgs? receivedArgs = null;
        var messageReceivedSource = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

        subscriber.OnMessageReceived += (sender, args) =>
        {
            if (args.PublishMessage.Topic == testTopic)
            {
                receivedArgs = args;
                messageReceivedSource.TrySetResult(true);
            }
        };

        await subscriber.ConnectAsync().ConfigureAwait(false);
        await publisher.ConnectAsync().ConfigureAwait(false);
        await subscriber.SubscribeAsync(testTopic, QualityOfService.AtLeastOnceDelivery).ConfigureAwait(false);
        await publisher.PublishAsync(testTopic, "payload", QualityOfService.AtLeastOnceDelivery).ConfigureAwait(false);

        await messageReceivedSource.Task.WaitAsync(TimeSpan.FromSeconds(5)).ConfigureAwait(false);
        Assert.NotNull(receivedArgs);
        Assert.True(receivedArgs!.PacketIdentifier.HasValue);

        var ex = await Assert.ThrowsAsync<HiveMQttClientException>(() => subscriber.AckAsync(receivedArgs!)).ConfigureAwait(false);
        Assert.Contains("Manual acknowledgement is not enabled", ex.Message);

        await subscriber.DisconnectAsync().ConfigureAwait(false);
        await publisher.DisconnectAsync().ConfigureAwait(false);
    }

    [Fact]
    public async Task AckAsync_WithEventArgs_InvalidPacketId_ThrowsAsync()
    {
        var subscriberOptions = new HiveMQClientOptionsBuilder()
            .WithClientId("ManualAckEventArgsInvalidPacketId")
            .WithManualAck(true)
            .Build();

        using var subscriber = new HiveMQClient(subscriberOptions);
        await subscriber.ConnectAsync().ConfigureAwait(false);

        var message = new MQTT5PublishMessage
        {
            Topic = "test/topic",
            Payload = Array.Empty<byte>(),
            QoS = QualityOfService.AtLeastOnceDelivery,
        };
        var args = new OnMessageReceivedEventArgs(message, 9999);

        var ex = await Assert.ThrowsAsync<HiveMQttClientException>(() => subscriber.AckAsync(args)).ConfigureAwait(false);
        Assert.Contains("No pending incoming publish", ex.Message);

        await subscriber.DisconnectAsync().ConfigureAwait(false);
    }
}
