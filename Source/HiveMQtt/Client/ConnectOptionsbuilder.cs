/*
 * Copyright 2025-present HiveMQ and the HiveMQ Community
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

/// <summary>
/// Builder class for the ConnectOptions class.
/// </summary>
public class ConnectOptionsBuilder
{
    private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

    private readonly ConnectOptions options;

    public ConnectOptionsBuilder() => this.options = new ConnectOptions();

    /// <summary>
    /// Sets the session expiry interval for the connect options.
    /// </summary>
    /// <param name="sessionExpiryInterval">The session expiry interval in seconds.</param>
    /// <returns>The ConnectOptionsBuilder instance.</returns>
    public ConnectOptionsBuilder WithSessionExpiryInterval(long sessionExpiryInterval)
    {
        this.options.SessionExpiryInterval = sessionExpiryInterval;
        return this;
    }

    /// <summary>
    /// Sets the keep alive for the connect options.
    /// </summary>
    /// <param name="keepAlive">The keep alive in seconds.</param>
    /// <returns>The ConnectOptionsBuilder instance.</returns>
    public ConnectOptionsBuilder WithKeepAlive(int keepAlive)
    {
        this.options.KeepAlive = keepAlive;
        return this;
    }

    /// <summary>
    /// Sets the clean start for the connect options.
    /// </summary>
    /// <param name="cleanStart">The clean start flag.</param>
    /// <returns>The ConnectOptionsBuilder instance.</returns>
    public ConnectOptionsBuilder WithCleanStart(bool cleanStart)
    {
        this.options.CleanStart = cleanStart;
        return this;
    }

    /// <summary>
    /// Builds the ConnectOptions instance.
    /// </summary>
    /// <returns>The ConnectOptions instance.</returns>
    public ConnectOptions Build() => this.options;
}
