namespace HiveMQtt.MQTT5;

using System.Buffers;

/// <summary>
/// An MQTT Connect Control Packet as defined in:
/// https://docs.oasis-open.org/mqtt/mqtt/v5.0/os/mqtt-v5.0-os.html#_Toc3901074.
/// </summary>
public class ConnAckPacket : ControlPacket
{
    public bool SessionPresent { get; set; }

    public int AckFlags { get; set; }

    public ConnAckReasonCode ReasonCode { get; set; }

    private readonly ReadOnlySequence<byte> rawPacketData;

    public ConnAckPacket(ReadOnlySequence<byte> data)
    {
        this.SessionPresent = false;

        this.rawPacketData = data;
        this.Decode();
    }

    public override ControlPacketType ControlPacketType => ControlPacketType.ConnAck;

    public void Decode()
    {
        var packetLength = this.rawPacketData.Length;
        var byteData = this.rawPacketData.ToArray();

        this.AckFlags = byteData[3];
        this.SessionPresent = (this.AckFlags & 0x1) == 0x0;

        this.ReasonCode = (ConnAckReasonCode)byteData[4];

        var vbi = new MemoryStream(byteData[5]);
        var propertyLength = DecodeVariableByteInteger(vbi);

        if (propertyLength.Length > 0)
        {
            var propertiesStart = 5 + propertyLength.Length;
            var x = DecodeProperties(new MemoryStream(byteData[propertiesStart]), propertyLength.Length);
        }
    }

}
