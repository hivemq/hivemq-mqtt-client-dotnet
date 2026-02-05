/*
 * Copyright 2025-present HiveMQ and the HiveMQ Community
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

namespace HiveMQtt.Test.HiveMQClient.Plan;

using System.Threading.Tasks;
using HiveMQtt.Client;
using HiveMQtt.Client.Exceptions;
using HiveMQtt.Client.Options;
using HiveMQtt.MQTT5.ReasonCodes;
using HiveMQtt.MQTT5.Types;
using Xunit;
using System.Text;

[Collection("Broker")]
public class BrokerEnforcementTest
{
    [Fact]
    public async Task WildcardSubscriptionAvailable_WhenFalse_RejectsWildcardSubscriptionsAsync()
    {
        // Arrange
        var client = new HiveMQClient();
        _ = await client.ConnectAsync().ConfigureAwait(false);

        // Manually override the connection properties to simulate a broker that does not support wildcard subscriptions
        client.Connection.ConnectionProperties.WildcardSubscriptionAvailable = false;

        var options = new SubscribeOptions();
        options.TopicFilters.Add(new TopicFilter("test/+/topic"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<HiveMQttClientException>(() =>
            client.SubscribeAsync(options)).ConfigureAwait(true);
        Assert.Contains("Wildcard subscriptions are not supported by the broker", exception.Message);
    }

    [Fact]
    public async Task WildcardSubscriptionAvailable_WhenTrue_AcceptsWildcardSubscriptionsAsync()
    {
        // Arrange
        var client = new HiveMQClient();
        _ = await client.ConnectAsync().ConfigureAwait(false);

        // Manually override the connection properties to simulate a broker that supports wildcard subscriptions
        client.Connection.ConnectionProperties.WildcardSubscriptionAvailable = true;

        var options = new SubscribeOptions();
        options.TopicFilters.Add(new TopicFilter("test/+/topic"));

        // Act & Assert
        var result = await client.SubscribeAsync(options).ConfigureAwait(true);
        Assert.NotNull(result);
        Assert.Single(result.Subscriptions);
        Assert.Equal("test/+/topic", result.Subscriptions[0].TopicFilter.Topic);
        Assert.Equal(SubAckReasonCode.GrantedQoS0, result.Subscriptions[0].SubscribeReasonCode);
    }

    [Fact]
    public async Task SharedSubscriptionAvailable_WhenFalse_RejectsSharedSubscriptionsAsync()
    {
        // Arrange
        var client = new HiveMQClient();
        _ = await client.ConnectAsync().ConfigureAwait(false);

        // Manually override the connection properties to simulate a broker that does not support shared subscriptions
        client.Connection.ConnectionProperties.SharedSubscriptionAvailable = false;

        var options = new SubscribeOptions();
        options.TopicFilters.Add(new TopicFilter("$share/group/test/topic"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<HiveMQttClientException>(() =>
            client.SubscribeAsync(options)).ConfigureAwait(true);
        Assert.Contains("Shared subscriptions are not supported by the broker", exception.Message);
    }

    [Fact]
    public async Task SharedSubscriptionAvailable_WhenTrue_AcceptsSharedSubscriptionsAsync()
    {
        // Arrange
        var client = new HiveMQClient();
        _ = await client.ConnectAsync().ConfigureAwait(false);

        // Manually override the connection properties to simulate a broker that supports shared subscriptions
        client.Connection.ConnectionProperties.SharedSubscriptionAvailable = true;

        var options = new SubscribeOptions();
        options.TopicFilters.Add(new TopicFilter("$share/group/test/topic"));

        // Act & Assert
        var result = await client.SubscribeAsync(options).ConfigureAwait(true);
        Assert.NotNull(result);
        Assert.Single(result.Subscriptions);
        Assert.Equal("$share/group/test/topic", result.Subscriptions[0].TopicFilter.Topic);
        Assert.Equal(SubAckReasonCode.GrantedQoS0, result.Subscriptions[0].SubscribeReasonCode);
    }

    [Fact]
    public async Task RetainAvailable_WhenFalse_RejectsRetainedPublishAsync()
    {
        // Arrange
        var client = new HiveMQClient();
        _ = await client.ConnectAsync().ConfigureAwait(false);

        // Manually override the connection properties to simulate a broker that does not support retained messages
        client.Connection.ConnectionProperties.RetainAvailable = false;

        // Update cache to reflect the manual override
        client.UpdateConnectionPropertyCache(client.Connection.ConnectionProperties);

        var message = new MQTT5PublishMessage
        {
            Topic = "test/topic",
            Payload = Encoding.ASCII.GetBytes("test message"),
            Retain = true,
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<HiveMQttClientException>(() =>
            client.PublishAsync(message)).ConfigureAwait(true);
        Assert.Contains("Retained messages are not supported by the broker", exception.Message);
    }

    [Fact]
    public async Task RetainAvailable_WhenTrue_AcceptsRetainedPublishAsync()
    {
        // Arrange
        var client = new HiveMQClient();
        _ = await client.ConnectAsync().ConfigureAwait(false);

        // Manually override the connection properties to simulate a broker that supports retained messages
        client.Connection.ConnectionProperties.RetainAvailable = true;

        var message = new MQTT5PublishMessage
        {
            Topic = "test/topic",
            Payload = Encoding.ASCII.GetBytes("test message"),
            Retain = true,
        };

        // Act & Assert
        var result = await client.PublishAsync(message).ConfigureAwait(true);
        Assert.NotNull(result);
        Assert.True(result.Message.Retain);
    }

    [Fact]
    public async Task RetainAvailable_WhenFalse_RejectsRetainAsPublishedSubscribeAsync()
    {
        // Arrange
        var client = new HiveMQClient();
        _ = await client.ConnectAsync().ConfigureAwait(false);

        // Manually override the connection properties to simulate a broker that does not support retained messages
        client.Connection.ConnectionProperties.RetainAvailable = false;

        // Update cache to reflect the manual override
        client.UpdateConnectionPropertyCache(client.Connection.ConnectionProperties);

        var options = new SubscribeOptions();
        options.TopicFilters.Add(new TopicFilter("test/topic", retainAsPublished: true));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<HiveMQttClientException>(() =>
            client.SubscribeAsync(options)).ConfigureAwait(true);
        Assert.Contains("Retained messages are not supported by the broker", exception.Message);
    }

    [Fact]
    public async Task RetainAvailable_WhenTrue_AcceptsRetainAsPublishedSubscribeAsync()
    {
        // Arrange
        var client = new HiveMQClient();
        _ = await client.ConnectAsync().ConfigureAwait(false);

        // Manually override the connection properties to simulate a broker that supports retained messages
        client.Connection.ConnectionProperties.RetainAvailable = true;

        var options = new SubscribeOptions();
        options.TopicFilters.Add(new TopicFilter("test/topic", retainAsPublished: true));

        // Act & Assert
        var result = await client.SubscribeAsync(options).ConfigureAwait(true);
        Assert.NotNull(result);
        Assert.Single(result.Subscriptions);
        Assert.True(result.Subscriptions[0].TopicFilter.RetainAsPublished);
        Assert.Equal(SubAckReasonCode.GrantedQoS0, result.Subscriptions[0].SubscribeReasonCode);
    }

    [Fact]
    public async Task SubscriptionIdentifiersAvailable_WhenFalse_RejectsSubscriptionIdentifiersAsync()
    {
        // Arrange
        var client = new HiveMQClient();
        _ = await client.ConnectAsync().ConfigureAwait(false);

        // Manually override the connection properties to simulate a broker that does not support subscription identifiers
        client.Connection.ConnectionProperties.SubscriptionIdentifiersAvailable = false;

        var options = new SubscribeOptions
        {
            SubscriptionIdentifier = 1,
        };
        options.TopicFilters.Add(new TopicFilter("test/topic"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<HiveMQttClientException>(() =>
            client.SubscribeAsync(options)).ConfigureAwait(true);
        Assert.Contains("Subscription identifiers are not supported by the broker", exception.Message);
    }

    [Fact]
    public async Task SubscriptionIdentifiersAvailable_WhenTrue_AcceptsSubscriptionIdentifiersAsync()
    {
        // Arrange
        var client = new HiveMQClient();
        _ = await client.ConnectAsync().ConfigureAwait(false);

        // Manually override the connection properties to simulate a broker that supports subscription identifiers
        client.Connection.ConnectionProperties.SubscriptionIdentifiersAvailable = true;

        var options = new SubscribeOptions
        {
            SubscriptionIdentifier = 1,
        };
        options.TopicFilters.Add(new TopicFilter("test/topic"));

        // Act & Assert
        var result = await client.SubscribeAsync(options).ConfigureAwait(true);
        Assert.NotNull(result);
        Assert.Single(result.Subscriptions);
        Assert.Equal(SubAckReasonCode.GrantedQoS0, result.Subscriptions[0].SubscribeReasonCode);
    }

    [Fact]
    public async Task TopicAliasMaximum_WhenZero_RejectsTopicAliasAsync()
    {
        // Arrange
        var client = new HiveMQClient();
        _ = await client.ConnectAsync().ConfigureAwait(false);

        // Manually override the connection properties to simulate a broker that does not support topic aliases
        client.Connection.ConnectionProperties.TopicAliasMaximum = 0;

        // Update cache to reflect the manual override
        client.UpdateConnectionPropertyCache(client.Connection.ConnectionProperties);

        var message = new MQTT5PublishMessage
        {
            Topic = "test/topic",
            Payload = Encoding.ASCII.GetBytes("test message"),
            TopicAlias = 1,
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<HiveMQttClientException>(() =>
            client.PublishAsync(message)).ConfigureAwait(true);
        Assert.Contains("Topic aliases are not supported by the broker", exception.Message);
    }

    [Fact]
    public async Task TopicAliasMaximum_WhenNonZero_AcceptsTopicAliasAsync()
    {
        // Arrange
        var client = new HiveMQClient();
        _ = await client.ConnectAsync().ConfigureAwait(false);

        // Manually override the connection properties to simulate a broker that supports topic aliases
        client.Connection.ConnectionProperties.TopicAliasMaximum = 10;

        var message = new MQTT5PublishMessage
        {
            Topic = "test/topic",
            Payload = Encoding.ASCII.GetBytes("test message"),
            TopicAlias = 1,
        };

        // Act & Assert
        var result = await client.PublishAsync(message).ConfigureAwait(true);
        Assert.NotNull(result);
        Assert.Equal(1, result.Message.TopicAlias);
    }

    [Fact]
    public async Task TopicAliasMaximum_WhenExceeded_RejectsTopicAliasAsync()
    {
        // Arrange
        var client = new HiveMQClient();
        _ = await client.ConnectAsync().ConfigureAwait(false);

        // Manually override the connection properties to simulate a broker with a topic alias maximum of 5
        client.Connection.ConnectionProperties.TopicAliasMaximum = 5;

        var message = new MQTT5PublishMessage
        {
            Topic = "test/topic",
            Payload = Encoding.ASCII.GetBytes("test message"),
            TopicAlias = 6,
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<HiveMQttClientException>(() =>
            client.PublishAsync(message)).ConfigureAwait(true);
        Assert.Contains("Topic alias exceeds broker's maximum allowed value", exception.Message);
    }
}
