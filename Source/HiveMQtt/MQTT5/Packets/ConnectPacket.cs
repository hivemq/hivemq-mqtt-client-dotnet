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
namespace HiveMQtt.MQTT5.Packets;

using System.IO;
using System.Text;
using HiveMQtt.Client.Options;
using HiveMQtt.MQTT5.Types;

/// <summary>
/// An MQTT Connect Control Packet as defined in:
/// https://docs.oasis-open.org/mqtt/mqtt/v5.0/os/mqtt-v5.0-os.html#_Toc3901033.
/// </summary>
public class ConnectPacket : ControlPacket
{
    private readonly HiveMQClientOptions clientOptions;

    private byte flags;

    public ConnectPacket(HiveMQClientOptions clientOptions)
    {
        this.clientOptions = clientOptions;
        this.LastWillProperties = new MQTT5LastWillProperties();
    }

    public override ControlPacketType ControlPacketType => ControlPacketType.Connect;

    /// <summary>
    /// Gets or sets the MQTT v5
    /// <see href="https://docs.oasis-open.org/mqtt/mqtt/v5.0/os/mqtt-v5.0-os.html#_Toc3901060">Will Properties</see>.
    /// </summary>
    internal MQTT5LastWillProperties LastWillProperties { get; set; }

    /// <summary>
    /// Encode this packet to be sent on the wire.
    /// </summary>
    /// <returns>An array of bytes ready to be sent.</returns>
    public byte[] Encode()
    {
        this.GatherConnectFlagsAndProperties();

        using (var vhAndPayloadStream = new MemoryStream())
        {
            // Variable Header
            vhAndPayloadStream.WriteByte(0);
            vhAndPayloadStream.WriteByte(4);
            vhAndPayloadStream.Write(Encoding.UTF8.GetBytes("MQTT"));
            vhAndPayloadStream.WriteByte(0x5); // Protocol Version
            vhAndPayloadStream.WriteByte(this.flags); // Connect Flags
            _ = EncodeTwoByteInteger(vhAndPayloadStream, this.clientOptions.KeepAlive);
            this.EncodeProperties(vhAndPayloadStream);

            // Payload
            // Client Identifier
            _ = EncodeUTF8String(vhAndPayloadStream, this.clientOptions.ClientId);

            // Last Will and Testament
            if (this.clientOptions.LastWillAndTestament != null)
            {
                // Will Properties
                this.EncodeLastWillProperties(vhAndPayloadStream);

                // Will Topic
                _ = EncodeUTF8String(vhAndPayloadStream, this.clientOptions.LastWillAndTestament.Topic);

                // Will Payload
                _ = EncodeBinaryData(vhAndPayloadStream, this.clientOptions.LastWillAndTestament.Payload);
            }

            // Username
            if (this.clientOptions.UserName != null)
            {
                _ = EncodeUTF8String(vhAndPayloadStream, this.clientOptions.UserName);
            }

            // Password
            if (this.clientOptions.Password != null)
            {
                _ = EncodeUTF8String(vhAndPayloadStream, this.clientOptions.Password);
            }

            // Construct the final packet
            var constructedPacket = new MemoryStream((int)vhAndPayloadStream.Length + 5);

            // Write the Fixed Header
            constructedPacket.WriteByte(((byte)ControlPacketType.Connect) << 4);
            _ = EncodeVariableByteInteger(constructedPacket, (int)vhAndPayloadStream.Length);

            // Copy the Variable Header and Payload
            vhAndPayloadStream.Position = 0;
            vhAndPayloadStream.CopyTo(constructedPacket);

            return constructedPacket.ToArray();
        }
    }

