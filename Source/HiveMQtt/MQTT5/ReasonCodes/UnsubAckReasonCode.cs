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
namespace HiveMQtt.MQTT5.ReasonCodes;

/// <summary>
/// MQTT v5.0 UNSUBACK Reason Codes as defined in:
/// https://docs.oasis-open.org/mqtt/mqtt/v5.0/os/mqtt-v5.0-os.html#_Toc3901194.
/// </summary>
public enum UnsubAckReasonCode
{
    /// <summary>
    /// The subscription is deleted.
    /// </summary>
    Success = 0x0,

    /// <summary>
    /// No matching Topic Filter is being used by the Client.
    /// </summary>
    NoSubscriptionExisted = 0x11,

    /// <summary>
    /// The unsubscribe could not be completed and the Server either does not wish to reveal the reason or none of the other Reason Codes apply.
    /// </summary>
    UnspecifiedError = 0x80,

    /// <summary>
    /// The UNSUBSCRIBE is valid but the Server does not accept it.
    /// </summary>
    ImplementationSpecificError = 0x83,

    /// <summary>
    /// The Client is not authorized to unsubscribe.
    /// </summary>
    NotAuthorized = 0x87,

    /// <summary>
    /// The Topic Filter is correctly formed but is not allowed for this Client.
    /// </summary>
    TopicFilterInvalid = 0x8F,

    /// <summary>
    /// The specified Packet Identifier is already in use.
    /// </summary>
    PacketIdentifierInUse = 0x91,
}
