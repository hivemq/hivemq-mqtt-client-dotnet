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

public class MQTT5Property
{
    public int ID { get; set; }

    public MQTT5DataType DataType { get; set; }

    public byte? ByteValue { get; set; }

    public byte[]? TwoByteValue { get; set; }

    public byte[]? FourByteValue { get; set; }

    public byte[]? BinaryDataValue { get; set; }

    public string? UTF8EncodedStringValue { get; set; }

    public string? UTF8EncodedStringPairValue { get; set; }
}
