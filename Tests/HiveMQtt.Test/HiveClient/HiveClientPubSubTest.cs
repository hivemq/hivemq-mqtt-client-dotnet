namespace HiveMQtt.Test.HiveClient;

using System.Threading.Tasks;
using HiveMQtt.Client;
using HiveMQtt.MQTT5.ReasonCodes;
using Xunit;

public class HiveClientPubSubTest
{
    [Fact]
    public async Task MostBasicPubSubAsync()
    {
        var client = new HiveClient();
        var connectResult = await client.ConnectAsync().ConfigureAwait(false);
        Assert.True(connectResult.ReasonCode == ConnAckReasonCode.Success);

        // Set the event handler for the message received event
        client.OnMessageReceived += (sender, args) =>
        {
            Assert.Equal("data/topic", args.PublishMessage.Topic);
            Assert.Equal("{\"interference\": \"1029384\"}", args.PublishMessage.PayloadAsString);
        };

        var subResult = await client.SubscribeAsync("data/topic").ConfigureAwait(false);

        var msg = new string(/*lang=json,strict*/ "{\"interference\": \"1029384\"}");
        var result = await client.PublishAsync("data/topic", msg).ConfigureAwait(false);

        // TODO: Add a way to check if the message was received on the topic
        var disconnectResult = await client.DisconnectAsync().ConfigureAwait(false);
        Assert.True(disconnectResult);
    }
}
