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
/// An MQTT Connect Control Packet as defined in:
/// https://docs.oasis-open.org/mqtt/mqtt/v5.0/os/mqtt-v5.0-os.html#_Toc3901074.
/// </summary>
public class UnsubAckPacket : ControlPacket
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UnsubAckPacket"/> class.
    /// </summary>
    /// <param name="packetData">The raw packet data off the wire.</param>
    public UnsubAckPacket(ReadOnlySequence<byte> packetData)
    {
        this.ReasonCodes = new List<UnsubAckReasonCode>();
        this.Decode(packetData);
    }

    /// <summary>
    /// Gets or sets the list of Reason Codes in this packet.
    /// </summary>
    public List<UnsubAckReasonCode> ReasonCodes { get; set; }

    /// <inheritdoc/>
    public override ControlPacketType ControlPacketType => ControlPacketType.SubAck;

    /// <summary>
    /// Decodes the raw packet data.
    /// </summary>
    /// <param name="packetData">The raw packet data off the wire.</param>
    public void Decode(ReadOnlySequence<byte> packetData)
    {
        var reader = new SequenceReader<byte>(packetData);
        this.PacketSize = packetData.Length;

        // Skip past the fixed header MQTT Control Packet type
        reader.Advance(1);

        // Remaining Length
        var fhRemainingLength = DecodeVariableByteInteger(ref reader, out var vbiLength);
        var variableHeaderStart = reader.Consumed;

        this.PacketIdentifier = (ushort)DecodePacketIdentifier(ref reader);

        var propertyLength = DecodeVariableByteInteger(ref reader, out var lengthOfPropertyLength);
        if (propertyLength > 0)
        {
            this.DecodeProperties(ref reader, propertyLength);
        }

        // Payload
        var variableHeaderLength = reader.Consumed - variableHeaderStart;
        var payloadLength = fhRemainingLength - variableHeaderLength;

        // The Payload contains a list of Reason Codes.
        for (var x = 0; x < payloadLength; x++)
        {
            reader.TryRead(out var reasonCode);
            this.ReasonCodes.Add((UnsubAckReasonCode)reasonCode);
        }
    }
}
