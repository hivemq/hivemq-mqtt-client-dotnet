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

using System.IO.Pipelines;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using HiveMQtt.Client.Exceptions;
using HiveMQtt.Client.Options;

public class TCPTransport : BaseTransport, IDisposable
{
    internal Socket? Socket { get; set; }

    internal Stream? Stream { get; set; }

    internal PipeReader? Reader { get; set; }

    internal PipeWriter? Writer { get; set; }

    internal HiveMQClientOptions Options { get; }

    // Semaphore to serialize write operations and prevent concurrent writes
    private readonly SemaphoreSlim writeSemaphore = new(1, 1);

    public TCPTransport(HiveMQClientOptions options) => this.Options = options;

    /// <summary>
    /// SSLStream Callback.  This is used to always allow invalid broker certificates.
    /// </summary>
    /// <param name="sender">An object that contains state information for this validation.</param>
    /// <param name="certificate">The certificate used to authenticate the remote party.</param>
    /// <param name="chain">The chain of certificate authorities associated with the remote certificate.</param>
    /// <param name="sslPolicyErrors">One or more errors associated with the remote certificate.</param>
    /// <returns>A Boolean that says every certificate is valid.</returns>
    internal static bool AllowInvalidBrokerCertificates(
        object sender,
        X509Certificate? certificate,
        X509Chain? chain,
        SslPolicyErrors sslPolicyErrors)
    {
        // Ignore the unused parameters
        _ = sender;
        _ = certificate;
        _ = chain;
        _ = sslPolicyErrors;

        return true;
    }

    /// <summary>
    /// SSLStream Callback.  This is used to validate TLS certificates.
    /// </summary>
    /// <param name="sender">An object that contains state information for this validation.</param>
    /// <param name="certificate">The certificate used to authenticate the remote party.</param>
    /// <param name="chain">The chain of certificate authorities associated with the remote certificate.</param>
    /// <param name="sslPolicyErrors">One or more errors associated with the remote certificate.</param>
    /// <returns>A Boolean indicating whether the TLS certificate is valid.</returns>
    internal static bool ValidateServerCertificate(
        object sender,
        X509Certificate? certificate,
        X509Chain? chain,
        SslPolicyErrors sslPolicyErrors)
    {
        // Ignore the sender parameter
        _ = sender;

        if (sslPolicyErrors == SslPolicyErrors.None)
        {
            return true;
        }

        Logger.Warn("Broker TLS Certificate error: {0}", sslPolicyErrors);

        // Log additional certificate details for debugging
        if (certificate != null)
        {
            Logger.Debug(CultureInfo.InvariantCulture, "Certificate Subject: {0}", certificate.Subject);
            Logger.Debug(CultureInfo.InvariantCulture, "Certificate Issuer: {0}", certificate.Issuer);
            Logger.Debug(CultureInfo.InvariantCulture, "Certificate Serial Number: {0}", certificate.GetSerialNumberString());
        }

        // Validate certificate chain if provided
        if (chain != null)
        {
            var chainStatus = chain.ChainStatus.Length > 0 ? string.Join(", ", chain.ChainStatus.Select(cs => cs.Status)) : "Valid";
            Logger.Debug(CultureInfo.InvariantCulture, "Certificate chain validation status: {0}", chainStatus);
        }

        // Do not allow this client to communicate with unauthenticated servers.
        return false;
    }

