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
using System.Text.Json;

/// <summary>
/// Sparkplug 3.0 STATE message payload. STATE messages use JSON format:
/// <c>{ "online": true|false, "timestamp": &lt;milliseconds since epoch&gt; }</c>.
/// </summary>
public sealed class SparkplugStatePayload
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
    };

    /// <summary>
    /// Tries to decode a STATE message payload from UTF-8 JSON bytes (Sparkplug 3.0 format).
    /// </summary>
    /// <param name="data">The raw payload bytes (UTF-8 JSON).</param>
    /// <param name="payload">When successful, the parsed STATE payload; otherwise null.</param>
    /// <returns>True if decoding succeeded; otherwise false.</returns>
    public static bool TryDecode(byte[]? data, out SparkplugStatePayload? payload)
    {
        payload = null;
        if (data is null || data.Length == 0)
        {
            return false;
        }

        try
        {
            var obj = JsonSerializer.Deserialize<JsonPayload>(data, JsonOptions);
            if (obj is null)
            {
                return false;
            }

            payload = new SparkplugStatePayload(obj.Online, obj.Timestamp);
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }

    /// <summary>
    /// Creates a STATE payload for "online" (birth).
    /// </summary>
    /// <param name="timestampMs">Timestamp in milliseconds since Unix epoch. If null, uses current UTC time.</param>
    /// <returns>A new STATE payload with online=true.</returns>
    public static SparkplugStatePayload CreateOnline(long? timestampMs = null)
    {
        var ts = timestampMs ?? DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        return new SparkplugStatePayload(online: true, ts);
    }

    /// <summary>
    /// Creates a STATE payload for "offline" (death/LWT). Use timestamp 0 for LWT when the exact time is unknown.
    /// </summary>
    /// <param name="timestampMs">Timestamp in milliseconds since Unix epoch (0 for LWT).</param>
    /// <returns>A new STATE payload with online=false.</returns>
    public static SparkplugStatePayload CreateOffline(long timestampMs = 0) =>
        new SparkplugStatePayload(online: false, timestampMs);

    /// <summary>
    /// Initializes a new instance of the <see cref="SparkplugStatePayload"/> class.
    /// </summary>
    /// <param name="online">True if the Primary Host Application is online; false if offline.</param>
    /// <param name="timestamp">Timestamp in milliseconds since Unix epoch.</param>
    public SparkplugStatePayload(bool online, long timestamp)
    {
        this.Online = online;
        this.Timestamp = timestamp;
    }

    /// <summary>
    /// Gets a value indicating whether the Primary Host Application is online.
    /// </summary>
    public bool Online { get; }

    /// <summary>
    /// Gets the timestamp in milliseconds since Unix epoch.
    /// </summary>
    public long Timestamp { get; }

    /// <summary>
    /// Encodes this payload to UTF-8 JSON bytes for publishing (Sparkplug 3.0 STATE format).
    /// </summary>
    /// <returns>The JSON payload as UTF-8 bytes.</returns>
    public byte[] ToUtf8Bytes()
    {
        var obj = new JsonPayload { Online = this.Online, Timestamp = this.Timestamp };
        return JsonSerializer.SerializeToUtf8Bytes(obj, JsonOptions);
    }

    private sealed class JsonPayload
    {
        public bool Online { get; set; }

        public long Timestamp { get; set; }
    }
}
