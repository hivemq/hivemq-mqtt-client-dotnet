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
namespace HiveMQtt.MQTT5.Types;

/// <summary>
/// MQTT v5 Property Types as defined in
/// https://docs.oasis-open.org/mqtt/mqtt/v5.0/os/mqtt-v5.0-os.html#_Toc3901027.
/// </summary>
public enum MQTT5PropertyType
{
    /// <summary>
    /// Payload Format Indicator.
    /// </summary>
    PayloadFormatIndicator = 0x1,

    /// <summary>
    /// Message Expiry Interval.
    /// </summary>
    MessageExpiryInterval = 0x02,

    /// <summary>
    /// Content Type.
    /// </summary>
    ContentType = 0x03,

    /// <summary>
    /// Response Topic.
    /// </summary>
    ResponseTopic = 0x08,

    /// <summary>
    /// Correlation Data.
    /// </summary>
    CorrelationData = 0x09,

    /// <summary>
    /// Subscription Identifier.
    /// </summary>
    SubscriptionIdentifier = 0x0B,

    /// <summary>
    /// Session Expiry Interval.
    /// </summary>
    SessionExpiryInterval = 0x11,

    /// <summary>
    /// Assigned Client Identifier.
    /// </summary>
    AssignedClientIdentifier = 0x12,

    /// <summary>
    /// Server Keep Alive.
    /// </summary>
    ServerKeepAlive = 0x13,

    /// <summary>
    /// Authentication Method.
    /// </summary>
    AuthenticationMethod = 0x15,

    /// <summary>
    /// Authentication Data.
    /// </summary>
    AuthenticationData = 0x16,

    /// <summary>
    /// Request Problem Information.
    /// </summary>
    RequestProblemInformation = 0x17,

    /// <summary>
    /// Will Delay Interval.
    /// </summary>
    WillDelayInterval = 0x18,

    /// <summary>
    /// Request Response Information.
    /// </summary>
    RequestResponseInformation = 0x19,

    /// <summary>
    /// Response Information.
    /// </summary>
    ResponseInformation = 0x1A,

    /// <summary>
    /// Server Reference.
    /// </summary>
    ServerReference = 0x1C,

    /// <summary>
    /// Reason String.
    /// </summary>
    ReasonString = 0x1F,

    /// <summary>
    /// Receive Maximum.
    /// </summary>
    ReceiveMaximum = 0x21,

    /// <summary>
    /// Topic Alias Maximum.
    /// </summary>
    TopicAliasMaximum = 0x22,

    /// <summary>
    /// Topic Alias.
    /// </summary>
    TopicAlias = 0x23,

    /// <summary>
    /// Maximum QoS.
    /// </summary>
    MaximumQoS = 0x24,

    /// <summary>
    /// Retain Available.
    /// </summary>
    RetainAvailable = 0x25,

    /// <summary>
    /// User Property.
    /// </summary>
    UserProperty = 0x26,

    /// <summary>
    /// Maximum Packet Size.
    /// </summary>
    MaximumPacketSize = 0x27,

    /// <summary>
    /// Wildcard Subscription Available.
    /// </summary>
    WildcardSubscriptionAvailable = 0x28,

    /// <summary>
    /// Subscription Identifier Available.
    /// </summary>
    SubscriptionIdentifierAvailable = 0x29,

    /// <summary>
    /// Shared Subscription Available.
    /// </summary>
    SharedSubscriptionAvailable = 0x2A,
}
