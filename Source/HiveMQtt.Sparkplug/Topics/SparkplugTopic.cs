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

namespace HiveMQtt.Sparkplug.Topics;

using System;
using System.Diagnostics.CodeAnalysis;

/// <summary>
/// Represents a Sparkplug B topic and provides methods for building and parsing topics.
/// <para>
/// Sparkplug topic format: {namespace}/{group_id}/{message_type}/{edge_node_id}[/{device_id}].
/// </para>
/// </summary>
public sealed class SparkplugTopic
{
    /// <summary>
    /// The default Sparkplug B namespace.
    /// </summary>
    public const string DefaultNamespace = "spBv1.0";

    /// <summary>
    /// Initializes a new instance of the <see cref="SparkplugTopic"/> class.
    /// </summary>
    /// <param name="namespace">The Sparkplug namespace (e.g., "spBv1.0").</param>
    /// <param name="groupId">The group ID.</param>
    /// <param name="messageType">The message type.</param>
    /// <param name="edgeNodeId">The Edge Node ID.</param>
    /// <param name="deviceId">The Device ID (optional, null for node-level messages).</param>
    /// <exception cref="ArgumentException">Thrown when required parameters are null or empty.</exception>
    public SparkplugTopic(
        string @namespace,
        string groupId,
        SparkplugMessageType messageType,
        string edgeNodeId,
        string? deviceId = null)
    {
        if (string.IsNullOrWhiteSpace(@namespace))
        {
            throw new ArgumentException("Namespace cannot be null or empty.", nameof(@namespace));
        }

        if (string.IsNullOrWhiteSpace(groupId))
        {
            throw new ArgumentException("Group ID cannot be null or empty.", nameof(groupId));
        }

        if (string.IsNullOrWhiteSpace(edgeNodeId))
        {
            throw new ArgumentException("Edge Node ID cannot be null or empty.", nameof(edgeNodeId));
        }

        this.Namespace = @namespace;
        this.GroupId = groupId;
        this.MessageType = messageType;
        this.EdgeNodeId = edgeNodeId;
        this.DeviceId = string.IsNullOrWhiteSpace(deviceId) ? null : deviceId;

        this.ValidateMessageTypeAndDeviceId();
    }

    /// <summary>
    /// Gets the namespace portion of the topic (e.g., "spBv1.0").
    /// </summary>
    public string Namespace { get; }

    /// <summary>
    /// Gets the group ID portion of the topic.
    /// </summary>
    public string GroupId { get; }

    /// <summary>
    /// Gets the message type (NBIRTH, NDEATH, NDATA, etc.).
    /// </summary>
    public SparkplugMessageType MessageType { get; }

    /// <summary>
    /// Gets the Edge Node ID portion of the topic.
    /// </summary>
    public string EdgeNodeId { get; }

    /// <summary>
    /// Gets the Device ID portion of the topic. Null for node-level messages.
    /// </summary>
    public string? DeviceId { get; }

    /// <summary>
    /// Gets a value indicating whether this topic includes a Device ID (is a device-level message).
    /// </summary>
    [MemberNotNullWhen(true, nameof(DeviceId))]
    public bool IsDeviceMessage => this.DeviceId is not null;

    /// <summary>
    /// Gets a value indicating whether this topic is a node-level message (no Device ID).
    /// </summary>
    public bool IsNodeMessage => this.DeviceId is null;

    /// <summary>
    /// Parses a topic string into a <see cref="SparkplugTopic"/> instance.
    /// </summary>
    /// <param name="topic">The topic string to parse.</param>
    /// <returns>A <see cref="SparkplugTopic"/> instance.</returns>
    /// <exception cref="ArgumentException">Thrown when the topic is null or empty.</exception>
    /// <exception cref="FormatException">Thrown when the topic format is invalid.</exception>
    public static SparkplugTopic Parse(string topic)
    {
        if (string.IsNullOrWhiteSpace(topic))
        {
            throw new ArgumentException("Topic cannot be null or empty.", nameof(topic));
        }

        var parts = topic.Split('/');

        if (parts.Length < 4)
        {
            throw new FormatException($"Invalid Sparkplug topic format. Expected at least 4 parts, got {parts.Length}. Topic: '{topic}'");
        }

        if (parts.Length > 5)
        {
            throw new FormatException($"Invalid Sparkplug topic format. Expected at most 5 parts, got {parts.Length}. Topic: '{topic}'");
        }

        var @namespace = parts[0];
        var groupId = parts[1];
        var messageTypeStr = parts[2];
        var edgeNodeId = parts[3];
        var deviceId = parts.Length == 5 ? parts[4] : null;

        if (!Enum.TryParse<SparkplugMessageType>(messageTypeStr, ignoreCase: false, out var messageType))
        {
            throw new FormatException($"Invalid Sparkplug message type: '{messageTypeStr}'. Valid types are: {string.Join(", ", Enum.GetNames(typeof(SparkplugMessageType)))}");
        }

        return new SparkplugTopic(@namespace, groupId, messageType, edgeNodeId, deviceId);
    }

