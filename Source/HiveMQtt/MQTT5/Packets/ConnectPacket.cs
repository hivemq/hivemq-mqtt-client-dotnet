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

    public ConnectPacket(HiveMQClientOptions clientOptions) => this.clientOptions = clientOptions;

    public override ControlPacketType ControlPacketType => ControlPacketType.Connect;

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
            _ = EncodeUTF8String(vhAndPayloadStream, this.clientOptions.ClientId);
            if (this.clientOptions.UserName != null)
            {
                _ = EncodeUTF8String(vhAndPayloadStream, this.clientOptions.UserName);
            }

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

        // TODO: LWT options
        if (this.clientOptions.UserName != null)
        {
            this.flags |= 0x80;
        }

        if (this.clientOptions.Password != null)
        {
            this.flags |= 0x40;
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
}
