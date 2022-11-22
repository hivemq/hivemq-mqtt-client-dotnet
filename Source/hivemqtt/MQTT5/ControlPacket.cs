namespace HiveMQtt.MQTT5;

using System.Globalization;
using System.Text;
using HiveMQtt.MQTT5.Exceptions;
using HiveMQtt.MQTT5.Types;

/// <summary>
/// A base class for all MQTT Control Packet types.
/// </summary>
public abstract class ControlPacket
{
    /// <summary>
    /// Gets the type of this <c>ControlPacket</c>.
    /// </summary>
    /// <value>ControlPacketType</value>
    public abstract ControlPacketType ControlPacketType { get; }

    public MQTT5Properties? Properties { get; set; }

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

    protected static int DecodeTwoByteInteger(MemoryStream stream)
    {
        // FIXME: Implement
    }

    protected static int DecodeFourByteInteger(MemoryStream stream)
    {
        // FIXME: Implement
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

    public static VariableByteInteger DecodeVariableByteInteger(MemoryStream stream)
    {
        var multiplier = 1;
        var value = 0;
        int encodedByte;
        var lengthInBytes = 0;

        do
        {
            lengthInBytes++;
            encodedByte = stream.ReadByte();
            value += (encodedByte & 127) * multiplier;

            if (multiplier > 128 * 128 * 128)
            {
                throw new MalformedVBIException();
            }

            multiplier *= 128;
        }
        while ((encodedByte & 128) != 0);

        return new VariableByteInteger
        {
            Value = value,
            Length = lengthInBytes,
        };
    }

    public bool DecodeProperties(MemoryStream stream, int length)
    {
        do
        {
            var propertyID = DecodeVariableByteInteger(stream);
            this.Properties = new MQTT5Properties();

            switch ((MQTT5PropertyType)propertyID.Value)
            {
                case MQTT5PropertyType.PayloadFormatIndicator:
                    this.Properties.PayloadFormatIndicator = (byte)stream.ReadByte();
                    break;
                case MQTT5PropertyType.MessageExpiryInterval:
                    this.Properties.MessageExpiryInterval = DecodeFourByteInteger(stream);
                    break;
                case MQTT5PropertyType.SessionExpiryInterval:
                    this.Properties.SessionExpiryInterval = DecodeFourByteInteger(stream);
                    break;
                default:
                    break;
            }

        }
        while (length > 0);


    }
}
