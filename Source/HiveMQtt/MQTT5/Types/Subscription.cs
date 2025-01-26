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
namespace HiveMQtt.MQTT5.Types;

using HiveMQtt.Client.Events;
using HiveMQtt.MQTT5.ReasonCodes;

public class Subscription
{
    public Subscription(TopicFilter topicFilter) => this.TopicFilter = topicFilter;

    public Subscription(string topicFilter) => this.TopicFilter = new TopicFilter(topicFilter);

    /// <summary>
    /// Gets the topic filter for the subscription.
    /// </summary>
    public TopicFilter TopicFilter { get; }

    /// <summary>
    /// Gets or sets the message handler for the subscription.
    /// </summary>
    public EventHandler<OnMessageReceivedEventArgs>? MessageReceivedHandler { get; set; }

    /// <summary>
    /// Gets or sets the reason code (result) for the subscribe call.
    /// <para>
    /// See <seealso cref="SubAckReasonCode">SubAckReasonCode</seealso> or the
    /// <seealso href="https://docs.oasis-open.org/mqtt/mqtt/v5.0/os/mqtt-v5.0-os.html#_Toc3901178">
    /// MQTT specification</seealso> for more information.
    /// </para>
    /// </summary>
    public SubAckReasonCode SubscribeReasonCode { get; set; }

    /// <summary>
    /// Gets or sets the reason code (result) for the unsubscribe call.
    /// <para>
    /// See <seealso cref="UnsubAckReasonCode">UnsubAckReasonCode</seealso> or the
    /// <seealso href="https://docs.oasis-open.org/mqtt/mqtt/v5.0/os/mqtt-v5.0-os.html#_Toc3901194">
    /// MQTT specification</seealso> for more information.
    /// </para>
    /// </summary>
    public UnsubAckReasonCode UnsubscribeReasonCode { get; set; }
}
