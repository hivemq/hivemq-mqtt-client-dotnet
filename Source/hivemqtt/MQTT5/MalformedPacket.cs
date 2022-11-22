namespace HiveMQtt.MQTT5;

using System.Buffers;
using System.IO;
using System.Text;

/// <summary>
/// A packet with bad or non sensical data.
/// </summary>
public class MalformedPacket : ControlPacket
{
    public override ControlPacketType ControlPacketType => ControlPacketType.Reserved;

    private readonly ReadOnlySequence<byte> packetData;

    public MalformedPacket(ReadOnlySequence<byte> buffer) => this.packetData = buffer;
}
