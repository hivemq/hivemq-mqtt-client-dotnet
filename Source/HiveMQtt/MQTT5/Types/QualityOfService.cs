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
/// MQTT delivers Application Messages according to the Quality of Service (QoS) levels defined in the following sections.
/// </summary>
public enum QualityOfService
{
    /// <summary>
    /// The message is delivered according to the capabilities of the underlying network.
    /// The message arrives at the receiver either once or not at all.
    ///
    /// <para>
    /// AKA: Fire and forget.
    /// </para>
    /// </summary>
    AtMostOnceDelivery = 0x0,

    /// <summary>
    /// Ensures that the message arrives at the receiver at least once.
    /// </summary>
    AtLeastOnceDelivery = 0x1,

    /// <summary>
    /// The highest Quality of Service level, for use when neither loss nor duplication of messages are acceptable.
    /// </summary>
    ExactlyOnceDelivery = 0x2,
}
