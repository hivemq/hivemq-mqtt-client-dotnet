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
/// Represents the online/offline state of a Sparkplug Edge Node.
/// </summary>
public sealed class SparkplugNodeState
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SparkplugNodeState"/> class.
    /// </summary>
    /// <param name="groupId">The group ID.</param>
    /// <param name="edgeNodeId">The Edge Node ID.</param>
    /// <param name="isOnline">Whether the node is considered online.</param>
    /// <param name="lastSeenUtc">When the node was last seen (birth or data).</param>
    public SparkplugNodeState(string groupId, string edgeNodeId, bool isOnline, DateTimeOffset? lastSeenUtc)
    {
        this.GroupId = groupId ?? throw new ArgumentNullException(nameof(groupId));
        this.EdgeNodeId = edgeNodeId ?? throw new ArgumentNullException(nameof(edgeNodeId));
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
    /// Gets a value indicating whether the node is currently considered online (has sent NBIRTH and not yet NDEATH).
    /// </summary>
    public bool IsOnline { get; }

    /// <summary>
    /// Gets the last time this node was seen (NBIRTH or NDATA), or null if never seen.
    /// </summary>
    public DateTimeOffset? LastSeenUtc { get; }
}
