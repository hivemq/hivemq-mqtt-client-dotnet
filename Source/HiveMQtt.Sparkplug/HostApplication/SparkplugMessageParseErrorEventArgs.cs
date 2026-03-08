// Copyright 2026-present HiveMQ and the HiveMQ Community
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

namespace HiveMQtt.Sparkplug.HostApplication;

using System;

/// <summary>
/// Event arguments when a Sparkplug message could not be parsed or is unsupported.
/// </summary>
public class SparkplugMessageParseErrorEventArgs : EventArgs
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SparkplugMessageParseErrorEventArgs"/> class.
    /// </summary>
    /// <param name="rawTopic">The raw MQTT topic.</param>
    /// <param name="payload">The raw payload, if any.</param>
    /// <param name="reason">The reason the message could not be processed.</param>
    public SparkplugMessageParseErrorEventArgs(string rawTopic, byte[]? payload, string reason)
    {
        this.RawTopic = rawTopic ?? throw new ArgumentNullException(nameof(rawTopic));
        this.Payload = payload;
        this.Reason = reason ?? throw new ArgumentNullException(nameof(reason));
    }

    /// <summary>
    /// Gets the raw MQTT topic string.
    /// </summary>
    public string RawTopic { get; }

    /// <summary>
    /// Gets the raw payload, or null if empty or not applicable.
    /// </summary>
    public byte[]? Payload { get; }

    /// <summary>
    /// Gets the reason the message could not be processed.
    /// </summary>
    public string Reason { get; }
}
