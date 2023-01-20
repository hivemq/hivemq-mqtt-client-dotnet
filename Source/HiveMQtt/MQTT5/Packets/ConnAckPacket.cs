namespace HiveMQtt.MQTT5.Packets;

using System.Buffers;
using HiveMQtt.MQTT5.ReasonCodes;

/// <summary>
/// An MQTT Connect Control Packet as defined in:
/// https://docs.oasis-open.org/mqtt/mqtt/v5.0/os/mqtt-v5.0-os.html#_Toc3901074.
/// </summary>
public class ConnAckPacket : ControlPacket
{
    public ConnAckPacket(ReadOnlySequence<byte> packetData)
    {
        this.SessionPresent = false;
        this.Decode(packetData);
    }

    public bool SessionPresent { get; set; }

    public int AckFlags { get; set; }

    public ConnAckReasonCode ReasonCode { get; set; }

    public override ControlPacketType ControlPacketType => ControlPacketType.ConnAck;

    public void Decode(ReadOnlySequence<byte> packetData)
    {
        var packetLength = packetData.Length;
        var reader = new SequenceReader<byte>(packetData);

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

        var propertyLength = DecodeVariableByteInteger(ref reader);
        _ = this.DecodeProperties(ref reader, propertyLength);
    }

}
