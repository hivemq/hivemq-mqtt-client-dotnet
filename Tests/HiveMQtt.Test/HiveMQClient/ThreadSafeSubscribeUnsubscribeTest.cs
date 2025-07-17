namespace HiveMQtt.Test.HiveMQClient;

using System.Collections.Concurrent;
using Client;
using Client.Options;
using MQTT5.ReasonCodes;
using MQTT5.Types;
using Xunit;
using Xunit.Abstractions;

public class ThreadSafeSubscribeUnsubscribeTest
{
    private readonly ITestOutputHelper testOutputHelper;

    public ThreadSafeSubscribeUnsubscribeTest(ITestOutputHelper testOutputHelper)
    {
        this.testOutputHelper = testOutputHelper;
    }

    [Fact]
    public async Task SubscribeUnsubscribe_InManyThreadsAsync()
    {
        const int workerCount = 100;
        const int iterationsPerWorker = 100;
        const int topicsPerIteration = 10;
        const int publishesPerIteration = 10;
        const int totalExpectedSuccesses = workerCount * iterationsPerWorker;

        var options = new HiveMQClientOptionsBuilder().WithClientId("ConcurrentSubscribeUnsubscribeAndPublish").Build();
        options.ResponseTimeoutInMs = 20000;
        var client = new HiveMQClient(options);
        Assert.NotNull(client);

        var connectResult = await client.ConnectAsync().ConfigureAwait(false);
        Assert.True(connectResult.ReasonCode == ConnAckReasonCode.Success);
        Assert.True(client.IsConnected());

        client.OnMessageReceived += (_, args) => { };
        _ = await client.SubscribeAsync("/test/#").ConfigureAwait(false);

        var exceptionMessages = new ConcurrentBag<string>();
        var successCount = 0;
        var tasks = new List<Task>();

        foreach (var workerId in Enumerable.Range(0, workerCount))
        {
            tasks.Add(Task.Run(async () =>
            {
                for (var i = 0; i < iterationsPerWorker; i++)
                {
                    var topicPrefix = $"/test/topic/{workerId}/{i}";

                    var topicsToManage = Enumerable.Range(0, topicsPerIteration)
                                                   .Select(j => $"{topicPrefix}/{(char)('a' + j)}")
                                                   .ToList();

                    try
                    {
                        var topicFilters = topicsToManage.Select(topic => new TopicFilter(topic, QualityOfService.ExactlyOnceDelivery)).ToList();
                        var subscribeOptions = new SubscribeOptions { TopicFilters = topicFilters };
                        _ = await client.SubscribeAsync(subscribeOptions).ConfigureAwait(false);

                        var publishTasks = new List<Task>(publishesPerIteration * 3);
                        for (var j = 0; j < publishesPerIteration; j++)
                        {
                            publishTasks.Add(client.PublishAsync(topicsToManage.First(), "Hello World"));
                            publishTasks.Add(client.PublishAsync(topicsToManage.Last(), "Hello World"));
                            publishTasks.Add(client.PublishAsync("/unknown/topic", "Hello World"));
                        }

                        await Task.WhenAll(publishTasks).ConfigureAwait(false);

                        var subscriptions = topicsToManage.Select(topic => new Subscription(new TopicFilter(topic))).ToList();
                        var unsubscribeOptions = new UnsubscribeOptions { Subscriptions = subscriptions };
                        _ = await client.UnsubscribeAsync(unsubscribeOptions).ConfigureAwait(false);

                        _ = Interlocked.Increment(ref successCount);
                    }
                    catch (Exception e)
                    {
                        var errorMessage = $"Worker {workerId}, Iteration {i}: {e}";
                        exceptionMessages.Add(errorMessage);
                        this.testOutputHelper.WriteLine(errorMessage);
                    }
                }
            }));
        }

        await Task.WhenAll(tasks).ConfigureAwait(false);

        Assert.Equal(totalExpectedSuccesses, successCount);
        Assert.Empty(exceptionMessages);
    }
}
