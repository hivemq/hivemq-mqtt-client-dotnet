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
using HiveMQtt.MQTT5.ReasonCodes;

/// <summary>
/// An MQTT Disconnect Control Packet as defined in:
/// https://docs.oasis-open.org/mqtt/mqtt/v5.0/os/mqtt-v5.0-os.html#_Toc3901205.
/// </summary>
public class DisconnectPacket : ControlPacket
{
    public DisconnectPacket()
    {
    }

    public DisconnectPacket(ReadOnlySequence<byte> packetData) => this.Decode(packetData);

    public override ControlPacketType ControlPacketType => ControlPacketType.Disconnect;

    public DisconnectReasonCode DisconnectReasonCode { get; set; }

    /// <summary>
    /// Encode this packet to be sent on the wire.
    /// </summary>
    /// <returns>An array of bytes ready to be sent.</returns>
    public static byte[] Encode()
    {
        using(var stream = new MemoryStream(8))
        {
            stream.Position = 2;

            // Variable Header - starts at byte 2
            stream.WriteByte((int)DisconnectReasonCode.NormalDisconnection);

            // Disconnect has no payload

            // Fixed Header - Add to the beginning of the stream
            var remainingLength = stream.Length - 2;

            stream.Position = 0;
            stream.WriteByte((byte)ControlPacketType.Disconnect << 4);
            EncodeVariableByteInteger(stream, (int)remainingLength);

            return stream.ToArray();
        };
    }

    public void Decode(ReadOnlySequence<byte> packetData)
    {
        var reader = new SequenceReader<byte>(packetData);

        // Skip past the Fixed Header
        reader.Advance(1);

        if (reader.TryRead(out var remainingLength))
        {
            if (remainingLength + 2 > packetData.Length)
            {
                // Not enough packet data / partial packet
                // FIXME: Send back to pipeline to get more data
            }
            else if (remainingLength < 1)
            {
                // Byte 1 in the Variable Header is the Disconnect Reason Code. If the Remaining Length is less
                // than 1 the value of 0x00 (Normal disconnection) is used.
                // See <see href="https://docs.oasis-open.org/mqtt/mqtt/v5.0/os/mqtt-v5.0-os.html#_Toc3901208">
                // Disconnect Reason Codes</see>.
                this.DisconnectReasonCode = DisconnectReasonCode.NormalDisconnection;
            }
            else
            {
                if (reader.TryRead(out var reasonCode))
                {
                    this.DisconnectReasonCode = (DisconnectReasonCode)reasonCode;

                    var propertyLength = DecodeVariableByteInteger(ref reader);
                    _ = this.DecodeProperties(ref reader, propertyLength);
                }
            }
        }
    }
}
