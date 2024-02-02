namespace HiveMQtt.Test.HiveMQClient;

using System.Threading.Tasks;
using HiveMQtt.Client;
using HiveMQtt.Client.Options;
using HiveMQtt.MQTT5.ReasonCodes;
using HiveMQtt.MQTT5.Types;
using Xunit;

public class LastWillAndTestamentBuilderTest
{
    [Fact]
    public async Task Basic_Last_Will_Async()
    {
        var lwt = new LastWillAndTestamentBuilder()
            .WithTopic("last/will")
            .WithPayload("last will message")
            .WithQualityOfServiceLevel(QualityOfService.AtLeastOnceDelivery)
            .Build();

        var options = new HiveMQClientOptions
        {
            LastWillAndTestament = lwt,
        };

        var client = new HiveMQClient(options);

        var connectResult = await client.ConnectAsync().ConfigureAwait(false);
        Assert.True(connectResult.ReasonCode == ConnAckReasonCode.Success);
        Assert.True(client.IsConnected());
    }

    [Fact]
    public async Task Last_Will_With_Properties_Async()
    {
        // Setup & Connect a client to listen for LWT
        var listenerClient = new HiveMQClient();
        var connectResult = await listenerClient.ConnectAsync().ConfigureAwait(false);
        Assert.True(connectResult.ReasonCode == ConnAckReasonCode.Success);
        Assert.True(listenerClient.IsConnected());

        var messagesReceived = 0;
        var taskLWTReceived = new TaskCompletionSource<bool>();

        // Set the event handler for the message received event
        listenerClient.OnMessageReceived += (sender, args) =>
        {
            messagesReceived++;
            Assert.Equal(QualityOfService.AtLeastOnceDelivery, args.PublishMessage.QoS);
            Assert.Equal("last/will7", args.PublishMessage.Topic);
            Assert.Equal("last will message", args.PublishMessage.PayloadAsString);
            Assert.Equal("application/text", args.PublishMessage.ContentType);
            Assert.Equal("response/topic", args.PublishMessage.ResponseTopic);
            Assert.Equal(new byte[] { 1, 2, 3, 4, 5 }, args.PublishMessage.CorrelationData);
            Assert.Equal(MQTT5PayloadFormatIndicator.UTF8Encoded, args.PublishMessage.PayloadFormatIndicator);
            Assert.Equal(100, args.PublishMessage.MessageExpiryInterval);
            Assert.Single(args.PublishMessage.UserProperties);
            Assert.True(args.PublishMessage.UserProperties.ContainsKey("userPropertyKey"));
            Assert.Equal("userPropertyValue", args.PublishMessage.UserProperties["userPropertyKey"]);

            Assert.NotNull(sender);

            // Notify that we've received the LWT message
            taskLWTReceived.SetResult(true);
        };

        var result = await listenerClient.SubscribeAsync("last/will", QualityOfService.AtLeastOnceDelivery).ConfigureAwait(false);
        Assert.Single(result.Subscriptions);
        Assert.Equal(SubAckReasonCode.GrantedQoS1, result.Subscriptions[0].SubscribeReasonCode);
        Assert.Equal("last/will7", result.Subscriptions[0].TopicFilter.Topic);

        var lwt = new LastWillAndTestamentBuilder()
            .WithTopic("last/will7")
            .WithPayload("last will message")
            .WithQualityOfServiceLevel(QualityOfService.AtLeastOnceDelivery)
            .WithContentType("application/text")
            .WithResponseTopic("response/topic")
            .WithCorrelationData(new byte[] { 1, 2, 3, 4, 5 })
            .WithPayloadFormatIndicator(MQTT5PayloadFormatIndicator.UTF8Encoded)
            .WithMessageExpiryInterval(100)
            .WithUserProperty("userPropertyKey", "userPropertyValue")
            .WithWillDelayInterval(5)
            .Build();

        // Setup & Connect the client with LWT
        var options = new HiveMQClientOptions
        {
            LastWillAndTestament = lwt,
        };

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
    }
}
