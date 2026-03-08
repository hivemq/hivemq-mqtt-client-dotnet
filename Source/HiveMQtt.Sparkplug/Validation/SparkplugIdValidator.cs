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

namespace HiveMQtt.Sparkplug.Validation;

using System;

/// <summary>
/// Validates Sparkplug B identifiers (Group ID, Edge Node ID, Device ID, Host Application ID) per spec.
/// IDs appear in MQTT topic segments and must not contain MQTT wildcards or null characters when using strict mode.
/// </summary>
public static class SparkplugIdValidator
{
    /// <summary>
    /// Maximum allowed length for an identifier (aligns with practical MQTT topic segment length).
    /// </summary>
    public const int MaxIdLength = 65535;

    /// <summary>
    /// Validates a Group ID. When strict is true, rejects null, empty, whitespace, and characters #, +, and null (U+0000).
    /// </summary>
    /// <param name="groupId">The Group ID to validate.</param>
    /// <param name="parameterName">The parameter name for exception messages (e.g. nameof(groupId)).</param>
    /// <param name="strict">When true, rejects identifiers containing MQTT wildcards (#, +) or null character.</param>
    /// <exception cref="ArgumentException">Thrown when the Group ID is invalid.</exception>
    public static void ValidateGroupId(string? groupId, string parameterName = "groupId", bool strict = false) =>
        ValidateIdentifier(groupId, parameterName, "Group ID", strict);

    /// <summary>
    /// Validates an Edge Node ID. When strict is true, rejects null, empty, whitespace, and characters #, +, and null (U+0000).
    /// </summary>
    /// <param name="edgeNodeId">The Edge Node ID to validate.</param>
    /// <param name="parameterName">The parameter name for exception messages.</param>
    /// <param name="strict">When true, rejects identifiers containing MQTT wildcards (#, +) or null character.</param>
    /// <exception cref="ArgumentException">Thrown when the Edge Node ID is invalid.</exception>
    public static void ValidateEdgeNodeId(string? edgeNodeId, string parameterName = "edgeNodeId", bool strict = false) =>
        ValidateIdentifier(edgeNodeId, parameterName, "Edge Node ID", strict);

    /// <summary>
    /// Validates a Device ID. When strict is true, rejects null, empty, whitespace, and characters #, +, and null (U+0000).
    /// </summary>
    /// <param name="deviceId">The Device ID to validate.</param>
    /// <param name="parameterName">The parameter name for exception messages.</param>
    /// <param name="strict">When true, rejects identifiers containing MQTT wildcards (#, +) or null character.</param>
    /// <exception cref="ArgumentException">Thrown when the Device ID is invalid.</exception>
    public static void ValidateDeviceId(string? deviceId, string parameterName = "deviceId", bool strict = false) =>
        ValidateIdentifier(deviceId, parameterName, "Device ID", strict);

    /// <summary>
    /// Validates a Host Application ID (used in STATE topic). When strict is true, rejects null, empty, whitespace, and characters #, +, and null (U+0000).
    /// </summary>
    /// <param name="hostApplicationId">The Host Application ID to validate.</param>
    /// <param name="parameterName">The parameter name for exception messages.</param>
    /// <param name="strict">When true, rejects identifiers containing MQTT wildcards (#, +) or null character.</param>
    /// <exception cref="ArgumentException">Thrown when the Host Application ID is invalid.</exception>
    public static void ValidateHostApplicationId(string? hostApplicationId, string parameterName = "hostApplicationId", bool strict = false) =>
        ValidateIdentifier(hostApplicationId, parameterName, "Host Application ID", strict);

    /// <summary>
    /// Validates a generic Sparkplug identifier (non-null, non-whitespace, optional length and strict character check).
    /// </summary>
    /// <param name="value">The value to validate.</param>
    /// <param name="parameterName">The parameter name for exception messages.</param>
    /// <param name="displayName">Display name for error messages (e.g. "Group ID").</param>
    /// <param name="strict">When true, rejects identifiers containing #, +, or null character.</param>
    /// <exception cref="ArgumentException">Thrown when the identifier is invalid.</exception>
    public static void ValidateIdentifier(string? value, string parameterName, string displayName, bool strict = false)
    {
        if (value is null)
        {
            throw new ArgumentException($"{displayName} cannot be null.", parameterName);
        }

        if (value.Length == 0)
        {
            throw new ArgumentException($"{displayName} cannot be empty.", parameterName);
        }

        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException($"{displayName} cannot be whitespace only.", parameterName);
        }

        if (value.Length > MaxIdLength)
        {
            throw new ArgumentException($"{displayName} cannot exceed {MaxIdLength} characters.", parameterName);
        }

        if (value.Contains('\0'))
        {
            throw new ArgumentException($"{displayName} cannot contain the null character (U+0000).", parameterName);
        }

        if (strict)
        {
            if (value.Contains('#'))
            {
                throw new ArgumentException($"{displayName} cannot contain '#' (multilevel wildcard) when strict validation is enabled.", parameterName);
            }

            if (value.Contains('+'))
            {
                throw new ArgumentException($"{displayName} cannot contain '+' (single-level wildcard) when strict validation is enabled.", parameterName);
            }
        }
    }
}
