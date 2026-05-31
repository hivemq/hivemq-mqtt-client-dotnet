namespace HiveMQtt.Test.HiveMQClient;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using HiveMQtt.Client;
using HiveMQtt.MQTT5.Types;
using Xunit;

[Collection("Broker")]
public class MessageOrderingTest
{
#pragma warning disable IDE0300 // Collection initialization - array syntax required for StyleCop SA1010
    private static readonly string[] GlobalBeforePerSubExpected = new[] { "global", "persub" };

    private static readonly int[] AsyncStartOrderExpected = new[] { 0, 1 };
#pragma warning restore IDE0300

    [Fact]
    public async Task QoS1_GlobalHandler_PreservesInvocationOrderAsync()
    {
        var testTopic = $"tests/MessageOrdering/QoS1Global/{Guid.NewGuid():N}";
        using var subscriber = new HiveMQClient(new HiveMQClientOptionsBuilder()
            .WithClientId($"MsgOrderSubQoS1_{Guid.NewGuid():N}")
            .Build());
        using var publisher = new HiveMQClient(new HiveMQClientOptionsBuilder()
            .WithClientId($"MsgOrderPubQoS1_{Guid.NewGuid():N}")
            .Build());

        var seen = await MessageOrderingTestHelper.RunOrderedReceiveTestAsync(
            subscriber,
            publisher,
            testTopic,
            QualityOfService.AtLeastOnceDelivery).ConfigureAwait(false);

        MessageOrderingTestHelper.AssertSequentialOrder(seen);
    }

    [Fact]
    public async Task QoS2_GlobalHandler_PreservesInvocationOrderAsync()
    {
        var testTopic = $"tests/MessageOrdering/QoS2Global/{Guid.NewGuid():N}";
        using var subscriber = new HiveMQClient(new HiveMQClientOptionsBuilder()
            .WithClientId($"MsgOrderSubQoS2_{Guid.NewGuid():N}")
            .Build());
        using var publisher = new HiveMQClient(new HiveMQClientOptionsBuilder()
            .WithClientId($"MsgOrderPubQoS2_{Guid.NewGuid():N}")
            .Build());

        var seen = await MessageOrderingTestHelper.RunOrderedReceiveTestAsync(
            subscriber,
            publisher,
            testTopic,
            QualityOfService.ExactlyOnceDelivery,
            settleMs: 5000).ConfigureAwait(false);

        MessageOrderingTestHelper.AssertSequentialOrder(seen);
    }

    [Fact]
    public async Task QoS1_PerSubscriptionHandler_PreservesInvocationOrderAsync()
    {
        var testTopic = $"tests/MessageOrdering/QoS1PerSub/{Guid.NewGuid():N}";
        using var subscriber = new HiveMQClient(new HiveMQClientOptionsBuilder()
            .WithClientId($"MsgOrderSubPerSub_{Guid.NewGuid():N}")
            .Build());
        using var publisher = new HiveMQClient(new HiveMQClientOptionsBuilder()
            .WithClientId($"MsgOrderPubPerSub_{Guid.NewGuid():N}")
            .Build());

        var seen = new List<int>();
        var seenLock = new object();
        var receivedCount = 0;

        var subscribeOptions = new SubscribeOptionsBuilder()
            .WithSubscription(testTopic, QualityOfService.AtLeastOnceDelivery, messageReceivedHandler: (_, args) =>
            {
                var idx = int.Parse(args.PublishMessage.PayloadAsString ?? "-1", System.Globalization.CultureInfo.InvariantCulture);
                Thread.Sleep(idx % 3 == 0 ? 5 : 1);
                lock (seenLock)
                {
                    seen.Add(idx);
                    receivedCount++;
                }
            })
            .Build();

        await subscriber.ConnectAsync().ConfigureAwait(false);
        await publisher.ConnectAsync().ConfigureAwait(false);
        await subscriber.SubscribeAsync(subscribeOptions).ConfigureAwait(false);

        for (var i = 0; i < MessageOrderingTestHelper.DefaultMessageCount; i++)
        {
            await publisher.PublishAsync(testTopic, i.ToString(System.Globalization.CultureInfo.InvariantCulture), QualityOfService.AtLeastOnceDelivery)
                .ConfigureAwait(false);
        }

        var deadline = Environment.TickCount64 + 3000;
        while (receivedCount < MessageOrderingTestHelper.DefaultMessageCount && Environment.TickCount64 < deadline)
        {
            await Task.Delay(50).ConfigureAwait(false);
        }

        await subscriber.DisconnectAsync().ConfigureAwait(false);
        await publisher.DisconnectAsync().ConfigureAwait(false);

        Assert.Equal(MessageOrderingTestHelper.DefaultMessageCount, receivedCount);
        MessageOrderingTestHelper.AssertSequentialOrder(seen);
    }

