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

using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using HiveMQtt.Client.Events;
using HiveMQtt.Client.Exceptions;
using HiveMQtt.MQTT5.ReasonCodes;

/// <inheritdoc />
public partial class HiveMQClient : IDisposable, IHiveMQClient
{
    /// <summary>
    /// When the client is configured to automatically reconnect, this event is used in the
    /// OnDisconnect event handler chain.
    /// </summary>
    private static async void AutomaticReconnectHandler(object? sender, AfterDisconnectEventArgs e)
    {
        // Use NullLogger for static method - logging is optional for automatic reconnect
        var logger = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;

        if (e.CleanDisconnect)
        {
            LogAutomaticReconnectCleanDisconnect(logger);
            return;
        }

        if (sender is null)
        {
            LogAutomaticReconnectSenderNull(logger);
            return;
        }

        var delay = 5000;
        var maxDelay = 60000;
        var reconnectAttempts = 0;
        var client = (HiveMQClient)sender;

        // Get logger from client instance if available
        var clientLogger = (ILogger)client.logger ?? logger;

        while (true)
        {
            await Task.Delay(delay).ConfigureAwait(false);
            reconnectAttempts++;

            try
            {
                LogAutomaticReconnectAttempting(clientLogger, reconnectAttempts);
                var connectResult = await client.ConnectAsync().ConfigureAwait(false);

                if (connectResult.ReasonCode != ConnAckReasonCode.Success)
                {
                    LogAutomaticReconnectFailed(clientLogger, connectResult.ReasonCode, connectResult.ReasonString);

                    // Double the delay with each failed retry to a maximum
                    delay = Math.Min(delay * 2, maxDelay);
                    LogAutomaticReconnectDelay(clientLogger, delay / 1000);
                }
                else
                {
                    LogAutomaticReconnectSuccess(clientLogger);
                    break;
                }
            }
            catch (HiveMQttClientException ex)
            {
                LogAutomaticReconnectException(clientLogger, ex);

                // Double the delay with each failed retry to a maximum
                delay = Math.Min(delay * 2, 60000);
                LogAutomaticReconnectDelay(clientLogger, delay / 1000);
            }
        }
    }
}
