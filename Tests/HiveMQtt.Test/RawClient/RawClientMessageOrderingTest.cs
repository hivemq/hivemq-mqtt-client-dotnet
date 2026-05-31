namespace HiveMQtt.Test.RawClient;

using System;
using System.Threading.Tasks;
using HiveMQtt.Client;
using HiveMQtt.MQTT5.Types;
using HiveMQtt.Test.HiveMQClient;
using Xunit;

[Collection("Broker")]
public class RawClientMessageOrderingTest
{
    [Fact]
    public async Task QoS1_GlobalHandler_PreservesInvocationOrderAsync()
    {
        var testTopic = $"tests/RawClientMessageOrdering/QoS1/{Guid.NewGuid():N}";
        using var subscriber = new RawClient(new HiveMQClientOptionsBuilder()
            .WithClientId($"RawMsgOrderSub_{Guid.NewGuid():N}")
            .Build());
        using var publisher = new HiveMQClient(new HiveMQClientOptionsBuilder()
            .WithClientId($"RawMsgOrderPub_{Guid.NewGuid():N}")
            .Build());

        var seen = new List<int>();
        var seenLock = new object();
        var receivedCount = 0;

        subscriber.OnMessageReceived += (_, args) =>
        {
            var idx = int.Parse(args.PublishMessage.PayloadAsString ?? "-1", System.Globalization.CultureInfo.InvariantCulture);
            Thread.Sleep(idx % 3 == 0 ? 5 : 1);
            lock (seenLock)
            {
                seen.Add(idx);
                receivedCount++;
            }
        };

        await subscriber.ConnectAsync().ConfigureAwait(false);
        await publisher.ConnectAsync().ConfigureAwait(false);

        await subscriber.SubscribeAsync(testTopic, QualityOfService.AtLeastOnceDelivery).ConfigureAwait(false);

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
}
