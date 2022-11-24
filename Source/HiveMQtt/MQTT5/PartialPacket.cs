namespace HiveMQtt.MQTT5;

using System.IO;
using System.Text;

/// <summary>
/// Part of a Control Packet.  Used when not all of the data has yet arrived
/// over the broker connection.
/// </summary>
public class PartialPacket : ControlPacket
{
    public PartialPacket() { }

    public override ControlPacketType ControlPacketType => ControlPacketType.Reserved;
}
