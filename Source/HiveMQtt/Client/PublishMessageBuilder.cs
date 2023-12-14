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
    ///
    /// Quality of Service (QoS) levels define the reliability and guarantee of message delivery between MQTT
    /// clients and brokers.  The choice of QoS level depends on the specific requirements of the application.
    /// QoS 0 is suitable for scenarios where occasional message loss is acceptable, while QoS 1 and QoS 2 are
    /// preferred for applications that require reliable and guaranteed message delivery.
    /// </summary>
    /// <param name="qos">The Quality of Service level from the `QualityOfService` enum.</param>
    /// <returns>The builder instance.</returns>
    public PublishMessageBuilder WithQualityOfService(QualityOfService qos)
    {
        this.message.QoS = qos;
        return this;
    }

    /// <summary>
    /// Sets a user property of the publish message.
    ///
    /// In MQTT 5, User Properties provide a flexible way to include custom key-value pairs in MQTT messages.
    /// User Properties allow clients to attach additional metadata or application-specific information to
    /// messages beyond the standard MQTT headers and payload. These properties can be used for various purposes
    /// such as message routing, filtering, or conveying application-specific data. User Properties are optional
    /// and can be included in MQTT packets like CONNECT, PUBLISH, SUBSCRIBE, UNSUBSCRIBE, and others. They enable
    /// extensibility and interoperability by allowing clients and brokers to exchange custom information in a
    /// standardized manner within the MQTT protocol.
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
    ///
    /// In MQTT 5, User Properties provide a flexible way to include custom key-value pairs in MQTT messages.
    /// User Properties allow clients to attach additional metadata or application-specific information to
    /// messages beyond the standard MQTT headers and payload. These properties can be used for various purposes
    /// such as message routing, filtering, or conveying application-specific data. User Properties are optional
    /// and can be included in MQTT packets like CONNECT, PUBLISH, SUBSCRIBE, UNSUBSCRIBE, and others. They enable
    /// extensibility and interoperability by allowing clients and brokers to exchange custom information in a
    /// standardized manner within the MQTT protocol.
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
    /// Sets the subscription identifier of the publish message.
    /// </summary>
    /// <param name="identifier">The subscription identifier.</param>
    /// <returns>The builder instance.</returns>
    public PublishMessageBuilder WithSubscriptionIdentifier(int identifier)
    {
        this.message.SubscriptionIdentifiers.Add(identifier);
        return this;
    }

    /// <summary>
    /// Sets the Payload Format Indicator of the publish message.
    ///
    /// The Payload Format Indicator indicates the format of the payload data being published. The Payload
    /// Format Indicator allows the sender to provide a hint to the receiver about how to interpret and
    /// process the payload.
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
