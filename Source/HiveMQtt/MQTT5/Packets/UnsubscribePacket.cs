namespace HiveMQtt.MQTT5.Packets;

using System.IO;

using HiveMQtt.MQTT5.Types;

/// <summary>
/// An MQTT Unsubscribe Control Packet.
///
/// See also <seealso href="https://docs.oasis-open.org/mqtt/mqtt/v5.0/os/mqtt-v5.0-os.html#_Toc3901179">
/// Unsubscribe Control Packet</seealso>.
/// </summary>
public class UnsubscribePacket : ControlPacket
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UnsubscribePacket"/> class
    /// with the options to be used for the publish.
    /// </summary>
    /// <param name="subscriptions">The list of subscriptions to unsubscribe.</param>
    /// <param name="packetIdentifier">A unique packet identifier for the packet to be created.</param>
    /// <param name="userProperties">User properties to be sent with the packet.</param>
    public UnsubscribePacket(List<Subscription> subscriptions, ushort packetIdentifier, Dictionary<string, string>? userProperties = null)
    {
        this.PacketIdentifier = packetIdentifier;
        this.Subscriptions = subscriptions;
        if (userProperties != null)
        {
            this.Properties.UserProperties = userProperties;
        }
    }

    /// <summary>
    /// Gets or sets the list of Subscriptions to unsubscribe.
    /// </summary>
    public List<Subscription> Subscriptions { get; set; }

    /// <inheritdoc/>
    public override ControlPacketType ControlPacketType => ControlPacketType.Unsubscribe;

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
        foreach (var subscription in this.Subscriptions)
        {
            EncodeUTF8String(stream, subscription.TopicFilter.Topic);
        }

        // Fixed Header
        stream.Position = 0;
        var length = stream.Length - 2;
        var byte1 = (byte)ControlPacketType.Unsubscribe << 4;
        byte1 |= 0x2;

        stream.WriteByte((byte)byte1);
        _ = EncodeVariableByteInteger(stream, (int)length);
        return stream.ToArray();
    }
}
