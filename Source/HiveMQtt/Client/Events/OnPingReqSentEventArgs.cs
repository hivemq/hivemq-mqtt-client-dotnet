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

using HiveMQtt.MQTT5.Packets;

/// <summary>
/// Event arguments for the <see cref="HiveMQClient.OnPingReqSent"/> event.
/// <para>This event is called when a PINGREQ packet is sent to the broker.</para>
/// <para><see cref="PingReqPacket"/> contains the sent PINGREQ packet.</para>
/// </summary>
public class OnPingReqSentEventArgs : EventArgs
{
    public OnPingReqSentEventArgs(PingReqPacket pingReqPacket) => this.PingReqPacket = pingReqPacket;

    public PingReqPacket PingReqPacket { get; set; }
}
