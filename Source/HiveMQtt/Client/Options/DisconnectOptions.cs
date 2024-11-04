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
using HiveMQtt.MQTT5.ReasonCodes;

/// <summary>
/// The options class for a Disconnect call.
/// </summary>
public class DisconnectOptions
{
    public DisconnectOptions()
    {
        this.UserProperties = new Dictionary<string, string>();

        // Set the default disconnect reason
        this.ReasonCode = DisconnectReasonCode.NormalDisconnection;
    }

    /// <summary>
    /// Gets or sets the reason code for the disconnection.  The default value is
    /// <c>NormalDisconnection</c>.
    /// </summary>
    public DisconnectReasonCode ReasonCode { get; set; }

    /// <summary>
    /// Gets or sets the session expiry.  This sets the expiration for the session
    /// to the indicated value.  The value represents the session expiration time
    /// in seconds.
    /// </summary>
    public int? SessionExpiryInterval { get; set; }

    /// <summary>
    /// Gets or sets the reason string for the disconnection.  This is a human readable
    /// string that is used for diagnostics only.
    /// </summary>
    public string? ReasonString { get; set; }

    /// <summary>
    /// Gets or sets the user properties for the disconnection.
    /// </summary>
    public Dictionary<string, string> UserProperties { get; set; }

    /// <summary>
    /// Validate that the options in this instance are valid.
    /// </summary>
    /// <exception cref="HiveMQttClientException">The exception raised if some value is out of range or invalid.</exception>
    public void Validate()
    {
        // Validate SessionExpiry is non-negative if provided
        if (this.SessionExpiryInterval.HasValue && this.SessionExpiryInterval < 0)
        {
            throw new HiveMQttClientException("Session expiry must be a non-negative value.");
        }

        // Validate ReasonString length (assuming max length of 65535 characters)
        if (this.ReasonString != null && this.ReasonString.Length > 65535)
        {
            throw new HiveMQttClientException("Reason string must not exceed 65535 characters.");
        }

        // Validate UserProperties for null keys or values
        foreach (var kvp in this.UserProperties)
        {
            if (kvp.Key == null || kvp.Value == null)
            {
                throw new HiveMQttClientException("User properties must not have null keys or values.");
            }
        }
    }
}
