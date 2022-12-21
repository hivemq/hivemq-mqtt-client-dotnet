namespace HiveMQtt.MQTT5.Publish;

using System.Buffers;
using System.IO;
using HiveMQtt.Client.Message;
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
    /// with the <seealso cref="MQTT5PublishMessage">MQTT5PublishMessage</seealso>
    /// to be used for the publish.
    /// </summary>
    /// <param name="message">The message to publish.</param>
    /// <param name="packetIdentifier">A unique packet identifier for the packet to be created.</param>
    public PublishPacket(MQTT5PublishMessage message, ushort packetIdentifier)
    {
        this.PacketIdentifier = packetIdentifier;
        this.Message = message;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PublishPacket"/> class
    /// with the raw packet data off the wire.
    /// </summary>
    /// <param name="data">The raw packet data off the wire.</param>
    public PublishPacket(ReadOnlySequence<byte> data)
    {
        this.Message = new MQTT5PublishMessage();
        this.rawPacketData = data;
        this.Decode();
    }

    /// <summary>
    /// Gets or sets the message for an outgoing Publish packet.
    /// </summary>
    public MQTT5PublishMessage Message { get; set; }

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
        var streamSize = this.Message.Payload?.Length ?? 0;

        var stream = new MemoryStream(streamSize + 100)
        {
            Position = 2,
        };

        this.GatherPublishFlagsAndProperties();

        // Variable Header - starts at byte 2
        // Topic Name
        if (this.Message.Topic != null)
        {
            _ = EncodeUTF8String(stream, this.Message.Topic);
        }

        // Packet Identifier
        if (this.Message.QoS > QualityOfService.AtMostOnceDelivery)
        {
            EncodeTwoByteInteger(stream, this.PacketIdentifier);
        }

        // Properties
        this.EncodeProperties(stream);

        // Payload
        if (this.Message.Payload != null)
        {
            _ = EncodeBinaryData(stream, this.Message.Payload);
        }

        // Fixed Header - starts at byte 0
        stream.Position = 0;

        var byte1 = (byte)ControlPacketType.Publish << 4;

        // DUP Flag
        if (this.Message.Duplicate is true)
        {
            byte1 &= 0x8;
        }

        // QoS Flag
        if (this.Message.QoS == QualityOfService.AtLeastOnceDelivery)
        {
            byte1 &= 0x2;
        }
        else if (this.Message.QoS == QualityOfService.ExactlyOnceDelivery)
        {
            byte1 &= 0x4;
        }

        // Retain Flag
        if (this.Message.Retained is true)
        {
            byte1 &= 0x1;
        }

        var length = stream.Length - 2;
        stream.WriteByte((byte)byte1);
        _ = EncodeVariableByteInteger(stream, (int)length);

        return stream.ToArray();
    }

    /// <summary>
    /// Gather the flags and properties for an outgoing Publish packet from <see cref="MQTT5PublishMessage"/>
    /// as data prepraration for encoding in <see cref="PublishPacket"/>.
    /// </summary>
    internal void GatherPublishFlagsAndProperties()
    {
        this.Message.Validate();

        // Convert the PublishMessage to the MQTT5 Properties
        if (this.Message.PayloadFormatIndicator.HasValue)
        {
            this.Properties.PayloadFormatIndicator = (byte)this.Message.PayloadFormatIndicator.Value;
        }

        if (this.Message.MessageExpiryInterval.HasValue)
        {
            this.Properties.MessageExpiryInterval = (uint)this.Message.MessageExpiryInterval.Value;
        }

        if (this.Message.TopicAlias.HasValue)
        {
            this.Properties.TopicAlias = (ushort)this.Message.TopicAlias.Value;
        }

        if (this.Message.ResponseTopic != null)
        {
            this.Properties.ResponseTopic = this.Message.ResponseTopic;
        }

        if (this.Message.CorrelationData != null)
        {
            this.Properties.CorrelationData = this.Message.CorrelationData;
        }

        if (this.Message.UserProperties != null)
        {
            this.Properties.UserProperties = this.Message.UserProperties;
        }

        // We never encode SubscriptionIdentifiers for an outgoing Publish packet
        // this.Message.SubscriptionIdentifiers

        if (this.Message.ContentType != null)
        {
            this.Properties.ContentType = this.Message.ContentType;
        }
    }
}
