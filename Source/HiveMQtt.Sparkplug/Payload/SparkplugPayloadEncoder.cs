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

namespace HiveMQtt.Sparkplug.Payload;

using System;
using Google.Protobuf;

/// <summary>
/// Provides methods for encoding and decoding Sparkplug B payloads.
/// </summary>
public static class SparkplugPayloadEncoder
{
    /// <summary>
    /// The maximum valid sequence number (0-255).
    /// </summary>
    public const int MaxSequenceNumber = 255;

    /// <summary>
    /// Encodes a Sparkplug B payload to a byte array.
    /// </summary>
    /// <param name="payload">The payload to encode.</param>
    /// <returns>The encoded byte array.</returns>
    /// <exception cref="ArgumentNullException">Thrown when payload is null.</exception>
    public static byte[] Encode(Protobuf.Payload payload)
    {
        ArgumentNullException.ThrowIfNull(payload);
        return payload.ToByteArray();
    }

    /// <summary>
    /// Decodes a byte array to a Sparkplug B payload.
    /// </summary>
    /// <param name="data">The byte array to decode.</param>
    /// <returns>The decoded payload.</returns>
    /// <exception cref="ArgumentNullException">Thrown when data is null.</exception>
    /// <exception cref="Google.Protobuf.InvalidProtocolBufferException">Thrown when the data is not a valid Sparkplug B payload.</exception>
    public static Protobuf.Payload Decode(byte[] data)
    {
        ArgumentNullException.ThrowIfNull(data);
        return Protobuf.Payload.Parser.ParseFrom(data);
    }

    /// <summary>
    /// Decodes a ReadOnlySpan to a Sparkplug B payload.
    /// </summary>
    /// <param name="data">The span to decode.</param>
    /// <returns>The decoded payload.</returns>
    /// <exception cref="Google.Protobuf.InvalidProtocolBufferException">Thrown when the data is not a valid Sparkplug B payload.</exception>
    public static Protobuf.Payload Decode(ReadOnlySpan<byte> data) =>
        Protobuf.Payload.Parser.ParseFrom(data);

    /// <summary>
    /// Attempts to decode a byte array to a Sparkplug B payload.
    /// </summary>
    /// <param name="data">The byte array to decode.</param>
    /// <param name="payload">When successful, contains the decoded payload; otherwise, null.</param>
    /// <returns>True if decoding succeeded; otherwise, false.</returns>
    public static bool TryDecode(byte[] data, out Protobuf.Payload? payload)
    {
        payload = null;

        if (data is not { Length: > 0 })
        {
            return false;
        }

        try
        {
            payload = Protobuf.Payload.Parser.ParseFrom(data);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Creates a new payload with the specified timestamp and sequence number.
    /// </summary>
    /// <param name="timestamp">The timestamp in milliseconds since epoch.</param>
    /// <param name="sequenceNumber">The sequence number (0-255).</param>
    /// <returns>A new payload instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when sequence number is out of range.</exception>
    public static Protobuf.Payload CreatePayload(ulong timestamp, int sequenceNumber)
    {
        if (sequenceNumber is < 0 or > MaxSequenceNumber)
        {
            throw new ArgumentOutOfRangeException(nameof(sequenceNumber), $"Sequence number must be between 0 and {MaxSequenceNumber}.");
        }

        return new Protobuf.Payload
        {
            Timestamp = timestamp,
            Seq = (ulong)sequenceNumber,
        };
    }

    /// <summary>
    /// Creates a new payload with the current timestamp and the specified sequence number.
    /// </summary>
    /// <param name="sequenceNumber">The sequence number (0-255).</param>
    /// <returns>A new payload instance.</returns>
    public static Protobuf.Payload CreatePayload(int sequenceNumber)
    {
        var timestamp = (ulong)DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        return CreatePayload(timestamp, sequenceNumber);
    }

    /// <summary>
    /// Computes the next sequence number, wrapping from 255 to 0.
    /// </summary>
    /// <param name="currentSequence">The current sequence number.</param>
    /// <returns>The next sequence number (wraps 255 -> 0).</returns>
    public static int NextSequenceNumber(int currentSequence) =>
        (currentSequence + 1) % (MaxSequenceNumber + 1);

    /// <summary>
    /// Gets the current timestamp as milliseconds since Unix epoch.
    /// </summary>
    /// <returns>The current timestamp.</returns>
    public static ulong GetCurrentTimestamp() =>
        (ulong)DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
}
