namespace HiveMQtt.Test.RawClient;

using System.Threading.Tasks;
using HiveMQtt.Client;
using HiveMQtt.Client.Exceptions;
using HiveMQtt.Client.Options;
using HiveMQtt.MQTT5.ReasonCodes;
using HiveMQtt.MQTT5.Types;
using Xunit;

[Collection("Broker")]
public class RawClientSubscribeTest
{
    [Fact]
    public async Task SubscribeWithSubscribeOptionsAsync()
    {
        var testTopic1 = "tests/RawClientSubscribeWithSubscribeOptionsAsync1";
        var testTopic2 = "tests/RawClientSubscribeWithSubscribeOptionsAsync2";
        var options = new HiveMQClientOptionsBuilder().WithClientId("RawClientSubscribeWithSubscribeOptionsAsync").Build();
        var client = new RawClient(options);

        var connectResult = await client.ConnectAsync().ConfigureAwait(false);
        Assert.True(connectResult.ReasonCode == ConnAckReasonCode.Success);

        var subscribeOptions = new SubscribeOptions();
        subscribeOptions.TopicFilters.Add(new TopicFilter(testTopic1, QualityOfService.AtLeastOnceDelivery));
        subscribeOptions.TopicFilters.Add(new TopicFilter(testTopic2, QualityOfService.AtMostOnceDelivery));

        var subResult = await client.SubscribeAsync(subscribeOptions).ConfigureAwait(false);
        Assert.NotEmpty(subResult.Subscriptions);
        Assert.Equal(2, subResult.Subscriptions.Count);
        Assert.Equal(SubAckReasonCode.GrantedQoS1, subResult.Subscriptions[0].SubscribeReasonCode);
        Assert.Equal(SubAckReasonCode.GrantedQoS0, subResult.Subscriptions[1].SubscribeReasonCode);

        await client.DisconnectAsync().ConfigureAwait(false);
        client.Dispose();
    }

    [Fact]
    public async Task SubscribeWhenNotConnectedAsync()
    {
        var options = new HiveMQClientOptionsBuilder().WithClientId("RawClientSubscribeWhenNotConnectedAsync").Build();
        var client = new RawClient(options);

        await Assert.ThrowsAsync<HiveMQttClientException>(() => client.SubscribeAsync("test/topic")).ConfigureAwait(false);

        client.Dispose();
    }

    [Fact]
    public async Task TestSubscribeEventsAsync()
    {
        var testTopic = "tests/RawClientTestSubscribeEventsAsync";
        var options = new HiveMQClientOptionsBuilder().WithClientId("RawClientTestSubscribeEventsAsync").Build();
        var client = new RawClient(options);

        var beforeSubscribeCalled = false;
        var afterSubscribeCalled = false;
        var onSubscribeSentCalled = false;
        var onSubAckReceivedCalled = false;

        client.BeforeSubscribe += (sender, args) =>
        {
            beforeSubscribeCalled = true;
            Assert.NotNull(args.Options);
            Assert.NotEmpty(args.Options.TopicFilters);
        };

        client.AfterSubscribe += (sender, args) =>
        {
            afterSubscribeCalled = true;
            Assert.NotNull(args.SubscribeResult);
            Assert.NotEmpty(args.SubscribeResult.Subscriptions);
        };

        client.OnSubscribeSent += (sender, args) =>
        {
            onSubscribeSentCalled = true;
            Assert.NotNull(args.SubscribePacket);
        };

        client.OnSubAckReceived += (sender, args) =>
        {
            onSubAckReceivedCalled = true;
            Assert.NotNull(args.SubAckPacket);
        };

        var connectResult = await client.ConnectAsync().ConfigureAwait(false);
        Assert.True(connectResult.ReasonCode == ConnAckReasonCode.Success);

        await client.SubscribeAsync(testTopic).ConfigureAwait(false);

        // Wait for events to complete
        await Task.Delay(200).ConfigureAwait(false);

        Assert.True(beforeSubscribeCalled);
        Assert.True(afterSubscribeCalled);
        Assert.True(onSubscribeSentCalled);
        Assert.True(onSubAckReceivedCalled);

        await client.DisconnectAsync().ConfigureAwait(false);
        client.Dispose();
    }

    [Fact]
    public async Task SubscribeWithAllQoSLevelsAsync()
    {
        var testTopic0 = "tests/RawClientSubscribeWithAllQoSLevelsAsync0";
        var testTopic1 = "tests/RawClientSubscribeWithAllQoSLevelsAsync1";
        var testTopic2 = "tests/RawClientSubscribeWithAllQoSLevelsAsync2";
        var options = new HiveMQClientOptionsBuilder().WithClientId("RawClientSubscribeWithAllQoSLevelsAsync").Build();
        var client = new RawClient(options);

        var connectResult = await client.ConnectAsync().ConfigureAwait(false);
        Assert.True(connectResult.ReasonCode == ConnAckReasonCode.Success);

        // Subscribe with QoS 0
        var subResult0 = await client.SubscribeAsync(testTopic0, QualityOfService.AtMostOnceDelivery).ConfigureAwait(false);
        Assert.Equal(SubAckReasonCode.GrantedQoS0, subResult0.Subscriptions[0].SubscribeReasonCode);

        // Subscribe with QoS 1
        var subResult1 = await client.SubscribeAsync(testTopic1, QualityOfService.AtLeastOnceDelivery).ConfigureAwait(false);
        Assert.Equal(SubAckReasonCode.GrantedQoS1, subResult1.Subscriptions[0].SubscribeReasonCode);

        // Subscribe with QoS 2
        var subResult2 = await client.SubscribeAsync(testTopic2, QualityOfService.ExactlyOnceDelivery).ConfigureAwait(false);
        Assert.Equal(SubAckReasonCode.GrantedQoS2, subResult2.Subscriptions[0].SubscribeReasonCode);

        await client.DisconnectAsync().ConfigureAwait(false);
        client.Dispose();
    }

    [Fact]
    public async Task SubscribeWithSubscriptionOptionsAsync()
    {
        var testTopic = "tests/RawClientSubscribeWithSubscriptionOptionsAsync";
        var options = new HiveMQClientOptionsBuilder().WithClientId("RawClientSubscribeWithSubscriptionOptionsAsync").Build();
        var client = new RawClient(options);

        var connectResult = await client.ConnectAsync().ConfigureAwait(false);
        Assert.True(connectResult.ReasonCode == ConnAckReasonCode.Success);

        // Subscribe with various subscription options
        var subResult = await client.SubscribeAsync(
            testTopic,
            QualityOfService.AtLeastOnceDelivery,
            noLocal: true,
            retainAsPublished: true,
            retainHandling: RetainHandling.SendAtSubscribe).ConfigureAwait(false);

        Assert.NotEmpty(subResult.Subscriptions);
        Assert.Equal(SubAckReasonCode.GrantedQoS1, subResult.Subscriptions[0].SubscribeReasonCode);

        await client.DisconnectAsync().ConfigureAwait(false);
        client.Dispose();
    }
}
