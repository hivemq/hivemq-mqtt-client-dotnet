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

/// <summary>
/// A packet with bad or nonsensical data.
/// </summary>
internal class MalformedPacket : ControlPacket
{
#pragma warning disable IDE0052
    private readonly ReadOnlySequence<byte> packetData;
#pragma warning restore IDE0052

    public MalformedPacket(ReadOnlySequence<byte> buffer) => this.packetData = buffer;

    public override ControlPacketType ControlPacketType => ControlPacketType.Reserved;
}
