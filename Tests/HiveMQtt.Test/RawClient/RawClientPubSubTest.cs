namespace HiveMQtt.Test.RawClient;

using System.Threading.Tasks;
using HiveMQtt.Client;
using HiveMQtt.MQTT5.ReasonCodes;
using HiveMQtt.MQTT5.Types;
using Xunit;

[Collection("Broker")]
public class RawClientPubSubTest
{
    [Fact]
    public async Task MostBasicPubSubAsync()
    {
        var testTopic = "tests/RawClientMostBasicPubSubAsync";
        var testPayload = new string(/*lang=json,strict*/ "{\"interference\": \"1029384\"}");
        var options = new HiveMQClientOptionsBuilder().WithClientId("RawClientMostBasicPubSubAsync").Build();
        var client = new RawClient(options);
        var taskCompletionSource = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

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
                var rawClient = (RawClient)sender;

                var disconnect = Task.Run(async () =>
                {
                    var disconnectResult = await rawClient.DisconnectAsync().ConfigureAwait(false);
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

        client.Dispose();
    }

    [Fact]
    public async Task QoS1PubSubAsync()
    {
        var testTopic = "tests/RawClientQoS1PubSubAsync";
        var testPayload = new string(/*lang=json,strict*/ "{\"interference\": \"1029384\"}");
        var options = new HiveMQClientOptionsBuilder().WithClientId("RawClientQoS1PubSubAsync").Build();
        var client = new RawClient(options);
        var taskCompletionSource = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
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
            Assert.Equal(QualityOfService.AtLeastOnceDelivery, args.PublishMessage.QoS);
            Assert.Equal(testTopic, args.PublishMessage.Topic);
            Assert.Equal(testPayload, args.PublishMessage.PayloadAsString);

            Interlocked.Increment(ref messagesReceived);
            if (messagesReceived == 10 && !taskCompletionSource.Task.IsCompleted)
            {
                taskCompletionSource.SetResult(true);
            }
        };

        Client.Results.PublishResult result;

        // Publish 10 messages
        for (var i = 0; i < 10; i++)
        {
            result = await client.PublishAsync(testTopic, testPayload, QualityOfService.AtLeastOnceDelivery).ConfigureAwait(false);
            Assert.IsType<Client.Results.PublishResult>(result);
            Assert.NotNull(result.QoS1ReasonCode);
            Assert.Equal(PubAckReasonCode.Success, result?.QoS1ReasonCode);
        }

        var taskResult = await taskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(5)).ConfigureAwait(false);
        Assert.True(taskResult);

        client.Dispose();
    }

    [Fact]
    public async Task QoS2PubSubAsync()
    {
        var testTopic = "tests/RawClientQoS2PubSubAsync";
        var testPayload = new string(/*lang=json,strict*/ "{\"interference\": \"1029384\"}");

        var options = new HiveMQClientOptionsBuilder().WithClientId("RawClientQoS2PubSubAsync").Build();
        var client = new RawClient(options);
        var taskCompletionSource = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
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
            Interlocked.Increment(ref messagesReceived);
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

        // Publish 5 messages
        for (var i = 0; i < 5; i++)
        {
            result = await client.PublishAsync(testTopic, testPayload, QualityOfService.ExactlyOnceDelivery).ConfigureAwait(false);
            Assert.IsType<Client.Results.PublishResult>(result);
            Assert.NotNull(result.QoS2ReasonCode);
            Assert.Equal(PubRecReasonCode.Success, result?.QoS2ReasonCode);
        }

        var taskResult = await taskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(5)).ConfigureAwait(false);
        Assert.True(taskResult);

        client.Dispose();
    }

    [Fact]
    public async Task NoSubscriptionTrackingAsync()
    {
        // Verify that RawClient does not maintain subscription state
        var options = new HiveMQClientOptionsBuilder().WithClientId("RawClientNoSubscriptionTrackingAsync").Build();
        var client = new RawClient(options);

        var connectResult = await client.ConnectAsync().ConfigureAwait(false);
        Assert.True(connectResult.ReasonCode == ConnAckReasonCode.Success);

        // Subscribe to a topic
        var subResult = await client.SubscribeAsync("test/topic", QualityOfService.AtLeastOnceDelivery).ConfigureAwait(false);
        Assert.NotEmpty(subResult.Subscriptions);

        // Disconnect and reconnect
        await client.DisconnectAsync().ConfigureAwait(false);
        connectResult = await client.ConnectAsync().ConfigureAwait(false);
        Assert.True(connectResult.ReasonCode == ConnAckReasonCode.Success);

        // RawClient should not have any subscription tracking
        // Note: RawClient doesn't expose Subscriptions property, so we can't verify this directly
        // But we can verify that re-subscribing works without issues
        var subResult2 = await client.SubscribeAsync("test/topic2", QualityOfService.AtLeastOnceDelivery).ConfigureAwait(false);
        Assert.NotEmpty(subResult2.Subscriptions);

        await client.DisconnectAsync().ConfigureAwait(false);
        client.Dispose();
    }
}
