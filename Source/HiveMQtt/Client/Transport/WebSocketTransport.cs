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

using Microsoft.Extensions.Logging;
using HiveMQtt.Client.Options;
using System.Buffers;
using System.Linq;
using System.Net.WebSockets;
using System.Security.Cryptography.X509Certificates;

/// <summary>
/// WebSocket transport implementation for MQTT over WebSocket connections.
/// </summary>
public partial class WebSocketTransport : BaseTransport, IDisposable
{
    internal HiveMQClientOptions Options { get; }

    internal ClientWebSocket Socket { get; private set; }

    // Semaphore to serialize write operations and prevent concurrent writes
    private readonly SemaphoreSlim writeSemaphore = new(1, 1);

    /// <summary>
    /// Initializes a new instance of the <see cref="WebSocketTransport"/> class.
    /// </summary>
    /// <param name="options">The HiveMQ client options containing WebSocket server URI and configuration.</param>
    /// <exception cref="ArgumentException">Thrown when the WebSocket URI scheme is invalid (must be "ws://" or "wss://").</exception>
    /// <remarks>
    /// <para>
    /// This constructor configures the WebSocket transport with the MQTT subprotocol. The "mqtt" subprotocol is required
    /// for MQTT over WebSocket connections as specified in the MQTT specification. The WebSocket server must support this
    /// subprotocol for the connection to succeed. If the server does not support the "mqtt" subprotocol, the WebSocket
    /// handshake will fail and the connection will be rejected.
    /// </para>
    /// <para>
    /// For secure WebSocket connections (wss://), TLS options are automatically configured based on the provided
    /// <paramref name="options"/>, including client certificates and certificate validation settings.
    /// </para>
    /// </remarks>
    public WebSocketTransport(HiveMQClientOptions options)
        : base(options.LoggerFactory)
    {
        this.Options = options;
        this.Socket = new ClientWebSocket();

        var uri = new Uri(this.Options.WebSocketServer);

        // Add the MQTT subprotocol as required by the MQTT specification for WebSocket connections
        // The server must support this subprotocol or the connection will fail during handshake
        this.Socket.Options.AddSubProtocol("mqtt");

        if (uri.Scheme is not "ws" and not "wss")
        {
            throw new ArgumentException("Invalid WebSocket URI scheme");
        }

        // Configure WebSocket-specific options
        this.ConfigureWebSocketOptions();

        // Configure TLS options for secure WebSocket (wss://)
        if (uri.Scheme == "wss")
        {
            this.ConfigureTlsOptions();
        }
    }

    /// <summary>
    /// SSLStream Callback.  This is used to validate TLS certificates for WebSocket connections.
    /// </summary>
    /// <param name="logger">The logger instance to use for logging.</param>
    /// <param name="sender">An object that contains state information for this validation.</param>
    /// <param name="certificate">The certificate used to authenticate the remote party.</param>
    /// <param name="chain">The chain of certificate authorities associated with the remote certificate.</param>
    /// <param name="sslPolicyErrors">One or more errors associated with the remote certificate.</param>
    /// <returns>A Boolean indicating whether the TLS certificate is valid.</returns>
    private static bool ValidateWebSocketServerCertificate(
        ILogger logger,
        object sender,
        X509Certificate? certificate,
        X509Chain? chain,
        System.Net.Security.SslPolicyErrors sslPolicyErrors)
    {
        // Ignore the sender parameter
        _ = sender;

        if (sslPolicyErrors == System.Net.Security.SslPolicyErrors.None)
        {
            return true;
        }

        LogBrokerTLSCertificateError(logger, sslPolicyErrors);

        // Log additional certificate details for debugging
        if (certificate != null)
        {
            LogWebSocketCertificateSubject(logger, certificate.Subject);
            LogWebSocketCertificateIssuer(logger, certificate.Issuer);
            LogWebSocketCertificateSerialNumber(logger, certificate.GetSerialNumberString());
        }

        // Validate certificate chain if provided
        if (chain != null)
        {
            var chainStatus = chain.ChainStatus.Length > 0 ? string.Join(", ", chain.ChainStatus.Select(cs => cs.Status)) : "Valid";
            LogWebSocketCertificateChainStatus(logger, chainStatus);
        }

        // Do not allow this client to communicate with unauthenticated servers.
        return false;
    }

