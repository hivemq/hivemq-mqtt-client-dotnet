namespace HiveMQtt.MQTT5.Publish;

using System.Buffers;
using System.IO;
using HiveMQtt.Client.Options;
using HiveMQtt.MQTT5.Types;

/// <summary>
/// An MQTT Publish Control Packet.
///
/// See also <seealso href="https://docs.oasis-open.org/mqtt/mqtt/v5.0/os/mqtt-v5.0-os.html#_Toc3901205">
/// Publish Control Packet</seealso>.
/// </summary>
internal class PublishPacket : ControlPacket
{
    private readonly ReadOnlySequence<byte> rawPacketData;

    /// <summary>
    /// Initializes a new instance of the <see cref="PublishPacket"/> class
    /// with the options to be used for the publish.
    /// </summary>
    /// <param name="options">The raw packet data off the wire.</param>
    /// <param name="packetIdentifier">A unique packet identifier for the packet to be created.</param>
    public PublishPacket(PublishOptions options, ushort packetIdentifier)
    {
        this.PacketIdentifier = packetIdentifier;
        this.Options = options;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PublishPacket"/> class
    /// with the raw packet data off the wire.
    /// </summary>
    /// <param name="data">The raw packet data off the wire.</param>
    public PublishPacket(ReadOnlySequence<byte> data)
    {
        this.Options = new PublishOptions();
        this.rawPacketData = data;
        this.Decode();
    }

    /// <summary>
    /// Gets or sets the options for an outgoing Publish packet.
    /// </summary>
    public PublishOptions Options { get; set; }

    public override ControlPacketType ControlPacketType => ControlPacketType.Publish;

    /// <summary>
    /// Decode the received MQTT Publish packet.
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

        this.GatherPublishFlagsAndProperties();

        // Variable Header - starts at byte 2

        // TODO: Validate topic name
        // TODO: Add support for topic aliases
        _ = EncodeUTF8String(stream, this.Options.Topic);

        if (this.Options.QoS > QualityOfService.AtMostOnceDelivery)
        {
            _ = EncodeTwoByteInteger(stream, this.PacketIdentifier);
        }

        this.EncodeProperties(stream);

        // Fixed Header - starts at byte 0


        return stream.ToArray();
    }

    /// <summary>
    /// Gather the flags and properties for an outgoing Publish packet from <see cref="PublishOptions"/>
    /// as data prepraration for encoding in <see cref="PublishPacket"/>.
    /// </summary>
    internal void GatherPublishFlagsAndProperties()
    {
        this.Options.ValidateOptions();

        // Properties
        if (this.Options.PayloadFormatIndicator.HasValue)
        {
            this.Properties.PayloadFormatIndicator = (byte)this.Options.PayloadFormatIndicator.Value;
        }

        if (this.Options.MessageExpiryInterval.HasValue)
        {
            this.Properties.MessageExpiryInterval = (uint)this.Options.MessageExpiryInterval.Value;
        }

        if (this.Options.TopicAlias.HasValue)
        {
            this.Properties.TopicAlias = (ushort)this.Options.TopicAlias.Value;
        }

        if (this.Options.ResponseTopic != null)
        {
            this.Properties.ResponseTopic = this.Options.ResponseTopic;
        }

        if (this.Options.CorrelationData != null)
        {
            this.Properties.CorrelationData = this.Options.CorrelationData;
        }

        if (this.Options.UserProperties != null)
        {
            this.Properties.UserProperties = this.Options.UserProperties;
        }

        // We never encode SubscriptionIdentifiers for an outgoing Publish packet
        // this.Options.SubscriptionIdentifiers

        if (this.Options.ContentType != null)
        {
            this.Properties.ContentType = this.Options.ContentType;
        }
    }
}
