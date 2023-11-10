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

using System.Text;
using HiveMQtt.Client.Exceptions;

public class MQTT5PublishMessage
{
    public MQTT5PublishMessage()
    {
        this.UserProperties = new Dictionary<string, string>();
        this.SubscriptionIdentifiers = new List<int>();
    }

    public MQTT5PublishMessage(string topic, QualityOfService? qos)
    {
        this.UserProperties = new Dictionary<string, string>();
        this.SubscriptionIdentifiers = new List<int>();
        this.Duplicate = false;
        this.Retain = false;
        this.Topic = topic;
        if (qos.HasValue)
        {
            this.QoS = qos;
        }
    }

    /// <summary>
    /// Gets or sets the topic of this Publish.
    /// </summary>
    public string? Topic { get; set; }

    /// <summary>
    /// Gets or sets the Quality of Service level for this publish.
    /// </summary>
    public QualityOfService? QoS { get; set; }

    /// <summary>
    /// Gets or sets the Payload Format Indicator.  This is used to indicate the format of the payload.
    /// </summary>
    public MQTT5PayloadFormatIndicator? PayloadFormatIndicator { get; set; }

    /// <summary>
    /// Gets or sets the Message Expiry Interval.  The Message Expiry Interval is the  the lifetime
    /// of the Application Message in seconds.
    /// <para>
    /// If absent, the Application Message does not expire.
    /// </para>
    /// </summary>
    public int? MessageExpiryInterval { get; set; }

    /// <summary>
    /// Gets or sets the Topic Alias.  A Topic Alias is an integer value that is used to identify the Topic
    /// instead of using the Topic Name.
    /// </summary>
    public int? TopicAlias { get; set; }

    /// <summary>
    /// Gets or sets a UTF-8 Encoded String which is used as the Topic Name for a response message.
    /// <para>
    /// See also <seealso href="https://docs.oasis-open.org/mqtt/mqtt/v5.0/os/mqtt-v5.0-os.html#_Request_/_Response">
    /// Request / Response</seealso>.
    /// </para>
    /// </summary>
    public string? ResponseTopic { get; set; }

    /// <summary>
    /// Gets or sets the binary data that is used by the sender of the Request Message to identify which request
    /// the Response Message is for when it is received.
    /// </summary>
    public byte[]? CorrelationData { get; set; }

    /// <summary>
    /// Gets or sets the User Properties for this Publish.
    /// </summary>
    public Dictionary<string, string> UserProperties { get; set; }

    /// <summary>
    /// Gets or sets the Subscription Identifiers for this Publish.
    /// <para>
    /// Multiple Subscription Identifiers will be included if the publication is the result of a match to more
    /// than one subscription, in this case their order is not significant.
    /// </para>
    /// <para>
    /// The Subscription Identifier can have the value of 1 to 268,435,455. It is a Protocol Error if the
    /// Subscription Identifier has a value of 0.
    /// </para>
    /// </summary>
    public List<int> SubscriptionIdentifiers { get; set; }

    /// <summary>
    /// Gets or sets a UTF-8 Encoded String describing the content of the Application Message.
    /// </summary>
    public string? ContentType { get; set; }

    /// <summary>
    ///  Gets or sets the UTF-8 encoded payload of this Publish.
    /// </summary>
    public byte[]? Payload { get; set; }

    /// <summary>
    /// Gets or sets the Payload as a string.
    /// </summary>
    public string PayloadAsString
    {
        get
        {
            if (this.Payload is null)
            {
                return string.Empty;
            }

            return Encoding.UTF8.GetString(this.Payload);
        }

        set
        {
            if (value is null)
            {
                this.Payload = null;
            }
            else
            {
                this.Payload = Encoding.UTF8.GetBytes(value);
            }
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether this publish should be retained by the MQTT broker.
    /// <para>
    /// If the Payload contains zero bytes it is processed normally by the Server but any retained message with
    /// the same topic name will be removed by the broker and any future subscribers for the topic will not
    /// receive a retained message. A retained message with a Payload containing zero bytes will not be stored
    /// as a retained message on the broker.
    /// </para>
    /// <para>
    /// See <seealso href="https://docs.oasis-open.org/mqtt/mqtt/v5.0/os/mqtt-v5.0-os.html#_Toc3901104">Retain</seealso>.
    /// </para>
    /// </summary>
    public bool Retain { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this packet is a retransmission.
    /// <para>
    /// This value is only used for QoS 1 and QoS 2 messages.
    /// </para>
    /// </summary>
    public bool Duplicate { get; set; }

    /// <summary>
    /// Validate that the options in this instance are valid.
    /// </summary>
    /// <exception cref="HiveMQttClientException">The exception raised if some value is out of range or invalid.</exception>
    public void Validate()
    {
        if (!this.PayloadFormatIndicator.HasValue)
        {
            this.PayloadFormatIndicator = MQTT5PayloadFormatIndicator.Unspecified;
        }

        if (this.MessageExpiryInterval.HasValue && (this.MessageExpiryInterval.Value is < 0 or > 268435455))
        {
            throw new HiveMQttClientException("Message Expiry Interval must be between 0 and 268_435_455.");
        }

        if (this.TopicAlias.HasValue && (this.TopicAlias.Value is < 1 or > 65535))
        {
            throw new HiveMQttClientException("Topic Alias must be between 1 and 65535.");
        }

        if (this.ResponseTopic != null && this.ResponseTopic.Length > 65535)
        {
            throw new HiveMQttClientException("Response Topic must be less than 65535 characters.");
        }

        if (this.CorrelationData != null && this.CorrelationData.Length > 65535)
        {
            throw new HiveMQttClientException("Correlation Data must be less than 65535 bytes.");
        }

        foreach (var property in this.UserProperties)
        {
            if (property.Key is not string key || key.Length > 65535)
            {
                throw new HiveMQttClientException("User Property Key must be less than 65535 characters.");
            }

            if (property.Value is not string value || value.Length > 65535)
            {
                throw new HiveMQttClientException("User Property Value must be less than 65535 characters.");
            }
        }

        if (this.SubscriptionIdentifiers != null)
        {
            foreach (var subscriptionIdentifier in this.SubscriptionIdentifiers)
            {
                if (subscriptionIdentifier is < 1 or > 268435455)
                {
                    throw new HiveMQttClientException("Subscription Identifier must be between 1 and 268_435_455.");
                }
            }
        }

        if (this.ContentType != null && this.ContentType.Length > 65535)
        {
            throw new HiveMQttClientException("Content Type must be less than 65535 characters.");
        }

        if (this.Payload != null && this.Payload.Length > 268435455)
        {
            throw new HiveMQttClientException("Payload must be less than 268_435_455 bytes.");
        }

        if (this.Topic == null && this.TopicAlias.HasValue)
        {
            throw new HiveMQttClientException("Topic Alias must be null if Topic is null.");
        }
    }
}