    /// <summary>
    /// Configure WebSocket-specific options from HiveMQClientOptions.
    /// </summary>
    private void ConfigureWebSocketOptions()
    {
        // Configure keep-alive interval if specified
        if (this.Options.WebSocketKeepAliveInterval.HasValue)
        {
            this.Socket.Options.KeepAliveInterval = this.Options.WebSocketKeepAliveInterval.Value;
            LogWebSocketKeepAliveInterval(this.Logger, this.Options.WebSocketKeepAliveInterval.Value);
        }

        // Configure custom request headers if specified
        if (this.Options.WebSocketRequestHeaders != null && this.Options.WebSocketRequestHeaders.Count > 0)
        {
            foreach (var header in this.Options.WebSocketRequestHeaders)
            {
                this.Socket.Options.SetRequestHeader(header.Key, header.Value);
            }

            LogWebSocketCustomHeadersAdded(this.Logger, this.Options.WebSocketRequestHeaders.Count);
        }

        // Configure proxy if specified
        if (this.Options.WebSocketProxy != null)
        {
            this.Socket.Options.Proxy = this.Options.WebSocketProxy;
            LogWebSocketProxyConfigured(this.Logger, this.Options.WebSocketProxy);
        }
    }

    /// <summary>
    /// Configure TLS options for secure WebSocket connections.
    /// </summary>
    private void ConfigureTlsOptions()
    {
        // Configure client certificates if provided
        if (this.Options.ClientCertificates != null && this.Options.ClientCertificates.Count > 0)
        {
            foreach (var certificate in this.Options.ClientCertificates)
            {
                if (certificate is X509Certificate2 x509Cert2)
                {
                    this.Socket.Options.ClientCertificates.Add(x509Cert2);
                }
            }

            LogWebSocketClientCertificatesAdded(this.Logger, this.Options.ClientCertificates.Count);
        }

        // Configure certificate validation callback
        if (this.Options.AllowInvalidBrokerCertificates)
        {
            LogWebSocketAllowingInvalidCertificates(this.Logger);
#pragma warning disable CA5359 // Do not disable certificate validation
            this.Socket.Options.RemoteCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true;
#pragma warning restore CA5359
        }
        else
        {
            // Use the same validation logic as TCPTransport
            var logger = this.Logger;
            this.Socket.Options.RemoteCertificateValidationCallback = (sender, certificate, chain, errors) =>
                ValidateWebSocketServerCertificate(logger, sender, certificate, chain, errors);
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
            // Log at Debug level since ConnectionManager will log the error and HiveMQClient will throw exception
            LogWebSocketConnectFailed(this.Logger, ex);
            return false;
        }
        catch (OperationCanceledException ex)
        {
            // Log at Debug level since ConnectionManager will log the error and HiveMQClient will throw exception
            LogWebSocketConnectFailed(this.Logger, ex);
            return false;
        }

        return this.Socket.State == WebSocketState.Open;
    }

    /// <summary>
    /// Close the WebSocket connection.
    /// </summary>
    /// <param name="shutdownPipeline">Whether to shutdown the pipeline.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>True if the close was successful, false otherwise.</returns>
    public override async Task<bool> CloseAsync(bool? shutdownPipeline = true, CancellationToken cancellationToken = default)
    {
        try
        {
            // Close the WebSocket if it's in a state that allows closing
            if (this.Socket.State is WebSocketState.Open or WebSocketState.CloseReceived or WebSocketState.CloseSent)
            {
                await this.Socket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, cancellationToken).ConfigureAwait(false);
            }
            else if (this.Socket.State == WebSocketState.Aborted)
            {
                LogWebSocketAlreadyAborted(this.Logger);
            }
            else if (this.Socket.State == WebSocketState.Closed)
            {
                LogWebSocketAlreadyClosed(this.Logger);
            }
            else
            {
                LogWebSocketState(this.Logger, this.Socket.State);
            }
        }
        catch (WebSocketException ex)
        {
            // WebSocket may have been closed by the server or already in an invalid state
            LogWebSocketCloseError(this.Logger, ex);

            // Don't return false here - the socket is likely already closed
        }
        catch (ObjectDisposedException ex)
        {
            // Socket has already been disposed
            LogWebSocketAlreadyDisposed(this.Logger, ex);
        }
        catch (InvalidOperationException ex)
        {
            // WebSocket may already be closed or in a closing state
            LogWebSocketAlreadyClosedOrClosing(this.Logger, ex);
        }
        catch (OperationCanceledException ex)
        {
            // Operation was cancelled
            LogWebSocketCloseCancelled(this.Logger, ex);
            return false;
        }

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
        // Serialize write operations to prevent concurrent writes that cause NotSupportedException
        try
        {
            await this.writeSemaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            // Semaphore wait was cancelled
            LogWebSocketWriteCancelled(this.Logger);
            return false;
        }

