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
namespace HiveMQtt.Client.Events;

using HiveMQtt.MQTT5;
using HiveMQtt.MQTT5.Packets;

/// <summary>
/// Event arguments for the <see cref="PublishPacket.OnPublishQoS2Complete"/> event.
/// <para>This event is called when a QoS level 2 publish as been completed.</para>
/// <para><see cref="OnPublishQoS2CompleteEventArgs.PacketList"/> contains the list of
/// packets sent/received in the QoS 2 transaction chain.</para>
/// </summary>
public class OnPublishQoS2CompleteEventArgs : EventArgs
{
    public OnPublishQoS2CompleteEventArgs(List<ControlPacket> packetList) => this.PacketList = packetList;

    public List<ControlPacket> PacketList { get; set; }
}
