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
/// Representation of a Variable Byte Integer as defined in:
/// https://docs.oasis-open.org/mqtt/mqtt/v5.0/os/mqtt-v5.0-os.html#_Toc3901011.
/// </summary>
public class VariableByteInteger
{
    /// <summary>
    /// Gets or sets the value of this variable byte integer.
    /// </summary>
    public int Value { get; set; }

    /// <summary>
    /// Gets or sets the length in bytes that this variable byte integer consumes when encoded.
    /// </summary>
    public int Length { get; set; }
}
