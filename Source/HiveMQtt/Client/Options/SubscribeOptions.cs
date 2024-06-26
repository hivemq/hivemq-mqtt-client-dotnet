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
namespace HiveMQtt.Client.Options;

using HiveMQtt.Client.Events;
using HiveMQtt.Client.Exceptions;
using HiveMQtt.MQTT5.Types;

public class SubscribeOptions
{
    public SubscribeOptions()
    {
        this.UserProperties = new Dictionary<string, string>();
        this.TopicFilters = new List<TopicFilter>();
    }

    /// <summary>
    /// Gets or sets the Subscription Identifier.
    /// </summary>
    public int? SubscriptionIdentifier { get; set; }

    /// <summary>
    /// Gets or sets the User Properties for this subscribe.
    /// </summary>
    public Dictionary<string, string> UserProperties { get; set; }

    /// <summary>
    /// Gets or sets the Topic Filters for this subscribe.
    /// </summary>
    public List<TopicFilter> TopicFilters { get; set; }

    /// <summary>
    /// Gets or sets the handlers for this subscribe.
    /// <para>
    /// Per subscription callbacks can be registered here. The key is the topic filter
    /// and the value is the handler.  If the topic string isn't found in one of the
    /// <c>List&lt;TopicFilters&gt;</c>, the handler will not be registered for that subscription.
    /// </para>
    /// </summary>
    public Dictionary<string, EventHandler<OnMessageReceivedEventArgs>> Handlers { get; set; } = new();

    /// <summary>
    /// Validate that the options in this instance are valid.
    /// </summary>
    /// <exception cref="HiveMQttClientException">The exception raised if some value is out of range or invalid.</exception>
    public void Validate()
    {
        if (this.TopicFilters.Count == 0)
        {
            throw new HiveMQttClientException("At least one topic filter must be specified for SubscribeOptions.");
        }
    }
}
