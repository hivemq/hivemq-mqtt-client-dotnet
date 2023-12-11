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
using HiveMQtt.MQTT5.Packets;

public class MQTT5PublishMessageBuilder
{
    private readonly MQTT5PublishMessage message;

    public MQTT5PublishMessageBuilder()
    {
        this.message = new MQTT5PublishMessage();
    }

    public MQTT5PublishMessageBuilder WithPayload(byte[] payload)
    {
        this.message.Payload = payload;
        return this;
    }

    public MQTT5PublishMessageBuilder WithPayload(string payload)
    {
        this.message.Payload = Encoding.UTF8.GetBytes(payload);
        return this;
    }

    public MQTT5PublishMessageBuilder WithTopic(string topic)
    {
        this.message.Topic = topic;
        return this;
    }

    public MQTT5PublishMessageBuilder WithQualityOfService(QualityOfService qos)
    {
        this.message.QoS = qos;
        return this;
    }

    public MQTT5PublishMessageBuilder WithUserProperty(string key, string value)
    {
        this.message.UserProperties[key] = value;
        return this;
    }

    public MQTT5PublishMessageBuilder WithUserProperties(Dictionary<string, string> properties)
    {
        foreach (var property in properties)
        {
            this.message.UserProperties[property.Key] = property.Value;
        }

        return this;
    }

    public MQTT5PublishMessageBuilder WithSubscriptionIdentifier(int identifier)
    {
        this.message.SubscriptionIdentifiers.Add(identifier);
        return this;
    }

    public MQTT5PublishMessageBuilder WithPayloadFormatIndicator(MQTT5PayloadFormatIndicator indicator)
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
