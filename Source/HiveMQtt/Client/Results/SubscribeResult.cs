/*
 * Copyright 2022-present HiveMQ and the HiveMQ Community
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
namespace HiveMQtt.Client.Results;

using HiveMQtt.Client.Options;
using HiveMQtt.MQTT5.Packets;
using HiveMQtt.MQTT5.Types;

/// <summary>
/// Results of a Subscribe operation.
/// </summary>
public class SubscribeResult
{
    internal SubscribeResult(SubscribeOptions options, SubAckPacket subAckPacket)
    {
        this.Options = options;
        this.Properties = subAckPacket.Properties;
        this.Subscriptions = new List<Subscription>();

        for (var i = 0; i < subAckPacket.ReasonCodes.Count; i++)
        {
            var reasonCode = subAckPacket.ReasonCodes[i];
            var topicFilter = options.TopicFilters[i];
            var subscription = new Subscription(topicFilter)
            {
                SubscribeReasonCode = reasonCode,
            };
            this.Subscriptions.Add(subscription);
        }
    }

    /// <summary>
    /// Gets the options used for the Subscribe operation.
    /// </summary>
    internal SubscribeOptions Options { get; }

    /// <summary>
    /// Gets the MQTT5Properties returned by the MQTT broker in the subscribe request.
    /// </summary>
    internal MQTT5Properties Properties { get; }

    /// <summary>
    /// Gets the list of Subscriptions that were a result of the subscribe call.
    /// </summary>
    public List<Subscription> Subscriptions { get; }

    /// <summary>
    /// Gets a Dictionary containing the User Properties returned by the MQTT broker.
    /// </summary>
    public Dictionary<string, string> UserProperties => this.Properties.UserProperties;

    /// <summary>
    /// Gets the Subscription for the given topic filter.
    /// <para>
    /// The topic filter must be the same as one used in the Subscribe operation.
    /// </para>
    /// <returns>The Subscription for the given topic filter or null if not found.</returns>
    /// </summary>
    public Subscription? GetSubscription(string topicFilter)
    {
        foreach (var subscription in this.Subscriptions)
        {
            if (subscription.TopicFilter.Topic == topicFilter)
            {
                return subscription;
            }
        }
        return null;
    }

    /// <summary>
    /// Gets the first Subscription in the list of Subscriptions or null if the list is empty.
    /// </summary>
    public Subscription? GetFirstSubscription()
    {
        return this.Subscriptions.FirstOrDefault();
    }
}
