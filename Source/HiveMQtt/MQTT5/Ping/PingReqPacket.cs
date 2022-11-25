namespace HiveMQtt.MQTT5;

using System;
using System.IO;
using System.Text;
using HiveMQtt;

/// <summary>
/// An MQTT PingReq Control Packet as defined in:
/// https://docs.oasis-open.org/mqtt/mqtt/v5.0/os/mqtt-v5.0-os.html#_Toc3901195
/// </summary>
public class PingReqPacket : ControlPacket
{
    public override ControlPacketType ControlPacketType => ControlPacketType.PingResp;


    public byte[] Encode()
    {
        var stream = new MemoryStream(2);

        // Fixed Header
        stream.WriteByte(((byte)ControlPacketType.PingReq) << 4);
        stream.WriteByte(0x0);

        var data = stream.GetBuffer();
        var segment = new ArraySegment<byte>(data, 0, (int)stream.Length);
        return segment.ToArray();
    }

}
