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
/// Options for the Sparkplug B Host Application.
/// </summary>
public class SparkplugHostApplicationOptions
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SparkplugHostApplicationOptions"/> class.
    /// </summary>
    public SparkplugHostApplicationOptions()
    {
        this.SparkplugNamespace = Topics.SparkplugTopic.DefaultNamespace;
        this.SparkplugTopicFilter = $"{Topics.SparkplugTopic.DefaultNamespace}/#";
        this.UseStateMessages = true;
    }

    /// <summary>
    /// Gets or sets the Host Application ID. Used in STATE topic (spBv1.0/STATE/{hostApplicationId}) and for identification.
    /// Must be set when using STATE messages or LWT.
    /// </summary>
    public string? HostApplicationId { get; set; }

    /// <summary>
    /// Gets or sets the Sparkplug namespace (e.g. "spBv1.0"). Used for subscriptions and STATE topic.
    /// </summary>
    public string SparkplugNamespace { get; set; }

    /// <summary>
    /// Gets or sets the topic filter to subscribe to for Sparkplug messages. Default is "spBv1.0/#".
    /// Can be scoped e.g. "spBv1.0/myGroup/#" to receive only a specific group.
    /// </summary>
    public string SparkplugTopicFilter { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to publish STATE (birth) on connect and STATE (death) on disconnect (or via LWT).
    /// </summary>
    public bool UseStateMessages { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to set a Last Will and Testament that publishes STATE (offline) if the Host disconnects ungracefully.
    /// Ignored if <see cref="UseStateMessages"/> is false.
    /// </summary>
    public bool UseStateLwt { get; set; } = true;

    /// <summary>
    /// Validates the options and throws if invalid.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when required options are missing or invalid.</exception>
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(this.SparkplugTopicFilter))
        {
            throw new InvalidOperationException("SparkplugTopicFilter cannot be null or empty.");
        }

        if (this.UseStateMessages && string.IsNullOrWhiteSpace(this.HostApplicationId))
        {
            throw new InvalidOperationException("HostApplicationId must be set when UseStateMessages is true.");
        }
    }
}
