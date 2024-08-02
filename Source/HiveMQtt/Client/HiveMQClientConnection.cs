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

using HiveMQtt.Client.Events;
using HiveMQtt.Client.Exceptions;
using HiveMQtt.Client.Internal;
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
        if (e.CleanDisconnect)
        {
            Logger.Debug("AutomaticReconnectHandler: Clean disconnect.  No need to reconnect.");
            return;
        }

        if (sender is null)
        {
            Logger.Warn("AutomaticReconnectHandler: Sender(client) is null.  Cannot reconnect.");
            return;
        }

        var delay = 5000;
        var maxDelay = 60000;
        var reconnectAttempts = 0;
        var client = (HiveMQClient)sender;

        while (true)
        {
            await Task.Delay(delay).ConfigureAwait(false);
            reconnectAttempts++;

            try
            {
                Logger.Info($"--> Attempting to reconnect to broker.  Attempt #{reconnectAttempts}.");
                var connectResult = await client.ConnectAsync().ConfigureAwait(false);

                if (connectResult.ReasonCode != ConnAckReasonCode.Success)
                {
                    Logger.Info($"--> Failed to reconnect to broker: {connectResult.ReasonCode}/{connectResult.ReasonString}");

                    // Double the delay with each failed retry to a maximum
                    delay = Math.Min(delay * 2, maxDelay);
                    Logger.Debug($"--> Will delay for {delay / 1000} seconds until next try.");
                }
                else
                {
                    Logger.Info("--> Reconnected successfully.");
                    break;
                }
            }
            catch (HiveMQttClientException ex)
            {
                Logger.Info($"--> Failed to reconnect: {ex.Message}");

                // Double the delay with each failed retry to a maximum
                delay = Math.Min(delay * 2, 60000);
                Logger.Debug($"--> Will delay for {delay / 1000} seconds until next try.");
            }
        }
    }

}
