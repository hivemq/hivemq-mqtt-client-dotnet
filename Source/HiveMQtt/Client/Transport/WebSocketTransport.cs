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

using HiveMQtt.Client.Options;
using System.Buffers;
using System.Net.WebSockets;

public class WebSocketTransport : BaseTransport, IDisposable
{
    internal HiveMQClientOptions Options { get; }

    internal ClientWebSocket Socket { get; private set; }

    public WebSocketTransport(HiveMQClientOptions options)
    {
        this.Options = options;
        this.Socket = new ClientWebSocket();

        var uri = new Uri(this.Options.WebSocketServer);
        this.Socket.Options.AddSubProtocol("mqtt");

        if (uri.Scheme is not "ws" and not "wss")
        {
            throw new ArgumentException("Invalid WebSocket URI scheme");
        }
    }

    public override async Task<bool> ConnectAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var uri = new Uri(this.Options.WebSocketServer);
            await this.Socket.ConnectAsync(uri, cancellationToken).ConfigureAwait(false);
        }
        catch (WebSocketException ex)
        {
            Logger.Error(ex, "Failed to connect to the WebSocket server");
            return false;
        }
        catch (OperationCanceledException ex)
        {
            Logger.Error(ex, "Failed to connect to the WebSocket server");
            return false;
        }

        return this.Socket.State == WebSocketState.Open;
    }

    /// <summary>
    /// Close the WebSocket connection.
    /// </summary>
    /// <param name="shutdownPipeline">Whether to shutdown the pipeline.</param>
    /// <returns>True if the close was successful, false otherwise.</returns>
    public override async Task<bool> CloseAsync(bool? shutdownPipeline = true)
    {
        await this.Socket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None).ConfigureAwait(false);
        return true;
    }

    /// <summary>
    /// Write to the WebSocket server.
    /// </summary>
    /// <param name="buffer">The buffer to write.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>True if the write was successful, false otherwise.</returns>
    /// <exception cref="WebSocketException">Thrown when the WebSocket server is not available.</exception>
    /// <exception cref="OperationCanceledException">Thrown when the operation is canceled.</exception>
    public override async Task<bool> WriteAsync(byte[] buffer, CancellationToken cancellationToken = default)
    {
        try
        {
            await this.Socket.SendAsync(buffer, WebSocketMessageType.Binary, true, cancellationToken).ConfigureAwait(false);
        }
        catch (WebSocketException ex)
        {
            Logger.Error(ex, "Failed to write to the WebSocket server");
            return false;
        }
        catch (OperationCanceledException ex)
        {
            Logger.Error(ex, "Failed to write to the WebSocket server");
            return false;
        }

        return true;
    }

    /// <summary>
    /// Read from the WebSocket server.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The read result.</returns>
    /// <exception cref="WebSocketException">Thrown when the WebSocket server is not available.</exception>
    /// <exception cref="OperationCanceledException">Thrown when the operation is canceled.</exception>
    public override async Task<TransportReadResult> ReadAsync(CancellationToken cancellationToken = default)
    {
        var buffer = new ArraySegment<byte>(new byte[8192]);
        WebSocketReceiveResult result;

        using (var ms = new MemoryStream())
        {
            // Read until the end of the message
            do
            {
                result = await this.Socket.ReceiveAsync(buffer, CancellationToken.None).ConfigureAwait(false);
                await ms.WriteAsync(buffer.AsMemory(buffer.Offset, result.Count), cancellationToken).ConfigureAwait(false);
            }
            while (!result.EndOfMessage);

            Logger.Trace($"Received {ms.Length} bytes");

            // Development
            // ms.Seek(0, SeekOrigin.Begin);
            if (result.MessageType == WebSocketMessageType.Binary)
            {
                // Prepare the result and return
                return new TransportReadResult(new ReadOnlySequence<byte>(ms.ToArray()));
            }
            else
            {
                return new TransportReadResult(true);
            }
        }
    }

    public override void AdvanceTo(SequencePosition consumed)
    {
        // No-op in websocket
    }

    public override void AdvanceTo(SequencePosition consumed, SequencePosition examined)
    {
        // No-op in websocket
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
        Logger.Trace("Disposing WebSocketTransport");

        // Check to see if Dispose has already been called.
        if (!this.disposed)
        {
            // If disposing equals true, dispose all managed
            // and unmanaged resources.
            if (disposing)
            {
                // Dispose WebSocket if it's not null
                if (this.Socket != null)
                {
                    try
                    {
                        if (this.Socket.State == WebSocketState.Open)
                        {
#pragma warning disable VSTHRD002 // Synchronous Wait in dispose pattern is intentional to ensure cleanup
                            this.Socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Disposing", CancellationToken.None).Wait(1000);
#pragma warning restore VSTHRD002
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Warn($"Error closing WebSocket: {ex.Message}");
                    }
                    finally
                    {
                        this.Socket.Dispose();
                        this.Socket = null!;
                    }
                }
            }

            // Note disposing has been done.
            this.disposed = true;
        }
    }

    private bool disposed;
}