        try
        {
            // Check if socket is in a valid state for writing
            if (this.Socket.State != WebSocketState.Open)
            {
                LogWebSocketNotWritable(this.Logger, this.Socket.State);
                return false;
            }

            await this.Socket.SendAsync(buffer, WebSocketMessageType.Binary, true, cancellationToken).ConfigureAwait(false);
        }
        catch (WebSocketException ex)
        {
            // Log at Debug level since ConnectionManager will log the error and write failures can be expected during disconnection
            LogWebSocketWriteFailed(this.Logger, ex);
            return false;
        }
        catch (OperationCanceledException ex)
        {
            // Log at Debug level since ConnectionManager will log the error and write failures can be expected during disconnection
            LogWebSocketWriteFailed(this.Logger, ex);
            return false;
        }
        finally
        {
            this.writeSemaphore.Release();
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
        // Check if socket is in a valid state for reading
        if (this.Socket.State is not WebSocketState.Open and not WebSocketState.CloseReceived)
        {
            LogWebSocketNotReadable(this.Logger, this.Socket.State);
            return new TransportReadResult(true);
        }

        // Use ArrayPool to reuse buffers and reduce GC pressure
        const int bufferSize = 8192;
        var rentedBuffer = ArrayPool<byte>.Shared.Rent(bufferSize);
        var buffer = new ArraySegment<byte>(rentedBuffer, 0, bufferSize);
        WebSocketReceiveResult result;

        try
        {
            // First read to determine if message is fragmented
            result = await this.Socket.ReceiveAsync(buffer, cancellationToken).ConfigureAwait(false);

            // Check if we received a close message
            if (result.MessageType is WebSocketMessageType.Close)
            {
                LogWebSocketReceivedCloseMessage(this.Logger, result.CloseStatus);
                return new TransportReadResult(true);
            }

            // Check if connection was closed during read
            if (result.CloseStatus.HasValue)
            {
                LogWebSocketConnectionClosedDuringRead(this.Logger, result.CloseStatus);
                return new TransportReadResult(true);
            }

            // Optimize for single-read messages (no fragmentation)
            if (result.EndOfMessage && result.MessageType is WebSocketMessageType.Binary)
            {
                // Single read message - copy directly from rented buffer
                if (result.Count > 0)
                {
                    var resultArray = new byte[result.Count];
                    Buffer.BlockCopy(rentedBuffer, 0, resultArray, 0, result.Count);
                    LogWebSocketReceivedBytesSingle(this.Logger, result.Count);
                    return new TransportReadResult(new ReadOnlySequence<byte>(resultArray));
                }
                else
                {
                    return new TransportReadResult(true);
                }
            }

            // Message is fragmented - use MemoryStream to accumulate
            using (var ms = new MemoryStream())
            {
                // Write the first chunk
                if (result.Count > 0)
                {
                    await ms.WriteAsync(rentedBuffer.AsMemory(0, result.Count), cancellationToken).ConfigureAwait(false);
                }

                // Continue reading if message is not complete
                while (!result.EndOfMessage && result.MessageType is not WebSocketMessageType.Close)
                {
                    result = await this.Socket.ReceiveAsync(buffer, cancellationToken).ConfigureAwait(false);

                    // Check if we received a close message
                    if (result.MessageType is WebSocketMessageType.Close)
                    {
                        LogWebSocketReceivedCloseMessage(this.Logger, result.CloseStatus);
                        return new TransportReadResult(true);
                    }

                    // Check if connection was closed during read
                    if (result.CloseStatus.HasValue)
                    {
                        LogWebSocketConnectionClosedDuringRead(this.Logger, result.CloseStatus);
                        return new TransportReadResult(true);
                    }

                    // Only write if we got actual data
                    if (result.Count > 0)
                    {
                        await ms.WriteAsync(rentedBuffer.AsMemory(0, result.Count), cancellationToken).ConfigureAwait(false);
                    }
                }

                LogWebSocketReceivedBytesFragmented(this.Logger, ms.Length);

                // Return data only for binary messages (MQTT uses binary)
                if (result.MessageType is WebSocketMessageType.Binary)
                {
                    // Copy from MemoryStream to final array
                    var resultArray = ms.ToArray();
                    return new TransportReadResult(new ReadOnlySequence<byte>(resultArray));
                }
                else
                {
                    // Text or other message types are not expected for MQTT
                    LogWebSocketUnexpectedMessageType(this.Logger, result.MessageType);
                    return new TransportReadResult(true);
                }
            }
        }
        catch (WebSocketException ex)
        {
            // Log at Debug level since ConnectionManager handles read failures gracefully
            // WebSocketException can occur during expected disconnections
            LogWebSocketReadFailed(this.Logger, ex);
            return new TransportReadResult(true);
        }
        catch (OperationCanceledException ex)
        {
            LogWebSocketReadCancelled(this.Logger, ex);
            return new TransportReadResult(true);
        }
        catch (InvalidOperationException ex)
        {
            // Socket may have been closed or aborted
            LogWebSocketOperationInvalid(this.Logger, ex);
            return new TransportReadResult(true);
        }
        finally
        {
            // Always return the rented buffer to the pool
            ArrayPool<byte>.Shared.Return(rentedBuffer);
        }
    }

