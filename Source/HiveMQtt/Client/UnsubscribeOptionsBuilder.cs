/*
 * Copyright 2023-present HiveMQ and the HiveMQ Community
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
namespace HiveMQtt.Client;

using HiveMQtt.Client.Options;
using HiveMQtt.MQTT5.Types;

public class UnsubscribeOptionsBuilder
{
    private readonly UnsubscribeOptions options;

    public UnsubscribeOptionsBuilder() => this.options = new UnsubscribeOptions();

    /// <summary>
    /// Adds a single subscription to the UnsubscribeOption.
    /// <para>
    /// You can retrieve existing subscriptions in the HiveMQClient.Subscriptions property or
    /// alternatively using the HiveMQClient.GetSubscriptionByTopic method.
    /// </para>
    /// </summary>
    /// <param name="subscription">A topic subscription.</param>
    /// <returns>The UnsubscribeOptionsBuilder instance.</returns>
    public UnsubscribeOptionsBuilder WithSubscription(Subscription subscription)
    {
        this.options.Subscriptions.Add(subscription);
        return this;
    }

    /// <summary>
    /// Adds a list of subscriptions to the UnsubscribeOption.
    /// <para>
    /// You can retrieve existing subscriptions in the HiveMQClient.Subscriptions property or
    /// alternatively using the HiveMQClient.GetSubscriptionByTopic method.
    /// </para>
    /// </summary>
    /// <param name="subscriptions">A list of topic subscriptions.</param>
    /// <returns>The UnsubscribeOptionsBuilder instance.</returns>
    public UnsubscribeOptionsBuilder WithSubscriptions(IEnumerable<Subscription> subscriptions)
    {
        foreach (var subscription in subscriptions)
        {
            this.options.Subscriptions.Add(subscription);
        }

        return this;
    }

    /// <summary>
    /// Adds a user property to be sent along with the unsubscribe request.
    /// <para>
    /// In MQTT 5, User Properties provide a mechanism for attaching custom key-value pairs to MQTT
    /// messages. User Properties allow clients to include additional metadata or application-specific
    /// information beyond the standard MQTT headers and payload. These properties can be used for
    /// various purposes such as message routing, filtering, or conveying application-specific data.
    /// User Properties are optional and can be included in MQTT packets like CONNECT, PUBLISH, SUBSCRIBE,
    /// UNSUBSCRIBE, and others. They enable extensibility and interoperability by allowing clients and
    /// brokers to exchange custom information in a standardized manner within the MQTT protocol.
    /// </para>
    /// </summary>
    /// <param name="key">The key of the user property.</param>
    /// <param name="value">The value of the user property.</param>
    /// <returns>The UnsubscribeOptionsBuilder instance.</returns>
    public UnsubscribeOptionsBuilder WithUserProperty(string key, string value)
    {
        this.options.UserProperties.Add(key, value);
        return this;
    }

    /// <summary>
    /// Adds user properties to be sent along with the unsubscribe request.
    /// <para>
    /// In MQTT 5, User Properties provide a mechanism for attaching custom key-value pairs to MQTT
    /// messages. User Properties allow clients to include additional metadata or application-specific
    /// information beyond the standard MQTT headers and payload. These properties can be used for
    /// various purposes such as message routing, filtering, or conveying application-specific data.
    /// User Properties are optional and can be included in MQTT packets like CONNECT, PUBLISH, SUBSCRIBE,
    /// UNSUBSCRIBE, and others. They enable extensibility and interoperability by allowing clients and
    /// brokers to exchange custom information in a standardized manner within the MQTT protocol.
    /// </para>
    /// </summary>
    /// <param name="userProperties">A dictionary of user properties.</param>
    /// <returns>The UnsubscribeOptionsBuilder instance.</returns>
    public UnsubscribeOptionsBuilder WithUserProperties(Dictionary<string, string> userProperties)
    {
        foreach (var userProperty in userProperties)
        {
            this.options.UserProperties.Add(userProperty.Key, userProperty.Value);
        }

        return this;
    }

    public UnsubscribeOptions Build()
    {
        this.options.Validate();
        return this.options;
    }
}