    [Fact]
    public async Task QoS1_ManualAck_GlobalHandler_PreservesInvocationOrderAsync()
    {
        var testTopic = $"tests/MessageOrdering/QoS1ManualAck/{Guid.NewGuid():N}";
        using var subscriber = new HiveMQClient(new HiveMQClientOptionsBuilder()
            .WithClientId($"MsgOrderSubManual_{Guid.NewGuid():N}")
            .WithManualAck(true)
            .Build());
        using var publisher = new HiveMQClient(new HiveMQClientOptionsBuilder()
            .WithClientId($"MsgOrderPubManual_{Guid.NewGuid():N}")
            .Build());

        var seen = await MessageOrderingTestHelper.RunOrderedReceiveTestAsync(
            subscriber,
            publisher,
            testTopic,
            QualityOfService.AtLeastOnceDelivery,
            onMessageReceived: args =>
            {
                if (args.PacketIdentifier.HasValue)
                {
                    subscriber.AckAsync(args).GetAwaiter().GetResult();
                }
            },
            messageCount: 20,
            settleMs: 5000).ConfigureAwait(false);

        MessageOrderingTestHelper.AssertSequentialOrder(seen, 20);
    }

    [Fact]
    public async Task QoS1_GlobalBeforePerSubscription_OnSameMessageAsync()
    {
        var testTopic = $"tests/MessageOrdering/GlobalBeforePerSub/{Guid.NewGuid():N}";
        using var subscriber = new HiveMQClient(new HiveMQClientOptionsBuilder()
            .WithClientId($"MsgOrderGlobalFirst_{Guid.NewGuid():N}")
            .Build());
        using var publisher = new HiveMQClient(new HiveMQClientOptionsBuilder()
            .WithClientId($"MsgOrderPubGlobalFirst_{Guid.NewGuid():N}")
            .Build());

        var order = new List<string>();
        var orderLock = new object();
        var received = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

        subscriber.OnMessageReceived += (_, _) =>
        {
            lock (orderLock)
            {
                order.Add("global");
            }
        };

        var subscribeOptions = new SubscribeOptionsBuilder()
            .WithSubscription(testTopic, QualityOfService.AtLeastOnceDelivery, messageReceivedHandler: (_, _) =>
            {
                lock (orderLock)
                {
                    order.Add("persub");
                    if (order.Count == 2)
                    {
                        received.TrySetResult(true);
                    }
                }
            })
            .Build();

        await subscriber.ConnectAsync().ConfigureAwait(false);
        await publisher.ConnectAsync().ConfigureAwait(false);
        await subscriber.SubscribeAsync(subscribeOptions).ConfigureAwait(false);
        await publisher.PublishAsync(testTopic, "x", QualityOfService.AtLeastOnceDelivery).ConfigureAwait(false);

        await received.Task.WaitAsync(TimeSpan.FromSeconds(5)).ConfigureAwait(false);

        await subscriber.DisconnectAsync().ConfigureAwait(false);
        await publisher.DisconnectAsync().ConfigureAwait(false);

        Assert.Equal(GlobalBeforePerSubExpected, order);
    }

