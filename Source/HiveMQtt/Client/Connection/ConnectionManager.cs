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

using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
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

    // This is used to know if and when we need to send a MQTT PingReq
    private readonly Stopwatch lastCommunicationTimer = new();

    // Semaphore to prevent concurrent disconnection attempts
    private readonly SemaphoreSlim disconnectionSemaphore = new(1, 1);

    // This is how we kill innocent and not so innocent Tasks
    private CancellationTokenSource cancellationTokenSource;

    // The state of the connection (thread-safe using Interlocked)
    private int stateValue = (int)ConnectState.Disconnected;

    /// <summary>
    /// Gets or sets the connection state in a thread-safe manner.
    /// Uses Volatile.Read for reads and Interlocked.Exchange for writes to ensure thread safety.
    /// </summary>
    internal ConnectState State
    {
        get => (ConnectState)Volatile.Read(ref this.stateValue);
        set => Interlocked.Exchange(ref this.stateValue, (int)value);
    }

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

    // We generate new Packet IDs here.
    internal PacketIDManager PacketIDManager { get; } = new();

    public PacketIDManager GetPacketIDManager() => this.PacketIDManager;

    // Event-like signal to indicate the connection reached Connected state
    private TaskCompletionSource<bool> connectedSignal = new(TaskCreationOptions.RunContinuationsAsynchronously);

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
        this.ResetConnectedSignal();

        // Connect the appropriate transport
        // Note: The actual transport is selected in ConnectAsync() based on WebSocketServer option.
        // For now, initialize with TCPTransport as a placeholder - it will be replaced during ConnectAsync()
        this.Transport = new TCPTransport(this.Client.Options);

        Logger.Trace("Trace Level Logging Legend:");
        Logger.Trace("    -(W)-   == ConnectionWriter");
        Logger.Trace("    -(PW)-  == ConnectionPublishWriter");
        Logger.Trace("    -(R)-   == ConnectionReader");
        Logger.Trace("    -(CM)-  == ConnectionMonitor");
        Logger.Trace("    -(RPH)- == ReceivedPacketsHandler");
    }

    internal Task WaitUntilConnectedAsync(CancellationToken cancellationToken) => this.connectedSignal.Task.WaitAsync(cancellationToken);

    internal void SignalConnected()
    {
        if (!this.connectedSignal.Task.IsCompleted)
        {
            this.connectedSignal.TrySetResult(true);
        }
    }

    internal void ResetConnectedSignal() => this.connectedSignal = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

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
#pragma warning disable VSTHRD103 // Cancel() used for .NET 6 compatibility - CancelAsync() requires .NET 8
        this.cancellationTokenSource?.Cancel();
#pragma warning restore VSTHRD103
        this.cancellationTokenSource?.Dispose();
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
        this.ConnectionMonitorThread = this.LaunchConnectionMonitorThreadAsync(this.cancellationTokenSource.Token);

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
#pragma warning disable VSTHRD103 // Cancel() used for .NET 6 compatibility - CancelAsync() requires .NET 8
        this.cancellationTokenSource.Cancel();
