namespace HiveMQtt.MQTT5.Ping;

using System;
using System.Buffers;

/// <summary>
/// An MQTT PingResp Control Packet.
///
/// See also <seealso href="https://docs.oasis-open.org/mqtt/mqtt/v5.0/os/mqtt-v5.0-os.html#_Toc3901200">
/// PingResp Control Packet</seealso>.
/// </summary>
public class PingRespPacket : ControlPacket
{
    public PingRespPacket(ReadOnlySequence<byte> data)
    {
        this.ReceivedOn = DateTime.UtcNow;
    }

    public override ControlPacketType ControlPacketType => ControlPacketType.PingResp;
}