    private async Task<bool> CreateTLSConnectionAsync(Stream stream)
    {
        Logger.Trace("Creating TLS connection");

        var tlsOptions = new SslClientAuthenticationOptions
        {
            TargetHost = this.Options.Host,
            EnabledSslProtocols = SslProtocols.Tls13 | SslProtocols.Tls12,
            ClientCertificates = this.Options.ClientCertificates,
        };

        if (this.Options.AllowInvalidBrokerCertificates)
        {
            Logger.Trace("Allowing invalid broker certificates");
#pragma warning disable CA5359
            var yesMan = new RemoteCertificateValidationCallback((sender, certificate, chain, errors) => true);
#pragma warning restore CA5359
            tlsOptions.RemoteCertificateValidationCallback = yesMan;
        }
        else
        {
            tlsOptions.RemoteCertificateValidationCallback = ValidateServerCertificate;
        }

        try
        {
            Logger.Trace("Authenticating TLS connection");
            this.Stream = new SslStream(stream);
            await ((SslStream)this.Stream).AuthenticateAsClientAsync(tlsOptions).ConfigureAwait(false);

            Logger.Info($"Connected via TLS: {((SslStream)this.Stream).IsEncrypted}");
            Logger.Debug($"Cipher Algorithm: {((SslStream)this.Stream).CipherAlgorithm}");
            Logger.Debug($"Cipher Strength: {((SslStream)this.Stream).CipherStrength}");
            Logger.Debug($"Hash Algorithm: {((SslStream)this.Stream).HashAlgorithm}");
            Logger.Debug($"Hash Strength: {((SslStream)this.Stream).HashStrength}");
            Logger.Debug($"Key Exchange Algorithm: {((SslStream)this.Stream).KeyExchangeAlgorithm}");
            Logger.Debug($"Key Exchange Strength: {((SslStream)this.Stream).KeyExchangeStrength}");

            var remoteCertificate = ((SslStream)this.Stream).RemoteCertificate;
            if (remoteCertificate != null)
            {
                Logger.Info($"Remote Certificate Subject: {remoteCertificate.Subject}");
                Logger.Info($"Remote Certificate Issuer: {remoteCertificate.Issuer}");
                Logger.Info($"Remote Certificate Serial Number: {remoteCertificate.GetSerialNumberString()}");
            }

            Logger.Info($"TLS Protocol: {((SslStream)this.Stream).SslProtocol}");
            return true;
        }
        catch (Exception e)
        {
            Logger.Error(e.Message);
            if (e.InnerException != null)
            {
                Logger.Error(e.InnerException.Message);
            }

            Logger.Error("Error while establishing TLS connection - closing the connection.");
            return false;
        }
    }

