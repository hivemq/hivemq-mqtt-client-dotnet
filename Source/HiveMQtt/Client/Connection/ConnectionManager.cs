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
namespace HiveMQtt.Client.Connection;

using System.Diagnostics;
using HiveMQtt.Client.Internal;
using HiveMQtt.Client.Transport;
using HiveMQtt.MQTT5;
using HiveMQtt.MQTT5.Packets;
using HiveMQtt.MQTT5.Types;

/// <summary>
/// Represents a connection manager for the MQTT client.
/// </summary>
public partial class ConnectionManager : IDisposable
{
    private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

    // The HiveMQClient this ConnectionManager is associated with
    internal HiveMQClient Client { get; }

    // This is how we kill innocent and not so innocent Tasks
    private CancellationTokenSource cancellationTokenSource;

    // The state of the connection
    internal ConnectState State { get; set; }

    // The protocol specific transport layer (TCP, WebSocket, etc.)
    internal BaseTransport Transport { get; set; }

    // The MQTT Properties for the active connection.
    internal MQTT5Properties ConnectionProperties { get; set; } = new();

    // The outgoing publish packets queue.  Publish packets are separated from other control packets
    // so that we can correctly respect the Broker's flow control.
    internal AwaitableQueueX<PublishPacket> OutgoingPublishQueue { get; } = new();

    // Non-publish control packets queue; everything else
    internal AwaitableQueueX<ControlPacket> SendQueue { get; } = new();

    // Received control packets queue
    internal AwaitableQueueX<ControlPacket> ReceivedQueue { get; } = new();

    // Incoming Publish QoS > 0 in-flight transactions indexed by packet identifier
    internal BoundedDictionaryX<int, List<ControlPacket>> IPubTransactionQueue { get; set; }

    // Outgoing Publish QoS > 0 in-flight transactions indexed by packet identifier
    internal BoundedDictionaryX<int, List<ControlPacket>> OPubTransactionQueue { get; set; }

    // We generate new PacketIDs here.
    internal PacketIDManager PacketIDManager { get; } = new();

    // This is used to know if and when we need to send a MQTT PingReq
    private readonly Stopwatch lastCommunicationTimer = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="ConnectionManager"/> class.
    /// </summary>
    /// <param name="client">The HiveMQClient this ConnectionManager is associated with.</param>
    public ConnectionManager(HiveMQClient client)
    {
        this.Client = client;
        this.cancellationTokenSource = new CancellationTokenSource();
        this.IPubTransactionQueue = new BoundedDictionaryX<int, List<ControlPacket>>(this.Client.Options.ClientReceiveMaximum);
        this.OPubTransactionQueue = new BoundedDictionaryX<int, List<ControlPacket>>(65535);
        this.State = ConnectState.Disconnected;

        // Connect the appropriate transport
        if (this.Client.Options.Host.StartsWith("ws://", StringComparison.OrdinalIgnoreCase) ||
            this.Client.Options.Host.StartsWith("wss://", StringComparison.OrdinalIgnoreCase))
        {
            // this.Transport = new WebSocketTransport(this.Client.Options);
            this.Transport = new TCPTransport(this.Client.Options);
        }
        else
        {
            this.Transport = new TCPTransport(this.Client.Options);
        }

        Logger.Trace("Trace Level Logging Legend:");
        Logger.Trace("    -(W)-   == ConnectionWriter");
        Logger.Trace("    -(PW)-  == ConnectionPublishWriter");
        Logger.Trace("    -(R)-   == ConnectionReader");
        Logger.Trace("    -(CM)-  == ConnectionMonitor");
        Logger.Trace("    -(RPH)- == ReceivedPacketsHandler");
    }

    internal async Task<bool> ConnectAsync()
    {
        // Connect the appropriate transport
        if (this.Client.Options.WebSocketServer.Length > 0)
        {
            this.Transport = new WebSocketTransport(this.Client.Options);
        }
        else
        {
            this.Transport = new TCPTransport(this.Client.Options);
        }

        // Reset the CancellationTokenSource in case this is a reconnect
        this.cancellationTokenSource.Dispose();
        this.cancellationTokenSource = new CancellationTokenSource();

        var connected = await this.Transport.ConnectAsync().ConfigureAwait(false);

        if (!connected)
        {
            Logger.Error("Failed to connect to broker");
            return false;
        }

        // Start the traffic processors
        this.ConnectionPublishWriterTask = this.ConnectionPublishWriterAsync(this.cancellationTokenSource.Token);
        this.ConnectionWriterTask = this.ConnectionWriterAsync(this.cancellationTokenSource.Token);
        this.ConnectionReaderTask = this.ConnectionReaderAsync(this.cancellationTokenSource.Token);
        this.ReceivedPacketsHandlerTask = this.ReceivedPacketsHandlerAsync(this.cancellationTokenSource.Token);
        this.ConnectionMonitorTask = this.ConnectionMonitorAsync(this.cancellationTokenSource.Token);

        return true;
    }

    /// <summary>
    /// Cancel all background tasks.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    internal async Task CancelBackgroundTasksAsync()
    {
        // Don't use CancelAsync here to maintain backwards compatibility
        // with >=.net6.0.  CancelAsync was introduced in .net8.0
        this.cancellationTokenSource.Cancel();

        // Delay for a short period to allow the tasks to cancel
        await Task.Delay(1000).ConfigureAwait(false);

        // Reset the tasks
        if (this.ConnectionPublishWriterTask is not null && this.ConnectionPublishWriterTask.IsCompleted)
        {
            this.ConnectionPublishWriterTask = null;
        }
        else
        {
            Logger.Error("ConnectionPublishWriterTask did not complete");
        }

        if (this.ConnectionWriterTask is not null && this.ConnectionWriterTask.IsCompleted)
        {
            this.ConnectionWriterTask = null;
        }
        else
        {
            Logger.Error("ConnectionWriterTask did not complete");
        }

        if (this.ConnectionReaderTask is not null && this.ConnectionReaderTask.IsCompleted)
        {
            this.ConnectionReaderTask = null;
        }
        else
        {
            Logger.Error("ConnectionReaderTask did not complete");
        }

        if (this.ReceivedPacketsHandlerTask is not null && this.ReceivedPacketsHandlerTask.IsCompleted)
        {
            this.ReceivedPacketsHandlerTask = null;
        }
        else
        {
            Logger.Error("ReceivedPacketsHandlerTask did not complete");
        }

        if (this.ConnectionMonitorTask is not null && this.ConnectionMonitorTask.IsCompleted)
        {
            this.ConnectionMonitorTask = null;
        }
        else
        {
            Logger.Error("ConnectionMonitorTask did not complete");
        }
    }

    /// <summary>
    /// https://learn.microsoft.com/en-us/dotnet/api/system.idisposable?view=net-6.0.
    /// </summary>
    public void Dispose()
    {
        // Dispose managed resources.
        this.cancellationTokenSource.Cancel();
        this.cancellationTokenSource.Dispose();

        this.Dispose();
        /*
          This object will be cleaned up by the Dispose method.
          Therefore, you should call GC.SuppressFinalize to
          take this object off the finalization queue
          and prevent finalization code for this object
          from executing a second time.
        */
        GC.SuppressFinalize(this);
    }
}
