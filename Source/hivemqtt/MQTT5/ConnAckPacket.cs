namespace HiveMQtt.MQTT5;

using System.Buffers;

/// <summary>
/// An MQTT Connect Control Packet as defined in:
/// https://docs.oasis-open.org/mqtt/mqtt/v5.0/os/mqtt-v5.0-os.html#_Toc3901074.
/// </summary>
public class ConnAckPacket : ControlPacket
{
    public bool SessionPresent { get; set; }

    public int AckFlags { get; set; }

    public ConnAckReasonCode ReasonCode { get; set; }

    private readonly ReadOnlySequence<byte> rawPacketData;

    public ConnAckPacket(ReadOnlySequence<byte> data)
    {
        this.SessionPresent = false;

        this.rawPacketData = data;
        this.Decode();
    }

    public override ControlPacketType ControlPacketType => ControlPacketType.ConnAck;

    public void Decode()
    {
        var packetLength = this.rawPacketData.Length;
        var reader = new SequenceReader<byte>(this.rawPacketData);

        // Skip past the Fixed Header
        reader.Advance(2);

        if (reader.TryRead(out var ackFlags))
        {
            this.SessionPresent = (ackFlags & 0x1) == 0x1;
        }

        if (reader.TryRead(out var reasonCode))
        {
            this.ReasonCode = (ConnAckReasonCode)reasonCode;
        }

        var propertyLength = DecodeVariableByteInteger(reader);
        DecodeProperties(reader, propertyLength);
    }

}