    /// <summary>
    /// Connect to the target host through an HTTP proxy using the CONNECT method.
    /// </summary>
    /// <param name="stream">The network stream connected to the proxy.</param>
    /// <param name="targetHost">The target host to tunnel to.</param>
    /// <param name="targetPort">The target port to tunnel to.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>True if the tunnel was established successfully, false otherwise.</returns>
    private async Task<bool> EstablishProxyTunnelAsync(NetworkStream stream, string targetHost, int targetPort, CancellationToken cancellationToken)
    {
        Logger.Trace($"Establishing HTTP CONNECT tunnel to {targetHost}:{targetPort}");

        var targetAddress = string.Format(CultureInfo.InvariantCulture, "{0}:{1}", targetHost, targetPort);

        // Build the CONNECT request
        var requestBuilder = new StringBuilder();
        requestBuilder.Append(CultureInfo.InvariantCulture, $"CONNECT {targetAddress} HTTP/1.1\r\n");
        requestBuilder.Append(CultureInfo.InvariantCulture, $"Host: {targetAddress}\r\n");

        // Add proxy authentication if credentials are provided
        if (this.Options.Proxy != null)
        {
            var proxyUri = this.GetProxyUri();
            if (proxyUri != null)
            {
                var credentials = this.Options.Proxy.Credentials?.GetCredential(proxyUri, "Basic");
                if (credentials != null && !string.IsNullOrEmpty(credentials.UserName))
                {
                    var authString = string.Format(CultureInfo.InvariantCulture, "{0}:{1}", credentials.UserName, credentials.Password);
                    var authBytes = Encoding.ASCII.GetBytes(authString);
                    var authBase64 = Convert.ToBase64String(authBytes);
                    requestBuilder.Append(CultureInfo.InvariantCulture, $"Proxy-Authorization: Basic {authBase64}\r\n");
                    Logger.Trace("Added Proxy-Authorization header");
                }
            }
        }

        requestBuilder.Append("\r\n");

        // Send the CONNECT request
        var requestBytes = Encoding.ASCII.GetBytes(requestBuilder.ToString());
        await stream.WriteAsync(requestBytes.AsMemory(), cancellationToken).ConfigureAwait(false);
        await stream.FlushAsync(cancellationToken).ConfigureAwait(false);

        Logger.Trace("Sent HTTP CONNECT request");

        // Read the proxy response
        var responseBuffer = new byte[4096];
        var responseBuilder = new StringBuilder();
        var totalBytesRead = 0;
        var headerComplete = false;

        while (!headerComplete && totalBytesRead < responseBuffer.Length)
        {
            var bytesRead = await stream.ReadAsync(responseBuffer.AsMemory(totalBytesRead, responseBuffer.Length - totalBytesRead), cancellationToken).ConfigureAwait(false);
            if (bytesRead == 0)
            {
                Logger.Error("Proxy closed connection before completing response");
                throw new HiveMQttClientException("Proxy closed connection before completing response");
            }

            totalBytesRead += bytesRead;
            var currentResponse = Encoding.ASCII.GetString(responseBuffer, 0, totalBytesRead);

            // Check if we have received the complete header (ends with \r\n\r\n)
            if (currentResponse.Contains("\r\n\r\n"))
            {
                headerComplete = true;
                responseBuilder.Append(currentResponse);
            }
        }

        var response = responseBuilder.ToString();
        Logger.Trace($"Received proxy response: {response.Split('\r')[0]}");

        // Parse the response status line
        var statusLine = response.Split('\r')[0];
        var statusParts = statusLine.Split(' ');

        if (statusParts.Length < 2)
        {
            Logger.Error($"Invalid proxy response: {statusLine}");
            throw new HiveMQttClientException($"Invalid proxy response: {statusLine}");
        }

        // Check for successful connection (HTTP/1.x 200)
        var statusCode = statusParts[1];
        if (statusCode != "200")
        {
            var errorMessage = string.Format(CultureInfo.InvariantCulture, "Proxy connection failed with status {0}: {1}", statusCode, statusLine);
            Logger.Error(errorMessage);
            throw new HiveMQttClientException(errorMessage);
        }

        Logger.Info($"HTTP CONNECT tunnel established to {targetAddress}");
        return true;
    }

    /// <summary>
    /// Gets the proxy URI from the configured proxy.
    /// </summary>
    /// <returns>The proxy URI, or null if not configured.</returns>
    private Uri? GetProxyUri()
    {
        if (this.Options.Proxy == null)
        {
            return null;
        }

        // Try to get the proxy URI for the target host
        var targetUri = new Uri($"http://{this.Options.Host}:{this.Options.Port}");
        var proxyUri = this.Options.Proxy.GetProxy(targetUri);

        return proxyUri;
    }

    /// <summary>
    /// Resolves the proxy endpoint (host and port) from the configured proxy.
    /// </summary>
    /// <returns>A tuple containing the proxy host and port, or null if not configured.</returns>
    private async Task<IPEndPoint?> ResolveProxyEndpointAsync()
    {
        var proxyUri = this.GetProxyUri();
        if (proxyUri == null)
        {
            return null;
        }

        var proxyHost = proxyUri.Host;
        var proxyPort = proxyUri.Port;

        IPEndPoint? proxyEndPoint = null;

        if (IPAddress.TryParse(proxyHost, out var parsedProxyIp))
        {
            proxyEndPoint = new IPEndPoint(parsedProxyIp, proxyPort);
        }
        else
        {
            var lookupResult = await LookupHostNameAsync(proxyHost, this.Options.PreferIPv6).ConfigureAwait(false);
            if (lookupResult != null)
            {
                proxyEndPoint = new IPEndPoint(lookupResult, proxyPort);
            }
        }

        return proxyEndPoint;
    }

