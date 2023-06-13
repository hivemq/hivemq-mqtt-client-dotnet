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
namespace HiveMQtt.MQTT5;

using System;
using System.Buffers;
using System.IO;
using System.Text;
using HiveMQtt.MQTT5.Exceptions;
using HiveMQtt.MQTT5.Types;

/// <summary>
/// A base class for all MQTT Control Packet types.
/// </summary>
public abstract class ControlPacket
{
    public ControlPacket() => this.Properties = new MQTT5Properties();

    /// <summary>
    /// Gets the timestamp of when this packet was sent.
    /// </summary>
    public DateTime SentOn { get; internal set; }

    /// <summary>
    /// Gets timestamp of when this packet was received.
    /// </summary>
    public DateTime ReceivedOn { get; internal set; }

    /// <summary>
    /// Gets the type of this <c>ControlPacket</c>.
    ///
    /// See also <seealso href="https://docs.oasis-open.org/mqtt/mqtt/v5.0/os/mqtt-v5.0-os.html#_Toc3901022">
    /// MQTT Control Packet Types</seealso>.
    /// </summary>
    public abstract ControlPacketType ControlPacketType { get; }

    /// <summary>
    /// Decode an MQTT Variable Byte Integer data representation.
    ///
    /// See also <seealso href="https://docs.oasis-open.org/mqtt/mqtt/v5.0/os/mqtt-v5.0-os.html#_Toc3901011">
    /// Data Representation: Variable Byte Integer</seealso>.
    /// </summary>
    /// <param name="reader"><cref>SequenceReader</cref> containing the packet data to be decoded.</param>
    /// <param name="lengthInBytes">The length of the Variable Byte Integer in bytes.</param>
    /// <returns>The integer value of the Variable Byte Integer.</returns>
    public static int DecodeVariableByteInteger(ref SequenceReader<byte> reader, out int lengthInBytes)
    {
        var multiplier = 1;
        var value = 0;
        byte encodedByte;
        lengthInBytes = 0;

        do
        {
            lengthInBytes++;

            if (reader.TryRead(out encodedByte))
            {
                value += (encodedByte & 127) * multiplier;

                if (multiplier > 128 * 128 * 128)
                {
                    throw new MalformedVBIException();
                }

                multiplier *= 128;
            }
        }
        while ((encodedByte & 128) != 0);

        return value;
    }

    public static int DecodeVariableByteInteger(ref SequenceReader<byte> reader) => DecodeVariableByteInteger(ref reader, out _);

    /// <summary>
    /// Gets or sets the MQTT v5
    /// <see href="https://docs.oasis-open.org/mqtt/mqtt/v5.0/os/mqtt-v5.0-os.html#_Toc3901027">Properties</see>.
    /// </summary>
    internal MQTT5Properties Properties { get; set; }

    /// <summary>
    /// Gets or sets the packet identifier for this packet.
    /// </summary>
    internal ushort PacketIdentifier { get; set; }

    /// <summary>
    /// Encode a UTF-8 string into a <c>MemoryStream</c>.
    ///
    /// See also <seealso href="https://docs.oasis-open.org/mqtt/mqtt/v5.0/os/mqtt-v5.0-os.html#_Toc3901010">
    /// Data Representation: UTF-8 Encoded String</seealso>.
    /// </summary>
    /// <param name="stream">The <cref>MemoryStream</cref> to write the UTF-8 string into.</param>
    /// <param name="s">The string to be encoded and written.</param>
    /// <returns>The number of bytes written into the MemoryStream.</returns>
    protected static int EncodeUTF8String(MemoryStream stream, string s)
    {
        var length = (ushort)s.Length;

        EncodeTwoByteInteger(stream, length);

        var stringBytes = Encoding.UTF8.GetBytes(s);
        stream.Write(stringBytes, 0, stringBytes.Length);

        return 2 + stringBytes.Length;
    }