#pragma warning restore VSTHRD103

        // Collect all active tasks
        var tasksToWait = new List<Task>();
        if (this.ConnectionPublishWriterTask is not null && !this.ConnectionPublishWriterTask.IsCompleted)
        {
            tasksToWait.Add(this.ConnectionPublishWriterTask);
        }

        if (this.ConnectionWriterTask is not null && !this.ConnectionWriterTask.IsCompleted)
        {
            tasksToWait.Add(this.ConnectionWriterTask);
        }

        if (this.ConnectionReaderTask is not null && !this.ConnectionReaderTask.IsCompleted)
        {
            tasksToWait.Add(this.ConnectionReaderTask);
        }

        if (this.ReceivedPacketsHandlerTask is not null && !this.ReceivedPacketsHandlerTask.IsCompleted)
        {
            tasksToWait.Add(this.ReceivedPacketsHandlerTask);
        }

        if (this.ConnectionMonitorThread is not null && !this.ConnectionMonitorThread.IsCompleted)
        {
            tasksToWait.Add(this.ConnectionMonitorThread);
        }

        // Actually await all tasks with a timeout
        if (tasksToWait.Count > 0)
        {
            try
            {
                // Wait for all tasks to complete with a 5 second timeout
                await Task.WhenAll(tasksToWait).WaitAsync(TimeSpan.FromSeconds(5)).ConfigureAwait(false);
                Logger.Trace("All background tasks completed successfully");
            }
            catch (TimeoutException)
            {
                Logger.Warn($"Background tasks did not complete within timeout. {tasksToWait.Count} task(s) may still be running.");
            }
            catch (Exception ex)
            {
                // Observe exceptions from tasks to prevent unobserved task exceptions
                Logger.Warn($"Exception while waiting for background tasks to complete: {ex.Message}");

                // Log individual task exceptions if any are faulted
                foreach (var task in tasksToWait)
                {
                    if (task.IsFaulted && task.Exception != null)
                    {
                        Logger.Warn($"Task faulted during cancellation: {task.Exception.GetBaseException().Message}");
                    }
                }
            }
        }

        // Clean up task references and observe any remaining exceptions
        if (this.ConnectionPublishWriterTask is not null)
        {
            if (this.ConnectionPublishWriterTask.IsFaulted)
            {
                // Observe the exception to prevent unobserved task exceptions
                _ = this.ConnectionPublishWriterTask.Exception;
            }

            this.ConnectionPublishWriterTask = null;
        }

        if (this.ConnectionWriterTask is not null)
        {
            if (this.ConnectionWriterTask.IsFaulted)
            {
                _ = this.ConnectionWriterTask.Exception;
            }

            this.ConnectionWriterTask = null;
        }

        if (this.ConnectionReaderTask is not null)
        {
            if (this.ConnectionReaderTask.IsFaulted)
            {
                _ = this.ConnectionReaderTask.Exception;
            }

            this.ConnectionReaderTask = null;
        }

        if (this.ReceivedPacketsHandlerTask is not null)
        {
            if (this.ReceivedPacketsHandlerTask.IsFaulted)
            {
                _ = this.ReceivedPacketsHandlerTask.Exception;
            }

            this.ReceivedPacketsHandlerTask = null;
        }

        if (this.ConnectionMonitorThread is not null)
        {
            if (this.ConnectionMonitorThread.IsFaulted)
            {
                _ = this.ConnectionMonitorThread.Exception;
            }

            this.ConnectionMonitorThread = null;
        }
    }

    /// <summary>
    /// https://learn.microsoft.com/en-us/dotnet/api/system.idisposable?view=net-6.0.
    /// </summary>
    public void Dispose()
    {
        this.Dispose(disposing: true);
        /*
          This object will be cleaned up by the Dispose method.
          Therefore, you should call GC.SuppressFinalize to
          take this object off the finalization queue
          and prevent finalization code for this object
          from executing a second time.
        */
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// https://learn.microsoft.com/en-us/dotnet/api/system.idisposable?view=net-6.0
    /// Dispose(bool disposing) executes in two distinct scenarios.
    /// If disposing equals true, the method has been called directly
    /// or indirectly by a user's code. Managed and unmanaged resources
    /// can be disposed.
    /// If disposing equals false, the method has been called by the
    /// runtime from inside finalize and you should not reference
    /// other objects. Only unmanaged resources can be disposed.
    /// </summary>
    /// <param name="disposing">True if called from user code.</param>
    protected virtual void Dispose(bool disposing)
    {
        Logger.Trace("Disposing ConnectionManager");

        // Check to see if Dispose has already been called.
        if (!this.disposed)
        {
            // If disposing equals true, dispose all managed
            // and unmanaged resources.
            if (disposing)
            {
                // Cancel and dispose the cancellation token source
                this.cancellationTokenSource?.Cancel();
                this.cancellationTokenSource?.Dispose();

                // Dispose transport if it implements IDisposable
                if (this.Transport is IDisposable disposableTransport)
                {
                    disposableTransport.Dispose();
                }

                // Dispose queues
                this.OutgoingPublishQueue?.Dispose();
                this.SendQueue?.Dispose();
                this.ReceivedQueue?.Dispose();
                this.IPubTransactionQueue?.Dispose();
                this.OPubTransactionQueue?.Dispose();

                // Dispose semaphore
                this.disconnectionSemaphore?.Dispose();
            }

            // Note disposing has been done.
            this.disposed = true;
        }
    }

    private bool disposed;
}
