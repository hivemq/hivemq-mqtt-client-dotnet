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
    /// <param name="unsubOptions">The constructed UnsubscribeOptions class.</param>
    /// <param name="packetIdentifier">A unique packet identifier for the packet to be created.</param>
    public UnsubscribePacket(UnsubscribeOptions unsubOptions, ushort packetIdentifier)
    {
        this.PacketIdentifier = packetIdentifier;
        this.Subscriptions = unsubOptions.Subscriptions;
        this.Properties.UserProperties = unsubOptions.UserProperties;
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
        using (var vhAndPayloadStream = new MemoryStream())
        {
            // Variable Header
            _ = EncodeTwoByteInteger(vhAndPayloadStream, (int)this.PacketIdentifier);
            this.EncodeProperties(vhAndPayloadStream);

            // Payload
            foreach (var subscription in this.Subscriptions)
            {
                EncodeUTF8String(vhAndPayloadStream, subscription.TopicFilter.Topic);
            }

            // Construct the final packet
            var constructedPacket = new MemoryStream((int)vhAndPayloadStream.Length + 5);

            // Write the Fixed Header
            var byte1 = (byte)ControlPacketType.Unsubscribe << 4;
            byte1 |= 0x2;
            constructedPacket.WriteByte((byte)byte1);
            _ = EncodeVariableByteInteger(constructedPacket, (int)vhAndPayloadStream.Length);

            // Copy the Variable Header and Payload
            vhAndPayloadStream.Position = 0;
            vhAndPayloadStream.CopyTo(constructedPacket);

            return constructedPacket.ToArray();
        }
    }
}
