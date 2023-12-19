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
namespace HiveMQtt.Client;

using System.Text;
using HiveMQtt.MQTT5.Types;

public class PublishMessageBuilder
{
    /// <summary>
    /// The publish message that is being built.
    /// </summary>
    private readonly MQTT5PublishMessage message;

    public PublishMessageBuilder()
    {
        this.message = new MQTT5PublishMessage();
    }

    /// <summary>
    /// Sets the payload of the publish message.
    /// </summary>
    /// <param name="payload">The payload as a byte array.</param>
    /// <returns>The builder instance.</returns>
    public PublishMessageBuilder WithPayload(byte[] payload)
    {
        this.message.Payload = payload;
        return this;
    }

    /// <summary>
    /// Sets the payload of the publish message.
    /// </summary>
    /// <param name="payload">The payload as a string.</param>
    /// <returns>The builder instance.</returns>
    public PublishMessageBuilder WithPayload(string payload)
    {
        this.message.Payload = Encoding.UTF8.GetBytes(payload);
        return this;
    }

    /// <summary>
    /// Sets the topic of the publish message.
    /// <para>
    /// An MQTT topic is a string-based identifier used for message communication in MQTT. It
    /// represents a specific subject or category to which messages can be published or subscribed.
    /// Topics are structured as a series of levels separated by forward slashes ("/"), allowing for
    /// hierarchical organization. Clients can publish messages to specific topics, and other clients
    /// can subscribe to topics of interest to receive those messages. Topics enable flexible and
    /// targeted message routing in MQTT communication.
    /// </para>
    /// </summary>
    /// <param name="topic">The topic.</param>
    /// <returns>The builder instance.</returns>
    public PublishMessageBuilder WithTopic(string topic)
    {
        this.message.Topic = topic;
        return this;
    }

    /// <summary>
    /// Sets the Quality of Service level of the publish message.
    /// <para>
    /// Quality of Service (QoS) levels define the reliability and guarantee of message delivery between MQTT
    /// clients and brokers.  The choice of QoS level depends on the specific requirements of the application.
    /// QoS 0 is suitable for scenarios where occasional message loss is acceptable, while QoS 1 and QoS 2 are
    /// preferred for applications that require reliable and guaranteed message delivery.
    /// </para>
    /// </summary>
    /// <param name="qos">The Quality of Service level from the `QualityOfService` enum.</param>
    /// <returns>The builder instance.</returns>
    public PublishMessageBuilder WithQualityOfService(QualityOfService qos)
    {
        this.message.QoS = qos;
        return this;
    }

    /// <summary>
    /// Sets the retain flag of the publish message.
    /// <para>
    /// The retain flag in an MQTT publish message is a property that indicates whether the broker
    /// should retain the last message published on a specific topic. When a message with the retain
    /// flag set is published, the broker stores the message and delivers it to new subscribers
    /// immediately upon their subscription. This allows new subscribers to receive the most recent
    /// message published on a topic, even if they were not subscribed at the time of publication.
    /// The retain flag is useful for transmitting important information or status updates that
    /// should be available to new subscribers.
    /// </para>
    /// </summary>
    /// <param name="retain">The retain flag.</param>
    /// <returns>The builder instance.</returns>
    public PublishMessageBuilder WithRetain(bool retain)
    {
        this.message.Retain = retain;
        return this;
    }

    /// <summary>
    /// Sets the duplicate flag of the publish message.
    /// <para>
    /// The duplicate flag in an MQTT publish message is a property that indicates whether the
    /// message is a duplicate of a previous message. It is used to ensure message delivery
    /// reliability in scenarios where the sender needs to retransmit a message. When a message
    /// is sent or retransmitted, the duplicate flag is set to inform the receiver that the message
    /// may have been previously received. The receiver can then handle the message accordingly,
    /// such as ignoring duplicates or processing them as needed. The duplicate flag helps prevent
    /// unintended message duplication and ensures the correct handling of messages in MQTT
    /// communication.
    /// </para>
    /// </summary>
    /// <param name="duplicate">The duplicate flag.</param>
    /// <returns>The builder instance.</returns>
    public PublishMessageBuilder WithDuplicate(bool duplicate)
    {
        this.message.Duplicate = duplicate;
        return this;
    }

