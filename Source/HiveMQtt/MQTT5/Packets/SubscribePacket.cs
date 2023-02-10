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

using System.IO;

using HiveMQtt.Client.Options;
using HiveMQtt.MQTT5.Types;

/// <summary>
/// An MQTT Subscribe Control Packet.
///
/// See also <seealso href="https://docs.oasis-open.org/mqtt/mqtt/v5.0/os/mqtt-v5.0-os.html#_Toc3901205">
/// Subscribe Control Packet</seealso>.
/// </summary>
public class SubscribePacket : ControlPacket
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SubscribePacket"/> class
    /// with the options to be used for the publish.
    /// </summary>
    /// <param name="options">The raw packet data off the wire.</param>
    /// <param name="packetIdentifier">A unique packet identifier for the packet to be created.</param>
    /// <param name="userProperties">User properties to be sent with the packet.</param>
    public SubscribePacket(SubscribeOptions options, ushort packetIdentifier, Dictionary<string, string>? userProperties = null)
    {
        this.PacketIdentifier = packetIdentifier;
        this.Options = options;
        if (userProperties != null)
        {
            this.Properties.UserProperties = userProperties;
        }
    }

    /// <summary>
    /// Gets or sets the options for an outgoing Subscribe packet.
    /// </summary>
    public SubscribeOptions Options { get; set; }

    /// <inheritdoc/>
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
        foreach (var tf in this.Options.TopicFilters)
        {
            EncodeUTF8String(stream, tf.Topic);

            var optionsByte = (byte)tf.QoS;
            if (tf.NoLocal is true)
            {
                optionsByte |= 0x4;
            }

            if (tf.RetainAsPublished is true)
            {
                optionsByte |= 0x8;
            }

            if (tf.RetainHandling is RetainHandling.SendAtSubscribeIfNewSubscription)
            {
                optionsByte |= 0x10;
            }
            else if (tf.RetainHandling is RetainHandling.DoNotSendAtSubscribe)
            {
                optionsByte |= 0x20;
            }
            stream.WriteByte(optionsByte);
        }

        // Fixed Header
        stream.Position = 0;
        var length = stream.Length - 2;
        var byte1 = (byte)ControlPacketType.Subscribe << 4;
        byte1 |= 0x2;

        stream.WriteByte((byte)byte1);
        _ = EncodeVariableByteInteger(stream, (int)length);
        return stream.ToArray();
    }

    /// <summary>
    /// Gather the flags and properties for a Subscribe packet from <see cref="SubscribeOptions"/>
    /// as data preparation for encoding in <see cref="SubscribePacket"/>.
    /// </summary>
    internal void GatherSubscribeFlagsAndProperties() => this.Options.ValidateOptions();
}
