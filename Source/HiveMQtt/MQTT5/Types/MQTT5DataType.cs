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
/// MQTT v5.0 PropertyType as defined in
/// https://docs.oasis-open.org/mqtt/mqtt/v5.0/os/mqtt-v5.0-os.html#_Toc3901027.
/// </summary>
public enum MQTT5DataType
{
    /// <summary>
    /// Property is a Byte.
    /// </summary>
    Byte = 0x0,

    /// <summary>
    /// Property is a Two Byte Integer.
    /// </summary>
    TwoByteInteger = 0x1,

    /// <summary>
    /// Property is a Four Byte Integer.
    /// </summary>
    FourByteInteger = 0x2,

    /// <summary>
    /// Property is a UTF-8 encoded string.
    /// </summary>
    UTF8EncodedString = 0x3,

    /// <summary>
    /// Property is a UTF-8 encoded string pair.
    /// </summary>
    UTF8EncodedStringPair = 0x4,

    /// <summary>
    /// Property is Binary Data.
    /// </summary>
    BinaryData = 0x5,

    /// <summary>
    /// Property is Variable Byte Integer.
    /// </summary>
    VariableByteInteger = 0x6,
}
