namespace HiveMQtt.MQTT5;

using System.IO;
using System.Text;
using HiveMQtt.Client;

/// <summary>
/// An MQTT Connect Control Packet as defined in:
/// https://docs.oasis-open.org/mqtt/mqtt/v5.0/os/mqtt-v5.0-os.html#_Toc3901033.
/// </summary>
public class ConnectPacket : ControlPacket
{
    private readonly HiveClientOptions clientOptions;

    private byte flags;

    public ConnectPacket(HiveClientOptions clientOptions) => this.clientOptions = clientOptions;

    public override ControlPacketType ControlPacketType => ControlPacketType.Connect;

    /// <summary>
    /// Encode this packet to be sent on the wire.
    /// </summary>
    /// <returns>An array of bytes ready to be sent.</returns>
    public byte[] Encode()
    {
        this.clientOptions.ValidateOptions();

        this.flags = 0x0;
        if (this.clientOptions.CleanStart is true)
        {
            this.flags |= 0x2;
        }

        // TODO: LWT options
        if (this.clientOptions.UserName != null)
        {
            this.flags |= 0x80;
        }

        if (this.clientOptions.Password != null)
        {
            this.flags |= 0x40;
        }

        var stream = new MemoryStream(100)
        {
            Position = 2,
        };

        // Variable Header - starts at byte 2
        stream.WriteByte(0);
        stream.WriteByte(4);
        stream.Write(Encoding.UTF8.GetBytes("MQTT"));
        stream.WriteByte(0x5); // Protocol Version
        stream.WriteByte(this.flags); // Connect Flags
        EncodeTwoByteInteger(stream, this.clientOptions.KeepAlive);
        stream.WriteByte(0x0); // Connect Properties

        // Payload
        EncodeUTF8String(stream, this.clientOptions.ClientId);

        var length = stream.Length - 2;

        // Fixed Header - Add to the beginning of the stream
        stream.Position = 0;
        stream.WriteByte(((byte)ControlPacketType.Connect) << 4);
        EncodeVariableByteInteger(stream, (int)length);

        return stream.ToArray();
    }
}