    [Fact]
    public async Task QoS1_AsyncHandler_PreservesInvocationStartOrderAsync()
    {
        var testTopic = $"tests/MessageOrdering/AsyncStartOrder/{Guid.NewGuid():N}";
        using var subscriber = new HiveMQClient(new HiveMQClientOptionsBuilder()
            .WithClientId($"MsgOrderAsyncStart_{Guid.NewGuid():N}")
            .Build());
        using var publisher = new HiveMQClient(new HiveMQClientOptionsBuilder()
            .WithClientId($"MsgOrderPubAsyncStart_{Guid.NewGuid():N}")
            .Build());

        var startOrder = new List<int>();
        var completionOrder = new List<int>();
        var lockObj = new object();
        var done = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

        subscriber.OnMessageReceived += async (_, args) =>
        {
            var idx = int.Parse(args.PublishMessage.PayloadAsString ?? "-1", System.Globalization.CultureInfo.InvariantCulture);
            lock (lockObj)
            {
                startOrder.Add(idx);
            }

            await Task.Delay(idx == 0 ? 50 : 1).ConfigureAwait(false);

            lock (lockObj)
            {
                completionOrder.Add(idx);
                if (completionOrder.Count == 2)
                {
                    done.TrySetResult(true);
                }
            }
        };

        await subscriber.ConnectAsync().ConfigureAwait(false);
        await publisher.ConnectAsync().ConfigureAwait(false);
        await subscriber.SubscribeAsync(testTopic, QualityOfService.AtLeastOnceDelivery).ConfigureAwait(false);

        await publisher.PublishAsync(testTopic, "0", QualityOfService.AtLeastOnceDelivery).ConfigureAwait(false);
        await publisher.PublishAsync(testTopic, "1", QualityOfService.AtLeastOnceDelivery).ConfigureAwait(false);

        await done.Task.WaitAsync(TimeSpan.FromSeconds(10)).ConfigureAwait(false);

        await subscriber.DisconnectAsync().ConfigureAwait(false);
        await publisher.DisconnectAsync().ConfigureAwait(false);

        Assert.Equal(AsyncStartOrderExpected, startOrder);
    }

    [Fact]
    public async Task ReentrantPublish_FromHandler_DoesNotDeadlockAsync() =>
        await RunReentrancyTestAsync(async (subscriber, publisher, testTopic, ackTopic) =>
            await publisher.PublishAsync(ackTopic, "done", QualityOfService.AtMostOnceDelivery).ConfigureAwait(false))
            .ConfigureAwait(false);

    [Fact]
    public async Task ReentrantSubscribe_FromHandler_DoesNotDeadlockAsync()
    {
        var testTopic = $"tests/MessageOrdering/ReentrantSubscribe/{Guid.NewGuid():N}";
        var newTopic = $"tests/MessageOrdering/ReentrantSubscribeNew/{Guid.NewGuid():N}";
        using var subscriber = new HiveMQClient(new HiveMQClientOptionsBuilder()
            .WithClientId($"MsgOrderReentSub_{Guid.NewGuid():N}")
            .Build());
        using var publisher = new HiveMQClient(new HiveMQClientOptionsBuilder()
            .WithClientId($"MsgOrderPubReentSub_{Guid.NewGuid():N}")
            .Build());

        var subscribeCompleted = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

        subscriber.OnMessageReceived += (_, args) =>
        {
            if (args.PublishMessage.Topic != testTopic)
            {
                return;
            }

            _ = Task.Run(async () =>
            {
                await subscriber.SubscribeAsync(newTopic, QualityOfService.AtMostOnceDelivery).ConfigureAwait(false);
                subscribeCompleted.TrySetResult(true);
            });
        };

        await subscriber.ConnectAsync().ConfigureAwait(false);
        await publisher.ConnectAsync().ConfigureAwait(false);
        await subscriber.SubscribeAsync(testTopic, QualityOfService.AtLeastOnceDelivery).ConfigureAwait(false);
        await publisher.PublishAsync(testTopic, "trigger", QualityOfService.AtLeastOnceDelivery).ConfigureAwait(false);

        await subscribeCompleted.Task.WaitAsync(TimeSpan.FromSeconds(10)).ConfigureAwait(false);

        await subscriber.DisconnectAsync().ConfigureAwait(false);
        await publisher.DisconnectAsync().ConfigureAwait(false);
    }

