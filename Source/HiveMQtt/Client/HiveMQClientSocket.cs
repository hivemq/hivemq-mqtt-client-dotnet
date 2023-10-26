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
namespace HiveMQtt.Client;

using System.IO.Pipelines;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;

using System.Threading;
using System.Threading.Tasks;

using HiveMQtt.Client.Exceptions;

/// <inheritdoc />
public partial class HiveMQClient : IDisposable, IHiveMQClient
{
    private Socket? socket;
    private Stream? stream;
    private PipeReader? reader;
    private PipeWriter? writer;
    private CancellationTokenSource cancellationSource;
    private CancellationToken outFlowCancellationToken;
    private CancellationToken inFlowCancellationToken;
    private CancellationToken receivedPacketsCancellationToken;

#pragma warning disable IDE0052
    private Task trafficOutflowProcessorTask;
    private Task trafficInflowProcessorTask;
    private Task receivedPacketsProcessorAsync;
#pragma warning restore IDE0052


    internal static bool ValidateServerCertificate(
        object sender,
        X509Certificate certificate,
        X509Chain chain,
        SslPolicyErrors sslPolicyErrors)
    {
        if (sslPolicyErrors == SslPolicyErrors.None)
        {
            return true;
        }

        Console.WriteLine("Certificate error: {0}", sslPolicyErrors);

        // Do not allow this client to communicate with unauthenticated servers.
        return false;
    }

    /// <summary>
    /// Make a TCP connection to a remote broker.
    /// </summary>
    /// <returns>A boolean representing the success or failure of the operation.</returns>
    internal async Task<bool> ConnectSocketAsync()
    {
        IPAddress? ipAddress = null;
        var ipHostInfo = await Dns.GetHostEntryAsync(this.Options.Host).ConfigureAwait(false);

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
                if (this.Options.PreferIPv6 && address.AddressFamily == AddressFamily.InterNetworkV6)
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

        IPEndPoint ipEndPoint = new(ipAddress, this.Options.Port);

        this.socket = new(ipEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

        try
        {
            await this.socket.ConnectAsync(ipEndPoint).ConfigureAwait(false);
        }
        catch (SocketException socketException)
        {
            throw new HiveMQttClientException("Failed to connect to broker", socketException);
        }

        var socketConnected = this.socket.Connected;

        if (!socketConnected || this.socket == null)
        {
            throw new HiveMQttClientException("Failed to connect socket");
        }

        // Setup the stream
        this.stream = new NetworkStream(this.socket);
        if (this.Options.UseTLS)
        {
            this.stream = new SslStream(this.stream, false, HiveMQClient.ValidateServerCertificate, null);
            await ((SslStream)this.stream).AuthenticateAsClientAsync(this.Options.Host).ConfigureAwait(false);
        }

        // Setup the Pipeline
        this.reader = PipeReader.Create(this.stream);
        this.writer = PipeWriter.Create(this.stream);

        // Reset the CancellationTokenSource in case this is a reconnect
        this.cancellationSource.Dispose();
        this.cancellationSource = new CancellationTokenSource();

        // Setup the cancellation tokens
        this.outFlowCancellationToken = this.cancellationSource.Token;
        this.inFlowCancellationToken = this.cancellationSource.Token;
        this.receivedPacketsCancellationToken = this.cancellationSource.Token;

        // Start the traffic processors
        this.trafficOutflowProcessorTask = this.TrafficOutflowProcessorAsync(this.outFlowCancellationToken);
        this.trafficInflowProcessorTask = this.TrafficInflowProcessorAsync(this.inFlowCancellationToken);
        this.receivedPacketsProcessorAsync = this.ReceivedPacketsProcessorAsync(this.receivedPacketsCancellationToken);

        Logger.Trace($"Socket connected to {this.socket.RemoteEndPoint}");
        return socketConnected;
    }

    internal bool CloseSocket(bool? shutdownPipeline = true)
    {
        if (shutdownPipeline == true)
        {
            // Shutdown the pipeline
            this.reader = null;
            this.writer = null;
        }

        // Shutdown the socket
        this.socket?.Shutdown(SocketShutdown.Both);
        this.socket?.Close();

        this.cancellationSource.Cancel();

        return true;
    }
}
