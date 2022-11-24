namespace HiveMQtt.MQTT5;

using System.Buffers;

/// <summary>
/// A packet with bad or non sensical data.
/// </summary>
public class MalformedPacket : ControlPacket
{
    public override ControlPacketType ControlPacketType => ControlPacketType.Reserved;

    private readonly ReadOnlySequence<byte> packetData;

    public MalformedPacket(ReadOnlySequence<byte> buffer) => this.packetData = buffer;
}
