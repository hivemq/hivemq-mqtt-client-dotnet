namespace HiveMQtt.Test.RawClient;

using System.Text;
using System.Threading.Tasks;
using HiveMQtt.Client;
using HiveMQtt.Client.Exceptions;
using HiveMQtt.Client.Options;
using HiveMQtt.MQTT5.ReasonCodes;
using HiveMQtt.MQTT5.Types;
using Xunit;

public class RawClientErrorTest
{
    [Fact]
    public async Task PublishWithInvalidTopicAliasAsync()
    {
        // Note: This test assumes the broker doesn't support topic aliases
        // In a real scenario, we'd need to check broker capabilities first
        var options = new HiveMQClientOptionsBuilder().WithClientId("RawClientPublishWithInvalidTopicAliasAsync").Build();
        var client = new RawClient(options);

        var connectResult = await client.ConnectAsync().ConfigureAwait(false);
        Assert.True(connectResult.ReasonCode == ConnAckReasonCode.Success);

        // Check if broker supports topic aliases
        var topicAliasMaximum = connectResult.Properties?.TopicAliasMaximum ?? 0;

        if (topicAliasMaximum == 0)
        {
            // Broker doesn't support topic aliases
            var message = new MQTT5PublishMessage
            {
                Topic = "test/topic",
                Payload = Encoding.UTF8.GetBytes("test"),
                QoS = QualityOfService.AtMostOnceDelivery,
                TopicAlias = 1, // Try to use topic alias when not supported
            };

            await Assert.ThrowsAsync<HiveMQttClientException>(() => client.PublishAsync(message)).ConfigureAwait(false);
        }

        await client.DisconnectAsync().ConfigureAwait(false);
        client.Dispose();
    }

    [Fact]
    public async Task PublishWithTopicAliasExceedingMaximumAsync()
    {
        var options = new HiveMQClientOptionsBuilder().WithClientId("RawClientPublishWithTopicAliasExceedingMaximumAsync").Build();
        var client = new RawClient(options);

        var connectResult = await client.ConnectAsync().ConfigureAwait(false);
        Assert.True(connectResult.ReasonCode == ConnAckReasonCode.Success);

        var topicAliasMaximum = connectResult.Properties?.TopicAliasMaximum ?? 0;

        if (topicAliasMaximum > 0)
        {
            // Try to use a topic alias that exceeds the maximum
            var message = new MQTT5PublishMessage
            {
                Topic = "test/topic",
                Payload = Encoding.UTF8.GetBytes("test"),
                QoS = QualityOfService.AtMostOnceDelivery,
                TopicAlias = topicAliasMaximum + 1,
            };

            await Assert.ThrowsAsync<HiveMQttClientException>(() => client.PublishAsync(message)).ConfigureAwait(false);
        }

        await client.DisconnectAsync().ConfigureAwait(false);
        client.Dispose();
    }

    [Fact]
    public async Task PublishWithRetainWhenNotSupportedAsync()
    {
        var options = new HiveMQClientOptionsBuilder().WithClientId("RawClientPublishWithRetainWhenNotSupportedAsync").Build();
        var client = new RawClient(options);

        var connectResult = await client.ConnectAsync().ConfigureAwait(false);
        Assert.True(connectResult.ReasonCode == ConnAckReasonCode.Success);

        var retainAvailable = connectResult.Properties?.RetainAvailable ?? true;

        if (!retainAvailable)
        {
            // Try to publish with retain when not supported
            var message = new MQTT5PublishMessage
            {
                Topic = "test/topic",
                Payload = Encoding.UTF8.GetBytes("test"),
                QoS = QualityOfService.AtMostOnceDelivery,
                Retain = true,
            };

            await Assert.ThrowsAsync<HiveMQttClientException>(() => client.PublishAsync(message)).ConfigureAwait(false);
        }

        await client.DisconnectAsync().ConfigureAwait(false);
        client.Dispose();
    }

    [Fact]
    public async Task SubscribeWithSubscriptionIdentifierWhenNotSupportedAsync()
    {
        var options = new HiveMQClientOptionsBuilder().WithClientId("RawClientSubscribeWithSubscriptionIdentifierWhenNotSupportedAsync").Build();
        var client = new RawClient(options);

        var connectResult = await client.ConnectAsync().ConfigureAwait(false);
        Assert.True(connectResult.ReasonCode == ConnAckReasonCode.Success);

        var subscriptionIdentifiersAvailable = connectResult.Properties?.SubscriptionIdentifiersAvailable ?? true;

        if (!subscriptionIdentifiersAvailable)
        {
            var subscribeOptions = new SubscribeOptions
            {
                SubscriptionIdentifier = 1,
            };
            subscribeOptions.TopicFilters.Add(new TopicFilter("test/topic", QualityOfService.AtMostOnceDelivery));

            await Assert.ThrowsAsync<HiveMQttClientException>(() => client.SubscribeAsync(subscribeOptions)).ConfigureAwait(false);
        }

        await client.DisconnectAsync().ConfigureAwait(false);
        client.Dispose();
    }

