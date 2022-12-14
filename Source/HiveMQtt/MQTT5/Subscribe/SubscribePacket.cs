namespace HiveMQtt.MQTT5.Subscribe;

using System.Buffers;
using System.IO;

/// <summary>
/// An MQTT Subscribe Control Packet.
///
/// See also <seealso href="https://docs.oasis-open.org/mqtt/mqtt/v5.0/os/mqtt-v5.0-os.html#_Toc3901205">
/// Subscribe Control Packet</seealso>
/// </summary>
internal class SubscribePacket : ControlPacket
{
    private readonly ReadOnlySequence<byte> rawPacketData;

    public SubscribePacket() { }

    public SubscribePacket(ReadOnlySequence<byte> data)
    {
        this.rawPacketData = data;
        this.Decode();
    }

    public override ControlPacketType ControlPacketType => ControlPacketType.Subscribe;

    /// <summary>
    /// Decode the received MQTT Subscribe packet.
    /// </summary>
    public void Decode()
    {
        var reader = new SequenceReader<byte>(this.rawPacketData);

        // Skip past the Fixed Header
        reader.Advance(1);

        if (reader.TryRead(out var remainingLength))
        {
            if ((remainingLength + 2) > this.rawPacketData.Length)
            {
                // Not enough packet data / partial packet
                // FIXME: Send back to pipeline to get more data
            }
            else
            {
                // FIXME: Implement
            }
        }
    }

    /// <summary>
    /// Encode this packet to be sent on the wire.
    /// </summary>
    /// <returns>An array of bytes ready to be sent.</returns>
    public byte[] Encode()
    {

        var stream = new MemoryStream(100)
        {
            Position = 2,
        };

        stream.WriteByte(0);
        // FIXME: Implement

        // Variable Header - starts at byte 2
        return stream.ToArray();
    }
}
