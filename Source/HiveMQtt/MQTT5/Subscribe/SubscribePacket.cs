namespace HiveMQtt.MQTT5.Subscribe;

using System.Buffers;
using System.IO;

using HiveMQtt.Client.Options;

/// <summary>
/// An MQTT Subscribe Control Packet.
///
/// See also <seealso href="https://docs.oasis-open.org/mqtt/mqtt/v5.0/os/mqtt-v5.0-os.html#_Toc3901205">
/// Subscribe Control Packet</seealso>
/// </summary>
internal class SubscribePacket : ControlPacket
{
    private readonly ReadOnlySequence<byte> rawPacketData;

    /// <summary>
    /// Initializes a new instance of the <see cref="SubscribePacket"/> class
    /// with the options to be used for the publish.
    /// </summary>
    /// <param name="options">The raw packet data off the wire.</param>
    /// <param name="packetIdentifier">A unique packet identifier for the packet to be created.</param>
    public SubscribePacket(SubscribeOptions options, ushort packetIdentifier)
    {
        this.PacketIdentifier = packetIdentifier;
        this.Options = options;
    }

    /// <summary>
    /// Gets or sets the options for an outgoing Subscribe packet.
    /// </summary>
    public SubscribeOptions Options { get; set; }

    public override ControlPacketType ControlPacketType => ControlPacketType.Subscribe;

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

        // Variable Header - starts at byte 2
        EncodeTwoByteInteger(stream, this.PacketIdentifier);
        this.EncodeProperties(stream);

        // Payload

        // Fixed Header
        stream.Position = 0;
        var length = stream.Length - 2;
        var byte1 = (byte)ControlPacketType.Subscribe << 4;
        byte1 &= 0x2;

        stream.WriteByte((byte)byte1);
        _ = EncodeVariableByteInteger(stream, (int)length);
        return stream.ToArray();
    }

    /// <summary>
    /// Gather the flags and properties for a Subscribe packet from <see cref="SubscribeOptions"/>
    /// as data prepraration for encoding in <see cref="SubscribePacket"/>.
    /// </summary>
    internal void GatherSubscribeFlagsAndProperties()
    {
        this.Options.ValidateOptions();
    }

}