    /// <summary>
    /// Make a TCP connection to a remote broker.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A boolean representing the success or failure of the operation.</returns>
    public override async Task<bool> ConnectAsync(CancellationToken cancellationToken = default)
    {
        IPEndPoint? connectionEndPoint = null;
        var useProxy = this.Options.Proxy != null;

        if (useProxy)
        {
            // When using a proxy, connect to the proxy server first
            connectionEndPoint = await this.ResolveProxyEndpointAsync().ConfigureAwait(false);
            if (connectionEndPoint == null)
            {
                throw new HiveMQttClientException("Failed to resolve proxy server address. Check your proxy configuration.");
            }

            Logger.Trace($"Using HTTP proxy at {connectionEndPoint}");
        }
        else
        {
            // Direct connection to broker
            if (IPAddress.TryParse(this.Options.Host, out var parsedIp))
            {
                connectionEndPoint = new IPEndPoint(parsedIp, this.Options.Port);
            }
            else
            {
                var lookupResult = await LookupHostNameAsync(this.Options.Host, this.Options.PreferIPv6).ConfigureAwait(false);

                if (lookupResult != null)
                {
                    connectionEndPoint = new IPEndPoint(lookupResult, this.Options.Port);
                }
            }

            if (connectionEndPoint == null)
            {
                throw new HiveMQttClientException("Failed to create IPEndPoint. Broker is no valid IP address or hostname.");
            }
        }

        this.Socket = new(connectionEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

        try
        {
            await this.Socket.ConnectAsync(connectionEndPoint).ConfigureAwait(false);
        }
        catch (SocketException socketException)
        {
            var target = useProxy ? "proxy server" : "broker";
            throw new HiveMQttClientException($"Failed to connect to {target}", socketException);
        }

        var socketConnected = this.Socket.Connected;
        if (!socketConnected || this.Socket == null)
        {
            throw new HiveMQttClientException("Failed to connect socket");
        }

        // Setup the stream
        var networkStream = new NetworkStream(this.Socket);
        this.Stream = networkStream;

        // If using a proxy, establish the HTTP CONNECT tunnel
        if (useProxy)
        {
            var tunnelEstablished = await this.EstablishProxyTunnelAsync(networkStream, this.Options.Host, this.Options.Port, cancellationToken).ConfigureAwait(false);
            if (!tunnelEstablished)
            {
                throw new HiveMQttClientException("Failed to establish HTTP CONNECT tunnel through proxy");
            }
        }

        if (this.Options.UseTLS)
        {
            var result = await this.CreateTLSConnectionAsync(this.Stream).ConfigureAwait(false);
            if (!result)
            {
                throw new HiveMQttClientException("Failed to create TLS connection");
            }
        }

        // Setup the Pipeline
        this.Reader = PipeReader.Create(this.Stream);
        this.Writer = PipeWriter.Create(this.Stream);

        if (useProxy)
        {
            Logger.Trace($"Socket connected to broker {this.Options.Host}:{this.Options.Port} through proxy");
        }
        else
        {
            Logger.Trace($"Socket connected to {this.Socket.RemoteEndPoint}");
        }

        return socketConnected;
    }

    /// <summary>
    /// Close the TCP connection.
    /// </summary>
    /// <param name="shutdownPipeline">A boolean indicating whether to shutdown the pipeline.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A boolean indicating whether the operation was successful.</returns>
    public override async Task<bool> CloseAsync(bool? shutdownPipeline = true, CancellationToken cancellationToken = default)
    {
        if (shutdownPipeline == true)
        {
            if (this.Reader != null && this.Writer != null)
            {
                // Dispose of the PipeReader and PipeWriter
                await this.Reader.CompleteAsync().ConfigureAwait(false);
                await this.Writer.CompleteAsync().ConfigureAwait(false);

                // Shutdown the pipeline
                this.Reader = null;
                this.Writer = null;
            }
        }

        if (this.Stream != null)
        {
            // Dispose of the Stream
            await this.Stream.FlushAsync(cancellationToken).ConfigureAwait(false);
            this.Stream.Close();
            await this.Stream.DisposeAsync().ConfigureAwait(false);
            this.Stream = null;
        }

        // Check if the socket is initialized and open
        if (this.Socket != null && this.Socket.Connected)
        {
            // Shutdown the socket
            this.Socket.Shutdown(SocketShutdown.Both);
            this.Socket.Close();
            this.Socket.Dispose();
            this.Socket = null;
        }

        return true;
    }

    /// <summary>
    /// Write a buffer to the stream.
    /// </summary>
    /// <param name="buffer">The buffer to write.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A boolean indicating whether the write was successful.</returns>
    /// <exception cref="HiveMQttClientException">Raised if the writer is null.</exception>
    public override async Task<bool> WriteAsync(byte[] buffer, CancellationToken cancellationToken = default)
    {
        if (this.Writer == null)
        {
            throw new HiveMQttClientException("TCP Transport Writer is null");
        }

        // Serialize write operations to prevent concurrent writes that cause NotSupportedException
        await this.writeSemaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            var source = new ReadOnlyMemory<byte>(buffer);
            var writeResult = await this.Writer.WriteAsync(source, cancellationToken).ConfigureAwait(false);

            if (writeResult.IsCompleted || writeResult.IsCanceled)
            {
                Logger.Debug($"-(TCP)- WriteAsync: The party is over. IsCompleted={writeResult.IsCompleted} IsCancelled={writeResult.IsCanceled}");
                return false;
            }

            return true;
        }
        finally
        {
            this.writeSemaphore.Release();
        }
    }

