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
using HiveMQtt.MQTT5.ReasonCodes;

/// <summary>
/// An MQTT PUBREL Control Packet as defined in:
/// https://docs.oasis-open.org/mqtt/mqtt/v5.0/os/mqtt-v5.0-os.html#_Toc3901141.
/// </summary>
public class PubRelPacket : ControlPacket
{
    public PubRelPacket(ushort packetIdentifier, PubRelReasonCode reasonCode)
    {
        this.PacketIdentifier = packetIdentifier;
        this.ReasonCode = reasonCode;
    }

    public PubRelPacket(ReadOnlySequence<byte> packetData) => this.Decode(packetData);

    public PubRelReasonCode ReasonCode { get; set; }

    public override ControlPacketType ControlPacketType => ControlPacketType.PubRel;

    /// <summary>
    /// Encode this packet to be sent on the wire.
    /// </summary>
    /// <returns>An array of bytes ready to be sent.</returns>
    public byte[] Encode()
    {
        using (var vhStream = new MemoryStream())
        {
            // Variable Header
            EncodeTwoByteInteger(vhStream, this.PacketIdentifier);
            vhStream.WriteByte((byte)this.ReasonCode);
            this.EncodeProperties(vhStream);

            // Construct the final packet
            var constructedPacket = new MemoryStream((int)vhStream.Length + 5);

            // Write the Fixed Header
            var byte1 = (byte)ControlPacketType.PubRel << 4;
            byte1 |= 0x2;
            constructedPacket.WriteByte((byte)byte1);
            _ = EncodeVariableByteInteger(constructedPacket, (int)vhStream.Length);

            // Copy the Variable Header and Payload
            vhStream.Position = 0;
            vhStream.CopyTo(constructedPacket);

            return constructedPacket.ToArray();
        }
    }

    /// <summary>
    /// Decode the packet data of an MQTT PUBREL Packet.
    /// </summary>
    /// <param name="packetData">The packet data as a ReadOnlySequence of bytes.</param>
    public void Decode(ReadOnlySequence<byte> packetData)
    {
        var reader = new SequenceReader<byte>(packetData);
        this.PacketSize = packetData.Length;

        reader.Advance(1);

        reader.TryRead(out var remainingLength);

        this.PacketIdentifier = (ushort)DecodePacketIdentifier(ref reader);

        if (reader.TryRead(out var reasonCode))
        {
            this.ReasonCode = (PubRelReasonCode)reasonCode;
        }

        var propertyLength = DecodeVariableByteInteger(ref reader);
        _ = this.DecodeProperties(ref reader, propertyLength);
    }
}
