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
namespace HiveMQtt.MQTT5;

using System.Buffers;
using System.Diagnostics;
using HiveMQtt.MQTT5.Packets;

/// <summary>
/// Decodes a Control Packet from a buffer.
/// </summary>
internal class PacketDecoder
{
    public static ControlPacket Decode(ReadOnlySequence<byte> buffer, out SequencePosition consumed)
    {
        try
        {
            consumed = buffer.Start;

            if (buffer.Length < 2)
            {
                // We need at least the MQTT Header
                return new PartialPacket();
            }

            // It's a waste to allocate a SequenceReader here for two bytes.
            var x = buffer.ToArray();

            // Byte 1: Control Packet Type
            var controlPacketType = x[0] >> 4;

            // Byte 2: Remaining Length of the Variable Header
            var remainingLengthOfVH = x[1];
            var packetLength = remainingLengthOfVH + 2;

            if (buffer.Length < packetLength)
            {
                // Not all data for this packet has arrived yet.  Try again...
                return new PartialPacket();
            }

            var packetData = buffer.Slice(0, packetLength);

            ControlPacket packet = controlPacketType switch
            {
                (int)ControlPacketType.ConnAck => new ConnAckPacket(packetData),
                (int)ControlPacketType.Disconnect => new DisconnectPacket(packetData),
                (int)ControlPacketType.PingResp => new PingRespPacket(),
                (int)ControlPacketType.Publish => new PublishPacket(packetData),
                (int)ControlPacketType.PubAck => new PubAckPacket(packetData),
                (int)ControlPacketType.PubRec => new PubRecPacket(packetData),
                (int)ControlPacketType.PubRel => new PubRelPacket(packetData),
                (int)ControlPacketType.PubComp => new PubCompPacket(packetData),
                (int)ControlPacketType.SubAck => new SubAckPacket(packetData),
                (int)ControlPacketType.UnsubAck => new UnsubAckPacket(packetData),
                _ => new MalformedPacket(packetData),
            };

            consumed = buffer.GetPosition(packetLength);
            return packet;
        }
        catch (System.Exception)
        {
            Trace.WriteLine("PacketDecoder.Decode: Exception caught.  Returning MalformedPacket.");
            consumed = buffer.Start;
            return new MalformedPacket(buffer);
        }
    }
}
