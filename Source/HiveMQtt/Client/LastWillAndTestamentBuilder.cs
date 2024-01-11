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

using HiveMQtt.MQTT5.Types;
using HiveMQtt.Client.Internal;
using HiveMQtt.Client.Exceptions;
using System.Text;

public class LastWillAndTestamentBuilder
{
    private string? topic;
    private byte[]? payload;
    private QualityOfService qos = QualityOfService.AtMostOnceDelivery;
    private bool retain;
    private long? willDelayInterval;
    private byte? payloadFormatIndicator;
    private long? messageExpiryInterval;
    private string? contentType;
    private string? responseTopic;
    private byte[]? correlationData;
    private Dictionary<string, string> userProperties = new Dictionary<string, string>();

    public LastWillAndTestamentBuilder WithTopic(string topic)
    {
        this.topic = topic;
        return this;
    }

    public LastWillAndTestamentBuilder WithPayload(byte[] payload)
    {
        this.payload = payload;
        return this;
    }

    public LastWillAndTestamentBuilder WithPayload(string payload)
    {
        this.payload = Encoding.UTF8.GetBytes(payload);
        return this;
    }

    public LastWillAndTestamentBuilder WithQualityOfServiceLevel(QualityOfService qos)
    {
        this.qos = qos;
        return this;
    }

    public LastWillAndTestamentBuilder WithRetain(bool retain)
    {
        this.retain = retain;
        return this;
    }

    /// <summary>
    /// Sets the Will Delay Interval.  This value is the delay the broker
    /// should wait before publishing the Last Will and Testament message.
    /// <para>
    /// If the Will Delay Interval is absent, the default value is 0 and
    /// there is no delay before the Will Message is published.
    /// </para>
    /// <para>
    /// Odd Fact: the maximum value of this option equates to
    /// a 136 year delay.  (2^32 - 1) seconds = 136 years.
    /// </para>
    /// </summary>
    /// <param name="willDelayInterval">The delay value in seconds.</param>
    /// <returns>The builder instance.</returns>
    public LastWillAndTestamentBuilder WithWillDelayInterval(long willDelayInterval)
    {
        this.willDelayInterval = willDelayInterval;
        return this;
    }

    /// <summary>
    /// Sets the Payload Format Indicator.  This value indicates the format
    /// of the payload.  The value is a single byte with the following
    /// possible values:
    /// <list type="bullet">
    /// <item>
    /// <term>0</term>
    /// <description>UTF-8 Encoded Character Data</description>
    /// </item>
    /// <item>
    /// <term>1</term>
    /// <description>Binary Data</description>
    /// </item>
    /// </list>
    /// <para>
    /// If the Payload Format Indicator is absent, the default value is 0.
    /// </para>
    /// </summary>
    /// <param name="payloadFormatIndicator">The PayloadFormatIndicator value.</param>
    /// <returns>The builder instance.</returns>
    public LastWillAndTestamentBuilder WithPayloadFormatIndicator(int payloadFormatIndicator)
    {
        if (payloadFormatIndicator is < 0 or > 1)
        {
            throw new HiveMQttClientException("Payload Format Indicator must be 0 or 1");
        }

        this.payloadFormatIndicator = (byte)payloadFormatIndicator;
        return this;
    }

    /// <summary>
    /// Sets the Message Expiry Interval.  This value is the time in seconds
    /// that the broker should retain the Last Will and Testament message.
    /// <para>
    /// If the Message Expiry Interval is absent, the default value is 0 and
    /// the message is retained until the Session ends.
    /// </para>
    /// <para>
    /// Odd Fact: the maximum value of this option equates to
    /// a 136 year delay.  (2^32 - 1) seconds = 136 years.
    /// </para>
    /// </summary>
    /// <param name="messageExpiryInterval">The delay value in seconds.</param>
    /// <returns>The builder instance.</returns>
    public LastWillAndTestamentBuilder WithMessageExpiryInterval(long messageExpiryInterval)
    {
        this.messageExpiryInterval = messageExpiryInterval;
        return this;
    }

    /// <summary>
    /// Sets the Content Type.  This value is a UTF-8 encoded string that
    /// indicates the content type of the payload.
    /// <para>
    /// The value of the Content Type is defined by the sending and
    /// receiving application.
    /// </para>
    /// </summary>
    /// <param name="contentType">The payload content type.</param>
    /// <returns>The builder instance.</returns>
    public LastWillAndTestamentBuilder WithContentType(string contentType)
    {
        this.contentType = contentType;
        return this;
    }

    /// <summary>
    /// Sets the Response Topic.  This value is a UTF-8 encoded string that
    /// indicates the topic the receiver should use to respond to the
    /// Last Will and Testament message.
    /// </summary>
    /// <param name="responseTopic">The topic name for a response message.</param>
    /// <returns>The builder instance.</returns>
    public LastWillAndTestamentBuilder WithResponseTopic(string responseTopic)
    {
        this.responseTopic = responseTopic;
        return this;
    }

    /// <summary>
    /// Sets the Correlation Data.  This value is a byte array that
    /// indicates the correlation data for the Last Will and Testament message.
    /// <para>
    /// The Correlation Data is used by the sender of the Request Message
    /// to identify which request the Response Message is for when it is
    /// received.
    /// </para>
    /// </summary>
    /// <param name="correlationData">The correlation data in bytes.</param>
    /// <returns>The builder instance.</returns>
    public LastWillAndTestamentBuilder WithCorrelationData(byte[] correlationData)
    {
        this.correlationData = correlationData;
        return this;
    }

    public LastWillAndTestamentBuilder WithUserProperty(string key, string value)
    {
        this.userProperties.Add(key, value);
        return this;
    }

    public LastWillAndTestamentBuilder WithUserProperties(Dictionary<string, string> properties)
    {
        foreach (var property in properties)
        {
            this.userProperties.Add(property.Key, property.Value);
        }

        return this;
    }

    public LastWillAndTestament Build()
    {
        if (this.topic is null)
        {
            throw new HiveMQttClientException("LastWillAndTestament requires a Topic: Topic must not be null");
        }
        else
        {
            Validator.ValidateTopicName(this.topic);
        }

        if (this.payload is null)
        {
            throw new HiveMQttClientException("LastWillAndTestament requires a Payload: Payload must not be null");
        }

        var lastWillAndTestament = new LastWillAndTestament(this.topic, this.payload, this.qos, this.retain);

        if (this.willDelayInterval.HasValue)
        {
            lastWillAndTestament.WillDelayInterval = this.willDelayInterval.Value;
        }

        if (this.payloadFormatIndicator.HasValue)
        {
            lastWillAndTestament.PayloadFormatIndicator = this.payloadFormatIndicator.Value;
        }

        if (this.messageExpiryInterval.HasValue)
        {
            lastWillAndTestament.MessageExpiryInterval = this.messageExpiryInterval.Value;
        }

        if (this.contentType is not null)
        {
            lastWillAndTestament.ContentType = this.contentType;
        }

        if (this.responseTopic is not null)
        {
            lastWillAndTestament.ResponseTopic = this.responseTopic;
        }

        if (this.correlationData is not null)
        {
            lastWillAndTestament.CorrelationData = this.correlationData;
        }

        if (this.userProperties.Count > 0)
        {
            lastWillAndTestament.UserProperties = this.userProperties;
        }

        lastWillAndTestament.Validate();
        return lastWillAndTestament;
    }
}
