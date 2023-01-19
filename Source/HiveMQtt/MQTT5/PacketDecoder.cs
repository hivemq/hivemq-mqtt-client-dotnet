namespace HiveMQtt.MQTT5;

using System.Buffers;
using HiveMQtt.MQTT5.Packets;

/// <summary>
/// An MQTT Connect Control Packet as defined in:
/// https://docs.oasis-open.org/mqtt/mqtt/v5.0/os/mqtt-v5.0-os.html#_Toc3901074.
/// </summary>
internal class PacketDecoder
{
    public static ControlPacket Decode(ReadOnlySequence<byte> buffer, out SequencePosition consumed)
    {
        consumed = buffer.Start;

        if (buffer.Length < 2)
        {
            // We need at least the MQTT Header
            return new PartialPacket();
        }

        // Byte 1: Control Packet Type
        // FIXME: ToArray?
        var x = buffer.ToArray();
        var controlPacketType = x[0] >> 4;

        // Byte 2: Remaining Length of the Variable Header
        var remainingLengthOfVH = x[1];
        var packetLength = remainingLengthOfVH + 2;

        if (buffer.Length < packetLength)
        {
            // Not all data for this packet has arrived yet.  Try again...
            return new PartialPacket();
        }

        var packetData = buffer.Slice(0, packetLength);

        ControlPacket packet = controlPacketType switch
        {
            (int)ControlPacketType.ConnAck => new ConnAckPacket(packetData),
            (int)ControlPacketType.Disconnect => new DisconnectPacket(packetData),
            (int)ControlPacketType.PingResp => new PingRespPacket(packetData),
            (int)ControlPacketType.Publish => new PublishPacket(packetData),
            (int)ControlPacketType.PubAck => new PubAckPacket(packetData),
            (int)ControlPacketType.SubAck => new SubAckPacket(packetData),
            (int)ControlPacketType.UnsubAck => new UnsubAckPacket(packetData),
            _ => new MalformedPacket(packetData),
        };

        consumed = buffer.GetPosition(packetLength);
        return packet;
    }
}