    /// <summary>
    /// Decode an MQTT UTF-8 string.
    ///
    /// See also <seealso href="https://docs.oasis-open.org/mqtt/mqtt/v5.0/os/mqtt-v5.0-os.html#_Toc3901010">
    /// Data Representation: UTF-8 Encoded String</seealso>.
    /// </summary>
    /// <param name="reader">SequenceReader containing the packet data to be decoded.</param>
    /// <returns>A string containing the UTF-8 string.</returns>
    protected static string? DecodeUTF8String(ref SequenceReader<byte> reader)
    {

        if (reader.TryReadBigEndian(out Int16 stringLength))
        {
            var array = new byte[stringLength];
            var span = new Span<byte>(array);

            for (var i = 0; i < stringLength; i++)
            {
                if (reader.TryRead(out var outValue))
                {
                    span[i] = outValue;
                }
            }

            return Encoding.UTF8.GetString(span.ToArray());
        }

        return null;
    }

    /// <summary>
    /// Encode a Two Byte Integer into a <c>MemoryStream</c>.
    ///
    /// See also <seealso href="https://docs.oasis-open.org/mqtt/mqtt/v5.0/os/mqtt-v5.0-os.html#_Toc3901008">
    /// Data Representation: Two Byte Integer</seealso>.
    /// </summary>
    /// <param name="stream">The <cref>MemoryStream</cref> to write the Two Byte Integer into.</param>
    /// <param name="value">The integer to be encoded and written.</param>
    /// <returns>The value 2 is always returned.</returns>
    protected static int EncodeTwoByteInteger(MemoryStream stream, int value)
    {
        var converted = Convert.ToUInt16(value);
        EncodeTwoByteInteger(stream, converted);
        return 2;
    }

    /// <summary>
    /// Encode a Two Byte Integer into a <c>MemoryStream</c>.
    ///
    /// See also <seealso href="https://docs.oasis-open.org/mqtt/mqtt/v5.0/os/mqtt-v5.0-os.html#_Toc3901008">
    /// Data Representation: Two Byte Integer</seealso>.
    /// </summary>
    /// <param name="stream">The <cref>MemoryStream</cref> to write the Two Byte Integer into.</param>
    /// <param name="value">The integer to be encoded and written.</param>
    protected static void EncodeTwoByteInteger(MemoryStream stream, UInt16 value)
    {
        var valueInBytes = BitConverter.GetBytes(value);

        if (BitConverter.IsLittleEndian)
        {
            Array.Reverse(valueInBytes);
        }

        stream.WriteByte(valueInBytes[0]);
        stream.WriteByte(valueInBytes[1]);
    }

    /// <summary>
    /// Decode an MQTT Two Byte Integer.
    ///
    /// See also <seealso href="https://docs.oasis-open.org/mqtt/mqtt/v5.0/os/mqtt-v5.0-os.html#_Toc3901008">
    /// Data Representation: Two Byte Integer</seealso>.
    /// </summary>
    /// <param name="reader">SequenceReader containing the packet data to be decoded.</param>
    /// <returns>The value of the two byte integer.</returns>
    protected static UInt16? DecodeTwoByteInteger(ref SequenceReader<byte> reader)
    {
        if (reader.TryReadBigEndian(out Int16 intValue))
        {
            return (UInt16)intValue;
        }
        return null;
    }

    /// <summary>
    /// Encode an Integer into a <c>MemoryStream</c> as an MQTT Variable Byte Integer.
    ///
    /// See also <seealso href="https://docs.oasis-open.org/mqtt/mqtt/v5.0/os/mqtt-v5.0-os.html#_Toc3901011">
    /// Data Representation: Variable Byte Integer</seealso>.
    /// </summary>
    /// <param name="stream">The <cref>MemoryStream</cref> to write the Two Byte Integer into.</param>
    /// <param name="number">The integer to be encoded and written.</param>
    /// <returns>The number of bytes written into the stream.</returns>
    protected static short EncodeVariableByteInteger(MemoryStream stream, int number)
    {
        if (number is < 0 or > 268435455)
        {
            throw new MQTTProtocolException("Value out of range for a variable byte integer: {value}");
        }

        if (number <= 0x7F)
        {
            stream.WriteByte((byte)number);
            return 1;
        }

        short written = 0;
        var value = number;
        do
        {
            var encodedByte = value % 0x80;
            value /= 0x80;
            if (value > 0)
            {
                encodedByte |= 0x80;
            }

            stream.WriteByte((byte)encodedByte);
            written++;
        }
        while (value > 0);

        return written;
    }

