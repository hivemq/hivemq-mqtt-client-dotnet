namespace HiveMQtt.Test.HiveMQClient;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HiveMQtt.Client;
using HiveMQtt.Client.Events;
using HiveMQtt.MQTT5.ReasonCodes;
using HiveMQtt.MQTT5.Types;
using Xunit;

internal static class MessageOrderingTestHelper
{
    internal const int DefaultMessageCount = 50;

    internal static async Task<List<int>> RunOrderedReceiveTestAsync(
        HiveMQClient subscriber,
        HiveMQClient publisher,
        string testTopic,
        QualityOfService qos,
        Action<OnMessageReceivedEventArgs>? onMessageReceived = null,
        int messageCount = DefaultMessageCount,
        int settleMs = 2000)
    {
        var seen = new List<int>();
        var seenLock = new object();
        var receivedCount = 0;

        subscriber.OnMessageReceived += (_, args) =>
        {
            var idx = int.Parse(args.PublishMessage.PayloadAsString ?? "-1", System.Globalization.CultureInfo.InvariantCulture);
            Thread.Sleep(idx % 3 == 0 ? 5 : 1);
            onMessageReceived?.Invoke(args);
            lock (seenLock)
            {
                seen.Add(idx);
                receivedCount++;
            }
        };

        var subConnect = await subscriber.ConnectAsync().ConfigureAwait(false);
        Assert.True(subConnect.ReasonCode == ConnAckReasonCode.Success);

        var pubConnect = await publisher.ConnectAsync().ConfigureAwait(false);
        Assert.True(pubConnect.ReasonCode == ConnAckReasonCode.Success);

        await subscriber.SubscribeAsync(testTopic, qos).ConfigureAwait(false);

        for (var i = 0; i < messageCount; i++)
        {
            await publisher.PublishAsync(testTopic, i.ToString(System.Globalization.CultureInfo.InvariantCulture), qos)
                .ConfigureAwait(false);
        }

        var deadline = Environment.TickCount64 + settleMs;
        while (receivedCount < messageCount && Environment.TickCount64 < deadline)
        {
            await Task.Delay(50).ConfigureAwait(false);
        }

        await subscriber.DisconnectAsync().ConfigureAwait(false);
        await publisher.DisconnectAsync().ConfigureAwait(false);

        Assert.Equal(messageCount, receivedCount);
        return seen;
    }

    internal static void AssertSequentialOrder(IReadOnlyList<int> seen, int messageCount = DefaultMessageCount)
    {
        var expected = Enumerable.Range(0, messageCount).ToList();
        Assert.Equal(expected, seen);
    }
}
