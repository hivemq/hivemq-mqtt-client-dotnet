namespace HiveMQtt.MQTT5;

using System.Buffers;
using System.Diagnostics;
using HiveMQtt.MQTT5.Packets;

/// <summary>
/// Decodes a Control Packet from a buffer.
/// </summary>
internal class PacketDecoder
{
    public static ControlPacket Decode(ReadOnlySequence<byte> buffer, out SequencePosition consumed)
    {
        try
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
                (int)ControlPacketType.PubRec => new PubRecPacket(packetData),
                (int)ControlPacketType.PubRel => new PubRelPacket(packetData),
                (int)ControlPacketType.PubComp => new PubCompPacket(packetData),
                (int)ControlPacketType.SubAck => new SubAckPacket(packetData),
                (int)ControlPacketType.UnsubAck => new UnsubAckPacket(packetData),
                _ => new MalformedPacket(packetData),
            };

            consumed = buffer.GetPosition(packetLength);
            return packet;
        }
        catch (System.Exception)
        {
            Trace.WriteLine("PacketDecoder.Decode: Exception caught.  Returning MalformedPacket.");
            consumed = buffer.Start;
            return new MalformedPacket(buffer);
        }
    }
}
