/*
 * Copyright 2023-present HiveMQ and the HiveMQ Community
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
/// MQTT version 5 properties as defined in
/// https://docs.oasis-open.org/mqtt/mqtt/v5.0/os/mqtt-v5.0-os.html#_Toc3901027.
/// </summary>
internal class MQTT5LastWillProperties
{
    public MQTT5LastWillProperties() => this.UserProperties = new Dictionary<string, string>();

    public UInt32? WillDelayInterval { get; set; }

    public byte? PayloadFormatIndicator { get; set; }

    public UInt32? MessageExpiryInterval { get; set; }

    public string? ContentType { get; set; }

    public string? ResponseTopic { get; set; }

    public byte[]? CorrelationData { get; set; }

    /// <summary>
    /// Gets or sets a Dictionary containing the User Properties to be sent with the Last Will and Testament message.
    /// </summary>
    public Dictionary<string, string> UserProperties { get; set; }
}
