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
/// MQTT v5.0 SUBACK Reason Codes as defined in:
/// https://docs.oasis-open.org/mqtt/mqtt/v5.0/os/mqtt-v5.0-os.html#_Toc3901178.
/// </summary>
public enum SubAckReasonCode
{
    /// <summary>
    /// The subscription is accepted and the maximum QoS sent will be QoS 0. This might be a lower QoS than was requested.
    /// </summary>
    GrantedQoS0 = 0x0,

    /// <summary>
    /// The subscription is accepted and the maximum QoS sent will be QoS 1. This might be a lower QoS than was requested.
    /// </summary>
    GrantedQoS1 = 0x1,

    /// <summary>
    /// The subscription is accepted and any received QoS will be sent to this subscription.
    /// </summary>
    GrantedQoS2 = 0x2,

    /// <summary>
    /// The subscription is not accepted and the Server either does not wish to reveal the reason or none of the other Reason Codes apply.
    /// </summary>
    UnspecifiedError = 0x80,

    /// <summary>
    /// The SUBSCRIBE is valid but the Server does not accept it.
    /// </summary>
    ImplementationSpecificError = 0x83,

    /// <summary>
    /// The Client is not authorized to make this subscription.
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

    /// <summary>
    /// An implementation or administrative imposed limit has been exceeded.
    /// </summary>
    QuotaExceeded = 0x97,

    /// <summary>
    /// The Server does not support Shared Subscriptions for this Client.
    /// </summary>
    SharedSubscriptionsNotSupported = 0x9E,

    /// <summary>
    /// The Server does not support Subscription Identifiers; the subscription is not accepted.
    /// </summary>
    SubscriptionIdentifiersNotSupported = 0xA1,

    /// <summary>
    /// The Server does not support Wildcard Subscriptions; the subscription is not accepted.
    /// </summary>
    WildcardSubscriptionsNotSupported = 0xA2,
}
