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
namespace HiveMQtt.Client.Internal;

/// <summary>
/// The state of the MQTT connection.
/// </summary>
internal enum ConnectState
{
    /// <summary>
    /// The connection is being established.
    /// </summary>
    Connecting = 0x00,

    /// <summary>
    /// The connection is established.
    /// </summary>
    Connected = 0x01,

    /// <summary>
    /// The connection is being disconnected.
    /// </summary>
    Disconnecting = 0x02,

    /// <summary>
    /// The connection is disconnected.
    /// </summary>
    Disconnected = 0x03,
}
