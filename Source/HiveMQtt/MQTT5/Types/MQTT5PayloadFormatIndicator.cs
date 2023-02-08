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
/// MQTT v5 Payload Format Indicator.
/// </summary>
public enum MQTT5PayloadFormatIndicator
{
    /// <summary>
    /// Indicates that the Payload is unspecified bytes, which is equivalent to not sending a Payload Format Indicator.
    /// </summary>
    Unspecified = 0,

    /// <summary>
    /// Indicates that the Payload is UTF-8 Encoded Character Data. The UTF-8 data in the Payload MUST be well-formed
    /// UTF-8 as defined by the Unicode specification [Unicode] and restated in RFC 3629 [RFC3629].
    /// </summary>
    UTF8Encoded = 1,
}
