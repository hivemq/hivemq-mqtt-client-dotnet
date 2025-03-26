namespace HiveMQtt.Test.HiveMQClient;

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

        // Small delay to ensure publish task starts first
        await Task.Delay(100).ConfigureAwait(false);

        // Connect in separate task
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

        // client 2 will receive the message and republish it to another topic
#pragma warning disable VSTHRD100 // Avoid async void methods
        async void Client2MessageHandler(object? sender, OnMessageReceivedEventArgs eventArgs)
        {
            Interlocked.Increment(ref client2MessageCount);
            if (sender is HiveMQClient client)
            {
                var publishResult = await client.PublishAsync("HMQ/3NodeQoS0SecondTopic", eventArgs.PublishMessage.PayloadAsString, QualityOfService.AtMostOnceDelivery).ConfigureAwait(true);
                Assert.NotNull(publishResult);
            }
        }
#pragma warning restore VSTHRD100 // Avoid async void methods

        client2.OnMessageReceived += Client2MessageHandler;

        // client 3 Subscribe to the secondary topic
        subscribeResult = await client3.SubscribeAsync("HMQ/3NodeQoS0SecondTopic", QualityOfService.AtMostOnceDelivery).ConfigureAwait(false);
        var client3MessageCount = 0;

        // client 3 will receive the final message
#pragma warning disable VSTHRD100 // Avoid async void methods
        void Client3MessageHandler(object? sender, OnMessageReceivedEventArgs eventArgs)
        {
            Interlocked.Increment(ref client3MessageCount);
            Assert.NotNull(eventArgs.PublishMessage);
            Assert.Equal("Hello World", eventArgs.PublishMessage.PayloadAsString);
        }
#pragma warning restore VSTHRD100 // Avoid async void methods

        client3.OnMessageReceived += Client3MessageHandler;

        // client 1 Publish 100 messages
        for (var i = 1; i <= 10; i++)
        {
            var publishResult = await client1.PublishAsync("HMQ/3NodeQoS0FirstTopic", "Hello World", QualityOfService.AtMostOnceDelivery).ConfigureAwait(false);
            Assert.NotNull(publishResult);
        }

        await Task.Delay(3000).ConfigureAwait(false);

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

        // client 2 will receive the message and republish it to another topic
#pragma warning disable VSTHRD100 // Avoid async void methods
        async void Client2MessageHandler(object? sender, OnMessageReceivedEventArgs eventArgs)
        {
            Interlocked.Increment(ref client2MessageCount);
            if (sender is HiveMQClient client)
            {
                var publishResult = await client.PublishAsync("HMQ/3NodeQoS1SecondTopic", eventArgs.PublishMessage.PayloadAsString, QualityOfService.AtLeastOnceDelivery).ConfigureAwait(false);
                Assert.NotNull(publishResult);
                Assert.Equal(publishResult.QoS1ReasonCode, PubAckReasonCode.Success);
            }
        }
#pragma warning restore VSTHRD100 // Avoid async void methods

        client2.OnMessageReceived += Client2MessageHandler;

        // client 3 Subscribe to the secondary topic
        subscribeResult = await client3.SubscribeAsync("HMQ/3NodeQoS1SecondTopic", QualityOfService.AtLeastOnceDelivery).ConfigureAwait(false);

        var client3MessageCount = 0;

        // client 3 will receive the final message
        void Client3MessageHandler(object? sender, OnMessageReceivedEventArgs eventArgs)
        {
            Interlocked.Increment(ref client3MessageCount);
            Assert.NotNull(eventArgs.PublishMessage);
            Assert.Equal("Hello World", eventArgs.PublishMessage.PayloadAsString);
        }

        client3.OnMessageReceived += Client3MessageHandler;

        // client 1 Publish 10 messages
        for (var i = 1; i <= 10; i++)
        {
            var publishResult = await client1.PublishAsync("HMQ/3NodeQoS1FirstTopic", "Hello World", QualityOfService.AtLeastOnceDelivery).ConfigureAwait(false);
            Assert.NotNull(publishResult);
        }

        await Task.Delay(2000).ConfigureAwait(false);

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

        // client 2 will receive the message and republish it to another topic
#pragma warning disable VSTHRD100 // Avoid async void methods
        async void Client2MessageHandler(object? sender, OnMessageReceivedEventArgs eventArgs)
        {
            Interlocked.Increment(ref client2MessageCount);
            var client = sender as HiveMQClient;
#pragma warning disable CS8602 // Dereference of a possibly null reference.
            var publishResult = await client.PublishAsync("HMQ/3NodeQoS2SecondTopic", eventArgs.PublishMessage.PayloadAsString, QualityOfService.ExactlyOnceDelivery).ConfigureAwait(true);
#pragma warning restore CS8602 // Dereference of a possibly null reference.
            Assert.NotNull(publishResult);
            Assert.Equal(publishResult.QoS2ReasonCode, PubRecReasonCode.Success);
        }
#pragma warning restore VSTHRD100 // Avoid async void methods

        client2.OnMessageReceived += Client2MessageHandler;

        // client 3 Subscribe to the secondary topic
        subscribeResult = await client3.SubscribeAsync("HMQ/3NodeQoS2SecondTopic", QualityOfService.ExactlyOnceDelivery).ConfigureAwait(false);

        // client 3 will receive the final message
        var client3MessageCount = 0;
        void Client3MessageHandler(object? sender, OnMessageReceivedEventArgs eventArgs)
        {
            Interlocked.Increment(ref client3MessageCount);
            Assert.NotNull(eventArgs.PublishMessage);
            Assert.Equal("Hello World", eventArgs.PublishMessage.PayloadAsString);
        }

        client3.OnMessageReceived += Client3MessageHandler;

        // client 1 Publish 10 messages
        for (var i = 1; i <= 10; i++)
        {
            var publishResult = await client1.PublishAsync("HMQ/3NodeQoS2FirstTopic", "Hello World", QualityOfService.ExactlyOnceDelivery).ConfigureAwait(false);
            Assert.NotNull(publishResult);
        }

        await Task.Delay(2000).ConfigureAwait(false);

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
}
