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
}
