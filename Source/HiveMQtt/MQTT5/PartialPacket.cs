namespace HiveMQtt.MQTT5;

/// <summary>
/// Part of a Control Packet.  Used when not all of the data has yet arrived
/// over the broker connection.
/// </summary>
internal class PartialPacket : ControlPacket
{
    public override ControlPacketType ControlPacketType => ControlPacketType.Reserved;
}
