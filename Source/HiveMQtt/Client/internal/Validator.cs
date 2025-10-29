/*
 * Copyright 2024-present HiveMQ and the HiveMQ Community
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
namespace HiveMQtt.Client.Internal;

using System.Text.RegularExpressions;

using HiveMQtt.Client.Exceptions;

public class Validator
{
    /// <summary>
    /// Validates the client identifier according to the MQTT v5.0 specification.
    /// </summary>
    /// <param name="clientId">The client identifier to validate.</param>
    public static void ValidateClientId(string clientId)
    {
        ArgumentNullException.ThrowIfNull(clientId);

        if (clientId.Length > 65535)
        {
            throw new HiveMQttClientException("Client identifier must not be longer than 65535 characters.");
        }

        if (clientId.Length == 0)
        {
            throw new HiveMQttClientException("Client identifier must not be empty.");
        }

        // Regular expression to match any character that is NOT in the specified set
        // We can't use GeneratedRegexAttribute because it's not available in .net 6.0
#pragma warning disable SYSLIB1045 // Use GeneratedRegexAttribute - not available in .NET 6
        var regex = new Regex("[^0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ]");
#pragma warning restore SYSLIB1045

        // Check if the input string contains any character that does not match the pattern
        if (regex.IsMatch(clientId))
        {
            throw new HiveMQttClientException("MQTT Client IDs can only contain: 0-9, a-z, A-Z");
        }
    }

    /// <summary>
    /// Validates a topic name string according to the MQTT v5.0 specification.
    /// </summary>
    /// <param name="topic">The topic name string to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when the topic name is null.</exception>
    /// <exception cref="HiveMQttClientException">Thrown when the topic name is longer than 65535 characters or empty.</exception>
    /// <exception cref="HiveMQttClientException">Thrown when the topic name contains any wildcard characters.</exception>
    /// <exception cref="HiveMQttClientException">Thrown when the topic name contains any null characters.</exception>
    public static void ValidateTopicName(string topic)
    {
        ArgumentNullException.ThrowIfNull(topic);

        if (topic.Length > 65535)
        {
            throw new HiveMQttClientException("A topic string must not be longer than 65535 characters.");
        }

        if (topic.Length == 0)
        {
            throw new HiveMQttClientException("A topic string must not be empty.");
        }

        // Topic names cannot contain wildcards (only TopicFilters can)
        if (topic.Contains('+') || topic.Contains('#'))
        {
            throw new HiveMQttClientException("A topic name must not contain any wildcard characters.");
        }

        if (topic.Contains('\0'))
        {
            throw new HiveMQttClientException("A topic name cannot contain any null characters.");
        }
    }

    /// <summary>
    /// Validates a topic filter string according to the MQTT v5.0 specification.
    /// </summary>
    /// <param name="topic">The topic filter string to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when the topic filter is null.</exception>
    /// <exception cref="HiveMQttClientException">Thrown when the topic filter is longer than 65535 characters or empty.</exception>
    /// <exception cref="HiveMQttClientException">Thrown when the topic filter contains any null characters.</exception>
    /// <exception cref="ArgumentException">Thrown when the topic filter contains invalid wildcard usage.</exception>
    public static void ValidateTopicFilter(string topic)
    {
        ArgumentNullException.ThrowIfNull(topic);

        if (topic.Length > 65535)
        {
            throw new HiveMQttClientException("A topic string must not be longer than 65535 characters.");
        }

        if (topic.Length == 0)
        {
            throw new HiveMQttClientException("A topic string must not be empty.");
        }

        if (topic.Contains('\0'))
        {
            throw new HiveMQttClientException("A topic name cannot contain any null characters.");
        }

        // Check for invalid usage of '#' wildcard
        if (topic.Contains('#'))
        {
            if (topic.IndexOf('#') != topic.Length - 1)
            {
                throw new ArgumentException("The '#' wildcard must be the last character in the topic filter.");
            }

            if (topic.Length > 1 && topic[^2] != '/')
            {
                throw new ArgumentException("The '#' wildcard must be preceded by a topic level separator or be the only character.");
            }
        }

        // Check for invalid usage of '+' wildcard
        var segments = topic.Split('/');
        foreach (var segment in segments)
        {
            if (segment.Contains('+') && segment != "+")
            {
                throw new ArgumentException("The '+' wildcard must stand alone and cannot be part of another string.");
            }
        }
    }
}
