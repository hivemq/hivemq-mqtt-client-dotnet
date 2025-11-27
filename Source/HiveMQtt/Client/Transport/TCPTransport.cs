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
using System.IO.Pipelines;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using HiveMQtt.Client.Exceptions;
using HiveMQtt.Client.Options;

/// <summary>
/// TCP transport implementation for MQTT over TCP connections.
/// </summary>
public partial class TCPTransport : BaseTransport, IDisposable
{
    internal Socket? Socket { get; set; }

    internal Stream? Stream { get; set; }

    internal PipeReader? Reader { get; set; }

    internal PipeWriter? Writer { get; set; }

    internal HiveMQClientOptions Options { get; }

    // Semaphore to serialize write operations and prevent concurrent writes
    private readonly SemaphoreSlim writeSemaphore = new(1, 1);

    public TCPTransport(HiveMQClientOptions options)
        : base(options.LoggerFactory) => this.Options = options;

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
    /// <param name="logger">The logger instance to use for logging.</param>
    /// <param name="sender">An object that contains state information for this validation.</param>
    /// <param name="certificate">The certificate used to authenticate the remote party.</param>
    /// <param name="chain">The chain of certificate authorities associated with the remote certificate.</param>
    /// <param name="sslPolicyErrors">One or more errors associated with the remote certificate.</param>
    /// <returns>A Boolean indicating whether the TLS certificate is valid.</returns>
    private static bool ValidateServerCertificate(
        ILogger logger,
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

        LogBrokerTLSCertificateError(logger, sslPolicyErrors);

        // Log additional certificate details for debugging
        if (certificate != null)
        {
            LogCertificateSubject(logger, certificate.Subject);
            LogCertificateIssuer(logger, certificate.Issuer);
            LogCertificateSerialNumber(logger, certificate.GetSerialNumberString());
        }

        // Validate certificate chain if provided
        if (chain != null)
        {
            var chainStatus = chain.ChainStatus.Length > 0 ? string.Join(", ", chain.ChainStatus.Select(cs => cs.Status)) : "Valid";
            LogCertificateChainStatus(logger, chainStatus);
        }

        // Do not allow this client to communicate with unauthenticated servers.
        return false;
    }

    private async Task<bool> CreateTLSConnectionAsync(Stream stream)
    {
        LogCreatingTLSConnection(this.Logger);

        var tlsOptions = new SslClientAuthenticationOptions
        {
            TargetHost = this.Options.Host,
            EnabledSslProtocols = SslProtocols.Tls13 | SslProtocols.Tls12,
            ClientCertificates = this.Options.ClientCertificates,
        };

        if (this.Options.AllowInvalidBrokerCertificates)
        {
            LogAllowingInvalidBrokerCertificates(this.Logger);
#pragma warning disable CA5359
            var yesMan = new RemoteCertificateValidationCallback((sender, certificate, chain, errors) => true);
#pragma warning restore CA5359
            tlsOptions.RemoteCertificateValidationCallback = yesMan;
        }
        else
        {
            var logger = this.Logger;
            tlsOptions.RemoteCertificateValidationCallback = (sender, certificate, chain, errors) =>
                ValidateServerCertificate(logger, sender, certificate, chain, errors);
        }

        try
        {
            LogAuthenticatingTLSConnection(this.Logger);
            this.Stream = new SslStream(stream);
            await ((SslStream)this.Stream).AuthenticateAsClientAsync(tlsOptions).ConfigureAwait(false);

            LogConnectedViaTLS(this.Logger, ((SslStream)this.Stream).IsEncrypted);
            LogCipherAlgorithm(this.Logger, ((SslStream)this.Stream).CipherAlgorithm);
            LogCipherStrength(this.Logger, ((SslStream)this.Stream).CipherStrength);
            LogHashAlgorithm(this.Logger, ((SslStream)this.Stream).HashAlgorithm);
            LogHashStrength(this.Logger, ((SslStream)this.Stream).HashStrength);
            LogKeyExchangeAlgorithm(this.Logger, ((SslStream)this.Stream).KeyExchangeAlgorithm);
            LogKeyExchangeStrength(this.Logger, ((SslStream)this.Stream).KeyExchangeStrength);

            var remoteCertificate = ((SslStream)this.Stream).RemoteCertificate;
            if (remoteCertificate != null)
            {
                LogRemoteCertificateSubject(this.Logger, remoteCertificate.Subject);
                LogRemoteCertificateIssuer(this.Logger, remoteCertificate.Issuer);
                LogRemoteCertificateSerialNumber(this.Logger, remoteCertificate.GetSerialNumberString());
            }

            LogTLSProtocol(this.Logger, ((SslStream)this.Stream).SslProtocol);
            return true;
        }
        catch (Exception e)
        {
            LogTLSConnectionError(this.Logger, e);
            if (e.InnerException != null)
            {
                LogTLSConnectionInnerException(this.Logger, e.InnerException);
            }

            LogTLSEstablishmentError(this.Logger);
            return false;
        }
    }

    /// <summary>
    /// Make a TCP connection to a remote broker.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A boolean representing the success or failure of the operation.</returns>
    public override async Task<bool> ConnectAsync(CancellationToken cancellationToken = default)
    {
        IPEndPoint? ipEndPoint = null;

        if (IPAddress.TryParse(this.Options.Host, out var parsedIp))
        {
            ipEndPoint = new IPEndPoint(parsedIp, this.Options.Port);
        }
        else
        {
            var lookupResult = await LookupHostNameAsync(this.Options.Host, this.Options.PreferIPv6).ConfigureAwait(false);

            if (lookupResult != null)
            {
                ipEndPoint = new IPEndPoint(lookupResult, this.Options.Port);
            }
        }

        if (ipEndPoint == null)
        {
            throw new HiveMQttClientException("Failed to create IPEndPoint. Broker is no valid IP address or hostname.");
        }

        this.Socket = new(ipEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

        try
        {
            await this.Socket.ConnectAsync(ipEndPoint).ConfigureAwait(false);
        }
        catch (SocketException socketException)
        {
            throw new HiveMQttClientException("Failed to connect to broker", socketException);
        }

        var socketConnected = this.Socket.Connected;
        if (!socketConnected || this.Socket == null)
        {
            throw new HiveMQttClientException("Failed to connect socket");
        }

        // Setup the stream
        this.Stream = new NetworkStream(this.Socket);

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

        LogSocketConnected(this.Logger, this.Socket.RemoteEndPoint);
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
                LogTCPWriteAsyncPartyOver(this.Logger, writeResult.IsCompleted, writeResult.IsCanceled);
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
                LogTCPReadAsyncPartyOver(this.Logger, readResult.IsCompleted, readResult.IsCanceled);
                return new TransportReadResult(true);
            }
        }
        catch (SocketException ex)
        {
            LogSocketExceptionInRead(this.Logger, ex, ex.Message);
            return new TransportReadResult(true);
        }
        catch (IOException ex)
        {
            LogIOExceptionInRead(this.Logger, ex, ex.Message);
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
        LogDisposingTCPTransport(this.Logger);

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
                        LogErrorClosingStream(this.Logger, ex, ex.Message);
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
                        LogErrorShuttingDownSocket(this.Logger, ex, ex.Message);
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
