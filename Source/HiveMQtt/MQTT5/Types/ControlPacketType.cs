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

/// <summary>
/// MQTT v5.0 Control Packet Types as defined in 2.1.2 MQTT Control Packet type:
/// https://docs.oasis-open.org/mqtt/mqtt/v5.0/os/mqtt-v5.0-os.html#_Toc3901022.
/// </summary>
public enum ControlPacketType
{
    /// <summary>
    /// Reserved.
    /// </summary>
    Reserved = 0x0,

    /// <summary>
    /// Connection Request.
    /// </summary>
    Connect = 0x1,

    /// <summary>
    /// Connect Acknowledgement.
    /// </summary>
    ConnAck = 0x2,

    /// <summary>
    /// Publish Message.
    /// </summary>
    Publish = 0x3,

    /// <summary>
    /// Publish Acknowledgement (QoS 1).
    /// </summary>
    PubAck = 0x4,

    /// <summary>
    /// Publish Received (QoS 2 delivery party 1).
    /// </summary>
    PubRec = 0x5,

    /// <summary>
    /// Publish Release (QoS 2 delivery party 2).
    /// </summary>
    PubRel = 0x6,

    /// <summary>
    /// Publish Complete(QoS 2 delivery party 3).
    /// </summary>
    PubComp = 0x7,

    /// <summary>
    /// Subscribe Request.
    /// </summary>
    Subscribe = 0x8,

    /// <summary>
    /// Subscribe Acknowledgement.
    /// </summary>
    SubAck = 0x9,

    /// <summary>
    /// Unsubscribe Request.
    /// </summary>
    Unsubscribe = 0xa,

    /// <summary>
    /// Unsubscribe Acknowledgement.
    /// </summary>
    UnsubAck = 0xb,

    /// <summary>
    /// PING Request.
    /// </summary>
    PingReq = 0xc,

    /// <summary>
    /// PING Response.
    /// </summary>
    PingResp = 0xd,

    /// <summary>
    /// Disconnect Notification.
    /// </summary>
    Disconnect = 0xe,

    /// <summary>
    /// Authentication Exchange.
    /// </summary>
    Auth = 0xf,
}
