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
namespace HiveMQtt.Client;

using HiveMQtt.Client.Options;
using HiveMQtt.MQTT5.ReasonCodes;

public class DisconnectOptionsBuilder
{
    private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

    private readonly DisconnectOptions options;

    public DisconnectOptionsBuilder() => this.options = new DisconnectOptions();

    /// <summary>
    /// Sets the session expiry interval for the disconnect.
    /// </summary>
    /// <param name="sessionExpiryInterval">The session expiry interval in seconds.</param>
    /// <returns>The builder instance.</returns>
    public DisconnectOptionsBuilder WithSessionExpiryInterval(int sessionExpiryInterval)
    {
        this.options.SessionExpiryInterval = sessionExpiryInterval;
        return this;
    }

    /// <summary>
    /// Sets the reason code for the disconnect.
    /// </summary>
    /// <param name="reasonCode">The reason code for the disconnect.</param>
    /// <returns>The builder instance.</returns>
    public DisconnectOptionsBuilder WithReasonCode(DisconnectReasonCode reasonCode)
    {
        this.options.ReasonCode = reasonCode;
        return this;
    }

    /// <summary>
    /// Sets the reason string for the disconnect.
    /// </summary>
    /// <param name="reasonString">The reason string for the disconnect.</param>
    /// <returns>The builder instance.</returns>
    /// <exception cref="ArgumentException">Thrown if the reason string is not between 1 and 65535 characters.</exception>
    /// <exception cref="ArgumentNullException">Thrown if the reason string is null.</exception>
    public DisconnectOptionsBuilder WithReasonString(string reasonString)
    {
        if (reasonString is null)
        {
            Logger.Error("Reason string cannot be null.");
            throw new ArgumentNullException(nameof(reasonString));
        }

        if (reasonString.Length is < 1 or > 65535)
        {
            Logger.Error("Reason string must be between 1 and 65535 characters.");
            throw new ArgumentException("Reason string must be between 1 and 65535 characters.");
        }

        this.options.ReasonString = reasonString;
        return this;
    }

    /// <summary>
    /// Adds a user property to the disconnect.
    /// </summary>
    /// <param name="key">The key for the user property.</param>
    /// <param name="value">The value for the user property.</param>
    /// <returns>The builder instance.</returns>
    /// <exception cref="ArgumentException">Thrown if the key or value is not between 1 and 65535 characters.</exception>
    /// <exception cref="ArgumentNullException">Thrown if the key or value is null.</exception>
    public DisconnectOptionsBuilder WithUserProperty(string key, string value)
    {
        if (key is null)
        {
            Logger.Error("User property key cannot be null.");
            throw new ArgumentNullException(nameof(key));
        }

        if (value is null)
        {
            Logger.Error("User property value cannot be null.");
            throw new ArgumentNullException(nameof(value));
        }

        if (key.Length is < 1 or > 65535)
        {
            Logger.Error("User property key must be between 1 and 65535 characters.");
            throw new ArgumentException("User property key must be between 1 and 65535 characters.");
        }

        if (value.Length is < 1 or > 65535)
        {
            Logger.Error("User property value must be between 1 and 65535 characters.");
            throw new ArgumentException("User property value must be between 1 and 65535 characters.");
        }

        this.options.UserProperties.Add(key, value);
        return this;
    }

    public DisconnectOptionsBuilder WithUserProperties(Dictionary<string, string> properties)
    {
        foreach (var property in properties)
        {
            this.options.UserProperties.Add(property.Key, property.Value);
        }

        return this;
    }

    /// <summary>
    /// Builds the SubscribeOptions based on the previous calls.  This
    /// step will also run validation on the final SubscribeOptions.
    /// </summary>
    /// <returns>The constructed SubscribeOptions instance.</returns>
    public DisconnectOptions Build()
    {
        this.options.Validate();
        return this.options;
    }
}
