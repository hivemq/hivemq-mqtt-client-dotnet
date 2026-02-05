namespace HiveMQtt.Test.HiveMQClient;

using System.Threading.Tasks;
using HiveMQtt.Client;
using HiveMQtt.Client.Options;
using HiveMQtt.MQTT5.ReasonCodes;
using HiveMQtt.MQTT5.Types;
using Xunit;

[Collection("Broker")]
public class LWTTest
{
    [Fact]
    public async Task Last_Will_With_Properties_Async()
    {
        // Setup & Connect a client to listen for LWT
        var listenerOptions = new HiveMQClientOptionsBuilder().WithClientId("Last_Will_With_Properties_Async-Listener").Build();
        var listenerClient = new HiveMQClient(listenerOptions);
        var connectResult = await listenerClient.ConnectAsync().ConfigureAwait(false);
        Assert.True(connectResult.ReasonCode == ConnAckReasonCode.Success);
        Assert.True(listenerClient.IsConnected());

        var messagesReceived = 0;
        var taskLWTReceived = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

        // Set the event handler for the message received event
        listenerClient.OnMessageReceived += (sender, args) =>
        {
            Interlocked.Increment(ref messagesReceived);
            Assert.Equal(QualityOfService.AtLeastOnceDelivery, args.PublishMessage.QoS);
            Assert.Equal("last/will2", args.PublishMessage.Topic);
            Assert.Equal("last will message", args.PublishMessage.PayloadAsString);
            Assert.Equal("application/text", args.PublishMessage.ContentType);
            Assert.Equal("response/topic", args.PublishMessage.ResponseTopic);
            var correlationData = new byte[] { 1, 2, 3, 4, 5 };
            Assert.Equal(correlationData, args.PublishMessage.CorrelationData);
            Assert.Equal(MQTT5PayloadFormatIndicator.UTF8Encoded, args.PublishMessage.PayloadFormatIndicator);
            Assert.Equal(100, args.PublishMessage.MessageExpiryInterval);
            Assert.Single(args.PublishMessage.UserProperties);
            Assert.True(args.PublishMessage.UserProperties.ContainsKey("userPropertyKey"));
            Assert.Equal("userPropertyValue", args.PublishMessage.UserProperties["userPropertyKey"]);

            Assert.NotNull(sender);

            // Notify that we've received the LWT message
            taskLWTReceived.SetResult(true);
        };

        var result = await listenerClient.SubscribeAsync("last/will2", QualityOfService.AtLeastOnceDelivery).ConfigureAwait(false);
        Assert.Single(result.Subscriptions);
        Assert.Equal(SubAckReasonCode.GrantedQoS1, result.Subscriptions[0].SubscribeReasonCode);
        Assert.Equal("last/will2", result.Subscriptions[0].TopicFilter.Topic);

        // Setup & Connect another client with a LWT
        var options = new HiveMQClientOptions
        {
            ClientId = "Last_Will_With_Properties_Async-LWTClient",
            LastWillAndTestament = new LastWillAndTestament("last/will2", "last will message"),
        };

        options.LastWillAndTestament.WillDelayInterval = 5;
        options.LastWillAndTestament.PayloadFormatIndicator = 1;
        options.LastWillAndTestament.MessageExpiryInterval = 100;
        options.LastWillAndTestament.QoS = QualityOfService.AtLeastOnceDelivery;
        options.LastWillAndTestament.ContentType = "application/text";
        options.LastWillAndTestament.ResponseTopic = "response/topic";
        var correlationData = new byte[] { 1, 2, 3, 4, 5 };
        options.LastWillAndTestament.CorrelationData = correlationData;
        options.LastWillAndTestament.UserProperties.Add("userPropertyKey", "userPropertyValue");

        var client = new HiveMQClient(options);
        connectResult = await client.ConnectAsync().ConfigureAwait(false);
        Assert.True(connectResult.ReasonCode == ConnAckReasonCode.Success);
        Assert.True(client.IsConnected());

        await Task.Delay(5000).ConfigureAwait(false);

        // Call DisconnectWithWillMessage.  listenerClient should receive the LWT message
        var disconnectOptions = new DisconnectOptions { ReasonCode = DisconnectReasonCode.DisconnectWithWillMessage };
        var disconnectResult = await client.DisconnectAsync(disconnectOptions).ConfigureAwait(false);

        // Wait until the LWT message is received
        var taskResult = await taskLWTReceived.Task.WaitAsync(TimeSpan.FromSeconds(25)).ConfigureAwait(false);
        Assert.True(taskResult);

        Assert.Equal(1, messagesReceived);
        await listenerClient.DisconnectAsync().ConfigureAwait(false);
    }
}
