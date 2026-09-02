namespace HiveMQtt.Test.HiveMQClient.Plan;

using System.Threading;
using System.Threading.Tasks;
using HiveMQtt.Client;
using HiveMQtt.MQTT5.ReasonCodes;
using HiveMQtt.MQTT5.Types;
using Xunit;

[Collection("Broker")]
public class PacketIDManagerTest
{
    [Fact]
    public async Task Send_1Mio_QoS1_QoS2_Messages_All_Ids_Released_Async()
    {
        // Arrange
        var clientOptions = new HiveMQClientOptionsBuilder()
            .WithClientId("PacketIDManagerTestClient")
            .WithBroker("localhost")
            .WithPort(1883)
            .Build();

        var client = new HiveMQClient(clientOptions);
        await client.ConnectAsync().ConfigureAwait(true);

        var packetIdManager = client.Connection.GetPacketIDManager(); // Assuming the client exposes the manager for validation
        Assert.Equal(0, packetIdManager.Count);

        // Testing with 500k messages, 250k QoS1 and 250k QoS2
        var qos1Messages = 250000;
        var qos2Messages = 250000;

        // Act
        for (var i = 0; i < qos1Messages; i++)
        {
#pragma warning disable IDE0300 // Collection initialization can be simplified - not available in .NET 6
            await client.PublishAsync(
                topic: "test/qos1",
                payload: new byte[] { 0x01 },
                qos: QualityOfService.AtLeastOnceDelivery).ConfigureAwait(true);
#pragma warning restore IDE0300
        }

        for (var i = 0; i < qos2Messages; i++)
        {
#pragma warning disable IDE0300 // Collection initialization can be simplified - not available in .NET 6
            await client.PublishAsync(
                topic: "test/qos2",
                payload: new byte[] { 0x02 },
                qos: QualityOfService.ExactlyOnceDelivery).ConfigureAwait(true);
#pragma warning restore IDE0300
        }

        await client.DisconnectAsync().ConfigureAwait(true);

        // Assert
        Assert.Equal(0, packetIdManager.Count); // All Packet IDs must be released
    }

    [Fact]
    public async Task Incoming_QoS1_Traffic_Does_Not_Grow_FreedPacketIds_Async()
    {
        const string topic = "tests/PacketIDManager/IncomingQoS1NoLeak";
        const int messageCount = 2000;

        var publisher = new HiveMQClient(new HiveMQClientOptionsBuilder()
            .WithClientId("PacketIDManagerIncomingLeakPublisher")
            .WithBroker("localhost")
            .WithPort(1883)
            .Build());

        var subscriber = new HiveMQClient(new HiveMQClientOptionsBuilder()
            .WithClientId("PacketIDManagerIncomingLeakSubscriber")
            .WithBroker("localhost")
            .WithPort(1883)
            .Build());

        await publisher.ConnectAsync().ConfigureAwait(true);
        await subscriber.ConnectAsync().ConfigureAwait(true);

        var received = 0;
        using var allReceived = new ManualResetEventSlim(false);
        subscriber.OnMessageReceived += (_, _) =>
        {
            if (Interlocked.Increment(ref received) >= messageCount)
            {
                allReceived.Set();
            }
        };

        var subResult = await subscriber.SubscribeAsync(topic, QualityOfService.AtLeastOnceDelivery).ConfigureAwait(true);
        Assert.Equal(SubAckReasonCode.GrantedQoS1, subResult.Subscriptions[0].SubscribeReasonCode);

        var packetIdManager = subscriber.Connection.GetPacketIDManager();
        await WaitForConditionAsync(() => packetIdManager.Count == 0, TimeSpan.FromSeconds(5)).ConfigureAwait(true);

        for (var i = 0; i < messageCount; i++)
        {
#pragma warning disable IDE0300
            await publisher.PublishAsync(topic, new byte[] { 0x01 }, QualityOfService.AtLeastOnceDelivery).ConfigureAwait(true);
#pragma warning restore IDE0300
        }

        Assert.True(allReceived.Wait(TimeSpan.FromSeconds(60)), $"Only received {received} of {messageCount} messages.");

        // Allow in-flight PubAcks to finish
        await Task.Delay(500).ConfigureAwait(true);

        // Before the fix, FreedCount would grow ~1:1 with incoming QoS1 messages
        Assert.Equal(messageCount, received);
        Assert.True(
            packetIdManager.FreedCount < 16,
            $"FreedPacketIds grew unexpectedly: FreedCount={packetIdManager.FreedCount}, Count={packetIdManager.Count}");
        Assert.Equal(0, packetIdManager.Count);

        await publisher.DisconnectAsync().ConfigureAwait(true);
        await subscriber.DisconnectAsync().ConfigureAwait(true);
        publisher.Dispose();
        subscriber.Dispose();
    }

    [Fact]
    public async Task Subscribe_And_Unsubscribe_Release_Packet_Ids_Async()
    {
        const string topic = "tests/PacketIDManager/SubUnsubRelease";

        var client = new HiveMQClient(new HiveMQClientOptionsBuilder()
            .WithClientId("PacketIDManagerSubUnsubRelease")
            .WithBroker("localhost")
            .WithPort(1883)
            .Build());

        await client.ConnectAsync().ConfigureAwait(true);
        var packetIdManager = client.Connection.GetPacketIDManager();
        Assert.Equal(0, packetIdManager.Count);

        for (var i = 0; i < 20; i++)
        {
            await client.SubscribeAsync(topic, QualityOfService.AtLeastOnceDelivery).ConfigureAwait(true);
            await WaitForConditionAsync(() => packetIdManager.Count == 0, TimeSpan.FromSeconds(5)).ConfigureAwait(true);

            await client.UnsubscribeAsync(topic).ConfigureAwait(true);
            await WaitForConditionAsync(() => packetIdManager.Count == 0, TimeSpan.FromSeconds(5)).ConfigureAwait(true);
        }

        // Reuse queue may hold recently freed IDs, but must stay bounded by sub/unsub cycle count
        Assert.True(packetIdManager.FreedCount <= 40, $"FreedCount={packetIdManager.FreedCount}");

        await client.DisconnectAsync().ConfigureAwait(true);
        client.Dispose();
    }

    private static async Task WaitForConditionAsync(Func<bool> condition, TimeSpan timeout)
    {
        var deadline = DateTime.UtcNow + timeout;
        while (!condition())
        {
            if (DateTime.UtcNow > deadline)
            {
                Assert.True(false, $"Condition not met within {timeout.TotalSeconds}s.");
            }

            await Task.Delay(10).ConfigureAwait(true);
        }
    }
}