    /// <summary>
    /// Encode an MQTT Binary Data data representation.
    ///
    /// See also <seealso href="https://docs.oasis-open.org/mqtt/mqtt/v5.0/os/mqtt-v5.0-os.html#_Toc3901012">
    /// Data Representation: Binary Data</seealso>.
    /// </summary>
    /// <param name="writer">MemoryStream to encode the data into.</param>
    /// <param name="binaryData">The binary data to encode.</param>
    /// <returns>A byte[] containing the binary data.</returns>
    protected static int EncodeBinaryData(MemoryStream writer, byte[] binaryData)
    {
        _ = EncodeTwoByteInteger(writer, binaryData.Length);
        writer.Write(binaryData);
        return 2 + binaryData.Length;
    }

    /// <summary>
    /// Decode an MQTT Binary Data data representation.
    ///
    /// See also <seealso href="https://docs.oasis-open.org/mqtt/mqtt/v5.0/os/mqtt-v5.0-os.html#_Toc3901012">
    /// Data Representation: Binary Data</seealso>.
    /// </summary>
    /// <param name="reader">SequenceReader containing the packet data to be decoded.</param>
    /// <returns>A byte[] containing the binary data.</returns>
    protected static byte[]? DecodeBinaryData(ref SequenceReader<byte> reader)
    {

        if (reader.TryReadBigEndian(out Int16 stringLength))
        {
            var array = new byte[stringLength];
            var span = new Span<byte>(array);

            for (var i = 0; i < stringLength; i++)
            {
                if (reader.TryRead(out var outValue))
                {
                    span[i] = outValue;
                }
            }

            return span.ToArray();
        }

        return null;
    }

    /// <summary>
    /// Encode a Four Byte Integer into a <c>MemoryStream</c>.
    ///
    /// See also <seealso href="https://docs.oasis-open.org/mqtt/mqtt/v5.0/os/mqtt-v5.0-os.html#_Toc3901009">
    /// Data Representation: Four Byte Integer</seealso>.
    /// </summary>
    /// <param name="stream">The <cref>MemoryStream</cref> to write the Four Byte Integer into.</param>
    /// <param name="value">The integer to be encoded and written.</param>
    /// <returns>The value 4 is always returned.</returns>
    protected static int EncodeFourByteInteger(MemoryStream stream, UInt32 value)
    {
        var valueInBytes = BitConverter.GetBytes(value);

        if (BitConverter.IsLittleEndian)
        {
            Array.Reverse(valueInBytes);
        }

        stream.WriteByte(valueInBytes[0]);
        stream.WriteByte(valueInBytes[1]);
        stream.WriteByte(valueInBytes[2]);
        stream.WriteByte(valueInBytes[3]);

        // We return this just as a helper to simplify code for this pattern:
        // propertiesLength += EncodeVariableByteInteger(...)
        // propertiesLength += EncodeFourByteInteger(...)
        return 4;
    }

    /// <summary>
    /// Decode an MQTT Four Byte Integer data representation.
    ///
    /// See also <seealso href="https://docs.oasis-open.org/mqtt/mqtt/v5.0/os/mqtt-v5.0-os.html#_Toc3901009">
    /// Data Representation: Binary Data</seealso>.
    /// </summary>
    /// <param name="reader">SequenceReader containing the packet data to be decoded.</param>
    /// <returns>The value of the four byte integer.</returns>
    protected static UInt32? DecodeFourByteInteger(ref SequenceReader<byte> reader)
    {
        if (reader.TryReadBigEndian(out Int32 intValue))
        {
            return (UInt32)intValue;
        }

        return null;
    }

    /// <summary>
    /// Reads and returns one byte.  This is centralized here for error handling.
    /// </summary>
    /// <param name="reader">SequenceReader containing the packet data to be decoded.</param>
    /// <returns>The byte value read from the SequenceReader.</returns>
    protected static byte? DecodeByte(ref SequenceReader<byte> reader)
    {
        if (reader.TryRead(out var byteValue))
        {
            return byteValue;
        }

        return null;
    }