    /// <summary>
    /// Read a buffer from the stream.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A TransportReadResult object containing the buffer.</returns>
    /// <exception cref="HiveMQttClientException">Raised if the reader is null.</exception>
    public override async Task<TransportReadResult> ReadAsync(CancellationToken cancellationToken = default)
    {
        if (this.Reader == null)
        {
            throw new HiveMQttClientException("Reader is null");
        }

        ReadResult readResult;
        try
        {
            readResult = await this.Reader.ReadAsync(cancellationToken).ConfigureAwait(false);

            if (readResult.IsCanceled || readResult.IsCompleted)
            {
                Logger.Debug($"-(TCP)- ReadAsync: The party is over. IsCompleted={readResult.IsCompleted} IsCancelled={readResult.IsCanceled}");
                return new TransportReadResult(true);
            }
        }
        catch (SocketException ex)
        {
            Logger.Debug($"SocketException in ReadAsync: {ex.Message}");
            return new TransportReadResult(true);
        }
        catch (IOException ex)
        {
            Logger.Debug($"SocketException in ReadAsync: {ex.Message}");
            return new TransportReadResult(true);
        }

        return new TransportReadResult(readResult.Buffer);
    }

    public override void AdvanceTo(SequencePosition consumed) => this.Reader?.AdvanceTo(consumed);

    public override void AdvanceTo(SequencePosition consumed, SequencePosition examined) => this.Reader?.AdvanceTo(consumed, examined);

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
        Logger.Trace("Disposing TCPTransport");

        // Check to see if Dispose has already been called.
        if (!this.disposed)
        {
            // If disposing equals true, dispose all managed
            // and unmanaged resources.
            if (disposing)
            {
                // Dispose of the write semaphore
                this.writeSemaphore?.Dispose();

                // Dispose of the PipeReader and PipeWriter
                this.Reader?.Complete();
                this.Writer?.Complete();
                this.Reader = null;
                this.Writer = null;

                // Dispose of the Stream
                if (this.Stream != null)
                {
                    try
                    {
                        this.Stream.Flush();
                        this.Stream.Close();
                    }
                    catch (Exception ex)
                    {
                        Logger.Warn($"Error closing stream: {ex.Message}");
                    }
                    finally
                    {
                        this.Stream.Dispose();
                        this.Stream = null;
                    }
                }

                // Dispose of the Socket
                if (this.Socket != null)
                {
                    try
                    {
                        if (this.Socket.Connected)
                        {
                            this.Socket.Shutdown(SocketShutdown.Both);
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Warn($"Error shutting down socket: {ex.Message}");
                    }
                    finally
                    {
                        this.Socket.Close();
                        this.Socket.Dispose();
                        this.Socket = null;
                    }
                }
            }

            // Note disposing has been done.
            this.disposed = true;
        }
    }

    private bool disposed;
}
