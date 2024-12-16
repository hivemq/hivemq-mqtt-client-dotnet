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
namespace HiveMQtt.MQTT5.Types;

using System.ComponentModel.DataAnnotations;
using HiveMQtt.Client.Exceptions;
using HiveMQtt.Client.Internal;

public class TopicFilter
{
    public TopicFilter(string topic, QualityOfService qos = QualityOfService.AtMostOnceDelivery, bool? noLocal = null, bool? retainAsPublished = null, RetainHandling? retainHandling = null)
    {
        Client.Internal.Validator.ValidateTopicFilter(topic);
        this.Topic = topic;
        this.QoS = qos;
        this.NoLocal = noLocal;
        this.RetainAsPublished = retainAsPublished;
        this.RetainHandling = retainHandling;
    }

    /// <summary>
    /// Gets or sets the topic for this filter.
    /// <para>
    /// The Topic is a UTF-8 encoded string that specifies a subscription topic.  It can
    /// include the wildcard characters # and + as defined in
    /// <seealso href="https://docs.oasis-open.org/mqtt/mqtt/v5.0/os/mqtt-v5.0-os.html#_Toc3901241">
    /// the MQTT specification</seealso>.
    /// </para>
    /// </summary>
    public string Topic { get; set; }

    /// <summary>
    /// Gets or sets the Quality of Service level for this filter.
    /// </summary>
    public QualityOfService QoS { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the broker will forward messages published by
    /// the client to the client itself.
    /// <para>
    /// The default value is false: the broker will forward all messages to the client.
    /// </para>
    /// </summary>
    public bool? NoLocal { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether Application Messages forwarded using this
    /// subscription keep the RETAIN flag they were published with.
    /// </summary>
    public bool? RetainAsPublished { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether retained messages are sent when the
    /// subscription is established.  This does not affect the sending of retained
    /// messages at any point after the subscribe.
    /// <para>
    /// The default value is RetainHandling.SendAtSubscribe.  See
    /// <seealso href="https://docs.oasis-open.org/mqtt/mqtt/v5.0/os/mqtt-v5.0-os.html#_Toc3901169">
    /// the MQTT specification</seealso> for more information.
    /// </para>
    /// </summary>
    public RetainHandling? RetainHandling { get; set; }

    /// <summary>
    /// Validates the filter.
    /// </summary>
    /// <exception cref="HiveMQttClientException">Raised if the topic filter is invalid.</exception>
    public void ValidateFilter()
    {
        if (string.IsNullOrEmpty(this.Topic))
        {
            throw new HiveMQttClientException("Topic filter must not be null or empty");
        }
    }
}
