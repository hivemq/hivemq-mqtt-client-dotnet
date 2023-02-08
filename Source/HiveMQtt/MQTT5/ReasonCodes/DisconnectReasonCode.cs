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
/// MQTT v5.0 Disconnect Reason Codes as defined in:
/// https://docs.oasis-open.org/mqtt/mqtt/v5.0/os/mqtt-v5.0-os.html#_Toc3901208.
/// </summary>
public enum DisconnectReasonCode
{
    /// <summary>
    /// Close the connection normally. Do not send the Will Message.
    /// </summary>
    NormalDisconnection = 0x0,

    /// <summary>
    /// The Client wishes to disconnect but requires that the Server also publishes its Will Message.
    /// </summary>
    DisconnectWithWillMessage = 0x04,

    /// <summary>
    /// The Connection is closed but the sender either does not wish to reveal the reason, or none of the other Reason Codes apply.
    /// </summary>
    UnspecifiedError = 0x80,

    /// <summary>
    /// The received packet does not conform to this specification.
    /// </summary>
    MalformedPacket = 0x81,

    /// <summary>
    /// An unexpected or out of order packet was received.
    /// </summary>
    ProtocolError = 0x82,

    /// <summary>
    /// The packet received is valid but cannot be processed by this implementation.
    /// </summary>
    ImplementationSpecificError = 0x83,

    /// <summary>
    /// The Topic Name is correctly formed, but is not accepted by this Client or Server.
    /// </summary>
    TopicNameInvalid = 0x90,

    /// <summary>
    /// The Client or Server has received more than Receive Maximum publication for which it has not sent PUBACK or PUBCOMP.
    /// </summary>
    ReceiveMaximumExceeded = 0x93,

    /// <summary>
    /// The Client or Server has received a PUBLISH packet containing a Topic Alias which is greater than the Maximum Topic Alias it sent in the CONNECT or CONNACK packet.
    /// </summary>
    TopicAliasInvalid = 0x94,

    /// <summary>
    /// The packet size is greater than Maximum Packet Size for this Client or Server.
    /// </summary>
    PacketTooLarge = 0x95,

    /// <summary>
    /// The received data rate is too high.
    /// </summary>
    MessageRateTooHigh = 0x96,

    /// <summary>
    /// An implementation or administrative imposed limit has been exceeded.
    /// </summary>
    QuotaExceeded = 0x97,

    /// <summary>
    /// The Connection is closed due to an administrative action.
    /// </summary>
    AdministrativeAction = 0x98,

    /// <summary>
    /// The payload format does not match the one specified by the Payload Format Indicator.
    /// </summary>
    PayloadFormatInvalid = 0x99,
}