    [Fact]
    public async Task ReentrantDisconnect_FromHandler_DoesNotDeadlockAsync()
    {
        var testTopic = $"tests/MessageOrdering/ReentrantDisconnect/{Guid.NewGuid():N}";
        using var subscriber = new HiveMQClient(new HiveMQClientOptionsBuilder()
            .WithClientId($"MsgOrderReentDisc_{Guid.NewGuid():N}")
            .Build());
        using var publisher = new HiveMQClient(new HiveMQClientOptionsBuilder()
            .WithClientId($"MsgOrderPubReentDisc_{Guid.NewGuid():N}")
            .Build());

        var disconnected = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

        subscriber.OnMessageReceived += async (_, _) =>
        {
            await subscriber.DisconnectAsync().ConfigureAwait(false);
            disconnected.TrySetResult(true);
        };

        await subscriber.ConnectAsync().ConfigureAwait(false);
        await publisher.ConnectAsync().ConfigureAwait(false);
        await subscriber.SubscribeAsync(testTopic, QualityOfService.AtLeastOnceDelivery).ConfigureAwait(false);
        await publisher.PublishAsync(testTopic, "trigger", QualityOfService.AtLeastOnceDelivery).ConfigureAwait(false);

        await disconnected.Task.WaitAsync(TimeSpan.FromSeconds(10)).ConfigureAwait(false);
        await publisher.DisconnectAsync().ConfigureAwait(false);
    }

    [Fact]
    public async Task ReentrantAckAsync_FromHandler_DoesNotDeadlockAsync()
    {
        var testTopic = $"tests/MessageOrdering/ReentrantAck/{Guid.NewGuid():N}";
        using var subscriber = new HiveMQClient(new HiveMQClientOptionsBuilder()
            .WithClientId($"MsgOrderReentAck_{Guid.NewGuid():N}")
            .WithManualAck(true)
            .Build());
        using var publisher = new HiveMQClient(new HiveMQClientOptionsBuilder()
            .WithClientId($"MsgOrderPubReentAck_{Guid.NewGuid():N}")
            .Build());

        var acked = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

        subscriber.OnMessageReceived += async (_, args) =>
        {
            await subscriber.AckAsync(args).ConfigureAwait(false);
            acked.TrySetResult(true);
        };

        await subscriber.ConnectAsync().ConfigureAwait(false);
        await publisher.ConnectAsync().ConfigureAwait(false);
        await subscriber.SubscribeAsync(testTopic, QualityOfService.AtLeastOnceDelivery).ConfigureAwait(false);
        await publisher.PublishAsync(testTopic, "1", QualityOfService.AtLeastOnceDelivery).ConfigureAwait(false);

        await acked.Task.WaitAsync(TimeSpan.FromSeconds(10)).ConfigureAwait(false);

        await subscriber.DisconnectAsync().ConfigureAwait(false);
        await publisher.DisconnectAsync().ConfigureAwait(false);
    }

    private static async Task RunReentrancyTestAsync(Func<HiveMQClient, HiveMQClient, string, string, Task> handlerAction)
    {
        var testTopic = $"tests/MessageOrdering/Reentrant/{Guid.NewGuid():N}";
        var ackTopic = $"tests/MessageOrdering/ReentrantAck/{Guid.NewGuid():N}";
        using var subscriber = new HiveMQClient(new HiveMQClientOptionsBuilder()
            .WithClientId($"MsgOrderReent_{Guid.NewGuid():N}")
            .Build());
        using var publisher = new HiveMQClient(new HiveMQClientOptionsBuilder()
            .WithClientId($"MsgOrderPubReent_{Guid.NewGuid():N}")
            .Build());

        var completed = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

        subscriber.OnMessageReceived += async (_, args) =>
        {
            if (args.PublishMessage.Topic == ackTopic)
            {
                completed.TrySetResult(true);
                return;
            }

            if (args.PublishMessage.Topic == testTopic)
            {
                await handlerAction(subscriber, publisher, testTopic, ackTopic).ConfigureAwait(false);
            }
        };

        await subscriber.ConnectAsync().ConfigureAwait(false);
        await publisher.ConnectAsync().ConfigureAwait(false);
        await subscriber.SubscribeAsync(testTopic, QualityOfService.AtLeastOnceDelivery).ConfigureAwait(false);
        await subscriber.SubscribeAsync(ackTopic, QualityOfService.AtMostOnceDelivery).ConfigureAwait(false);
        await publisher.PublishAsync(testTopic, "trigger", QualityOfService.AtLeastOnceDelivery).ConfigureAwait(false);

        await completed.Task.WaitAsync(TimeSpan.FromSeconds(10)).ConfigureAwait(false);

        await subscriber.DisconnectAsync().ConfigureAwait(false);
        await publisher.DisconnectAsync().ConfigureAwait(false);
    }
}
