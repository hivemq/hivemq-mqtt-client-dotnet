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
/// MQTT v5.0 AUTH Reason Codes as defined in:
/// https://docs.oasis-open.org/mqtt/mqtt/v5.0/os/mqtt-v5.0-os.html#_Toc3901220.
/// </summary>
public enum AuthReasonCode
{
    /// <summary>
    /// Authentication is successful.
    /// </summary>
    Success = 0x00,

    /// <summary>
    /// Continue the authentication with another step.
    /// </summary>
    ContinueAuthentication = 0x18,

    /// <summary>
    /// Initiate a re-authentication.
    /// </summary>
    ReAuthenticate = 0x19,
}
