namespace HiveMQtt.MQTT5;

using System;
using System.Buffers;

/// <summary>
/// An MQTT PingResp Control Packet as defined in:
/// https://docs.oasis-open.org/mqtt/mqtt/v5.0/os/mqtt-v5.0-os.html#_Toc3901200
/// </summary>
public class PingRespPacket : ControlPacket
{
    public PingRespPacket(ReadOnlySequence<byte> data)
    {
        this.RawPacketData = data;
        this.ReceivedOn = DateTime.UtcNow;
    }

    public DateTime ReceivedOn { get; internal set; }

    public override ControlPacketType ControlPacketType => ControlPacketType.PingResp;
}
