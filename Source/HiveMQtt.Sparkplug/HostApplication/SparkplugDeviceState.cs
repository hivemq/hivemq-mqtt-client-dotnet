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
/// Represents the online/offline state of a Sparkplug Device (under an Edge Node).
/// </summary>
public sealed class SparkplugDeviceState
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SparkplugDeviceState"/> class.
    /// </summary>
    /// <param name="groupId">The group ID.</param>
    /// <param name="edgeNodeId">The Edge Node ID.</param>
    /// <param name="deviceId">The Device ID.</param>
    /// <param name="isOnline">Whether the device is considered online.</param>
    /// <param name="lastSeenUtc">When the device was last seen (birth or data).</param>
    public SparkplugDeviceState(string groupId, string edgeNodeId, string deviceId, bool isOnline, DateTimeOffset? lastSeenUtc)
    {
        this.GroupId = groupId ?? throw new ArgumentNullException(nameof(groupId));
        this.EdgeNodeId = edgeNodeId ?? throw new ArgumentNullException(nameof(edgeNodeId));
        this.DeviceId = deviceId ?? throw new ArgumentNullException(nameof(deviceId));
        this.IsOnline = isOnline;
        this.LastSeenUtc = lastSeenUtc;
    }

    /// <summary>
    /// Gets the group ID.
    /// </summary>
    public string GroupId { get; }

    /// <summary>
    /// Gets the Edge Node ID.
    /// </summary>
    public string EdgeNodeId { get; }

    /// <summary>
    /// Gets the Device ID.
    /// </summary>
    public string DeviceId { get; }

    /// <summary>
    /// Gets a value indicating whether the device is currently considered online (has sent DBIRTH and not yet DDEATH).
    /// </summary>
    public bool IsOnline { get; }

    /// <summary>
    /// Gets the last time this device was seen (DBIRTH or DDATA), or null if never seen.
    /// </summary>
    public DateTimeOffset? LastSeenUtc { get; }
}
