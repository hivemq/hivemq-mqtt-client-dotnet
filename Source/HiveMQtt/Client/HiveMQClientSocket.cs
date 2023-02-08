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
using System.Net.Sockets;

/// <inheritdoc />
public partial class HiveMQClient : IDisposable, IHiveMQClient
{
    private Socket? socket;
    private NetworkStream? stream;
    private PipeReader? reader;
    private PipeWriter? writer;

    /// <summary>
    /// Make a TCP connection to a remote broker.
    /// </summary>
    /// <returns>A boolean representing the success or failure of the operation.</returns>
    internal async Task<bool> ConnectSocketAsync()
    {
        // Console.WriteLine($"Connecting socket... {this.Options.Host}:{this.Options.Port}");
        var ipHostInfo = await Dns.GetHostEntryAsync(this.Options.Host).ConfigureAwait(false);
        var ipAddress = ipHostInfo.AddressList[1];
        IPEndPoint ipEndPoint = new(ipAddress, this.Options.Port);

        this.socket = new(ipEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        await this.socket.ConnectAsync(ipEndPoint).ConfigureAwait(false);

        var socketConnected = this.socket.Connected;

        if (!socketConnected || this.socket == null)
        {
            // FIXME: Use a real exception
            throw new InvalidOperationException("Failed to connect socket");
        }

        // Setup the Pipeline
        this.stream = new NetworkStream(this.socket);
        this.reader = PipeReader.Create(this.stream);
        this.writer = PipeWriter.Create(this.stream);

        // Start the traffic processors
        this.trafficOutflowProcessor = this.TrafficOutflowProcessorAsync();
        this.trafficInflowProcessor = this.TrafficInflowProcessorAsync();

        // Console.WriteLine($"Socket connected to {this.socket.RemoteEndPoint}");
        return socketConnected;
    }

    internal bool CloseSocket()
    {
        // Shutdown the traffic processors
        this.trafficOutflowProcessor = null;
        this.trafficInflowProcessor = null;

        // Shutdown the pipeline
        this.reader = null;
        this.writer = null;

        // Shutdown the socket
        this.socket?.Shutdown(SocketShutdown.Both);
        this.socket?.Close();

        return true;
    }
}