    /// <summary>
    /// Gather the connect flags and properties needed for a connect call.
    /// </summary>
    internal void GatherConnectFlagsAndProperties()
    {
        this.clientOptions.ValidateOptions();

        this.flags = 0x0;
        if (this.clientOptions.CleanStart is true)
        {
            this.flags |= 0x2;
        }

        if (this.clientOptions.LastWillAndTestament != null)
        {
            // Will Flag
            this.flags |= 0x4;

            // Will QoS
            if (this.clientOptions.LastWillAndTestament.QoS == QualityOfService.AtLeastOnceDelivery)
            {
                this.flags |= 0x8;
            }
            else if (this.clientOptions.LastWillAndTestament.QoS == QualityOfService.ExactlyOnceDelivery)
            {
                this.flags |= 0x10;
            }

            // Will Retain
            if (this.clientOptions.LastWillAndTestament.Retain)
            {
                this.flags |= 0x20;
            }
        }

        if (this.clientOptions.Password != null)
        {
            this.flags |= 0x40;
        }

        if (this.clientOptions.UserName != null)
        {
            this.flags |= 0x80;
        }

        // Properties
        this.Properties.SessionExpiryInterval = (UInt32)this.clientOptions.SessionExpiryInterval;

        if (this.clientOptions.ClientReceiveMaximum != null)
        {
            this.Properties.ReceiveMaximum = (UInt16)this.clientOptions.ClientReceiveMaximum;
        }

        if (this.clientOptions.ClientMaximumPacketSize != null)
        {
            this.Properties.MaximumPacketSize = (UInt16)this.clientOptions.ClientMaximumPacketSize;
        }

        if (this.clientOptions.ClientTopicAliasMaximum != null)
        {
            this.Properties.TopicAliasMaximum = (ushort)RangeValidate(
                (int)this.clientOptions.ClientTopicAliasMaximum,
                MQTT5DataType.TwoByteInteger);
        }

        if (this.clientOptions.RequestResponseInformation != null)
        {
            if (this.clientOptions.RequestResponseInformation == true)
            {
                this.Properties.RequestResponseInformation = 1;
            }
            else
            {
                this.Properties.RequestResponseInformation = 0;
            }
        }

        if (this.clientOptions.RequestProblemInformation != null)
        {
            if (this.clientOptions.RequestProblemInformation == true)
            {
                this.Properties.RequestProblemInformation = 1;
            }
            else
            {
                this.Properties.RequestProblemInformation = 0;
            }
        }

        if (this.clientOptions.UserProperties != null)
        {
            this.Properties.UserProperties = this.clientOptions.UserProperties;
        }

        if (this.clientOptions.AuthenticationMethod != null)
        {
            this.Properties.AuthenticationMethod = this.clientOptions.AuthenticationMethod;
        }

        if (this.clientOptions.AuthenticationData != null)
        {
            this.Properties.AuthenticationData = this.clientOptions.AuthenticationData;
        }

        // Last Will and Testament Properties
        if (this.clientOptions.LastWillAndTestament != null)
        {
            if (this.clientOptions.LastWillAndTestament.WillDelayInterval.HasValue)
            {
                this.LastWillProperties.WillDelayInterval = (UInt32)this.clientOptions.LastWillAndTestament.WillDelayInterval;
            }

            if (this.clientOptions.LastWillAndTestament.PayloadFormatIndicator.HasValue)
            {
                this.LastWillProperties.PayloadFormatIndicator = (byte)this.clientOptions.LastWillAndTestament.PayloadFormatIndicator;
            }

            if (this.clientOptions.LastWillAndTestament.MessageExpiryInterval.HasValue)
            {
                this.LastWillProperties.MessageExpiryInterval = (UInt32)this.clientOptions.LastWillAndTestament.MessageExpiryInterval;
            }

            if (this.clientOptions.LastWillAndTestament.ContentType != null)
            {
                this.LastWillProperties.ContentType = this.clientOptions.LastWillAndTestament.ContentType;
            }

            if (this.clientOptions.LastWillAndTestament.ResponseTopic != null)
            {
                this.LastWillProperties.ResponseTopic = this.clientOptions.LastWillAndTestament.ResponseTopic;
            }

            if (this.clientOptions.LastWillAndTestament.CorrelationData != null)
            {
                this.LastWillProperties.CorrelationData = this.clientOptions.LastWillAndTestament.CorrelationData;
            }

            if (this.clientOptions.LastWillAndTestament.UserProperties != null)
            {
                this.LastWillProperties.UserProperties = this.clientOptions.LastWillAndTestament.UserProperties;
            }
        }
    }

