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

using Microsoft.Extensions.Logging;
using HiveMQtt.Client.Options;
using HiveMQtt.MQTT5.ReasonCodes;

public class DisconnectOptionsBuilder
{
    // Static logger factory that can be set to enable logging from DisconnectOptionsBuilder
    // Set by HiveMQClient when it's initialized with a logger factory
    private static ILoggerFactory? loggerFactory;

    // Cached logger instance (created lazily when factory is set)
    private static ILogger? cachedLogger;

    // Logger instance created from the factory (or NullLogger if no factory is set)
    private static ILogger Logger
    {
        get
        {
            if (cachedLogger != null)
            {
                return cachedLogger;
            }

            if (loggerFactory != null)
            {
                cachedLogger = loggerFactory.CreateLogger<DisconnectOptionsBuilder>();
                return cachedLogger;
            }

            return Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
        }
    }

    /// <summary>
    /// Sets the logger factory for DisconnectOptionsBuilder logging.
    /// This is called by HiveMQClient to enable builder-level logging.
    /// </summary>
    /// <param name="factory">The logger factory to use for creating loggers, or null to disable logging.</param>
    internal static void SetLoggerFactory(ILoggerFactory? factory)
    {
        loggerFactory = factory;
        cachedLogger = null; // Reset cached logger so it will be recreated with new factory
    }

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
            Logger.LogError("Reason string cannot be null.");
            throw new ArgumentNullException(nameof(reasonString));
        }

        if (reasonString.Length is < 1 or > 65535)
        {
            Logger.LogError("Reason string must be between 1 and 65535 characters.");
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
            Logger.LogError("User property key cannot be null.");
            throw new ArgumentNullException(nameof(key));
        }

        if (value is null)
        {
            Logger.LogError("User property value cannot be null.");
            throw new ArgumentNullException(nameof(value));
        }

        if (key.Length is < 1 or > 65535)
        {
            Logger.LogError("User property key must be between 1 and 65535 characters.");
            throw new ArgumentException("User property key must be between 1 and 65535 characters.");
        }

        if (value.Length is < 1 or > 65535)
        {
            Logger.LogError("User property value must be between 1 and 65535 characters.");
            throw new ArgumentException("User property value must be between 1 and 65535 characters.");
        }

        this.options.UserProperties.Add(key, value);
        return this;
    }

    /// <summary>
    /// Adds a dictionary of user properties to the disconnect.
    /// </summary>
    /// <param name="properties">The dictionary of user properties to add.</param>
    /// <returns>The builder instance.</returns>
    /// <exception cref="ArgumentException">Thrown if a key or value is not between 1 and 65535 characters.</exception>
    /// <exception cref="ArgumentNullException">Thrown if the key or value is null.</exception>
    public DisconnectOptionsBuilder WithUserProperties(Dictionary<string, string> properties)
    {
        foreach (var property in properties)
        {
            if (property.Key is null)
            {
                Logger.LogError("User property key cannot be null.");
                throw new ArgumentNullException(nameof(properties));
            }

            if (property.Value is null)
            {
                Logger.LogError("User property value cannot be null.");
                throw new ArgumentNullException(nameof(properties));
            }

            if (property.Key.Length is < 1 or > 65535)
            {
                Logger.LogError("User property key must be between 1 and 65535 characters.");
                throw new ArgumentException("User property key must be between 1 and 65535 characters.");
            }

            if (property.Value.Length is < 1 or > 65535)
            {
                Logger.LogError("User property value must be between 1 and 65535 characters.");
                throw new ArgumentException("User property value must be between 1 and 65535 characters.");
            }

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
