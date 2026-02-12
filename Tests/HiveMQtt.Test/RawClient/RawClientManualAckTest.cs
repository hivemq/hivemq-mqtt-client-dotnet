namespace HiveMQtt.Test.RawClient;

using System;
using System.Threading;
using HiveMQtt.Client;
using HiveMQtt.Client.Events;
using HiveMQtt.Client.Exceptions;
using HiveMQtt.MQTT5.ReasonCodes;
using HiveMQtt.MQTT5.Types;
using Xunit;

public class RawClientManualAckTest
{
    [Fact]
    public async Task AckAsync_WhenNotConnected_ThrowsAsync()
    {
        var options = new HiveMQClientOptionsBuilder()
            .WithClientId("RawClientManualAckNotConnected")
            .WithManualAck(true)
            .Build();
        using var client = new RawClient(options);

        var ex = await Assert.ThrowsAsync<HiveMQttClientException>(() => client.AckAsync(1)).ConfigureAwait(false);
        Assert.Contains("not connected", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ManualAck_QoS1_ReceiveThenAckAsync_SuccessAsync()
    {
        var testTopic = "tests/RawClientManualAckQoS1";
        var testPayload = "RawClient manual ack QoS 1 payload";

        var subscriberOptions = new HiveMQClientOptionsBuilder()
            .WithClientId("RawClientManualAckSubscriberQoS1")
            .WithManualAck(true)
            .Build();
        var publisherOptions = new HiveMQClientOptionsBuilder()
            .WithClientId("RawClientManualAckPublisherQoS1")
            .Build();

        using var subscriber = new RawClient(subscriberOptions);
        using var publisher = new RawClient(publisherOptions);

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
        var testTopic = "tests/RawClientManualAckQoS2";
        var testPayload = "RawClient manual ack QoS 2 payload";

        var subscriberOptions = new HiveMQClientOptionsBuilder()
            .WithClientId("RawClientManualAckSubscriberQoS2")
            .WithManualAck(true)
            .Build();
        var publisherOptions = new HiveMQClientOptionsBuilder()
            .WithClientId("RawClientManualAckPublisherQoS2")
            .Build();

        using var subscriber = new RawClient(subscriberOptions);
        using var publisher = new RawClient(publisherOptions);

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
        var testTopic = "tests/RawClientManualAckDisabledThrows";
        var subscriberOptions = new HiveMQClientOptionsBuilder()
            .WithClientId("RawClientManualAckDisabledSubscriber")
            .WithManualAck(false)
            .Build();
        var publisherOptions = new HiveMQClientOptionsBuilder()
            .WithClientId("RawClientManualAckDisabledPublisher")
            .Build();

        using var subscriber = new RawClient(subscriberOptions);
        using var publisher = new RawClient(publisherOptions);

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
            .WithClientId("RawClientManualAckInvalidPacketId")
            .WithManualAck(true)
            .Build();

        const ushort invalidPacketId = 9999;
        using var subscriber = new RawClient(subscriberOptions);
        await subscriber.ConnectAsync().ConfigureAwait(false);

        var ex = await Assert.ThrowsAsync<HiveMQttClientException>(() => subscriber.AckAsync(invalidPacketId)).ConfigureAwait(false);
        Assert.Contains("No pending incoming publish", ex.Message);

        await subscriber.DisconnectAsync().ConfigureAwait(false);
    }

    [Fact]
    public async Task AckAsync_DoubleAck_ThrowsAlreadyAcknowledgedAsync()
    {
        var testTopic = "tests/RawClientManualAckDoubleAck";
        var subscriberOptions = new HiveMQClientOptionsBuilder()
            .WithClientId("RawClientManualAckDoubleAckSubscriber")
            .WithManualAck(true)
            .Build();
        var publisherOptions = new HiveMQClientOptionsBuilder()
            .WithClientId("RawClientManualAckDoubleAckPublisher")
            .Build();

        using var subscriber = new RawClient(subscriberOptions);
        using var publisher = new RawClient(publisherOptions);

        ushort? receivedPacketId = null;
        var messageReceivedSource = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

        subscriber.OnMessageReceived += (sender, args) =>
        {
            if (args.PublishMessage.Topic == testTopic && args.PacketIdentifier.HasValue)
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
        var packetId = receivedPacketId!.Value;

        await subscriber.AckAsync(packetId).ConfigureAwait(false);

        var ex = await Assert.ThrowsAsync<HiveMQttClientException>(() => subscriber.AckAsync(packetId)).ConfigureAwait(false);
        Assert.Contains("acknowledged", ex.Message, StringComparison.OrdinalIgnoreCase);

        await subscriber.DisconnectAsync().ConfigureAwait(false);
        await publisher.DisconnectAsync().ConfigureAwait(false);
    }

    [Fact]
    public async Task PacketIdentifier_Populated_ForQoS1Async()
    {
        var testTopic = "tests/RawClientPacketIdQoS1";
        var subscriberOptions = new HiveMQClientOptionsBuilder()
            .WithClientId("RawClientPacketIdSubscriberQoS1")
            .WithManualAck(false)
            .Build();
        var publisherOptions = new HiveMQClientOptionsBuilder()
            .WithClientId("RawClientPacketIdPublisherQoS1")
            .Build();

        using var subscriber = new RawClient(subscriberOptions);
        using var publisher = new RawClient(publisherOptions);

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
    public async Task PacketIdentifier_Populated_ForQoS2Async()
    {
        var testTopic = "tests/RawClientPacketIdQoS2";
        var subscriberOptions = new HiveMQClientOptionsBuilder()
            .WithClientId("RawClientPacketIdSubscriberQoS2")
            .WithManualAck(false)
            .Build();
        var publisherOptions = new HiveMQClientOptionsBuilder()
            .WithClientId("RawClientPacketIdPublisherQoS2")
            .Build();

        using var subscriber = new RawClient(subscriberOptions);
        using var publisher = new RawClient(publisherOptions);

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
        await subscriber.SubscribeAsync(testTopic, QualityOfService.ExactlyOnceDelivery).ConfigureAwait(false);
        await publisher.PublishAsync(testTopic, "payload", QualityOfService.ExactlyOnceDelivery).ConfigureAwait(false);

        await messageReceivedSource.Task.WaitAsync(TimeSpan.FromSeconds(5)).ConfigureAwait(false);
        Assert.True(receivedPacketId.HasValue);
        Assert.True(receivedPacketId!.Value > 0);

        await subscriber.DisconnectAsync().ConfigureAwait(false);
        await publisher.DisconnectAsync().ConfigureAwait(false);
    }

    [Fact]
    public async Task ManualAck_QoS0_PacketIdentifierNull_NoAckRequiredAsync()
    {
        var testTopic = "tests/RawClientManualAckQoS0NoAck";
        var testPayload = "RawClient QoS 0 with manual ack - no ack required";

        var subscriberOptions = new HiveMQClientOptionsBuilder()
            .WithClientId("RawClientManualAckSubscriberQoS0")
            .WithManualAck(true)
            .Build();
        var publisherOptions = new HiveMQClientOptionsBuilder()
            .WithClientId("RawClientManualAckPublisherQoS0")
            .Build();

        using var subscriber = new RawClient(subscriberOptions);
        using var publisher = new RawClient(publisherOptions);

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
        await subscriber.SubscribeAsync(testTopic, QualityOfService.AtMostOnceDelivery).ConfigureAwait(false);
        await publisher.PublishAsync(testTopic, testPayload, QualityOfService.AtMostOnceDelivery).ConfigureAwait(false);

        await messageReceivedSource.Task.WaitAsync(TimeSpan.FromSeconds(5)).ConfigureAwait(false);
        Assert.NotNull(receivedArgs);
        Assert.Null(receivedArgs!.PacketIdentifier);
        Assert.Equal(testPayload, receivedArgs.PublishMessage.PayloadAsString);

        await subscriber.AckAsync(receivedArgs).ConfigureAwait(false);

        await subscriber.DisconnectAsync().ConfigureAwait(false);
        await publisher.DisconnectAsync().ConfigureAwait(false);
    }

    [Fact]
    public async Task ManualAck_QoS1_TwoMessages_TwoAcks_BothPubAcksSentAsync()
    {
        var testTopic = "tests/RawClientManualAckTwoAcks";
        var subscriberOptions = new HiveMQClientOptionsBuilder()
            .WithClientId("RawClientManualAckTwoAcksSubscriber")
            .WithManualAck(true)
            .Build();
        var publisherOptions = new HiveMQClientOptionsBuilder()
            .WithClientId("RawClientManualAckTwoAcksPublisher")
            .Build();

        using var subscriber = new RawClient(subscriberOptions);
        using var publisher = new RawClient(publisherOptions);

        var firstMessageSource = new TaskCompletionSource<ushort>(TaskCreationOptions.RunContinuationsAsynchronously);
        var secondMessageSource = new TaskCompletionSource<ushort>(TaskCreationOptions.RunContinuationsAsynchronously);
        var messageCount = 0;
        var pubAckCount = 0;
        var pubAckSentSource = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

        subscriber.OnMessageReceived += (sender, args) =>
        {
            if (args.PublishMessage.Topic != testTopic || !args.PacketIdentifier.HasValue)
            {
                return;
            }

            var id = args.PacketIdentifier.Value;
            var count = Interlocked.Increment(ref messageCount);
            if (count == 1)
            {
                firstMessageSource.TrySetResult(id);
            }
            else if (count == 2)
            {
                secondMessageSource.TrySetResult(id);
            }
        };

        subscriber.OnPubAckSent += (sender, args) =>
        {
            var count = Interlocked.Increment(ref pubAckCount);
            if (count >= 2)
            {
                pubAckSentSource.TrySetResult(true);
            }
        };

        await subscriber.ConnectAsync().ConfigureAwait(false);
        await publisher.ConnectAsync().ConfigureAwait(false);
        await subscriber.SubscribeAsync(testTopic, QualityOfService.AtLeastOnceDelivery).ConfigureAwait(false);

        await publisher.PublishAsync(testTopic, "first", QualityOfService.AtLeastOnceDelivery).ConfigureAwait(false);
        var id1 = await firstMessageSource.Task.WaitAsync(TimeSpan.FromSeconds(10)).ConfigureAwait(false);
        await subscriber.AckAsync(id1).ConfigureAwait(false);

        await publisher.PublishAsync(testTopic, "second", QualityOfService.AtLeastOnceDelivery).ConfigureAwait(false);
        var id2 = await secondMessageSource.Task.WaitAsync(TimeSpan.FromSeconds(10)).ConfigureAwait(false);
        await subscriber.AckAsync(id2).ConfigureAwait(false);

        await pubAckSentSource.Task.WaitAsync(TimeSpan.FromSeconds(10)).ConfigureAwait(false);
        Assert.Equal(2, pubAckCount);

        await subscriber.DisconnectAsync().ConfigureAwait(false);
        await publisher.DisconnectAsync().ConfigureAwait(false);
    }

    [Fact]
    public async Task AckAsync_WithEventArgs_Null_ThrowsAsync()
    {
        var options = new HiveMQClientOptionsBuilder()
            .WithClientId("RawClientManualAckEventArgsNull")
            .WithManualAck(true)
            .Build();
        using var client = new RawClient(options);

        await Assert.ThrowsAsync<ArgumentNullException>(() => client.AckAsync(null!)).ConfigureAwait(false);
    }

    [Fact]
    public async Task AckAsync_WithEventArgs_PacketIdentifierNull_CompletesWithoutThrowAsync()
    {
        var options = new HiveMQClientOptionsBuilder()
            .WithClientId("RawClientManualAckEventArgsQoS0")
            .WithManualAck(true)
            .Build();
        using var client = new RawClient(options);
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
        var testTopic = "tests/RawClientManualAckEventArgsQoS1";
        var testPayload = "RawClient manual ack via args QoS 1 payload";

        var subscriberOptions = new HiveMQClientOptionsBuilder()
            .WithClientId("RawClientManualAckSubscriberEventArgsQoS1")
            .WithManualAck(true)
            .Build();
        var publisherOptions = new HiveMQClientOptionsBuilder()
            .WithClientId("RawClientManualAckPublisherEventArgsQoS1")
            .Build();

        using var subscriber = new RawClient(subscriberOptions);
        using var publisher = new RawClient(publisherOptions);

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
        var testTopic = "tests/RawClientManualAckEventArgsQoS2";
        var testPayload = "RawClient manual ack via args QoS 2 payload";

        var subscriberOptions = new HiveMQClientOptionsBuilder()
            .WithClientId("RawClientManualAckSubscriberEventArgsQoS2")
            .WithManualAck(true)
            .Build();
        var publisherOptions = new HiveMQClientOptionsBuilder()
            .WithClientId("RawClientManualAckPublisherEventArgsQoS2")
            .Build();

        using var subscriber = new RawClient(subscriberOptions);
        using var publisher = new RawClient(publisherOptions);

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
        var testTopic = "tests/RawClientManualAckEventArgsDisabledThrows";
        var subscriberOptions = new HiveMQClientOptionsBuilder()
            .WithClientId("RawClientManualAckDisabledEventArgsSubscriber")
            .WithManualAck(false)
            .Build();
        var publisherOptions = new HiveMQClientOptionsBuilder()
            .WithClientId("RawClientManualAckDisabledEventArgsPublisher")
            .Build();

        using var subscriber = new RawClient(subscriberOptions);
        using var publisher = new RawClient(publisherOptions);

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
            .WithClientId("RawClientManualAckEventArgsInvalidPacketId")
            .WithManualAck(true)
            .Build();

        const ushort invalidPacketId = 9999;
        using var subscriber = new RawClient(subscriberOptions);
        await subscriber.ConnectAsync().ConfigureAwait(false);

        var message = new MQTT5PublishMessage
        {
            Topic = "test/topic",
            Payload = Array.Empty<byte>(),
            QoS = QualityOfService.AtLeastOnceDelivery,
        };
        var args = new OnMessageReceivedEventArgs(message, invalidPacketId);

        var ex = await Assert.ThrowsAsync<HiveMQttClientException>(() => subscriber.AckAsync(args)).ConfigureAwait(false);
        Assert.Contains("No pending incoming publish", ex.Message);

        await subscriber.DisconnectAsync().ConfigureAwait(false);
    }
}
