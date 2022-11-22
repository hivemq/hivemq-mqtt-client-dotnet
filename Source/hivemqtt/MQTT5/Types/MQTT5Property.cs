namespace HiveMQtt.MQTT5.Types;

public class MQTT5Property
{
    public int ID { get; set; }

    public MQTT5DataType DataType { get; set; }

    public byte? ByteValue { get; set; }

    public byte[]? TwoByteValue { get; set; }

    public byte[]? FourByteValue { get; set; }

    public byte[]? BinaryDataValue { get; set; }

    public string? UTF8EncodedStringValue { get; set; }

    public string? UTF8EncodedStringPairValue { get; set; }
}