    /// <summary>
    /// Attempts to parse a topic string into a <see cref="SparkplugTopic"/> instance.
    /// </summary>
    /// <param name="topic">The topic string to parse.</param>
    /// <param name="result">When successful, contains the parsed topic; otherwise, null.</param>
    /// <returns>True if parsing succeeded; otherwise, false.</returns>
    public static bool TryParse(string topic, [NotNullWhen(true)] out SparkplugTopic? result)
    {
        result = null;

        if (string.IsNullOrWhiteSpace(topic))
        {
            return false;
        }

        var parts = topic.Split('/');

        if (parts.Length < 4 || parts.Length > 5)
        {
            return false;
        }

        if (!Enum.TryParse<SparkplugMessageType>(parts[2], ignoreCase: false, out var messageType))
        {
            return false;
        }

        try
        {
            result = new SparkplugTopic(
                parts[0],
                parts[1],
                messageType,
                parts[3],
                parts.Length == 5 ? parts[4] : null);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Creates a topic for a node birth message.
    /// </summary>
    /// <param name="groupId">The group ID.</param>
    /// <param name="edgeNodeId">The Edge Node ID.</param>
    /// <param name="namespace">The namespace (defaults to spBv1.0).</param>
    /// <returns>A new SparkplugTopic instance.</returns>
    public static SparkplugTopic NodeBirth(string groupId, string edgeNodeId, string @namespace = DefaultNamespace) =>
        new(@namespace, groupId, SparkplugMessageType.NBIRTH, edgeNodeId);

    /// <summary>
    /// Creates a topic for a node death message.
    /// </summary>
    /// <param name="groupId">The group ID.</param>
    /// <param name="edgeNodeId">The Edge Node ID.</param>
    /// <param name="namespace">The namespace (defaults to spBv1.0).</param>
    /// <returns>A new SparkplugTopic instance.</returns>
    public static SparkplugTopic NodeDeath(string groupId, string edgeNodeId, string @namespace = DefaultNamespace) =>
        new(@namespace, groupId, SparkplugMessageType.NDEATH, edgeNodeId);

    /// <summary>
    /// Creates a topic for a node data message.
    /// </summary>
    /// <param name="groupId">The group ID.</param>
    /// <param name="edgeNodeId">The Edge Node ID.</param>
    /// <param name="namespace">The namespace (defaults to spBv1.0).</param>
    /// <returns>A new SparkplugTopic instance.</returns>
    public static SparkplugTopic NodeData(string groupId, string edgeNodeId, string @namespace = DefaultNamespace) =>
        new(@namespace, groupId, SparkplugMessageType.NDATA, edgeNodeId);

    /// <summary>
    /// Creates a topic for a node command message.
    /// </summary>
    /// <param name="groupId">The group ID.</param>
    /// <param name="edgeNodeId">The Edge Node ID.</param>
    /// <param name="namespace">The namespace (defaults to spBv1.0).</param>
    /// <returns>A new SparkplugTopic instance.</returns>
    public static SparkplugTopic NodeCommand(string groupId, string edgeNodeId, string @namespace = DefaultNamespace) =>
        new(@namespace, groupId, SparkplugMessageType.NCMD, edgeNodeId);

    /// <summary>
    /// Creates a topic for a device birth message.
    /// </summary>
    /// <param name="groupId">The group ID.</param>
    /// <param name="edgeNodeId">The Edge Node ID.</param>
    /// <param name="deviceId">The Device ID.</param>
    /// <param name="namespace">The namespace (defaults to spBv1.0).</param>
    /// <returns>A new SparkplugTopic instance.</returns>
    public static SparkplugTopic DeviceBirth(string groupId, string edgeNodeId, string deviceId, string @namespace = DefaultNamespace) =>
        new(@namespace, groupId, SparkplugMessageType.DBIRTH, edgeNodeId, deviceId);

    /// <summary>
    /// Creates a topic for a device death message.
    /// </summary>
    /// <param name="groupId">The group ID.</param>
    /// <param name="edgeNodeId">The Edge Node ID.</param>
    /// <param name="deviceId">The Device ID.</param>
    /// <param name="namespace">The namespace (defaults to spBv1.0).</param>
    /// <returns>A new SparkplugTopic instance.</returns>
    public static SparkplugTopic DeviceDeath(string groupId, string edgeNodeId, string deviceId, string @namespace = DefaultNamespace) =>
        new(@namespace, groupId, SparkplugMessageType.DDEATH, edgeNodeId, deviceId);

    /// <summary>
    /// Creates a topic for a device data message.
    /// </summary>
    /// <param name="groupId">The group ID.</param>
    /// <param name="edgeNodeId">The Edge Node ID.</param>
    /// <param name="deviceId">The Device ID.</param>
    /// <param name="namespace">The namespace (defaults to spBv1.0).</param>
    /// <returns>A new SparkplugTopic instance.</returns>
    public static SparkplugTopic DeviceData(string groupId, string edgeNodeId, string deviceId, string @namespace = DefaultNamespace) =>
        new(@namespace, groupId, SparkplugMessageType.DDATA, edgeNodeId, deviceId);

    /// <summary>
    /// Creates a topic for a device command message.
    /// </summary>
    /// <param name="groupId">The group ID.</param>
    /// <param name="edgeNodeId">The Edge Node ID.</param>
    /// <param name="deviceId">The Device ID.</param>
    /// <param name="namespace">The namespace (defaults to spBv1.0).</param>
    /// <returns>A new SparkplugTopic instance.</returns>
    public static SparkplugTopic DeviceCommand(string groupId, string edgeNodeId, string deviceId, string @namespace = DefaultNamespace) =>
        new(@namespace, groupId, SparkplugMessageType.DCMD, edgeNodeId, deviceId);

    /// <summary>
    /// Builds the topic string from this instance.
    /// </summary>
    /// <returns>The complete topic string.</returns>
    public string Build()
    {
        if (this.DeviceId is not null)
        {
            return $"{this.Namespace}/{this.GroupId}/{this.MessageType}/{this.EdgeNodeId}/{this.DeviceId}";
        }

        return $"{this.Namespace}/{this.GroupId}/{this.MessageType}/{this.EdgeNodeId}";
    }

    /// <inheritdoc/>
    public override string ToString() => this.Build();

    /// <inheritdoc/>
    public override bool Equals(object? obj) =>
        obj is SparkplugTopic other &&
        this.Namespace == other.Namespace &&
        this.GroupId == other.GroupId &&
        this.MessageType == other.MessageType &&
        this.EdgeNodeId == other.EdgeNodeId &&
        this.DeviceId == other.DeviceId;

    /// <inheritdoc/>
    public override int GetHashCode() =>
        HashCode.Combine(this.Namespace, this.GroupId, this.MessageType, this.EdgeNodeId, this.DeviceId);

    private void ValidateMessageTypeAndDeviceId()
    {
        var requiresDeviceId = this.MessageType is SparkplugMessageType.DBIRTH
            or SparkplugMessageType.DDEATH
            or SparkplugMessageType.DDATA
            or SparkplugMessageType.DCMD;

        var forbidsDeviceId = this.MessageType is SparkplugMessageType.NBIRTH
            or SparkplugMessageType.NDEATH
            or SparkplugMessageType.NDATA
            or SparkplugMessageType.NCMD
            or SparkplugMessageType.STATE;

        if (requiresDeviceId && this.DeviceId is null)
        {
            throw new ArgumentException($"Device ID is required for {this.MessageType} messages.", nameof(this.DeviceId));
        }

        if (forbidsDeviceId && this.DeviceId is not null)
        {
            throw new ArgumentException($"Device ID must not be specified for {this.MessageType} messages.", nameof(this.DeviceId));
        }
    }
}
