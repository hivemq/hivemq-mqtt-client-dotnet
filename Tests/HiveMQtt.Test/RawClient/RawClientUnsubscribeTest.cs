namespace HiveMQtt.Test.RawClient;

using System.Threading.Tasks;
using HiveMQtt.Client;
using HiveMQtt.Client.Events;
using HiveMQtt.Client.Exceptions;
using HiveMQtt.Client.Options;
using HiveMQtt.MQTT5.ReasonCodes;
using HiveMQtt.MQTT5.Types;
using Xunit;

public class RawClientUnsubscribeTest
{
    [Fact]
    public async Task BasicUnsubscribeAsync()
    {
        var testTopic = "tests/RawClientBasicUnsubscribeAsync";
        var options = new HiveMQClientOptionsBuilder().WithClientId("RawClientBasicUnsubscribeAsync").Build();
        var client = new RawClient(options);

        var connectResult = await client.ConnectAsync().ConfigureAwait(false);
        Assert.True(connectResult.ReasonCode == ConnAckReasonCode.Success);

        // Subscribe first
        var subResult = await client.SubscribeAsync(testTopic).ConfigureAwait(false);
        Assert.NotEmpty(subResult.Subscriptions);

        // Unsubscribe
        var unsubscribeOptions = new UnsubscribeOptions();
        unsubscribeOptions.Subscriptions.Add(new Subscription(testTopic));

        var unsubResult = await client.UnsubscribeAsync(unsubscribeOptions).ConfigureAwait(false);
        Assert.NotEmpty(unsubResult.Subscriptions);
        Assert.Equal(UnsubAckReasonCode.Success, unsubResult.Subscriptions[0].UnsubscribeReasonCode);

        await client.DisconnectAsync().ConfigureAwait(false);
        client.Dispose();
    }

    [Fact]
    public async Task UnsubscribeMultipleTopicsAsync()
    {
        var testTopic1 = "tests/RawClientUnsubscribeMultipleTopicsAsync1";
        var testTopic2 = "tests/RawClientUnsubscribeMultipleTopicsAsync2";
        var testTopic3 = "tests/RawClientUnsubscribeMultipleTopicsAsync3";
        var options = new HiveMQClientOptionsBuilder().WithClientId("RawClientUnsubscribeMultipleTopicsAsync").Build();
        var client = new RawClient(options);

        var connectResult = await client.ConnectAsync().ConfigureAwait(false);
        Assert.True(connectResult.ReasonCode == ConnAckReasonCode.Success);

        // Subscribe to multiple topics
        var subResult1 = await client.SubscribeAsync(testTopic1).ConfigureAwait(false);
        var subResult2 = await client.SubscribeAsync(testTopic2).ConfigureAwait(false);
        var subResult3 = await client.SubscribeAsync(testTopic3).ConfigureAwait(false);

        // Unsubscribe from all
        var unsubscribeOptions = new UnsubscribeOptions();
        unsubscribeOptions.Subscriptions.Add(new Subscription(testTopic1));
        unsubscribeOptions.Subscriptions.Add(new Subscription(testTopic2));
        unsubscribeOptions.Subscriptions.Add(new Subscription(testTopic3));

        var unsubResult = await client.UnsubscribeAsync(unsubscribeOptions).ConfigureAwait(false);
        Assert.Equal(3, unsubResult.Subscriptions.Count);
        Assert.All(unsubResult.Subscriptions, sub => Assert.Equal(UnsubAckReasonCode.Success, sub.UnsubscribeReasonCode));

        await client.DisconnectAsync().ConfigureAwait(false);
        client.Dispose();
    }

    [Fact]
    public async Task UnsubscribeWhenNotConnectedAsync()
    {
        var options = new HiveMQClientOptionsBuilder().WithClientId("RawClientUnsubscribeWhenNotConnectedAsync").Build();
        var client = new RawClient(options);

        var unsubscribeOptions = new UnsubscribeOptions();
        unsubscribeOptions.Subscriptions.Add(new Subscription("test/topic"));

        await Assert.ThrowsAsync<HiveMQttClientException>(() => client.UnsubscribeAsync(unsubscribeOptions)).ConfigureAwait(false);

        client.Dispose();
    }

    [Fact]
    public async Task TestUnsubscribeEventsAsync()
    {
        var testTopic = "tests/RawClientTestUnsubscribeEventsAsync";
        var options = new HiveMQClientOptionsBuilder().WithClientId("RawClientTestUnsubscribeEventsAsync").Build();
        var client = new RawClient(options);

        var beforeUnsubscribeCalled = false;
        var afterUnsubscribeCalled = false;
        var onUnsubscribeSentCalled = false;
        var onUnsubAckReceivedCalled = false;

        client.BeforeUnsubscribe += (sender, args) =>
        {
            beforeUnsubscribeCalled = true;
            Assert.NotNull(args.Subscriptions);
            Assert.NotEmpty(args.Subscriptions);
        };

        client.AfterUnsubscribe += (sender, args) =>
        {
            afterUnsubscribeCalled = true;
            Assert.NotNull(args.UnsubscribeResult);
            Assert.NotEmpty(args.UnsubscribeResult.Subscriptions);
        };

        client.OnUnsubscribeSent += (sender, args) =>
        {
            onUnsubscribeSentCalled = true;
            Assert.NotNull(args.UnsubscribePacket);
        };

        client.OnUnsubAckReceived += (sender, args) =>
        {
            onUnsubAckReceivedCalled = true;
            Assert.NotNull(args.UnsubAckPacket);
        };

        var connectResult = await client.ConnectAsync().ConfigureAwait(false);
        Assert.True(connectResult.ReasonCode == ConnAckReasonCode.Success);

        // Subscribe first
        await client.SubscribeAsync(testTopic).ConfigureAwait(false);

        // Unsubscribe
        var unsubscribeOptions = new UnsubscribeOptions();
        unsubscribeOptions.Subscriptions.Add(new Subscription(testTopic));

        await client.UnsubscribeAsync(unsubscribeOptions).ConfigureAwait(false);

        // Wait for events to complete
        await Task.Delay(200).ConfigureAwait(false);

        Assert.True(beforeUnsubscribeCalled);
        Assert.True(afterUnsubscribeCalled);
        Assert.True(onUnsubscribeSentCalled);
        Assert.True(onUnsubAckReceivedCalled);

        await client.DisconnectAsync().ConfigureAwait(false);
        client.Dispose();
    }
}