    /// <summary>
    /// Sets the response topic of the publish message.
    /// <para>
    /// In MQTT 5 publish, the response topic is an optional property that the publisher
    /// can include in the message. It specifies the topic to which the recipient should
    /// send a response or acknowledgement related to the published message. The response
    /// topic allows for request-response communication patterns, where the publisher can
    /// indicate where the response should be sent. This enables more advanced message
    /// exchange scenarios and facilitates bidirectional communication in MQTT 5.
    /// </para>
    /// </summary>
    /// <param name="responseTopic">The response topic.</param>
    /// <returns>The builder instance.</returns>
    public PublishMessageBuilder WithResponseTopic(string responseTopic)
    {
        this.message.ResponseTopic = responseTopic;
        return this;
    }

    /// <summary>
    /// Sets the correlation data of the publish message.
    /// <para>
    /// Correlation data in an MQTT 5 publish message is a property that allows the sender to
    /// include additional contextual information or identifiers related to the message. It
    /// is typically used to correlate or associate the message with other data or processes
    /// in the system. Correlation data can be any arbitrary byte sequence and is useful for
    /// tracking or linking messages with specific operations or workflows. It enables more
    /// advanced message processing and coordination in MQTT communication.
    /// </para>
    /// </summary>
    /// <param name="correlationData">The correlation data as a byte array.</param>
    /// <returns>The builder instance.</returns>
    public PublishMessageBuilder WithCorrelationData(byte[] correlationData)
    {
        this.message.CorrelationData = correlationData;
        return this;
    }

    /// <summary>
    /// Sets the correlation data of the publish message.
    /// <para>
    /// Correlation data in an MQTT 5 publish message is a property that allows the sender to
    /// include additional contextual information or identifiers related to the message. It
    /// is typically used to correlate or associate the message with other data or processes
    /// in the system. Correlation data can be any arbitrary byte sequence and is useful for
    /// tracking or linking messages with specific operations or workflows. It enables more
    /// advanced message processing and coordination in MQTT communication.
    /// </para>
    /// </summary>
    /// <param name="correlationData">The correlation data as a string.</param>
    /// <returns>The builder instance.</returns>
    public PublishMessageBuilder WithCorrelationData(string correlationData)
    {
        this.message.CorrelationData = Encoding.UTF8.GetBytes(correlationData);
        return this;
    }

    /// <summary>
    /// Sets the content type of the publish message.
    /// <para>
    /// The content type in an MQTT 5 publish message is a property that specifies the type
    /// or format of the payload data being published. It provides additional information
    /// about the content of the message payload, allowing the receiver to interpret and
    /// process the data accordingly. The content type can be used to indicate the MIME type,
    /// media type, or any other format identifier that helps the receiver understand how to
    /// handle the payload. It enables more flexible and context-aware message processing in
    /// MQTT communication.
    /// </para>
    /// </summary>
    /// <param name="contentType">The content type.</param>
    /// <returns>The builder instance.</returns>
    public PublishMessageBuilder WithContentType(string contentType)
    {
        this.message.ContentType = contentType;
        return this;
    }

    /// <summary>
    /// Sets the message expiry interval of the publish message.
    /// <para>
    /// The message expiry interval in an MQTT 5 publish message is a property that specifies
    /// the maximum duration for which the message should be considered valid. It allows the
    /// sender to set a time limit on the message's relevance and ensures that outdated or
    /// expired messages are not delivered to subscribers. If a message's expiry interval is
    /// reached before it is delivered, the broker will discard the message instead of
    /// delivering it. The message expiry interval helps maintain message freshness and
    /// relevance in MQTT communication.
    /// </para>
    /// </summary>
    /// <param name="messageExpiryInterval">The message expiry interval.</param>
    /// <returns>The builder instance.</returns>
    public PublishMessageBuilder WithMessageExpiryInterval(int messageExpiryInterval)
    {
        this.message.MessageExpiryInterval = messageExpiryInterval;
        return this;
    }

    /// <summary>
    /// Sets the subscription identifiers of the publish message.
    /// <para>
    /// The subscription identifier in an MQTT 5 publish message is a property that associates
    /// the message with a specific subscription made by the client. It allows the sender to
    /// include an identifier that uniquely identifies the subscription for which the message
    /// is intended. This identifier helps the receiver to match the incoming message with the
    /// corresponding subscription, enabling more efficient and targeted message delivery. The
    /// subscription identifier enhances the scalability and performance of MQTT communication
    /// by reducing the need for extensive topic matching operations.
    /// </para>
    /// </summary>
    /// <param name="subscriptionIdentifier">The subscription identifier.</param>
    /// <returns>The builder instance.</returns>
    public PublishMessageBuilder WithSubscriptionIdentifier(int subscriptionIdentifier)
    {
        this.message.SubscriptionIdentifiers.Add(subscriptionIdentifier);
        return this;
    }

