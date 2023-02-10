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
/// Defines the Retain Handling options for a subscription as defined in
/// the <see href="https://docs.oasis-open.org/mqtt/mqtt/v5.0/os/mqtt-v5.0-os.html#_Toc3901169">
/// MQTT Specification</see>.
/// </summary>
public enum RetainHandling
{
    /// <summary>
    /// Send retained messages at the time of the subscribe.
    /// </summary>
    SendAtSubscribe = 0x0,

    /// <summary>
    /// Send retained messages at subscribe only if the subscription does not currently exist.
    /// </summary>
    SendAtSubscribeIfNewSubscription = 0x1,

    /// <summary>
    /// Do not send retained messages at the time of the subscribe.
    /// </summary>
    DoNotSendAtSubscribe = 0x2,
}
