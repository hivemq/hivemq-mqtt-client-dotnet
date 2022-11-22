namespace HiveMQtt.MQTT5;

using System.Buffers;

/// <summary>
/// An MQTT Connect Control Packet as defined in:
/// https://docs.oasis-open.org/mqtt/mqtt/v5.0/os/mqtt-v5.0-os.html#_Toc3901074.
/// </summary>
public class PacketDecoder
{
    public static ControlPacket Decode(ReadOnlySequence<byte> buffer)
    {
        if (buffer.Length < 2)
        {
            // We need at least the MQTT Header
            return new PartialPacket();
        }

        var x = buffer.ToArray();
        var type = x[0] >> 4;

        var vbi = new MemoryStream(x[1]);
        var remainingLength = ControlPacket.DecodeVariableByteInteger(vbi);
        var packetLength = remainingLength + 2;

        if (buffer.Length < packetLength)
        {
            // Not all data for this packet has arrived yet.  Try again...
            return new PartialPacket();
        }

        var packetData = buffer.Slice(0, packetLength);

        ControlPacket packet = type switch
        {
            (int)ControlPacketType.ConnAck => new ConnAckPacket(packetData),
            _ => new MalformedPacket(packetData),
        };

        return packet;
    }
}
