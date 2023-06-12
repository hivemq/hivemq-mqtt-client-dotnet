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
/// An MQTT SUBACK Control Packet as defined in:
/// https://docs.oasis-open.org/mqtt/mqtt/v5.0/os/mqtt-v5.0-os.html#_Toc3901187.
/// </summary>
public class SubAckPacket : ControlPacket
{
    public SubAckPacket(ReadOnlySequence<byte> packetData)
    {
        this.ReasonCodes = new List<SubAckReasonCode>();
        this.Decode(packetData);
    }

    /// <summary>
    /// Gets or sets the list of Reason Codes.
    /// </summary>
    public List<SubAckReasonCode> ReasonCodes { get; set; }

    /// <inheritdoc/>
    public override ControlPacketType ControlPacketType => ControlPacketType.SubAck;

    /// <summary>
    /// Decodes the raw packet data.
    /// </summary>
    public void Decode(ReadOnlySequence<byte> packetData)
    {
        var reader = new SequenceReader<byte>(packetData);

        // Skip past the fixed header MQTT Control Packet type
        reader.Advance(1);

        // Remaining Length
        var fhRemainingLength = DecodeVariableByteInteger(ref reader, out var vbiLength);
        var variableHeaderStart = reader.Consumed;

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
            this.ReasonCodes.Add((SubAckReasonCode)reasonCode);
        }
    }
}
