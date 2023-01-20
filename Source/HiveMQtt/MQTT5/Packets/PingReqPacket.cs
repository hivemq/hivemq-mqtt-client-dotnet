namespace HiveMQtt.MQTT5.Packets;

using System.IO;

/// <summary>
/// An MQTT PingReq Control Packet.
///
/// See also <seealso href="https://docs.oasis-open.org/mqtt/mqtt/v5.0/os/mqtt-v5.0-os.html#_Toc3901195">
/// PingReq Control Packet</seealso>.
/// </summary>
public class PingReqPacket : ControlPacket
{
    public override ControlPacketType ControlPacketType => ControlPacketType.PingResp;

    /// <summary>
    /// Encode this packet to be sent on the wire.
    /// </summary>
    /// <returns>An array of bytes ready to be sent.</returns>
    public static byte[] Encode()
    {
        var stream = new MemoryStream(2);

        // Fixed Header
        stream.WriteByte(((byte)ControlPacketType.PingReq) << 4);
        stream.WriteByte(0x0);

        return stream.ToArray();
    }
}
