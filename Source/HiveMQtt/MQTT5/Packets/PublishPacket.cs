/*
 * Copyright 2022-present HiveMQ and the HiveMQ Community
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
namespace HiveMQtt.MQTT5.Packets;

using System.Buffers;
using System.IO;
using HiveMQtt.Client.Events;
using HiveMQtt.MQTT5.Types;

/// <summary>
/// An MQTT Publish Control Packet.
///
/// See also <seealso href="https://docs.oasis-open.org/mqtt/mqtt/v5.0/os/mqtt-v5.0-os.html#_Toc3901205">
/// Publish Control Packet</seealso>.
/// </summary>
public class PublishPacket : ControlPacket
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PublishPacket"/> class
    /// with the <seealso cref="MQTT5PublishMessage">MQTT5PublishMessage</seealso>
    /// to be used for the publish.
    /// </summary>
    /// <param name="message">The message to publish.</param>
    /// <param name="packetIdentifier">A unique packet identifier for the packet to be created.</param>
    public PublishPacket(MQTT5PublishMessage message, int packetIdentifier)
    {
        this.PacketIdentifier = (ushort)packetIdentifier;
        this.Message = message;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PublishPacket"/> class
    /// with the raw packet data off the wire.
    /// </summary>
    /// <param name="packetData">The raw packet data off the wire.</param>
    public PublishPacket(ReadOnlySequence<byte> packetData)
    {
        this.Message = new MQTT5PublishMessage();
        this.Decode(packetData);
    }

    /// <summary>
    /// Gets or sets the message for an outgoing Publish packet.
    /// </summary>
    public MQTT5PublishMessage Message { get; set; }

    public override ControlPacketType ControlPacketType => ControlPacketType.Publish;

    /// <summary>
    /// Valid for outgoing Publish messages QoS 1.  An event that is fired after the the QoS 1 publish transaction is complete.
    /// </summary>
    public event EventHandler<OnPublishQoS1CompleteEventArgs> OnPublishQoS1Complete = new((client, e) => { });

    internal virtual void OnPublishQoS1CompleteEventLauncher(PubAckPacket packet)
    {
        var eventArgs = new OnPublishQoS1CompleteEventArgs(packet);
        Logger.Trace("OnPublishQoS1CompleteEventLauncher");
        this.OnPublishQoS1Complete?.Invoke(this, eventArgs);
    }

    /// <summary>
    /// Valid for outgoing Publish messages QoS 2.  An event that is fired after the the QoS 2 PubComp is received.
    /// </summary>
    public event EventHandler<OnPublishQoS2CompleteEventArgs> OnPublishQoS2Complete = new((client, e) => { });

    internal virtual void OnPublishQoS2CompleteEventLauncher(List<ControlPacket> packetList)
    {
        var eventArgs = new OnPublishQoS2CompleteEventArgs(packetList);
        Logger.Trace("OnPublishQoS2CompleteEventLauncher");
        this.OnPublishQoS2Complete?.Invoke(this, eventArgs);
    }

    /// <summary>
    /// Decode the received MQTT Publish packet.
    /// </summary>
    /// <param name="packetData">The raw packet data off the wire.</param>
    public void Decode(ReadOnlySequence<byte> packetData)
    {
        var reader = new SequenceReader<byte>(packetData);
        this.PacketSize = packetData.Length;

        if (reader.TryRead(out var fixedHeaderFlags))
        {
            // Dup Flag
            this.Message.Duplicate = (fixedHeaderFlags & 0x8) == 0x8;

            // QoS Flag
            if ((fixedHeaderFlags & 0x6) == 0x2)
            {
                this.Message.QoS = QualityOfService.AtLeastOnceDelivery;
            }
            else if ((fixedHeaderFlags & 0x6) == 0x4)
            {
                this.Message.QoS = QualityOfService.ExactlyOnceDelivery;
            }
            else
            {
                this.Message.QoS = QualityOfService.AtMostOnceDelivery;
            }

            // Retain Flag
            this.Message.Retain = (fixedHeaderFlags & 0x1) == 0x1;
        }

        // Remaining Length
        var fhRemainingLength = DecodeVariableByteInteger(ref reader, out var vbiLength);
        var variableHeaderStart = reader.Consumed;

        // Variable Header
        // Topic Name
        this.Message.Topic = DecodeUTF8String(ref reader);

        // Packet Identifier
        if (this.Message.QoS != QualityOfService.AtMostOnceDelivery)
        {
            this.PacketIdentifier = (ushort)DecodePacketIdentifier(ref reader);
        }

        // Properties
        var propertyLength = DecodeVariableByteInteger(ref reader, out var lengthOfPropertyLength);
        if (propertyLength > 0)
        {
            this.DecodeProperties(ref reader, propertyLength);
            if (this.Properties.PayloadFormatIndicator == (byte)MQTT5PayloadFormatIndicator.UTF8Encoded)
            {
                this.Message.PayloadFormatIndicator = MQTT5PayloadFormatIndicator.UTF8Encoded;
            }
            else
            {
                this.Message.PayloadFormatIndicator = MQTT5PayloadFormatIndicator.Unspecified;
            }

            if (this.Properties.MessageExpiryInterval != null)
            {
                this.Message.MessageExpiryInterval = (int)this.Properties.MessageExpiryInterval;
            }

            if (this.Properties.TopicAlias != null)
            {
                this.Message.TopicAlias = (ushort)this.Properties.TopicAlias;
            }

            if (this.Properties.ResponseTopic != null)
            {
                this.Message.ResponseTopic = this.Properties.ResponseTopic;
            }

            if (this.Properties.CorrelationData != null)
            {
                this.Message.CorrelationData = this.Properties.CorrelationData;
            }

            this.Message.UserProperties = this.Properties.UserProperties;

            if (this.Properties.SubscriptionIdentifier != null)
            {
                this.Message.SubscriptionIdentifiers.Add((int)this.Properties.SubscriptionIdentifier);
            }

            if (this.Properties.ContentType != null)
            {
                this.Message.ContentType = this.Properties.ContentType;
            }
        } // End properties

        var variableHeaderLength = reader.Consumed - variableHeaderStart;
        var payloadLength = fhRemainingLength - variableHeaderLength;

        // Payload
        this.Message.Payload = new byte[payloadLength];
        for (var i = 0; i < payloadLength; i++)
        {
            if (reader.TryRead(out var b))
            {
                this.Message.Payload[i] = b;
            }
        }
    }

    /// <summary>
    /// Encode this packet to be sent on the wire.
    /// </summary>
    /// <returns>An array of bytes ready to be sent.</returns>
    public byte[] Encode()
    {
        var payloadLength = this.Message.Payload?.Length ?? 0;

        using (var vhAndPayloadStream = new MemoryStream(payloadLength + 256))
        {
            this.GatherPublishFlagsAndProperties();

            // Variable Header
            // Topic Name
            if (this.Message.Topic != null)
            {
                _ = EncodeUTF8String(vhAndPayloadStream, this.Message.Topic);
            }

            // Packet Identifier
            if (this.Message.QoS > QualityOfService.AtMostOnceDelivery)
            {
                EncodeTwoByteInteger(vhAndPayloadStream, this.PacketIdentifier);
            }

            // Properties
            this.EncodeProperties(vhAndPayloadStream);

            // Payload
            if (this.Message.Payload != null)
            {
                for (var i = 0; i < this.Message.Payload?.Length; i++)
                {
                    vhAndPayloadStream.WriteByte(this.Message.Payload[i]);
                }
            }

            // Construct the Fixed Header
            var byte1 = (byte)ControlPacketType.Publish << 4;

            // DUP Flag
            if (this.Message.Duplicate is true)
            {
                byte1 |= 0x8;
            }

            // QoS Flag
            if (this.Message.QoS == QualityOfService.AtLeastOnceDelivery)
            {
                byte1 |= 0x2;
            }
            else if (this.Message.QoS == QualityOfService.ExactlyOnceDelivery)
            {
                byte1 |= 0x4;
            }

            // Retain Flag
            if (this.Message.Retain is true)
            {
                byte1 |= 0x1;
            }

            // Largest possible size of a fixed header is (5):
            // byte 1: MQTT Control Packet Type & Flags
            // byte 2: Remaining Length encoded as a variable byte integer
            // byte 3: Remaining Length encoded as a variable byte integer (max size is 4)
            // byte 4: Remaining Length encoded as a variable byte integer (max size is 4)
            // byte 5: Remaining Length encoded as a variable byte integer (max size is 4)

            // Construct the final packet
            var constructedPacket = new MemoryStream((int)vhAndPayloadStream.Length + 5);

            // Write the Fixed Header
            constructedPacket.WriteByte((byte)byte1);
            _ = EncodeVariableByteInteger(constructedPacket, (int)vhAndPayloadStream.Length);

            // Copy the Variable Header and Payload
            vhAndPayloadStream.Position = 0;
            vhAndPayloadStream.CopyTo(constructedPacket);

            return constructedPacket.ToArray();
        }
    }

    /// <summary>
    /// Gather the flags and properties for an outgoing Publish packet from <see cref="MQTT5PublishMessage"/>
    /// as data preparation for encoding in <see cref="PublishPacket"/>.
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
