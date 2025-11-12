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

/// <summary>
/// An MQTT PingReq Control Packet.
///
/// See also <seealso href="https://docs.oasis-open.org/mqtt/mqtt/v5.0/os/mqtt-v5.0-os.html#_Toc3901195">
/// PingReq Control Packet</seealso>.
/// </summary>
public class PingReqPacket : ControlPacket
{
    public override ControlPacketType ControlPacketType => ControlPacketType.PingResp;

    /// <summary>
    /// Encode this packet to be sent on the wire.
    /// </summary>
    /// <returns>An array of bytes ready to be sent.</returns>
    public static byte[] Encode()
    {
        // PingReq is always 2 bytes - use stackalloc for zero allocation
#pragma warning disable IDE0302 // Collection initialization - stackalloc is not a collection
        Span<byte> buffer = stackalloc byte[2];
#pragma warning restore IDE0302
        buffer[0] = ((byte)ControlPacketType.PingReq) << 4;
        buffer[1] = 0x0;

        // Return a new array (required for async operations)
        return buffer.ToArray();
    }
}
