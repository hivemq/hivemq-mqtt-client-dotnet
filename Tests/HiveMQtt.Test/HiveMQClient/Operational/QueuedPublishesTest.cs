namespace HiveMQtt.Test.HiveMQClient;

using System.Text;
using HiveMQtt.Client;
using HiveMQtt.MQTT5.Types;
using Xunit;

public class QueuedPublishesTest
{
    [Fact]
    public async Task Queued_Messages_Chain_Async()
    {
        var firstTopic = "hmq-tests-qmc/q1";
        var secondTopic = "hmq-tests-qmc/q2";
        var batchSize = 1000;

        ///////////////////////////////////////////////////////////////
        // Publish 1000 messages with an incrementing payload
        ///////////////////////////////////////////////////////////////
        var publisherOptions = new HiveMQClientOptionsBuilder()
                                    .WithClientId("hmq-tests-qmc/q1-publisher")
                                    .WithCleanStart(false)
                                    .WithSessionExpiryInterval(40000)
                                    .Build();
        var publishClient = new HiveMQClient(publisherOptions);
        await publishClient.ConnectAsync().ConfigureAwait(false);

        for (var i = 0; i < batchSize; i++)
        {
            // Make a JSON string payload with the current number
            var payload = Encoding.UTF8.GetBytes($"{{\"number\":{i}}}");

            // Publish the message to the topic "hmq-tests/q1" with exactly once delivery
            await publishClient.PublishAsync(firstTopic, payload, QualityOfService.ExactlyOnceDelivery).ConfigureAwait(false);
        }

        // DON'T disconnect the publishing client
        // await publishClient.DisconnectAsync().ConfigureAwait(false);

        ////////////////////////////////////////////////////////////////////////////
        // Subscribe to the first topic and relay the messages to a second topic
        ////////////////////////////////////////////////////////////////////////////
        var subscriberOptions = new HiveMQClientOptionsBuilder()
                                        .WithClientId("hmq-tests-qmc/q1-q2-relay")
                                        .WithCleanStart(false)
                                        .WithSessionExpiryInterval(40000)
                                        .Build();
        var subscribeClient = new HiveMQClient(subscriberOptions);

        var relayCount = 0;
        subscribeClient.OnMessageReceived += async (sender, args) =>
        {
            // Republish the Message to the second topic
            var payload = args.PublishMessage.Payload;
            var publishResult = await subscribeClient.PublishAsync(secondTopic, payload, QualityOfService.ExactlyOnceDelivery).ConfigureAwait(false);
            Assert.NotNull(publishResult.QoS2ReasonCode);

            // Atomically increment the relayCount
            Interlocked.Increment(ref relayCount);
        };

        await subscribeClient.ConnectAsync().ConfigureAwait(false);
        await subscribeClient.SubscribeAsync(firstTopic, QualityOfService.ExactlyOnceDelivery).ConfigureAwait(false);

        // Wait until relayCount is batchSize
        await Task.Delay(5000).ConfigureAwait(false);
        Assert.Equal(batchSize, relayCount);

        ////////////////////////////////////////////////////////////////////////////
        // Subscribe to the second topic and count the received messages
        ////////////////////////////////////////////////////////////////////////////
        var receiverOptions = new HiveMQClientOptionsBuilder()
                                    .WithClientId("hmq-tests-qmc/q2-receiver")
                                    .WithCleanStart(false)
                                    .WithSessionExpiryInterval(40000)
                                    .Build();
        var receiverClient = new HiveMQClient(receiverOptions);

        var receivedCount = 0;
        receiverClient.OnMessageReceived += (sender, args) => Interlocked.Increment(ref receivedCount);

        await receiverClient.ConnectAsync().ConfigureAwait(false);
        await receiverClient.SubscribeAsync(secondTopic, QualityOfService.ExactlyOnceDelivery).ConfigureAwait(false);

        // Wait for the receiver to receive all messages
        await Task.Delay(5000).ConfigureAwait(false);
        Assert.Equal(batchSize, receivedCount);
    }
}
