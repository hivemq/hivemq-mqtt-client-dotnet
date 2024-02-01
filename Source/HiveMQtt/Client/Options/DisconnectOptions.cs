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
    public int? SessionExpiry { get; set; }

    /// <summary>
    /// Gets or sets the reason string for the disconnection.  This is a human readable
    /// string that is used for diagnostics only.
    /// </summary>
    public string? ReasonString { get; set; }

    /// <summary>
    /// Gets or sets the user properties for the disconnection.
    /// </summary>
    public Dictionary<string, string> UserProperties { get; set; }
}
