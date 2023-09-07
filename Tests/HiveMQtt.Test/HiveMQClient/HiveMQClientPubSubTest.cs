namespace HiveMQtt.Test.HiveMQClient;

using System.Threading.Tasks;
using HiveMQtt.Client;
using HiveMQtt.MQTT5.ReasonCodes;
using HiveMQtt.MQTT5.Types;
using Xunit;

public class HiveMQClientPubSubTest
{
    [Fact]
    public async Task MostBasicPubSubAsync()
    {
        var testTopic = "tests/MostBasicPubSubAsync";
        var testPayload = new string(/*lang=json,strict*/ "{\"interference\": \"1029384\"}");
        var client = new HiveMQClient();
        var taskCompletionSource = new TaskCompletionSource<bool>();

        var connectResult = await client.ConnectAsync().ConfigureAwait(false);
        Assert.True(connectResult.ReasonCode == ConnAckReasonCode.Success);

        // Set the event handler for the message received event
        client.OnMessageReceived += (sender, args) =>
        {
            Assert.Equal(testTopic, args.PublishMessage.Topic);
            Assert.Equal(testPayload, args.PublishMessage.PayloadAsString);

            // Disconnect after receiving the message
            if (sender != null)
            {
                var client = (HiveMQClient)sender;

                var disconnect = Task.Run(async () =>
                {
                    var disconnectResult = await client.DisconnectAsync().ConfigureAwait(false);
                    Assert.True(disconnectResult);
                    return disconnectResult;
                });
                Assert.True(disconnect.Result);
            }

            taskCompletionSource.SetResult(true);
        };

        var subResult = await client.SubscribeAsync(testTopic).ConfigureAwait(false);
        var result = await client.PublishAsync(testTopic, testPayload).ConfigureAwait(false);

        var taskResult = await taskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(5)).ConfigureAwait(false);
        Assert.True(taskResult);
    }

    [Fact]
    public async Task QoS1PubSubAsync()
    {
        var testTopic = "tests/QoS1PubSubAsync";
        var testPayload = new string(/*lang=json,strict*/ "{\"interference\": \"1029384\"}");
        var client = new HiveMQClient();
        var taskCompletionSource = new TaskCompletionSource<bool>();
        var messagesReceived = 0;

        var connectResult = await client.ConnectAsync().ConfigureAwait(false);
        Assert.True(connectResult.ReasonCode == ConnAckReasonCode.Success);

        // Subscribe with QoS1
        var subResult = await client.SubscribeAsync(testTopic, QualityOfService.AtLeastOnceDelivery).ConfigureAwait(false);
        Assert.NotEmpty(subResult.Subscriptions);
        Assert.Equal(SubAckReasonCode.GrantedQoS1, subResult.Subscriptions[0].SubscribeReasonCode);

        // Set the event handler for the message received event
        client.OnMessageReceived += (sender, args) =>
        {
            messagesReceived++;
            Assert.Equal(QualityOfService.AtLeastOnceDelivery, args.PublishMessage.QoS);
            Assert.Equal(testTopic, args.PublishMessage.Topic);
            Assert.Equal(testPayload, args.PublishMessage.PayloadAsString);

            if (messagesReceived >= 5)
            {
                taskCompletionSource.SetResult(true);
            }
        };

        Client.Results.PublishResult result;

        // Publish 10 messages
        for (var i = 0; i < 5; i++)
        {
            result = await client.PublishAsync(testTopic, testPayload, QualityOfService.AtLeastOnceDelivery).ConfigureAwait(false);
            Assert.IsType<Client.Results.PublishResult>(result);
            Assert.NotNull(result.QoS1ReasonCode);
            Assert.Equal(PubAckReasonCode.Success, result?.QoS1ReasonCode);
        }

        var taskResult = await taskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(5)).ConfigureAwait(false);
        Assert.True(taskResult);
    }

    [Fact]
    public async Task QoS2PubSubAsync()
    {
        var testTopic = "tests/QoS2PubSubAsync";
        var testPayload = new string(/*lang=json,strict*/ "{\"interference\": \"1029384\"}");

        var client = new HiveMQClient();
        var taskCompletionSource = new TaskCompletionSource<bool>();
        var connectResult = await client.ConnectAsync().ConfigureAwait(false);
        Assert.True(connectResult.ReasonCode == ConnAckReasonCode.Success);
        var messagesReceived = 0;

        // Subscribe with QoS2
        var subResult = await client.SubscribeAsync(testTopic, QualityOfService.ExactlyOnceDelivery).ConfigureAwait(false);
        Assert.NotEmpty(subResult.Subscriptions);
        Assert.Equal(SubAckReasonCode.GrantedQoS2, subResult.Subscriptions[0].SubscribeReasonCode);

        // Set the event handler for the message received event
        client.OnMessageReceived += (sender, args) =>
        {
            messagesReceived++;
            Assert.Equal(QualityOfService.ExactlyOnceDelivery, args.PublishMessage.QoS);
            Assert.Equal(testTopic, args.PublishMessage.Topic);
            Assert.Equal(testPayload, args.PublishMessage.PayloadAsString);

            Assert.NotNull(sender);

            if (messagesReceived >= 5)
            {
                taskCompletionSource.SetResult(true);
            }
        };

        Client.Results.PublishResult result;

        // Publish 10 messages
        for (var i = 0; i < 5; i++)
        {
            result = await client.PublishAsync(testTopic, testPayload, QualityOfService.ExactlyOnceDelivery).ConfigureAwait(false);
            Assert.IsType<Client.Results.PublishResult>(result);
            Assert.NotNull(result.QoS2ReasonCode);
            Assert.Equal(PubRecReasonCode.Success, result?.QoS2ReasonCode);
        }

        var taskResult = await taskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(5)).ConfigureAwait(false);
        Assert.True(taskResult);
    }

    [Fact]
    public async Task LargeMessagePubSubAsync()
    {
        var testTopic = "tests/LargeMessagePubSubAsync";
        var testPayload = "1. A delusion starts like any other idea, as an egg. Identical on the outside, perfectly formed. From the shell, you'd never know anything was wrong. It's what's inside that matters. 2. A delusion starts like any other idea, as an egg. Identical on the outside, perfectly formed. From the shell, you'd never know anything was wrong. It's what's inside that matters. 3. A delusion starts like any other idea, as an egg. Identical on the outside, perfectly formed. From the shell, you'd never know anything was wrong. It's what's inside that matters.";
        var client = new HiveMQClient();
        var taskCompletionSource = new TaskCompletionSource<bool>();

        var connectResult = await client.ConnectAsync().ConfigureAwait(false);
        Assert.True(connectResult.ReasonCode == ConnAckReasonCode.Success);

        // Set the event handler for the message received event
        client.OnMessageReceived += (sender, args) =>
        {
            Assert.Equal(testTopic, args.PublishMessage.Topic);
            Assert.Equal(testPayload, args.PublishMessage.PayloadAsString);

            // Disconnect after receiving the message
            if (sender != null)
            {
                var client = (HiveMQClient)sender;

                var disconnect = Task.Run(async () =>
                {
                    var disconnectResult = await client.DisconnectAsync().ConfigureAwait(false);
                    Assert.True(disconnectResult);
                    return disconnectResult;
                });
                Assert.True(disconnect.Result);
            }

            taskCompletionSource.SetResult(true);
        };

        var subResult = await client.SubscribeAsync(testTopic).ConfigureAwait(false);

        // length == 548
        var result = await client.PublishAsync(testTopic, testPayload).ConfigureAwait(false);

        var taskResult = await taskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(5)).ConfigureAwait(false);
        Assert.True(taskResult);
    }

    [Fact]
    public async Task OneMBMessagePubSubAsync()
    {
        var testTopic = new string("tests/OneMBMessagePubSubAsync");
        var testPayload = new byte[1024 * 1024];

        // Initialize the array to 0x05
        for (var i = 0; i < testPayload.Length; i++)
        {
            testPayload[i] = 0x05;
        }

        var client = new HiveMQClient();
        var taskCompletionSource = new TaskCompletionSource<bool>();

        var connectResult = await client.ConnectAsync().ConfigureAwait(false);
        Assert.True(connectResult.ReasonCode == ConnAckReasonCode.Success);

        // Set the event handler for the message received event
        client.OnMessageReceived += (sender, args) =>
        {
            Assert.Equal(testTopic, args.PublishMessage.Topic);

            Assert.Equal(1048576, args.PublishMessage.Payload?.Length);

            // Assert that all data has arrived in tact
            for (var i = 0; i < args.PublishMessage.Payload?.Length; i++)
            {
                Assert.Equal(0x05, args.PublishMessage.Payload[i]);
            }

            // Disconnect after receiving the message
            if (sender != null)
            {
                var client = (HiveMQClient)sender;

                var disconnect = Task.Run(async () =>
                {
                    var disconnectResult = await client.DisconnectAsync().ConfigureAwait(false);
                    Assert.True(disconnectResult);
                    return disconnectResult;
                });
                Assert.True(disconnect.Result);
            }

            taskCompletionSource.SetResult(true);
        };

        var subResult = await client.SubscribeAsync(testTopic).ConfigureAwait(false);

        var result = await client.PublishAsync(testTopic, testPayload).ConfigureAwait(false);

        var taskResult = await taskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(15)).ConfigureAwait(false);
        Assert.True(taskResult);
    }
}
