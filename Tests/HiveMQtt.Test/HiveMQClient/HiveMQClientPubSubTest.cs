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
        var client = new HiveMQClient();
        var connectResult = await client.ConnectAsync().ConfigureAwait(false);
        Assert.True(connectResult.ReasonCode == ConnAckReasonCode.Success);

        // Set the event handler for the message received event
        client.OnMessageReceived += (sender, args) =>
        {
            Assert.Equal("tests/MostBasicPubSubAsync", args.PublishMessage.Topic);
            Assert.Equal("{\"interference\": \"1029384\"}", args.PublishMessage.PayloadAsString);

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
        };

        var subResult = await client.SubscribeAsync("tests/MostBasicPubSubAsync").ConfigureAwait(false);

        var msg = new string(/*lang=json,strict*/ "{\"interference\": \"1029384\"}");
        var result = await client.PublishAsync("tests/MostBasicPubSubAsync", msg).ConfigureAwait(false);

        await Task.Delay(1000).ConfigureAwait(false);
    }

    [Fact]
    public async Task QoS1PubSubAsync()
    {
        var client = new HiveMQClient();
        var connectResult = await client.ConnectAsync().ConfigureAwait(false);
        Assert.True(connectResult.ReasonCode == ConnAckReasonCode.Success);

        // Set the event handler for the message received event
        client.OnMessageReceived += (sender, args) =>
        {
            Assert.Equal(QualityOfService.AtLeastOnceDelivery, args.PublishMessage.QoS);
            Assert.Equal("tests/QoS1PubSubAsync", args.PublishMessage.Topic);
            Assert.Equal("{\"interference\": \"1029384\"}", args.PublishMessage.PayloadAsString);

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
        };

        // Subscribe with QoS1
        var subResult = await client.SubscribeAsync("tests/QoS1PubSubAsync", QualityOfService.AtLeastOnceDelivery).ConfigureAwait(false);
        Assert.NotEmpty(subResult.Subscriptions);
        Assert.Equal(SubAckReasonCode.GrantedQoS1, subResult.Subscriptions[0].SubscribeReasonCode);

        // Publish a QoS1 message
        var msg = new string(/*lang=json,strict*/ "{\"interference\": \"1029384\"}");
        var result = await client.PublishAsync("tests/QoS1PubSubAsync", msg, QualityOfService.AtLeastOnceDelivery).ConfigureAwait(false);

        await Task.Delay(1000).ConfigureAwait(false);
    }

    [Fact]
    public async Task QoS2PubSubAsync()
    {
        var client = new HiveMQClient();
        var connectResult = await client.ConnectAsync().ConfigureAwait(false);
        Assert.True(connectResult.ReasonCode == ConnAckReasonCode.Success);

        // Set the event handler for the message received event
        client.OnMessageReceived += (sender, args) =>
        {
            Assert.Equal(QualityOfService.ExactlyOnceDelivery, args.PublishMessage.QoS);
            Assert.Equal("tests/QoS1PubSubAsync", args.PublishMessage.Topic);
            // FIXME: Payload garbled
            Assert.Equal("{\"interference\": \"1029384\"}", args.PublishMessage.PayloadAsString);

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
        };

        // Subscribe with QoS1
        var subResult = await client.SubscribeAsync("tests/QoS1PubSubAsync", QualityOfService.ExactlyOnceDelivery).ConfigureAwait(false);
        Assert.NotEmpty(subResult.Subscriptions);
        Assert.Equal(SubAckReasonCode.GrantedQoS2, subResult.Subscriptions[0].SubscribeReasonCode);

        // Publish a QoS1 message
        var msg = new string(/*lang=json,strict*/ "{\"interference\": \"1029384\"}");
        var result = await client.PublishAsync("tests/QoS1PubSubAsync", msg, QualityOfService.ExactlyOnceDelivery).ConfigureAwait(false);

        await Task.Delay(1000).ConfigureAwait(false);
    }
}