    /// <summary>
    /// Reads and returns one byte to be interpreted as a boolean value.
    /// A Byte with a value of either 0 or 1. It is a Protocol Error to have a value other than 0 or 1.
    /// </summary>
    /// <param name="reader">SequenceReader containing the packet data to be decoded.</param>
    /// <returns>The byte value read from the SequenceReader.</returns>
    protected static bool DecodeByteAsBool(ref SequenceReader<byte> reader)
    {
        if (reader.TryRead(out var byteValue))
        {
            if (byteValue == 0x0)
            {
                return false;
            }
            else if (byteValue == 0x1)
            {
                return true;
            }
            else
            {
                throw new MQTTProtocolException($"Invalid boolean value. {byteValue} is not a valid boolean value.");
            }
        }

        return false;
    }

    /// <summary>
    /// Encode a stream of MQTT Properties.
    /// <para>
    ///
    /// See also <seealso href="https://docs.oasis-open.org/mqtt/mqtt/v5.0/os/mqtt-v5.0-os.html#_Toc3901027">
    /// Variable Header Properties</seealso>.
    /// </para>
    /// </summary>
    /// <param name="writer">MemoryStream to encode the properties into.</param>
    protected void EncodeProperties(MemoryStream writer)
    {
        var propertiesLength = 0;

        if (writer.Length > int.MaxValue)
        {
            throw new ArgumentOutOfRangeException("writer", "The writer stream is too large to encode.");
        }

        var propertyStream = new MemoryStream((int)writer.Length);

        if (this.Properties.PayloadFormatIndicator != null)
        {
            propertiesLength += EncodeVariableByteInteger(propertyStream, (int)MQTT5PropertyType.PayloadFormatIndicator);
            propertyStream.WriteByte((byte)this.Properties.PayloadFormatIndicator);
            propertiesLength++;
        }

        if (this.Properties.MessageExpiryInterval != null)
        {
            propertiesLength += EncodeVariableByteInteger(propertyStream, (int)MQTT5PropertyType.MessageExpiryInterval);
            propertiesLength += EncodeFourByteInteger(propertyStream, (uint)this.Properties.MessageExpiryInterval);
        }

        if (this.Properties.ContentType != null)
        {
            propertiesLength += EncodeVariableByteInteger(propertyStream, (int)MQTT5PropertyType.ContentType);
            propertiesLength += EncodeUTF8String(propertyStream, this.Properties.ContentType);
        }

        if (this.Properties.ResponseTopic != null)
        {
            propertiesLength += EncodeVariableByteInteger(propertyStream, (int)MQTT5PropertyType.ResponseTopic);
            propertiesLength += EncodeUTF8String(propertyStream, this.Properties.ResponseTopic);
        }

        if (this.Properties.CorrelationData != null)
        {
            propertiesLength += EncodeVariableByteInteger(propertyStream, (int)MQTT5PropertyType.CorrelationData);
            propertiesLength += EncodeBinaryData(propertyStream, this.Properties.CorrelationData);
        }

        if (this.Properties.SubscriptionIdentifier != null)
        {
            propertiesLength += EncodeVariableByteInteger(propertyStream, (int)MQTT5PropertyType.SubscriptionIdentifier);
            propertiesLength += EncodeVariableByteInteger(propertyStream, (int)this.Properties.SubscriptionIdentifier);
        }

        if (this.Properties.SessionExpiryInterval != null)
        {
            propertiesLength += EncodeVariableByteInteger(propertyStream, (int)MQTT5PropertyType.SessionExpiryInterval);
            propertiesLength += EncodeFourByteInteger(propertyStream, (uint)this.Properties.SessionExpiryInterval);
        }

        if (this.Properties.AuthenticationMethod != null)
        {
            propertiesLength += EncodeVariableByteInteger(propertyStream, (int)MQTT5PropertyType.AuthenticationMethod);
            propertiesLength += EncodeUTF8String(propertyStream, this.Properties.AuthenticationMethod);
        }

        if (this.Properties.AuthenticationData != null)
        {
            propertiesLength += EncodeVariableByteInteger(propertyStream, (int)MQTT5PropertyType.AuthenticationData);
            propertiesLength += EncodeBinaryData(propertyStream, this.Properties.AuthenticationData);
        }

        if (this.Properties.RequestProblemInformation != null)
        {
            propertiesLength += EncodeVariableByteInteger(propertyStream, (int)MQTT5PropertyType.RequestProblemInformation);
            propertyStream.WriteByte((byte)this.Properties.RequestProblemInformation);
            propertiesLength++;
        }

        if (this.Properties.WillDelayInterval != null)
        {
            propertiesLength += EncodeVariableByteInteger(propertyStream, (int)MQTT5PropertyType.WillDelayInterval);
            propertiesLength += EncodeFourByteInteger(propertyStream, (uint)this.Properties.WillDelayInterval);
        }

        if (this.Properties.RequestResponseInformation != null)
        {
            propertiesLength += EncodeVariableByteInteger(propertyStream, (int)MQTT5PropertyType.RequestResponseInformation);
            propertyStream.WriteByte((byte)this.Properties.RequestResponseInformation);
            propertiesLength++;
        }

        if (this.Properties.ServerReference != null)
        {
            propertiesLength += EncodeVariableByteInteger(propertyStream, (int)MQTT5PropertyType.ServerReference);
            propertiesLength += EncodeUTF8String(propertyStream, this.Properties.ServerReference);
        }

        if (this.Properties.ReasonString != null)
        {
            propertiesLength += EncodeVariableByteInteger(propertyStream, (int)MQTT5PropertyType.ReasonString);
            propertiesLength += EncodeUTF8String(propertyStream, this.Properties.ReasonString);
        }

        if (this.Properties.ReceiveMaximum != null)
        {
            propertiesLength += EncodeVariableByteInteger(propertyStream, (int)MQTT5PropertyType.ReceiveMaximum);
            propertiesLength += EncodeTwoByteInteger(propertyStream, (int)this.Properties.ReceiveMaximum);
        }

        if (this.Properties.TopicAliasMaximum != null)
        {
            propertiesLength += EncodeVariableByteInteger(propertyStream, (int)MQTT5PropertyType.TopicAliasMaximum);
            propertiesLength += EncodeTwoByteInteger(propertyStream, (int)this.Properties.TopicAliasMaximum);
        }

        if (this.Properties.TopicAlias != null)
        {
            propertiesLength += EncodeVariableByteInteger(propertyStream, (int)MQTT5PropertyType.TopicAlias);
            propertiesLength += EncodeTwoByteInteger(propertyStream, (int)this.Properties.TopicAlias);
        }

        if (this.Properties.MaximumPacketSize != null)
        {
            propertiesLength += EncodeVariableByteInteger(propertyStream, (int)MQTT5PropertyType.MaximumPacketSize);
            propertiesLength += EncodeFourByteInteger(propertyStream, (uint)this.Properties.MaximumPacketSize);
        }

        if (this.Properties.UserProperties.Count > 0)
        {
            foreach (var property in this.Properties.UserProperties)
            {
                propertiesLength += EncodeVariableByteInteger(propertyStream, (int)MQTT5PropertyType.UserProperty);
                propertiesLength += EncodeUTF8String(propertyStream, (string)property.Key);
                propertiesLength += EncodeUTF8String(propertyStream, (string)property.Value);
            }
        }

        _ = EncodeVariableByteInteger(writer, propertiesLength);

        _ = propertyStream.Seek(0, SeekOrigin.Begin);
        propertyStream.CopyTo(writer);
    }

