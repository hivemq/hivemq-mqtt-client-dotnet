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
using HiveMQtt.MQTT5.Exceptions;
using HiveMQtt.MQTT5.ReasonCodes;

/// <summary>
/// An MQTT PUBREC Control Packet as defined in:
/// https://docs.oasis-open.org/mqtt/mqtt/v5.0/os/mqtt-v5.0-os.html#_Toc3901131.
/// </summary>
public class PubRecPacket : ControlPacket
{
    public PubRecPacket(ushort packetIdentifier, PubRecReasonCode reasonCode)
    {
        this.PacketIdentifier = packetIdentifier;
        this.ReasonCode = reasonCode;
    }

    public PubRecPacket(ReadOnlySequence<byte> packetData) => this.Decode(packetData);

    public PubRecReasonCode ReasonCode { get; set; }

    public override ControlPacketType ControlPacketType => ControlPacketType.PubRec;

    /// <summary>
    /// Encode this packet to be sent on the wire.
    /// </summary>
    /// <returns>An array of bytes ready to be sent.</returns>
    public byte[] Encode()
    {
        using (var vhStream = new MemoryStream())
        {
            // Variable Header
            ControlPacket.EncodeTwoByteInteger(vhStream, this.PacketIdentifier);
            vhStream.WriteByte((byte)this.ReasonCode);
            this.EncodeProperties(vhStream);

            // Construct the final packet
            var constructedPacket = new MemoryStream((int)vhStream.Length + 5);

            // Write the Fixed Header
            constructedPacket.WriteByte(((byte)ControlPacketType.PubRec) << 4);
            _ = EncodeVariableByteInteger(constructedPacket, (int)vhStream.Length);

            // Copy the Variable Header and Payload
            vhStream.Position = 0;
            vhStream.CopyTo(constructedPacket);

            return constructedPacket.ToArray();
        };
    }

    public void Decode(ReadOnlySequence<byte> packetData)
    {
        var reader = new SequenceReader<byte>(packetData);

        // Skip past the fixed header MQTT Control Packet type
        reader.Advance(1);

        // Remaining Length
        var fhRemainingLength = DecodeVariableByteInteger(ref reader, out var vbiLength);

        // FIXME: Centralize packet identifier validation
        var packetIdentifier = DecodeTwoByteInteger(ref reader);
        if (packetIdentifier != null && packetIdentifier.Value > 0 && packetIdentifier.Value <= ushort.MaxValue)
        {
            this.PacketIdentifier = packetIdentifier.Value;
        }
        else
        {
            throw new MQTTProtocolException("Invalid packet identifier");
        }

        if (reader.TryRead(out var reasonCode))
        {
            this.ReasonCode = (PubRecReasonCode)reasonCode;
        }

        var propertyLength = DecodeVariableByteInteger(ref reader);
        _ = this.DecodeProperties(ref reader, propertyLength);
    }
}
