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
/// An MQTT PUBACK Control Packet as defined in:
/// https://docs.oasis-open.org/mqtt/mqtt/v5.0/os/mqtt-v5.0-os.html#_Toc3901121.
/// </summary>
public class PubAckPacket : ControlPacket
{
    public PubAckPacket(ushort packetIdentifier, PubAckReasonCode reasonCode)
    {
        this.PacketIdentifier = packetIdentifier;
        this.ReasonCode = reasonCode;
    }

    public PubAckPacket(ReadOnlySequence<byte> packetData)
    {
        this.SessionPresent = false;
        this.Decode(packetData);
    }

    public bool SessionPresent { get; set; }

    public int AckFlags { get; set; }

    public PubAckReasonCode ReasonCode { get; set; }

    public override ControlPacketType ControlPacketType => ControlPacketType.PubAck;

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
            constructedPacket.WriteByte(((byte)ControlPacketType.PubAck) << 4);
            _ = EncodeVariableByteInteger(constructedPacket, (int)vhStream.Length);

            // Copy the Variable Header and Payload
            vhStream.Position = 0;
            vhStream.CopyTo(constructedPacket);

            return constructedPacket.ToArray();
        }
    }

    /// <summary>
    /// Decode the raw packet data of a PUBACK packet.
    /// </summary>
    /// <param name="packetData">The raw packet data.</param>
    public void Decode(ReadOnlySequence<byte> packetData)
    {
        var reader = new SequenceReader<byte>(packetData);
        this.PacketSize = packetData.Length;

        // Skip past the fixed header MQTT Control Packet type
        reader.Advance(1);

        // Remaining Length
        var fhRemainingLength = DecodeVariableByteInteger(ref reader, out var vbiLength);

        this.PacketIdentifier = (ushort)DecodePacketIdentifier(ref reader);

        if (reader.TryRead(out var reasonCode))
        {
            this.ReasonCode = (PubAckReasonCode)reasonCode;
        }

        var propertyLength = DecodeVariableByteInteger(ref reader);
        _ = this.DecodeProperties(ref reader, propertyLength);
    }
}
