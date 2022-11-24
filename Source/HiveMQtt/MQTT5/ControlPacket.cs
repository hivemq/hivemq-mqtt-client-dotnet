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

    public MQTT5Properties Properties { get; set; }

    /// <summary>
    /// Gets the type of this <c>ControlPacket</c>.
    /// </summary>
    /// <value>ControlPacketType</value>
    public abstract ControlPacketType ControlPacketType { get; }


    protected static void EncodeUTF8String(MemoryStream stream, string s)
    {
        var length = (ushort)s.Length;

        EncodeTwoByteInteger(stream, length);

        var stringBytes = Encoding.UTF8.GetBytes(s);
        stream.Write(stringBytes, 0, stringBytes.Length);
    }

    protected static void EncodeTwoByteInteger(MemoryStream stream, int value)
    {
        var converted = Convert.ToUInt16(value);
        EncodeTwoByteInteger(stream, converted);
    }

    protected static void EncodeTwoByteInteger(MemoryStream stream, ushort value)
    {
        var valueInBytes = BitConverter.GetBytes(value);

        if (BitConverter.IsLittleEndian)
        {
            Array.Reverse(valueInBytes);
        }

        stream.WriteByte(valueInBytes[0]);
        stream.WriteByte(valueInBytes[1]);
    }

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
    /// Decode "Variable Byte Integer" data representation as defined in:
    /// https://docs.oasis-open.org/mqtt/mqtt/v5.0/os/mqtt-v5.0-os.html#_Toc3901011
    /// </summary>
    /// <param name="reader">SequenceReader containing the packet data to be decoded.</param>
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
    /// Decode "UTF-8 Encoded String" data representation as defined in:
    /// https://docs.oasis-open.org/mqtt/mqtt/v5.0/os/mqtt-v5.0-os.html#_Toc3901010
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
    /// Decode "Binary Data" data representation as defined in:
    /// https://docs.oasis-open.org/mqtt/mqtt/v5.0/os/mqtt-v5.0-os.html#_Toc3901012
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
    /// Decode "Two Byte Integer" data representation as defined in:
    /// https://docs.oasis-open.org/mqtt/mqtt/v5.0/os/mqtt-v5.0-os.html#_Toc3901008
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
    /// Decode "Four Byte Integer" data representation as defined in:
    /// https://docs.oasis-open.org/mqtt/mqtt/v5.0/os/mqtt-v5.0-os.html#_Toc3901009
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