    /// <summary>
    /// Advances the reader's examined position to the specified position.
    /// </summary>
    /// <param name="consumed">The position to advance the consumed position to.</param>
    /// <remarks>
    /// <para>
    /// This method is a no-op for WebSocket transport. The AdvanceTo method is part of the
    /// <see cref="System.IO.Pipelines.PipeReader"/> API used by TCPTransport for buffering and backpressure management.
    /// </para>
    /// <para>
    /// WebSocket transport does not use the Pipelines API because the WebSocket API provides its own message
    /// framing and buffering. Messages are read directly from the WebSocket connection and returned as complete
    /// <see cref="ReadOnlySequence{T}"/> buffers. There is no need to track consumed/examined positions since
    /// each read operation returns a complete message boundary.
    /// </para>
    /// </remarks>
    public override void AdvanceTo(SequencePosition consumed)
    {
        // No-op in websocket - WebSocket API handles message boundaries directly
        // Unlike TCPTransport which uses PipeReader, WebSocket reads return complete messages
    }

    /// <summary>
    /// Advances the reader's consumed and examined positions to the specified positions.
    /// </summary>
    /// <param name="consumed">The position to advance the consumed position to.</param>
    /// <param name="examined">The position to advance the examined position to.</param>
    /// <remarks>
    /// <para>
    /// This method is a no-op for WebSocket transport. The AdvanceTo method is part of the
    /// <see cref="System.IO.Pipelines.PipeReader"/> API used by TCPTransport for buffering and backpressure management.
    /// </para>
    /// <para>
    /// WebSocket transport does not use the Pipelines API because the WebSocket API provides its own message
    /// framing and buffering. Messages are read directly from the WebSocket connection and returned as complete
    /// <see cref="ReadOnlySequence{T}"/> buffers. There is no need to track consumed/examined positions since
    /// each read operation returns a complete message boundary.
    /// </para>
    /// </remarks>
    public override void AdvanceTo(SequencePosition consumed, SequencePosition examined)
    {
        // No-op in websocket - WebSocket API handles message boundaries directly
        // Unlike TCPTransport which uses PipeReader, WebSocket reads return complete messages
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
        LogDisposingWebSocketTransport(this.Logger);

        // Check to see if Dispose has already been called.
        if (!this.disposed)
        {
            // If disposing equals true, dispose all managed
            // and unmanaged resources.
            if (disposing)
            {
                // Dispose of the write semaphore
                this.writeSemaphore?.Dispose();

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
                        LogWebSocketDisposeCloseError(this.Logger, ex, ex.Message);
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
