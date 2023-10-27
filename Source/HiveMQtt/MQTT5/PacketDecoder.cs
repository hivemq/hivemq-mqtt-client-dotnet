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
using HiveMQtt.MQTT5.Packets;

/// <summary>
/// Decodes a Control Packet from a buffer.
/// </summary>
internal class PacketDecoder
{
    private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

    public static Boolean TryDecode(ReadOnlySequence<byte> buffer, out ControlPacket decodedPacket, out SequencePosition consumed)
    {
        try
        {
            consumed = buffer.Start;
            if (buffer.Length < 2)
            {
                // We need at least the MQTT Header
                decodedPacket = new PartialPacket();
                consumed = default;
                return false;
            }

            var srBuffer = new SequenceReader<byte>(buffer);

            // Byte 1: Control Packet Type
            srBuffer.TryRead(out var cpByte);
            var controlPacketType = cpByte >> 4;

            // Byte 2-5: Remaining Length of the Variable Header
            // Size of VBI in vbiLengthInBytes
            var remainingLength = ControlPacket.DecodeVariableByteInteger(ref srBuffer, out var vbiLengthInBytes);

            // control packet byte + variable byte integer length + remaining length
            var packetLength = 1 + vbiLengthInBytes + remainingLength;

            if (buffer.Length < packetLength)
            {
                // Not all data for this packet has arrived yet.  Try again...
                Logger.Trace("PacketDecoder.Decode: Not all data for this packet has arrived yet.  Returning PartialPacket.");
                decodedPacket = new PartialPacket();
                consumed = default;
                return false;
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
            decodedPacket = packet;

            Logger.Trace($"PacketDecoder: Decoded Packet: consumed={consumed.GetInteger()}, packet={packet} id={packet.PacketIdentifier}");
            return true;
        }
        catch (Exception ex)
        {
            Logger.Error(ex, $"PacketDecoder.Decode: Exception caught.  Returning MalformedPacket.");
            consumed = buffer.Start;
            decodedPacket = new MalformedPacket(buffer);
            return false;
        }
    }
}
