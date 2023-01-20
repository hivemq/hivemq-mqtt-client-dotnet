namespace HiveMQtt.MQTT5;

using System.Buffers;

/// <summary>
/// A packet with bad or non sensical data.
/// </summary>
internal class MalformedPacket : ControlPacket
{
    public override ControlPacketType ControlPacketType => ControlPacketType.Reserved;

    public MalformedPacket(ReadOnlySequence<byte> buffer) => this.packetData = buffer;

    private readonly ReadOnlySequence<byte> packetData;
}
