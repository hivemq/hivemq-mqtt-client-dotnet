namespace HiveMQtt.Test.HiveMQClient;

using System.Collections.Concurrent;
using System.Text;
using System.Threading.Tasks;
using HiveMQtt.Client;
using HiveMQtt.Client.Events;
using HiveMQtt.MQTT5.ReasonCodes;
using HiveMQtt.MQTT5.Types;
using Xunit;

public class PublishTest
{
    [Fact]
    public async Task MostBasicPublishAsync()
    {
        var options = new HiveMQClientOptionsBuilder().WithClientId("MostBasicPublishAsync").Build();
        var client = new HiveMQClient(options);
        var connectResult = await client.ConnectAsync().ConfigureAwait(false);
        Assert.True(connectResult.ReasonCode == ConnAckReasonCode.Success);

        var msg = new string(/*lang=json,strict*/ "{\"interference\": \"1029384\"}");
        var result = await client.PublishAsync("tests/MostBasicPublishAsync", msg).ConfigureAwait(false);

        var disconnectResult = await client.DisconnectAsync().ConfigureAwait(false);
        Assert.True(disconnectResult);

        client.Dispose();
    }

    [Fact]
    public async Task PublishBeforeConnectAsync()
    {
        var options = new HiveMQClientOptionsBuilder().WithClientId("PublishBeforeConnectAsync").Build();
        var client = new HiveMQClient(options);

        // Start publish task before connecting
        var publishTask = Task.Run(async () =>
        {
            var msg = new string(/*lang=json,strict*/ "{\"interference\": \"1029384\"}");
            var result = await client.PublishAsync("tests/PublishBeforeConnectAsync", msg).ConfigureAwait(false);
            Assert.NotNull(result);
            Assert.NotNull(result.Message);
            return result;
        });

        // Connect - publish task will wait for connection automatically
        var connectResult = await client.ConnectAsync().ConfigureAwait(false);
        Assert.True(connectResult.ReasonCode == ConnAckReasonCode.Success);

        // Wait for publish to complete and verify result
        var publishResult = await publishTask.ConfigureAwait(false);
        Assert.NotNull(publishResult);

        var disconnectResult = await client.DisconnectAsync().ConfigureAwait(false);
        Assert.True(disconnectResult);

        client.Dispose();
    }

    [Fact]
    public async Task MostBasicPublishWithQoS0Async()
    {
        var options = new HiveMQClientOptionsBuilder().WithClientId("MostBasicPublishWithQoS0Async").Build();
        var client = new HiveMQClient(options);
        var connectResult = await client.ConnectAsync().ConfigureAwait(false);
        Assert.True(connectResult.ReasonCode == ConnAckReasonCode.Success);

        var msg = new string(/*lang=json,strict*/ "{\"interference\": \"1029384\"}");
        var result = await client.PublishAsync("tests/MostBasicPublishWithQoS0Async", msg, QualityOfService.AtMostOnceDelivery).ConfigureAwait(false);

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

        var msg = new string(/*lang=json,strict*/ "{\"interference\": \"1029384\"}");
        var result = await client.PublishAsync("tests/MostBasicPublishWithQoS1Async", msg, QualityOfService.AtLeastOnceDelivery).ConfigureAwait(false);
        Assert.NotNull(result.QoS1ReasonCode);
        Assert.Equal(PubAckReasonCode.NoMatchingSubscribers, result?.QoS1ReasonCode);

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

        var msg = new string(/*lang=json,strict*/ "{\"interference\": \"1029384\"}");
        var result = await client.PublishAsync("tests/MostBasicPublishWithQoS2Async", msg, QualityOfService.ExactlyOnceDelivery).ConfigureAwait(false);
        Assert.NotNull(result.QoS2ReasonCode);
        Assert.Equal(PubRecReasonCode.NoMatchingSubscribers, result?.QoS2ReasonCode);

        var disconnectResult = await client.DisconnectAsync().ConfigureAwait(false);
        Assert.True(disconnectResult);

        client.Dispose();
    }

