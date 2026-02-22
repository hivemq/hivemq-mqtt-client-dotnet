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

namespace HiveMQtt.Sparkplug.EdgeNode;

using System;

/// <summary>
/// Options for the Sparkplug B Edge Node.
/// </summary>
public class SparkplugEdgeNodeOptions
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SparkplugEdgeNodeOptions"/> class.
    /// </summary>
    public SparkplugEdgeNodeOptions()
    {
        this.SparkplugNamespace = Topics.SparkplugTopic.DefaultNamespace;
    }

    /// <summary>
    /// Gets or sets the Group ID. Identifies the group this Edge Node belongs to.
    /// </summary>
    public string? GroupId { get; set; }

    /// <summary>
    /// Gets or sets the Edge Node ID. Uniquely identifies this Edge Node within the group.
    /// </summary>
    public string? EdgeNodeId { get; set; }

    /// <summary>
    /// Gets or sets the Sparkplug namespace (e.g. "spBv1.0"). Used for topic building and subscriptions.
    /// </summary>
    public string SparkplugNamespace { get; set; }

    /// <summary>
    /// Validates the options and throws if invalid.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when required options are missing or invalid.</exception>
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(this.GroupId))
        {
            throw new InvalidOperationException("GroupId cannot be null or empty.");
        }

        if (string.IsNullOrWhiteSpace(this.EdgeNodeId))
        {
            throw new InvalidOperationException("EdgeNodeId cannot be null or empty.");
        }

        if (string.IsNullOrWhiteSpace(this.SparkplugNamespace))
        {
            throw new InvalidOperationException("SparkplugNamespace cannot be null or empty.");
        }
    }
}
