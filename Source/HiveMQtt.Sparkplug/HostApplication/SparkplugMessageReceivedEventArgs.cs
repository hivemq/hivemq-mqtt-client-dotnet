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
using HiveMQtt.Sparkplug.Protobuf;
using HiveMQtt.Sparkplug.Topics;

/// <summary>
/// Event arguments for Sparkplug message received events (NBIRTH, NDEATH, NDATA, DBIRTH, DDEATH, DDATA, STATE).
/// </summary>
public class SparkplugMessageReceivedEventArgs : EventArgs
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SparkplugMessageReceivedEventArgs"/> class.
    /// </summary>
    /// <param name="topic">The parsed Sparkplug topic.</param>
    /// <param name="payload">The decoded Sparkplug B payload.</param>
    /// <param name="rawTopic">The raw MQTT topic string.</param>
    public SparkplugMessageReceivedEventArgs(SparkplugTopic topic, Payload payload, string rawTopic)
    {
        this.Topic = topic ?? throw new ArgumentNullException(nameof(topic));
        this.Payload = payload ?? throw new ArgumentNullException(nameof(payload));
        this.RawTopic = rawTopic ?? throw new ArgumentNullException(nameof(rawTopic));
        this.StatePayload = null;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SparkplugMessageReceivedEventArgs"/> class for STATE messages (Sparkplug 3.0 JSON payload).
    /// </summary>
    /// <param name="topic">The parsed Sparkplug topic (MessageType must be STATE).</param>
    /// <param name="rawTopic">The raw MQTT topic string.</param>
    /// <param name="statePayload">The decoded STATE payload (online, timestamp).</param>
    public SparkplugMessageReceivedEventArgs(SparkplugTopic topic, string rawTopic, SparkplugStatePayload statePayload)
    {
        this.Topic = topic ?? throw new ArgumentNullException(nameof(topic));
        this.Payload = new Payload();
        this.RawTopic = rawTopic ?? throw new ArgumentNullException(nameof(rawTopic));
        this.StatePayload = statePayload ?? throw new ArgumentNullException(nameof(statePayload));
    }

    /// <summary>
    /// Gets the parsed Sparkplug topic.
    /// </summary>
    public SparkplugTopic Topic { get; }

    /// <summary>
    /// Gets the decoded Sparkplug B payload. For STATE messages this is an empty payload; use <see cref="StatePayload"/> instead.
    /// </summary>
    public Payload Payload { get; }

    /// <summary>
    /// Gets the decoded STATE message payload (Sparkplug 3.0 JSON). Non-null only when <see cref="MessageType"/> is <see cref="Topics.SparkplugMessageType.STATE"/>.
    /// For STATE topics, <see cref="Topic"/>.<see cref="Topics.SparkplugTopic.EdgeNodeId"/> is the Primary Host Application ID.
    /// </summary>
    public SparkplugStatePayload? StatePayload { get; }

    /// <summary>
    /// Gets the raw MQTT topic string.
    /// </summary>
    public string RawTopic { get; }

    /// <summary>
    /// Gets the Sparkplug message type (convenience for Topic.MessageType).
    /// </summary>
    public SparkplugMessageType MessageType => this.Topic.MessageType;

    /// <summary>
    /// Gets the group ID (convenience for Topic.GroupId).
    /// </summary>
    public string GroupId => this.Topic.GroupId;

    /// <summary>
    /// Gets the Edge Node ID (convenience for Topic.EdgeNodeId).
    /// </summary>
    public string EdgeNodeId => this.Topic.EdgeNodeId;

    /// <summary>
    /// Gets the Device ID if this is a device-level message; otherwise null.
    /// </summary>
    public string? DeviceId => this.Topic.DeviceId;
}
