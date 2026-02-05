namespace HiveMQtt.Test.HiveMQClient;

using System.Text;
using System.Threading.Tasks;
using HiveMQtt.Client;
using HiveMQtt.MQTT5.Types;
using Xunit;

[Collection("Broker")]
public class QueuedPublishesTest
{
    // Synchronization for subscriptions to be ready before publishing
#pragma warning disable IDE0090 // Use 'new(...)' instead of 'new Type(...)'
    private TaskCompletionSource<bool> subscriptionsReady = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

    // Synchronization for all messages to be relayed
    private TaskCompletionSource<bool> allMessagesRelayed = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

    // Synchronization for all messages to be received
    private TaskCompletionSource<bool> allMessagesReceived = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
#pragma warning restore IDE0090

    // Unique id per test run to avoid broker session/topic bleed from previous runs
    private string runId = string.Empty;

    [Fact]
    public async Task Queued_Messages_Chain_Async()
    {
        var batchSize = 1000;
        this.runId = Guid.NewGuid().ToString("N")[..8];

        // Reset synchronization sources for this test run
        this.subscriptionsReady = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        this.allMessagesRelayed = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        this.allMessagesReceived = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

        var tasks = new[]
        {
            Task.Run(this.RelayClientAsync),
            Task.Run(this.ReceiverClientAsync),
            Task.Run(this.PublisherClientAsync),
        };

        var results = await Task.WhenAll(tasks).ConfigureAwait(false);
        Assert.Equal(batchSize, results[0]);
        Assert.Equal(batchSize, results[1]);
        Assert.Equal(batchSize, results[2]);
    }

    private async Task<int> PublisherClientAsync()
    {
        var batchSize = 1000;
        var firstTopic = $"hmq-tests-qmc/{this.runId}/q1";

        ///////////////////////////////////////////////////////////////
        // Publish 1000 messages with an incrementing payload
        ///////////////////////////////////////////////////////////////
        var publisherOptions = new HiveMQClientOptionsBuilder()
                                    .WithClientId($"hmq-tests-qmc-q1-publisher-{this.runId}")
                                    .WithCleanStart(false)
                                    .WithSessionExpiryInterval(40000)
                                    .Build();
        var publishClient = new HiveMQClient(publisherOptions);
        await publishClient.ConnectAsync().ConfigureAwait(false);

        // Wait for subscriptions to be ready instead of fixed delay
        await this.subscriptionsReady.Task.WaitAsync(TimeSpan.FromSeconds(10)).ConfigureAwait(false);

        for (var i = 0; i < batchSize; i++)
        {
            // Make a JSON string payload with the current number
            var payload = Encoding.UTF8.GetBytes($"{{\"number\":{i}}}");

            // Publish the message to the topic "hmq-tests/q1" with exactly once delivery
            await publishClient.PublishAsync(firstTopic, payload, QualityOfService.ExactlyOnceDelivery).ConfigureAwait(false);
        }

        await publishClient.DisconnectAsync().ConfigureAwait(false);
        return batchSize;
    }

    private async Task<int> RelayClientAsync()
    {
        var firstTopic = $"hmq-tests-qmc/{this.runId}/q1";
        var secondTopic = $"hmq-tests-qmc/{this.runId}/q2";

        ////////////////////////////////////////////////////////////////////////////
        // Subscribe to the first topic and relay the messages to a second topic
        ////////////////////////////////////////////////////////////////////////////
        var subscriberOptions = new HiveMQClientOptionsBuilder()
                                        .WithClientId($"hmq-tests-qmc-q1-q2-relay-{this.runId}")
                                        .WithCleanStart(false)
                                        .WithSessionExpiryInterval(40000)
                                        .Build();
        var subscribeClient = new HiveMQClient(subscriberOptions);

        var relayCount = 0;
        var batchSize = 1000;
#pragma warning disable CS8652
#pragma warning disable CS8604
        subscribeClient.OnMessageReceived += async (sender, args) =>
        {
            // Republish the Message to the second topic
            var payload = args.PublishMessage.Payload;
            var publishResult = await subscribeClient.PublishAsync(secondTopic, payload, QualityOfService.ExactlyOnceDelivery).ConfigureAwait(false);
            Assert.NotNull(publishResult?.QoS2ReasonCode);

            // Atomically increment the relayCount
            var count = Interlocked.Increment(ref relayCount);

            // Signal when all messages are relayed
            if (count == batchSize)
            {
                this.allMessagesRelayed.TrySetResult(true);
            }
        };
#pragma warning restore CS8652
#pragma warning restore CS8604

        await subscribeClient.ConnectAsync().ConfigureAwait(false);
        await subscribeClient.SubscribeAsync(firstTopic, QualityOfService.ExactlyOnceDelivery).ConfigureAwait(false);

        // Signal that subscription is ready
        this.subscriptionsReady.TrySetResult(true);

        // Wait until all messages are relayed with timeout instead of fixed delay
        await this.allMessagesRelayed.Task.WaitAsync(TimeSpan.FromSeconds(30)).ConfigureAwait(false);
        await subscribeClient.DisconnectAsync().ConfigureAwait(false);
        return relayCount;
    }

    private async Task<int> ReceiverClientAsync()
    {
        var secondTopic = $"hmq-tests-qmc/{this.runId}/q2";

        ////////////////////////////////////////////////////////////////////////////
        // Subscribe to the second topic and count the received messages
        ////////////////////////////////////////////////////////////////////////////
        var receiverOptions = new HiveMQClientOptionsBuilder()
                                    .WithClientId($"hmq-tests-qmc-q2-receiver-{this.runId}")
                                    .WithCleanStart(false)
                                    .WithSessionExpiryInterval(40000)
                                    .Build();
        var receiverClient = new HiveMQClient(receiverOptions);

        var receivedCount = 0;
        var batchSize = 1000;
        receiverClient.OnMessageReceived += (sender, args) =>
        {
            var count = Interlocked.Increment(ref receivedCount);

            // Signal when all messages are received
            if (count == batchSize)
            {
                this.allMessagesReceived.TrySetResult(true);
            }
        };

        await receiverClient.ConnectAsync().ConfigureAwait(false);
        await receiverClient.SubscribeAsync(secondTopic, QualityOfService.ExactlyOnceDelivery).ConfigureAwait(false);

        // Signal that subscription is ready
        this.subscriptionsReady.TrySetResult(true);

        // Wait for the receiver to receive all messages with timeout instead of fixed delay
        await this.allMessagesReceived.Task.WaitAsync(TimeSpan.FromSeconds(30)).ConfigureAwait(false);
        await receiverClient.DisconnectAsync().ConfigureAwait(false);

        return receivedCount;
    }
}
