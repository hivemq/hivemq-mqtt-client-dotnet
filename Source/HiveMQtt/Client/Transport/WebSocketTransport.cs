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
        var buffer = new ArraySegment<byte>(new Byte[8192]);
        WebSocketReceiveResult result;

        using (var ms = new MemoryStream())
        {
            // Read until the end of the message
            do
            {
                result = await this.Socket.ReceiveAsync(buffer, CancellationToken.None).ConfigureAwait(false);
                await ms.WriteAsync(buffer.Array, buffer.Offset, result.Count, cancellationToken).ConfigureAwait(false);
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
