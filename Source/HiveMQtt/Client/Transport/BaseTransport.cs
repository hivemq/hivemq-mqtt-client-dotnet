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
namespace HiveMQtt.Client.Transport;

using System.Net;
using System.Net.Sockets;
using HiveMQtt.Client.Exceptions;

public abstract class BaseTransport
{
    protected static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

    public abstract Task<bool> ConnectAsync(CancellationToken cancellationToken = default);

    public abstract Task<bool> CloseAsync(bool? shutdownPipeline = true, CancellationToken cancellationToken = default);

    public abstract Task<bool> WriteAsync(byte[] buffer, CancellationToken cancellationToken = default);

    public abstract Task<TransportReadResult> ReadAsync(CancellationToken cancellationToken = default);

    public abstract void AdvanceTo(SequencePosition consumed);

    public abstract void AdvanceTo(SequencePosition consumed, SequencePosition examined);

    /// <summary>
    /// Lookup the hostname and return the IP address.
    /// </summary>
    /// <param name="host">The hostname to lookup.</param>
    /// <param name="preferIPv6">A value indicating whether to prefer IPv6 addresses.</param>
    /// <returns>The IP address of the hostname.</returns>
    /// <exception cref="HiveMQttClientException">Thrown when the hostname cannot be resolved.</exception>
    protected static async Task<IPAddress?> LookupHostNameAsync(string host, bool preferIPv6)
    {
        try
        {
            IPAddress? ipAddress = null;
            var ipHostInfo = await Dns.GetHostEntryAsync(host).ConfigureAwait(false);

            if (ipHostInfo.AddressList.Length == 0)
            {
                throw new HiveMQttClientException("Failed to resolve host");
            }

            // DNS Address resolution logic.  If DNS returns multiple records, how do we handle?
            // If we have a single record, we can use that.
            // If we have multiple records, we can use the first one with respect to the PreferIPv6 option.
            if (ipHostInfo.AddressList.Length == 1)
            {
                ipAddress = ipHostInfo.AddressList[0];
            }
            else
            {
                // Loop through each to find a preferred address
                foreach (var address in ipHostInfo.AddressList)
                {
                    if (preferIPv6 && address.AddressFamily == AddressFamily.InterNetworkV6)
                    {
                        ipAddress = address;
                        break;
                    }

                    if (address.AddressFamily == AddressFamily.InterNetwork)
                    {
                        ipAddress = address;
                        break;
                    }
                }
            }

            // We have multiple address returned, but none of them match the PreferIPv6 option.
            // Use the first one whatever it is.
            ipAddress ??= ipHostInfo.AddressList[0];
            return ipAddress;
        }
        catch (SocketException socketException)
        {
            Logger.Debug(socketException.Message);
            return null;
        }
    }
}