    /// <summary>
    /// Sets the subscription identifiers of the publish message.
    /// <para>
    /// The subscription identifier in an MQTT 5 publish message is a property that associates
    /// the message with a specific subscription made by the client. It allows the sender to
    /// include an identifier that uniquely identifies the subscription for which the message
    /// is intended. This identifier helps the receiver to match the incoming message with the
    /// corresponding subscription, enabling more efficient and targeted message delivery. The
    /// subscription identifier enhances the scalability and performance of MQTT communication
    /// by reducing the need for extensive topic matching operations.
    /// </para>
    /// </summary>
    /// <param name="subscriptionIdentifiers">The subscription identifiers.</param>
    /// <returns>The builder instance.</returns>
    public PublishMessageBuilder WithSubscriptionIdentifiers(List<int> subscriptionIdentifiers)
    {
        this.message.SubscriptionIdentifiers.AddRange(subscriptionIdentifiers);
        return this;
    }

    /// <summary>
    /// Sets the topic alias of the publish message.
    /// <para>
    /// The topic alias in an MQTT 5 publish message is a property that allows the sender to
    /// include a previously defined topic alias instead of the full topic name. It is used
    /// to reduce the size of the message and improve network efficiency by avoiding the
    /// repetition of the full topic name. The topic alias is a short integer value that
    /// corresponds to a topic name previously defined in a Topic Alias Maximum packet. It
    /// enables more efficient message transmission and delivery in MQTT communication.
    /// </para>
    /// </summary>
    /// <param name="topicAlias">The topic alias.</param>
    /// <returns>The builder instance.</returns>
    public PublishMessageBuilder WithTopicAlias(int topicAlias)
    {
        this.message.TopicAlias = topicAlias;
        return this;
    }

    /// <summary>
    /// Sets a user property of the publish message.
    /// <para>
    /// In MQTT 5, User Properties provide a flexible way to include custom key-value pairs in MQTT messages.
    /// User Properties allow clients to attach additional metadata or application-specific information to
    /// messages beyond the standard MQTT headers and payload. These properties can be used for various purposes
    /// such as message routing, filtering, or conveying application-specific data. User Properties are optional
    /// and can be included in MQTT packets like CONNECT, PUBLISH, SUBSCRIBE, UNSUBSCRIBE, and others. They enable
    /// extensibility and interoperability by allowing clients and brokers to exchange custom information in a
    /// standardized manner within the MQTT protocol.
    /// </para>
    /// </summary>
    /// <param name="key">The key of the user property.</param>
    /// <param name="value">The value of the user property.</param>
    /// <returns>The builder instance.</returns>
    public PublishMessageBuilder WithUserProperty(string key, string value)
    {
        this.message.UserProperties[key] = value;
        return this;
    }

    /// <summary>
    /// Sets multiple user properties of the publish message.
    /// <para>
    /// In MQTT 5, User Properties provide a flexible way to include custom key-value pairs in MQTT messages.
    /// User Properties allow clients to attach additional metadata or application-specific information to
    /// messages beyond the standard MQTT headers and payload. These properties can be used for various purposes
    /// such as message routing, filtering, or conveying application-specific data. User Properties are optional
    /// and can be included in MQTT packets like CONNECT, PUBLISH, SUBSCRIBE, UNSUBSCRIBE, and others. They enable
    /// extensibility and interoperability by allowing clients and brokers to exchange custom information in a
    /// standardized manner within the MQTT protocol.
    /// </para>
    /// </summary>
    /// <param name="properties">The user properties as a dictionary.</param>
    /// <returns>The builder instance.</returns>
    public PublishMessageBuilder WithUserProperties(Dictionary<string, string> properties)
    {
        foreach (var property in properties)
        {
            this.message.UserProperties[property.Key] = property.Value;
        }

        return this;
    }

    /// <summary>
    /// Sets the Payload Format Indicator of the publish message.
    /// <para>
    /// The Payload Format Indicator indicates the format of the payload data being published. The Payload
    /// Format Indicator allows the sender to provide a hint to the receiver about how to interpret and
    /// process the payload.
    /// </para>
    /// </summary>
    /// <param name="indicator">The Payload Format Indicator.</param>
    /// <returns>The builder instance.</returns>
    public PublishMessageBuilder WithPayloadFormatIndicator(MQTT5PayloadFormatIndicator indicator)
    {
        this.message.PayloadFormatIndicator = indicator;
        return this;
    }

    public MQTT5PublishMessage Build()
    {
        this.message.Validate();
        return this.message;
    }
}