    /// <summary>
    /// Decode a stream of MQTT Properties.
    ///
    /// The resulting properties are populated in <cref>this.Properties</cref>.
    ///
    /// See also <seealso href="https://docs.oasis-open.org/mqtt/mqtt/v5.0/os/mqtt-v5.0-os.html#_Toc3901027">
    /// Variable Header Properties</seealso>.
    /// </summary>
    /// <param name="reader">SequenceReader containing the packet data to be decoded.</param>
    /// <param name="length">Length of the properties to be decoded.</param>
    /// <returns>A boolean representing the success or failure of the decoding process.</returns>
    protected bool DecodeProperties(ref SequenceReader<byte> reader, int length)
    {
        if (length == 0)
        {
            return true;
        }

        var readerStart = reader.Consumed;

        do
        {
            var propertyID = DecodeVariableByteInteger(ref reader);

            switch ((MQTT5PropertyType)propertyID)
            {
                case MQTT5PropertyType.PayloadFormatIndicator:
                    this.Properties.PayloadFormatIndicator = DecodeByte(ref reader);
                    break;
                case MQTT5PropertyType.MessageExpiryInterval:
                    this.Properties.MessageExpiryInterval = DecodeFourByteInteger(ref reader);
                    break;
                case MQTT5PropertyType.ContentType:
                    this.Properties.ContentType = DecodeUTF8String(ref reader);
                    break;
                case MQTT5PropertyType.ResponseTopic:
                    this.Properties.ResponseTopic = DecodeUTF8String(ref reader);
                    break;
                case MQTT5PropertyType.CorrelationData:
                    this.Properties.CorrelationData = DecodeBinaryData(ref reader);
                    break;
                case MQTT5PropertyType.SubscriptionIdentifier:
                    this.Properties.SubscriptionIdentifier = (Int32)DecodeVariableByteInteger(ref reader);
                    break;
                case MQTT5PropertyType.SessionExpiryInterval:
                    this.Properties.SessionExpiryInterval = DecodeFourByteInteger(ref reader);
                    break;
                case MQTT5PropertyType.AssignedClientIdentifier:
                    this.Properties.AssignedClientIdentifier = DecodeUTF8String(ref reader);
                    break;
                case MQTT5PropertyType.ServerKeepAlive:
                    this.Properties.ServerKeepAlive = DecodeTwoByteInteger(ref reader);
                    break;
                case MQTT5PropertyType.AuthenticationMethod:
                    this.Properties.AuthenticationMethod = DecodeUTF8String(ref reader);
                    break;
                case MQTT5PropertyType.AuthenticationData:
                    this.Properties.AuthenticationData = DecodeBinaryData(ref reader);
                    break;
                case MQTT5PropertyType.RequestProblemInformation:
                    this.Properties.RequestProblemInformation = DecodeByte(ref reader);
                    break;
                case MQTT5PropertyType.WillDelayInterval:
                    this.Properties.WillDelayInterval = DecodeFourByteInteger(ref reader);
                    break;
                case MQTT5PropertyType.RequestResponseInformation:
                    this.Properties.RequestResponseInformation = DecodeByte(ref reader);
                    break;
                case MQTT5PropertyType.ResponseInformation:
                    this.Properties.ResponseInformation = DecodeUTF8String(ref reader);
                    break;
                case MQTT5PropertyType.ServerReference:
                    this.Properties.ServerReference = DecodeUTF8String(ref reader);
                    break;
                case MQTT5PropertyType.ReasonString:
                    this.Properties.ReasonString = DecodeUTF8String(ref reader);
                    break;
                case MQTT5PropertyType.ReceiveMaximum:
                    this.Properties.ReceiveMaximum = DecodeTwoByteInteger(ref reader);
                    break;
                case MQTT5PropertyType.TopicAliasMaximum:
                    this.Properties.TopicAliasMaximum = DecodeTwoByteInteger(ref reader);
                    break;
                case MQTT5PropertyType.TopicAlias:
                    this.Properties.TopicAlias = DecodeTwoByteInteger(ref reader);
                    break;
                case MQTT5PropertyType.MaximumQoS:
                    this.Properties.MaximumQoS = DecodeByte(ref reader);
                    break;
                case MQTT5PropertyType.RetainAvailable:
                    this.Properties.RetainAvailable = DecodeByteAsBool(ref reader);
                    break;
                case MQTT5PropertyType.UserProperty:
                    var key = DecodeUTF8String(ref reader);
                    var value = DecodeUTF8String(ref reader);

                    if (key != null && value != null)
                    {
                        this.Properties.UserProperties.Add(key, value);
                    }

                    break;
                case MQTT5PropertyType.MaximumPacketSize:
                    this.Properties.MaximumPacketSize = DecodeFourByteInteger(ref reader);
                    break;
                case MQTT5PropertyType.WildcardSubscriptionAvailable:
                    this.Properties.WildcardSubscriptionAvailable = DecodeByteAsBool(ref reader);
                    break;
                case MQTT5PropertyType.SubscriptionIdentifierAvailable:
                    this.Properties.SubscriptionIdentifiersAvailable = DecodeByteAsBool(ref reader);
                    break;
                case MQTT5PropertyType.SharedSubscriptionAvailable:
                    this.Properties.SharedSubscriptionAvailable = DecodeByteAsBool(ref reader);
                    break;

                default:
                    break;
            }
        }
        while (reader.Consumed - readerStart < length);

        return true;
    }
}