    /// <summary>
    /// Encode a stream of Last Will and Testament Properties.
    /// </summary>
    /// <param name="writer">MemoryStream to encode the properties into.</param>
    protected void EncodeLastWillProperties(MemoryStream writer)
    {
        var propertiesLength = 0;

        if (writer.Length > int.MaxValue)
        {
            throw new ArgumentOutOfRangeException(nameof(writer), "The writer stream is too large to encode.");
        }

        var propertyStream = new MemoryStream((int)writer.Length);

        if (this.LastWillProperties.WillDelayInterval != null)
        {
            propertiesLength += EncodeVariableByteInteger(propertyStream, (int)MQTT5PropertyType.WillDelayInterval);
            propertiesLength += EncodeFourByteInteger(propertyStream, (uint)this.LastWillProperties.WillDelayInterval);
        }

        if (this.LastWillProperties.PayloadFormatIndicator != null)
        {
            propertiesLength += EncodeVariableByteInteger(propertyStream, (int)MQTT5PropertyType.PayloadFormatIndicator);
            propertyStream.WriteByte((byte)this.LastWillProperties.PayloadFormatIndicator);
            propertiesLength++;
        }

        if (this.LastWillProperties.MessageExpiryInterval != null)
        {
            propertiesLength += EncodeVariableByteInteger(propertyStream, (int)MQTT5PropertyType.MessageExpiryInterval);
            propertiesLength += EncodeFourByteInteger(propertyStream, (uint)this.LastWillProperties.MessageExpiryInterval);
        }

        if (this.LastWillProperties.ContentType != null)
        {
            propertiesLength += EncodeVariableByteInteger(propertyStream, (int)MQTT5PropertyType.ContentType);
            propertiesLength += EncodeUTF8String(propertyStream, this.LastWillProperties.ContentType);
        }

        if (this.LastWillProperties.ResponseTopic != null)
        {
            propertiesLength += EncodeVariableByteInteger(propertyStream, (int)MQTT5PropertyType.ResponseTopic);
            propertiesLength += EncodeUTF8String(propertyStream, this.LastWillProperties.ResponseTopic);
        }

        if (this.LastWillProperties.CorrelationData != null)
        {
            propertiesLength += EncodeVariableByteInteger(propertyStream, (int)MQTT5PropertyType.CorrelationData);
            propertiesLength += EncodeBinaryData(propertyStream, this.LastWillProperties.CorrelationData);
        }

        if (this.LastWillProperties.UserProperties.Count > 0)
        {
            foreach (var property in this.LastWillProperties.UserProperties)
            {
                propertiesLength += EncodeVariableByteInteger(propertyStream, (int)MQTT5PropertyType.UserProperty);
                propertiesLength += EncodeUTF8String(propertyStream, property.Key);
                propertiesLength += EncodeUTF8String(propertyStream, property.Value);
            }
        }

        _ = EncodeVariableByteInteger(writer, propertiesLength);

        _ = propertyStream.Seek(0, SeekOrigin.Begin);
        propertyStream.CopyTo(writer);
    }

    /// <summary>
    /// Validate a value against a given range.
    /// </summary>
    /// <param name="value">The value to validate.</param>
    /// <param name="min">The minimum value.</param>
    /// <param name="max">The maximum value.</param>
    /// <returns>The value if it is within the range, otherwise the closest value.</returns>
    internal static int RangeValidate(int value, int min, int max)
    {
        var result = value;

        if (value < min)
        {
            result = min;
        }
        else if (value > max)
        {
            result = max;
        }

        return result;
    }

    /// <summary>
    /// Validate a value against the range of a given MQTT5DataType.
    /// </summary>
    /// <param name="value">The value to validate.</param>
    /// <param name="type">The type to validate against.</param>
    /// <returns>The value if it is within the range of the type, otherwise the closest value.</returns>
    internal static int RangeValidate(int value, MQTT5DataType type)
    {
        var result = value;

        switch (type)
        {
            case MQTT5DataType.TwoByteInteger:
                if (value < 0)
                {
                    result = 0;
                }
                else if (value > 255)
                {
                    result = 255;
                }

                break;
            case MQTT5DataType.FourByteInteger:
                if (value < 0)
                {
                    result = 0;
                }
                else if (value > 65535)
                {
                    result = 65535;
                }

                break;
            case MQTT5DataType.VariableByteInteger:
                if (value < 0)
                {
                    result = 0;
                }
                else if (value > 268435455)
                {
                    result = 268435455;
                }

                break;
            default:
                result = value;
                break;
        }

        return result;
    }

}