    /// <summary>
    /// PublishResult exposes QoS1ReasonString (null when broker does not send ReasonString).
    /// Validates the client surfaces the property end-to-end after receiving PUBACK from broker.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [Fact]
    public async Task PublishQoS1_ResultExposesReasonString_WhenBrokerSendsNoReasonStringAsync()
    {
        var options = new HiveMQClientOptionsBuilder().WithClientId("PublishQoS1_ReasonString_Async").Build();
        var client = new HiveMQClient(options);
        var connectResult = await client.ConnectAsync().ConfigureAwait(false);
        Assert.True(connectResult.ReasonCode == ConnAckReasonCode.Success);

        var result = await client.PublishAsync("tests/PublishQoS1_ReasonString", "payload", QualityOfService.AtLeastOnceDelivery).ConfigureAwait(false);
        Assert.NotNull(result.QoS1ReasonCode);
        Assert.NotNull(result.Message);

        // Broker typically does not send ReasonString for normal PUBACK; property must be accessible and null.
        _ = result.QoS1ReasonString;
        Assert.Null(result.QoS1ReasonString);

        var disconnectResult = await client.DisconnectAsync().ConfigureAwait(false);
        Assert.True(disconnectResult);
        client.Dispose();
    }

    /// <summary>
    /// OnPubAckReceived event args expose PubAckPacket.ReasonString (null when broker does not send it).
    /// Validates the client surfaces ReasonString on the packet end-to-end after receiving PUBACK.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [Fact]
    public async Task PublishQoS1_OnPubAckReceived_PubAckPacketReasonStringAccessibleAsync()
    {
        string? reasonStringFromEvent = null;
        var options = new HiveMQClientOptionsBuilder().WithClientId("PublishQoS1_OnPubAck_ReasonString_Async").Build();
        var client = new HiveMQClient(options);
        client.OnPubAckReceived += (_, args) =>
        {
            Assert.NotNull(args.PubAckPacket);
            reasonStringFromEvent = args.PubAckPacket.ReasonString;
        };

        var connectResult = await client.ConnectAsync().ConfigureAwait(false);
        Assert.True(connectResult.ReasonCode == ConnAckReasonCode.Success);

        _ = await client.PublishAsync("tests/PublishQoS1_OnPubAck_ReasonString", "payload", QualityOfService.AtLeastOnceDelivery).ConfigureAwait(false);
        Assert.Null(reasonStringFromEvent);

        var disconnectResult = await client.DisconnectAsync().ConfigureAwait(false);
        Assert.True(disconnectResult);
        client.Dispose();
    }