    [Fact]
    public async Task SubscribeWithRetainAsPublishedWhenNotSupportedAsync()
    {
        var options = new HiveMQClientOptionsBuilder().WithClientId("RawClientSubscribeWithRetainAsPublishedWhenNotSupportedAsync").Build();
        var client = new RawClient(options);

        var connectResult = await client.ConnectAsync().ConfigureAwait(false);
        Assert.True(connectResult.ReasonCode == ConnAckReasonCode.Success);

        var retainAvailable = connectResult.Properties?.RetainAvailable ?? true;

        if (!retainAvailable)
        {
            var subscribeOptions = new SubscribeOptions();
            subscribeOptions.TopicFilters.Add(new TopicFilter("test/topic", QualityOfService.AtMostOnceDelivery)
            {
                RetainAsPublished = true,
            });

            await Assert.ThrowsAsync<HiveMQttClientException>(() => client.SubscribeAsync(subscribeOptions)).ConfigureAwait(false);
        }

        await client.DisconnectAsync().ConfigureAwait(false);
        client.Dispose();
    }

    [Fact]
    public async Task SubscribeWithSharedSubscriptionWhenNotSupportedAsync()
    {
        var options = new HiveMQClientOptionsBuilder().WithClientId("RawClientSubscribeWithSharedSubscriptionWhenNotSupportedAsync").Build();
        var client = new RawClient(options);

        var connectResult = await client.ConnectAsync().ConfigureAwait(false);
        Assert.True(connectResult.ReasonCode == ConnAckReasonCode.Success);

        var sharedSubscriptionAvailable = connectResult.Properties?.SharedSubscriptionAvailable ?? true;

        if (!sharedSubscriptionAvailable)
        {
            var subscribeOptions = new SubscribeOptions();
            subscribeOptions.TopicFilters.Add(new TopicFilter("$share/group/test/topic", QualityOfService.AtMostOnceDelivery));

            await Assert.ThrowsAsync<HiveMQttClientException>(() => client.SubscribeAsync(subscribeOptions)).ConfigureAwait(false);
        }

        await client.DisconnectAsync().ConfigureAwait(false);
        client.Dispose();
    }

    [Fact]
    public async Task SubscribeWithWildcardWhenNotSupportedAsync()
    {
        var options = new HiveMQClientOptionsBuilder().WithClientId("RawClientSubscribeWithWildcardWhenNotSupportedAsync").Build();
        var client = new RawClient(options);

        var connectResult = await client.ConnectAsync().ConfigureAwait(false);
        Assert.True(connectResult.ReasonCode == ConnAckReasonCode.Success);

        var wildcardSubscriptionAvailable = connectResult.Properties?.WildcardSubscriptionAvailable ?? true;

        if (!wildcardSubscriptionAvailable)
        {
            var subscribeOptions = new SubscribeOptions();
            subscribeOptions.TopicFilters.Add(new TopicFilter("test/+/topic", QualityOfService.AtMostOnceDelivery));

            await Assert.ThrowsAsync<HiveMQttClientException>(() => client.SubscribeAsync(subscribeOptions)).ConfigureAwait(false);
        }

        await client.DisconnectAsync().ConfigureAwait(false);
        client.Dispose();
    }

    [Fact]
    public async Task PublishWithQoSExceedingMaximumAsync()
    {
        var options = new HiveMQClientOptionsBuilder().WithClientId("RawClientPublishWithQoSExceedingMaximumAsync").Build();
        var client = new RawClient(options);

        var connectResult = await client.ConnectAsync().ConfigureAwait(false);
        Assert.True(connectResult.ReasonCode == ConnAckReasonCode.Success);

        var maximumQoS = connectResult.Properties?.MaximumQoS;

        if (maximumQoS.HasValue && maximumQoS.Value < 2)
        {
            // Try to publish with QoS 2 when broker only supports up to QoS 1
            var message = new MQTT5PublishMessage
            {
                Topic = "test/topic",
                Payload = Encoding.UTF8.GetBytes("test"),
                QoS = QualityOfService.ExactlyOnceDelivery, // QoS 2
            };

            // Should not throw, but QoS should be reduced
            var result = await client.PublishAsync(message).ConfigureAwait(false);
            Assert.NotNull(result);
            // QoS should be reduced to maximumQoS
            Assert.True((int)result.Message.QoS.Value <= maximumQoS.Value);
        }

        await client.DisconnectAsync().ConfigureAwait(false);
        client.Dispose();
    }
}
