namespace HiveMQtt.MQTT5;

using System;
using System.Buffers;
using System.IO;
using System.Text;
using HiveMQtt.MQTT5.Types;

/// <summary>
/// A base class for all MQTT Control Packet types.
/// </summary>
public abstract class ControlPacket
{
    public ControlPacket() => this.Properties = new MQTT5Properties();

    /// <summary>
    /// Gets or sets the MQTT v5
    /// <see href="https://docs.oasis-open.org/mqtt/mqtt/v5.0/os/mqtt-v5.0-os.html#_Toc3901027">Properties</see>.
    /// </summary>
    public MQTT5Properties Properties { get; set; }

    /// <summary>
    /// Gets the raw packet data received from or to be sent on the wire.
    /// </summary>
    public ReadOnlySequence<byte> RawPacketData { get; internal set; }

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
    /// Encode a UTF-8 string into a <c>MemoryStream</c>.
    ///
    /// See also <seealso href="https://docs.oasis-open.org/mqtt/mqtt/v5.0/os/mqtt-v5.0-os.html#_Toc3901010">
    /// Data Representation: UTF-8 Encoded String</seealso>.
    /// </summary>
    /// <param name="stream">The <cref>MemoryStream</cref> to write the UTF-8 string into.</param>
    /// <param name="s">The string to be encoded and written.</param>
    protected static void EncodeUTF8String(MemoryStream stream, string s)
    {
        var length = (ushort)s.Length;

        EncodeTwoByteInteger(stream, length);

        var stringBytes = Encoding.UTF8.GetBytes(s);
        stream.Write(stringBytes, 0, stringBytes.Length);
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
    protected static void EncodeTwoByteInteger(MemoryStream stream, int value)
    {
        var converted = Convert.ToUInt16(value);
        EncodeTwoByteInteger(stream, converted);
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
    protected static int? DecodeTwoByteInteger(ref SequenceReader<byte> reader)
    {
        if (reader.TryReadBigEndian(out Int16 intValue))
        {
            return intValue;
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
    protected static void EncodeVariableByteInteger(MemoryStream stream, int number)
    {
        do
        {
            var digit = number % 0x80;
            number /= 0x80;
            if (number > 0)
            {
                number |= 0x80;
            }

            stream.WriteByte((byte)digit);
        }
        while (number > 0);
    }

    /// <summary>
    /// Decode an MQTT Variable Byte Integer data representation.
    ///
    /// See also <seealso href="https://docs.oasis-open.org/mqtt/mqtt/v5.0/os/mqtt-v5.0-os.html#_Toc3901011">
    /// Data Representation: Variable Byte Integer</seealso>.
    /// </summary>
    /// <param name="reader"><cref>SequenceReader</cref> containing the packet data to be decoded.</param>
    /// <returns>The integer value of the Variable Byte Integer.</returns>
    protected static int DecodeVariableByteInteger(ref SequenceReader<byte> reader)
    {
        var multiplier = 1;
        var value = 0;
        byte encodedByte;
        var lengthInBytes = 0;

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
    /// Decode an MQTT Four Byte Integer data representation.
    ///
    /// See also <seealso href="https://docs.oasis-open.org/mqtt/mqtt/v5.0/os/mqtt-v5.0-os.html#_Toc3901009">
    /// Data Representation: Binary Data</seealso>.
    /// </summary>
    /// <param name="reader">SequenceReader containing the packet data to be decoded.</param>
    /// <returns>The value of the four byte integer.</returns>
    protected static int? DecodeFourByteInteger(ref SequenceReader<byte> reader)
    {
        if (reader.TryReadBigEndian(out Int32 intValue))
        {
            return intValue;
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
                // FIXME: Raise Protocol Error
            }
        }

        return false;
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
                    this.Properties.SubscriptionIdentifier = DecodeVariableByteInteger(ref reader);
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
                    this.Properties.RetainAvailable = DecodeByte(ref reader);
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
                    this.Properties.SubscriptionIdentifierAvailable = DecodeByteAsBool(ref reader);
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
