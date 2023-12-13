/*
 * Copyright 2022-present HiveMQ and the HiveMQ Community
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
namespace HiveMQtt.Client.Options;

using HiveMQtt.Client.Exceptions;
using HiveMQtt.MQTT5.Types;

public class UnsubscribeOptions
{
    public UnsubscribeOptions()
    {
        this.Subscriptions = new List<Subscription>();
        this.UserProperties = new Dictionary<string, string>();
    }

    /// <summary>
    /// Gets or sets the Subscriptions for this unsubscribe.
    /// </summary>
    public List<Subscription> Subscriptions { get; set; }

    /// <summary>
    /// Gets or sets the User Properties for this unsubscribe.
    /// </summary>
    public Dictionary<string, string> UserProperties { get; set; }

    /// <summary>
    /// Validate that the options in this instance are valid.
    /// </summary>
    /// <exception cref="HiveMQttClientException">Raises this exception if the options are invalid.</exception>
    public void Validate()
    {
        if (this.Subscriptions.Count == 0)
        {
            throw new HiveMQttClientException("At least one topic filter must be specified for UnsubscribeOptions.");
        }
    }
}