    /// <summary>
    /// PublishResult exposes QoS2ReasonString (null when broker does not send ReasonString).
    /// Validates the client surfaces the property end-to-end after receiving PUBREC from broker.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [Fact]
    public async Task PublishQoS2_ResultExposesReasonString_WhenBrokerSendsNoReasonStringAsync()
    {
        var options = new HiveMQClientOptionsBuilder().WithClientId("PublishQoS2_ReasonString_Async").Build();
        var client = new HiveMQClient(options);
        var connectResult = await client.ConnectAsync().ConfigureAwait(false);
        Assert.True(connectResult.ReasonCode == ConnAckReasonCode.Success);

        var result = await client.PublishAsync("tests/PublishQoS2_ReasonString", "payload", QualityOfService.ExactlyOnceDelivery).ConfigureAwait(false);
        Assert.NotNull(result.QoS2ReasonCode);
        Assert.NotNull(result.Message);
        _ = result.QoS2ReasonString;
        Assert.Null(result.QoS2ReasonString);

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

        var msg = new string(/*lang=json,strict*/ "{\"interference\": \"1029384\"}");
        Client.Results.PublishResult result;

        for (var i = 1; i <= 10; i++)
        {
            result = await client.PublishAsync("tests/MultiPublishWithQoS0Async", msg, QualityOfService.AtMostOnceDelivery).ConfigureAwait(false);
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

        var msg = new string(/*lang=json,strict*/ "{\"interference\": \"1029384\"}");
        Client.Results.PublishResult result;

        for (var i = 1; i <= 10; i++)
        {
            result = await client.PublishAsync("tests/MultiPublishWithQoS1Async", msg, QualityOfService.AtLeastOnceDelivery).ConfigureAwait(false);
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

        var msg = new string(/*lang=json,strict*/ "{\"interference\": \"1029384\"}");
        Client.Results.PublishResult result;

        for (var i = 1; i <= 10; i++)
        {
            result = await client.PublishAsync("tests/MultiPublishWithQoS2Async", msg, QualityOfService.ExactlyOnceDelivery).ConfigureAwait(false);
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

        var msg = new string(/*lang=json,strict*/ "{\"interference\": \"1029384\"}");
        var result = await client.PublishAsync("tests/PublishWithOptionsAsync", msg).ConfigureAwait(false);

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

        var msg = new MQTT5PublishMessage("tests/PublishPayloadFormatIndicatorAsync", QualityOfService.AtMostOnceDelivery)
        {
            PayloadFormatIndicator = MQTT5PayloadFormatIndicator.UTF8Encoded,
            Payload = Encoding.ASCII.GetBytes("blah"),
        };

        var taskCompletionSource = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        client.OnPublishSent += (sender, args) =>
        {
            Assert.Equal(MQTT5PayloadFormatIndicator.UTF8Encoded, args.PublishPacket.Message.PayloadFormatIndicator);
            taskCompletionSource.SetResult(true);
        };

        var result = await client.PublishAsync(msg).ConfigureAwait(false);
        var eventResult = await taskCompletionSource.Task.ConfigureAwait(false);

        var disconnectResult = await client.DisconnectAsync().ConfigureAwait(false);
        Assert.True(disconnectResult);

        client.Dispose();
    }

    [Fact]
    public async Task ThreeNodeQoS0ChainedPublishesAsync()
    {
        var options = new HiveMQClientOptionsBuilder().WithClientId("ThreeNodeQoS0ChainedPublishesAsync1").Build();
        var client1 = new HiveMQClient(options); // publish message

        options = new HiveMQClientOptionsBuilder().WithClientId("ThreeNodeQoS0ChainedPublishesAsync2").Build();
        var client2 = new HiveMQClient(options); // receive and re-publish to another topic

        options = new HiveMQClientOptionsBuilder().WithClientId("ThreeNodeQoS0ChainedPublishesAsync3").Build();
        var client3 = new HiveMQClient(options); // receive republished message

        // Connect client 1
        var connectResult = await client1.ConnectAsync().ConfigureAwait(false);
        Assert.True(connectResult.ReasonCode == ConnAckReasonCode.Success);

        // Connect client 2
        connectResult = await client2.ConnectAsync().ConfigureAwait(false);
        Assert.True(connectResult.ReasonCode == ConnAckReasonCode.Success);

        // Connect client 3
        connectResult = await client3.ConnectAsync().ConfigureAwait(false);
        Assert.True(connectResult.ReasonCode == ConnAckReasonCode.Success);

        // client 2 Subscribe to the topic
        var subscribeResult = await client2.SubscribeAsync("HMQ/3NodeQoS0FirstTopic", QualityOfService.AtMostOnceDelivery).ConfigureAwait(false);
        var client2MessageCount = 0;

        // client 3 Subscribe to the secondary topic (declare before handlers to allow cross-referencing)
        subscribeResult = await client3.SubscribeAsync("HMQ/3NodeQoS0SecondTopic", QualityOfService.AtMostOnceDelivery).ConfigureAwait(false);
        var client3MessageCount = 0;

        // Use TaskCompletionSource to wait for all messages instead of fixed delay
        var allMessagesReceived = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

        // client 2 will receive the message and republish it to another topic
#pragma warning disable VSTHRD100 // Avoid async void methods
        async void Client2MessageHandler(object? sender, OnMessageReceivedEventArgs eventArgs)
        {
            var count = Interlocked.Increment(ref client2MessageCount);
            if (sender is HiveMQClient client)
            {
                var publishResult = await client.PublishAsync("HMQ/3NodeQoS0SecondTopic", eventArgs.PublishMessage.PayloadAsString, QualityOfService.AtMostOnceDelivery).ConfigureAwait(true);
                Assert.NotNull(publishResult);
            }

            // Check if all messages received (check in both handlers to handle race conditions)
            if (count == 10 && client3MessageCount == 10)
            {
                allMessagesReceived.TrySetResult(true);
            }
        }
#pragma warning restore VSTHRD100 // Avoid async void methods

        client2.OnMessageReceived += Client2MessageHandler;

        // client 3 will receive the final message
        void Client3MessageHandler(object? sender, OnMessageReceivedEventArgs eventArgs)
        {
            var count = Interlocked.Increment(ref client3MessageCount);
            Assert.NotNull(eventArgs.PublishMessage);
            Assert.Equal("Hello World", eventArgs.PublishMessage.PayloadAsString);

            // Check if all messages received (check in both handlers to handle race conditions)
            if (count == 10 && client2MessageCount == 10)
            {
                allMessagesReceived.TrySetResult(true);
            }
        }

        client3.OnMessageReceived += Client3MessageHandler;

        // client 1 Publish 10 messages
        for (var i = 1; i <= 10; i++)
        {
            var publishResult = await client1.PublishAsync("HMQ/3NodeQoS0FirstTopic", "Hello World", QualityOfService.AtMostOnceDelivery).ConfigureAwait(false);
            Assert.NotNull(publishResult);
        }

        // Wait for all messages to be received with timeout instead of fixed delay
        await allMessagesReceived.Task.WaitAsync(TimeSpan.FromSeconds(10)).ConfigureAwait(false);

        Assert.Equal(10, client2MessageCount);
        Assert.Equal(10, client3MessageCount);

        Assert.Equal(0, client1.Connection.OutgoingPublishQueue.Count);
        Assert.Equal(0, client2.Connection.OutgoingPublishQueue.Count);
        Assert.Equal(0, client3.Connection.OutgoingPublishQueue.Count);

        Assert.Equal(0, client1.Connection.ReceivedQueue.Count);
        Assert.Equal(0, client2.Connection.ReceivedQueue.Count);
        Assert.Equal(0, client3.Connection.ReceivedQueue.Count);

        Assert.Equal(0, client1.Connection.SendQueue.Count);
        Assert.Equal(0, client2.Connection.SendQueue.Count);
        Assert.Equal(0, client3.Connection.SendQueue.Count);

        Assert.Equal(0, client1.Connection.OPubTransactionQueue.Count);
        Assert.Equal(0, client2.Connection.OPubTransactionQueue.Count);
        Assert.Equal(0, client3.Connection.OPubTransactionQueue.Count);

        Assert.Equal(0, client1.Connection.IPubTransactionQueue.Count);
        Assert.Equal(0, client2.Connection.IPubTransactionQueue.Count);
        Assert.Equal(0, client3.Connection.IPubTransactionQueue.Count);

        // All done, disconnect all clients
        var disconnectResult = await client1.DisconnectAsync().ConfigureAwait(false);
        Assert.True(disconnectResult);
        disconnectResult = await client2.DisconnectAsync().ConfigureAwait(false);
        Assert.True(disconnectResult);
        disconnectResult = await client3.DisconnectAsync().ConfigureAwait(false);
        Assert.True(disconnectResult);

        client1.Dispose();
        client2.Dispose();
        client3.Dispose();
    }

    [Fact]
    public async Task ThreeNodeQoS1ChainedPublishesAsync()
    {
        var options = new HiveMQClientOptionsBuilder().WithClientId("ThreeNodeQoS1ChainedPublishesAsync1").Build();
        var client1 = new HiveMQClient(options); // publish message

        options = new HiveMQClientOptionsBuilder().WithClientId("ThreeNodeQoS1ChainedPublishesAsync2").Build();
        var client2 = new HiveMQClient(options); // receive and re-publish to another topic

        options = new HiveMQClientOptionsBuilder().WithClientId("ThreeNodeQoS1ChainedPublishesAsync3").Build();
        var client3 = new HiveMQClient(options); // receive republished message

        // Connect client 1
        var connectResult = await client1.ConnectAsync().ConfigureAwait(false);
        Assert.True(connectResult.ReasonCode == ConnAckReasonCode.Success);

        // Connect client 2
        connectResult = await client2.ConnectAsync().ConfigureAwait(false);
        Assert.True(connectResult.ReasonCode == ConnAckReasonCode.Success);

        // Connect client 3
        connectResult = await client3.ConnectAsync().ConfigureAwait(false);
        Assert.True(connectResult.ReasonCode == ConnAckReasonCode.Success);

        // client 2 Subscribe to the topic
        var subscribeResult = await client2.SubscribeAsync("HMQ/3NodeQoS1FirstTopic", QualityOfService.AtLeastOnceDelivery).ConfigureAwait(false);
        var client2MessageCount = 0;

        // client 3 Subscribe to the secondary topic (declare before handlers to allow cross-referencing)
        subscribeResult = await client3.SubscribeAsync("HMQ/3NodeQoS1SecondTopic", QualityOfService.AtLeastOnceDelivery).ConfigureAwait(false);
        var client3MessageCount = 0;

        // Use TaskCompletionSource to wait for all messages instead of fixed delay
        var allMessagesReceived = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

        // client 2 will receive the message and republish it to another topic
#pragma warning disable VSTHRD100 // Avoid async void methods
        async void Client2MessageHandler(object? sender, OnMessageReceivedEventArgs eventArgs)
        {
            var count = Interlocked.Increment(ref client2MessageCount);
            if (sender is HiveMQClient client)
            {
                var publishResult = await client.PublishAsync("HMQ/3NodeQoS1SecondTopic", eventArgs.PublishMessage.PayloadAsString, QualityOfService.AtLeastOnceDelivery).ConfigureAwait(false);
                Assert.NotNull(publishResult);
                Assert.Equal(publishResult.QoS1ReasonCode, PubAckReasonCode.Success);
            }

            // Check if all messages received (check in both handlers to handle race conditions)
            if (count == 10 && client3MessageCount == 10)
            {
                allMessagesReceived.TrySetResult(true);
            }
        }
#pragma warning restore VSTHRD100 // Avoid async void methods

        client2.OnMessageReceived += Client2MessageHandler;

        // client 3 will receive the final message
        void Client3MessageHandler(object? sender, OnMessageReceivedEventArgs eventArgs)
        {
            var count = Interlocked.Increment(ref client3MessageCount);
            Assert.NotNull(eventArgs.PublishMessage);
            Assert.Equal("Hello World", eventArgs.PublishMessage.PayloadAsString);

            // Check if all messages received (check in both handlers to handle race conditions)
            if (count == 10 && client2MessageCount == 10)
            {
                allMessagesReceived.TrySetResult(true);
            }
        }

        client3.OnMessageReceived += Client3MessageHandler;

        // client 1 Publish 10 messages
        for (var i = 1; i <= 10; i++)
        {
            var publishResult = await client1.PublishAsync("HMQ/3NodeQoS1FirstTopic", "Hello World", QualityOfService.AtLeastOnceDelivery).ConfigureAwait(false);
            Assert.NotNull(publishResult);
        }

        // Wait for all messages to be received with timeout instead of fixed delay
        await allMessagesReceived.Task.WaitAsync(TimeSpan.FromSeconds(10)).ConfigureAwait(false);

        Assert.Equal(10, client2MessageCount);
        Assert.Equal(10, client3MessageCount);

        Assert.Equal(0, client1.Connection.OutgoingPublishQueue.Count);
        Assert.Equal(0, client2.Connection.OutgoingPublishQueue.Count);
        Assert.Equal(0, client3.Connection.OutgoingPublishQueue.Count);

        Assert.Equal(0, client1.Connection.ReceivedQueue.Count);
        Assert.Equal(0, client2.Connection.ReceivedQueue.Count);
        Assert.Equal(0, client3.Connection.ReceivedQueue.Count);

        Assert.Equal(0, client1.Connection.SendQueue.Count);
        Assert.Equal(0, client2.Connection.SendQueue.Count);
        Assert.Equal(0, client3.Connection.SendQueue.Count);

        Assert.Equal(0, client1.Connection.OPubTransactionQueue.Count);
        Assert.Equal(0, client2.Connection.OPubTransactionQueue.Count);
        Assert.Equal(0, client3.Connection.OPubTransactionQueue.Count);

        Assert.Equal(0, client1.Connection.IPubTransactionQueue.Count);
        Assert.Equal(0, client2.Connection.IPubTransactionQueue.Count);
        Assert.Equal(0, client3.Connection.IPubTransactionQueue.Count);

        // All done, disconnect all clients
        var disconnectResult = await client1.DisconnectAsync().ConfigureAwait(false);
        Assert.True(disconnectResult);
        disconnectResult = await client2.DisconnectAsync().ConfigureAwait(false);
        Assert.True(disconnectResult);
        disconnectResult = await client3.DisconnectAsync().ConfigureAwait(false);
        Assert.True(disconnectResult);

        client1.Dispose();
        client2.Dispose();
        client3.Dispose();
    }

    [Fact]
    public async Task ThreeNodeQoS2ChainedPublishesAsync()
    {
        var options = new HiveMQClientOptionsBuilder().WithClientId("ThreeNodeQoS2ChainedPublishesAsync1").Build();
        var client1 = new HiveMQClient(options); // publish message

        options = new HiveMQClientOptionsBuilder().WithClientId("ThreeNodeQoS2ChainedPublishesAsync2").Build();
        var client2 = new HiveMQClient(options); // receive and re-publish to another topic

        options = new HiveMQClientOptionsBuilder().WithClientId("ThreeNodeQoS2ChainedPublishesAsync3").Build();
        var client3 = new HiveMQClient(options); // receive republished message

        // Connect client 1
        var connectResult = await client1.ConnectAsync().ConfigureAwait(false);
        Assert.True(connectResult.ReasonCode == ConnAckReasonCode.Success);

        // Connect client 2
        connectResult = await client2.ConnectAsync().ConfigureAwait(false);
        Assert.True(connectResult.ReasonCode == ConnAckReasonCode.Success);

        // Connect client 3
        connectResult = await client3.ConnectAsync().ConfigureAwait(false);
        Assert.True(connectResult.ReasonCode == ConnAckReasonCode.Success);

        // client 2 Subscribe to the topic
        var subscribeResult = await client2.SubscribeAsync("HMQ/3NodeQoS2FirstTopic", QualityOfService.ExactlyOnceDelivery).ConfigureAwait(false);
        var client2MessageCount = 0;

        // client 3 Subscribe to the secondary topic (declare before handlers to allow cross-referencing)
        subscribeResult = await client3.SubscribeAsync("HMQ/3NodeQoS2SecondTopic", QualityOfService.ExactlyOnceDelivery).ConfigureAwait(false);
        var client3MessageCount = 0;

        // Use TaskCompletionSource to wait for all messages instead of fixed delay
        var allMessagesReceived = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

        // client 2 will receive the message and republish it to another topic
#pragma warning disable VSTHRD100 // Avoid async void methods
        async void Client2MessageHandler(object? sender, OnMessageReceivedEventArgs eventArgs)
        {
            var count = Interlocked.Increment(ref client2MessageCount);
            var client = sender as HiveMQClient;
#pragma warning disable CS8602 // Dereference of a possibly null reference.
            var publishResult = await client.PublishAsync("HMQ/3NodeQoS2SecondTopic", eventArgs.PublishMessage.PayloadAsString, QualityOfService.ExactlyOnceDelivery).ConfigureAwait(true);
#pragma warning restore CS8602 // Dereference of a possibly null reference.
            Assert.NotNull(publishResult);
            Assert.Equal(publishResult.QoS2ReasonCode, PubRecReasonCode.Success);

            // Check if all messages received (check in both handlers to handle race conditions)
            if (count == 10 && client3MessageCount == 10)
            {
                allMessagesReceived.TrySetResult(true);
            }
        }
#pragma warning restore VSTHRD100 // Avoid async void methods

        client2.OnMessageReceived += Client2MessageHandler;

        // client 3 will receive the final message
        void Client3MessageHandler(object? sender, OnMessageReceivedEventArgs eventArgs)
        {
            var count = Interlocked.Increment(ref client3MessageCount);
            Assert.NotNull(eventArgs.PublishMessage);
            Assert.Equal("Hello World", eventArgs.PublishMessage.PayloadAsString);

            // Check if all messages received (check in both handlers to handle race conditions)
            if (count == 10 && client2MessageCount == 10)
            {
                allMessagesReceived.TrySetResult(true);
            }
        }

        client3.OnMessageReceived += Client3MessageHandler;

        // client 1 Publish 10 messages
        for (var i = 1; i <= 10; i++)
        {
            var publishResult = await client1.PublishAsync("HMQ/3NodeQoS2FirstTopic", "Hello World", QualityOfService.ExactlyOnceDelivery).ConfigureAwait(false);
            Assert.NotNull(publishResult);
        }

        // Wait for all messages to be received with timeout instead of fixed delay
        await allMessagesReceived.Task.WaitAsync(TimeSpan.FromSeconds(10)).ConfigureAwait(false);

        Assert.Equal(10, client2MessageCount);
        Assert.Equal(10, client3MessageCount);

        Assert.Equal(0, client1.Connection.OutgoingPublishQueue.Count);
        Assert.Equal(0, client2.Connection.OutgoingPublishQueue.Count);
        Assert.Equal(0, client3.Connection.OutgoingPublishQueue.Count);

        Assert.Equal(0, client1.Connection.ReceivedQueue.Count);
        Assert.Equal(0, client2.Connection.ReceivedQueue.Count);
        Assert.Equal(0, client3.Connection.ReceivedQueue.Count);

        Assert.Equal(0, client1.Connection.SendQueue.Count);
        Assert.Equal(0, client2.Connection.SendQueue.Count);
        Assert.Equal(0, client3.Connection.SendQueue.Count);

        Assert.Equal(0, client1.Connection.OPubTransactionQueue.Count);
        Assert.Equal(0, client2.Connection.OPubTransactionQueue.Count);
        Assert.Equal(0, client3.Connection.OPubTransactionQueue.Count);

        Assert.Equal(0, client1.Connection.IPubTransactionQueue.Count);
        Assert.Equal(0, client2.Connection.IPubTransactionQueue.Count);
        Assert.Equal(0, client3.Connection.IPubTransactionQueue.Count);

        // All done, disconnect all clients
        var disconnectResult = await client1.DisconnectAsync().ConfigureAwait(false);
        Assert.True(disconnectResult);
        disconnectResult = await client2.DisconnectAsync().ConfigureAwait(false);
        Assert.True(disconnectResult);
        disconnectResult = await client3.DisconnectAsync().ConfigureAwait(false);
        Assert.True(disconnectResult);

        client1.Dispose();
        client2.Dispose();
        client3.Dispose();
    }

    [Fact]
    public async Task PublishWithoutQoSAsync()
    {
        var options = new HiveMQClientOptionsBuilder().WithClientId("PublishWithoutQoSAsync").Build();
        var client = new HiveMQClient(options);
        var connectResult = await client.ConnectAsync().ConfigureAwait(false);
        Assert.True(connectResult.ReasonCode == ConnAckReasonCode.Success);

        var msg = new string(/*lang=json,strict*/ "{\"interference\": \"1029384\"}");
        var publishResult = await client.PublishAsync("tests/PublishWithoutQoSAsync", msg).ConfigureAwait(false);
        Assert.NotNull(publishResult);
        Assert.Equal(publishResult.Message.QoS, QualityOfService.AtMostOnceDelivery);

        var disconnectResult = await client.DisconnectAsync().ConfigureAwait(false);
        Assert.True(disconnectResult);

        client.Dispose();
    }

    /// <summary>
    /// Test to validate that concurrent write operations don't cause NotSupportedException.
    /// This test specifically targets the fix for GitHub Issue #258 where concurrent writes
    /// from ConnectionWriterAsync and ConnectionPublishWriterAsync caused race conditions.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test operation.</returns>
    [Fact]
    public async Task ConcurrentWriteOperations_ShouldNotThrowNotSupportedExceptionAsync()
    {
        const int concurrentTasks = 50;
        const int messagesPerTask = 20;

        var options = new HiveMQClientOptionsBuilder()
            .WithClientId("ConcurrentWriteOperationsTest")
            .WithAutomaticReconnect(true)
            .Build();

        var client = new HiveMQClient(options);
        var connectResult = await client.ConnectAsync().ConfigureAwait(false);
        Assert.True(connectResult.ReasonCode == ConnAckReasonCode.Success);

        var exceptions = new ConcurrentBag<Exception>();
        var successCount = 0;
        var tasks = new List<Task>();

        // Create multiple concurrent tasks that will trigger both publish and control packet writes
        for (var i = 0; i < concurrentTasks; i++)
        {
            var taskId = i;
            tasks.Add(Task.Run(async () =>
            {
                try
                {
                    for (var j = 0; j < messagesPerTask; j++)
                    {
                        // Mix of QoS levels to trigger different write paths
                        var qos = (QualityOfService)(j % 3);
                        var topic = $"tests/concurrent/{taskId}/{j}";
                        var payload = $"{{\"taskId\":{taskId},\"messageId\":{j},\"timestamp\":\"{DateTime.UtcNow:O}\"}}";

                        var result = await client.PublishAsync(topic, payload, qos).ConfigureAwait(false);
                        Assert.NotNull(result);
                    }

                    Interlocked.Increment(ref successCount);
                }
                catch (Exception ex)
                {
                    exceptions.Add(ex);
                }
            }));
        }

        // Wait for all tasks to complete
        await Task.WhenAll(tasks).ConfigureAwait(false);

        // Verify no NotSupportedException occurred
        var notSupportedExceptions = exceptions.Where(ex =>
            ex is NotSupportedException &&
            ex.Message.Contains("WriteAsync method cannot be called when another write operation is pending"))
            .ToList();

        Assert.Empty(notSupportedExceptions);

        // Verify most operations succeeded
        Assert.True(successCount > concurrentTasks * 0.8, $"Only {successCount}/{concurrentTasks} tasks succeeded");

        var disconnectResult = await client.DisconnectAsync().ConfigureAwait(false);
        Assert.True(disconnectResult);

        client.Dispose();
    }

    /// <summary>
    /// Test to validate QoS2 publish operations under high load, which was the specific
    /// scenario reported in GitHub Issue #258.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test operation.</returns>
    [Fact]
    public async Task QoS2PublishUnderLoad_ShouldNotCauseConcurrentWriteExceptionAsync()
    {
        const int messageCount = 200;
        const int concurrentPublishers = 10;

        var options = new HiveMQClientOptionsBuilder()
            .WithClientId("QoS2LoadTest")
            .WithAutomaticReconnect(true)
            .Build();

        var client = new HiveMQClient(options);
        var connectResult = await client.ConnectAsync().ConfigureAwait(false);
        Assert.True(connectResult.ReasonCode == ConnAckReasonCode.Success);

        var exceptions = new ConcurrentBag<Exception>();
        var publishedCount = 0;
        var tasks = new List<Task>();

        // Create concurrent publishers that will generate QoS2 control packets
        for (var i = 0; i < concurrentPublishers; i++)
        {
            var publisherId = i;
            tasks.Add(Task.Run(async () =>
            {
                try
                {
                    var messagesPerPublisher = messageCount / concurrentPublishers;
                    for (var j = 0; j < messagesPerPublisher; j++)
                    {
                        var topic = $"tests/qos2load/{publisherId}/{j}";
                        var payload = $"{{\"publisherId\":{publisherId},\"messageId\":{j},\"qos\":2,\"timestamp\":\"{DateTime.UtcNow:O}\"}}";

                        // Use QoS2 to trigger PUBREC/PUBREL/PUBCOMP control packets
                        var result = await client.PublishAsync(topic, payload, QualityOfService.ExactlyOnceDelivery).ConfigureAwait(false);
                        Assert.NotNull(result);
                        Assert.NotNull(result.QoS2ReasonCode);

                        Interlocked.Increment(ref publishedCount);
                    }
                }
                catch (Exception ex)
                {
                    exceptions.Add(ex);
                }
            }));
        }

        // Wait for all publishers to complete
        await Task.WhenAll(tasks).ConfigureAwait(false);

        // Verify no concurrent write exceptions occurred
        var concurrentWriteExceptions = exceptions.Where(ex =>
            ex is NotSupportedException &&
            ex.Message.Contains("WriteAsync method cannot be called when another write operation is pending"))
            .ToList();

        Assert.Empty(concurrentWriteExceptions);

        // Verify most messages were published successfully
        Assert.True(publishedCount > messageCount * 0.9, $"Only {publishedCount}/{messageCount} messages published successfully");

        var disconnectResult = await client.DisconnectAsync().ConfigureAwait(false);
        Assert.True(disconnectResult);

        client.Dispose();
    }

    /// <summary>
    /// Test to validate that rapid publish operations with mixed QoS levels
    /// don't cause concurrent write issues.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test operation.</returns>
    [Fact]
    public async Task RapidMixedQoSPublish_ShouldNotCauseConcurrentWriteExceptionAsync()
    {
        const int totalMessages = 100;

        var options = new HiveMQClientOptionsBuilder()
            .WithClientId("RapidMixedQoSTest")
            .WithAutomaticReconnect(true)
            .Build();

        var client = new HiveMQClient(options);
        var connectResult = await client.ConnectAsync().ConfigureAwait(false);
        Assert.True(connectResult.ReasonCode == ConnAckReasonCode.Success);

        var exceptions = new ConcurrentBag<Exception>();
        var publishedCount = 0;

        // Rapid-fire publishes with different QoS levels
        var publishTasks = new List<Task>();
        for (var i = 0; i < totalMessages; i++)
        {
            var messageId = i;
            var qos = (QualityOfService)(i % 3); // Cycle through QoS 0, 1, 2
            var topic = $"tests/rapid/{messageId}";
            var payload = $"{{\"messageId\":{messageId},\"qos\":{(int)qos},\"timestamp\":\"{DateTime.UtcNow:O}\"}}";

            publishTasks.Add(Task.Run(async () =>
            {
                try
                {
                    var result = await client.PublishAsync(topic, payload, qos).ConfigureAwait(false);
                    Assert.NotNull(result);
                    Interlocked.Increment(ref publishedCount);
                }
                catch (Exception ex)
                {
                    exceptions.Add(ex);
                }
            }));
        }

        // Wait for all publishes to complete
        await Task.WhenAll(publishTasks).ConfigureAwait(false);

        // Verify no concurrent write exceptions occurred
        var concurrentWriteExceptions = exceptions.Where(ex =>
            ex is NotSupportedException &&
            ex.Message.Contains("WriteAsync method cannot be called when another write operation is pending"))
            .ToList();

        Assert.Empty(concurrentWriteExceptions);

        // Verify all messages were published successfully
        Assert.Equal(totalMessages, publishedCount);

        var disconnectResult = await client.DisconnectAsync().ConfigureAwait(false);
        Assert.True(disconnectResult);

        client.Dispose();
    }
}
