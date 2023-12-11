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

using HiveMQtt.MQTT5.Types;

public class SubscribeOptionsBuilder
{
    private readonly SubscribeOptions options;

    public SubscribeOptionsBuilder()
    {
        this.options = new SubscribeOptions();
    }

    /// <summary>
    /// Sets the Subscription Identifier (AKA TopicFilter).
    ///
    /// See also: <see href="https://www.hivemq.com/blog/mqtt-essentials-part-5-mqtt-topics-best-practices/">MQTT Topics Best Practices</see>.
    ///
    /// </summary>
    /// <param name="topic">The string topic name.</param>
    /// <param name="qos">The quality of service level.</param>
    /// <param name="noLocal">A boolean indicating whether this client will receive the messages published by this client.</param>
    /// <param name="retainAsPublished">A boolean indicating whether Application Messages forwarded using this subscription keep the RETAIN flag they were published with.</param>
    /// <param name="retainHandling">A RetainHandling value indicating whether retained messages are sent when the subscription is established.</param>
    /// <returns>SubscribeOptionsBuilder to continue the build process.</returns>
    public SubscribeOptionsBuilder WithSubscription(string topic, QualityOfService qos, bool? noLocal = null, bool? retainAsPublished = null, RetainHandling? retainHandling = RetainHandling.SendAtSubscribe)
    {
        this.options.TopicFilters.Add(new TopicFilter(topic, qos, noLocal, retainAsPublished, retainHandling));
        return this;
    }

    public SubscribeOptionsBuilder WithUserProperties(string key, string value)
    {
        this.options.UserProperties.Add(key, value);
        return this;
    }

    public SubscribeOptions Build()
    {
        this.options.Validate();
        return this.options;
    }
}
