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

using System.Data;
using System.IO.Pipelines;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using HiveMQtt.Client.Exceptions;
using HiveMQtt.Client.Options;

public class TCPTransport : BaseTransport, IDisposable
{
    internal Socket? Socket { get; set; }

    internal Stream? Stream { get; set; }

    internal PipeReader? Reader { get; set; }

    internal PipeWriter? Writer { get; set; }

    internal HiveMQClientOptions Options { get; }

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
        // Ignore the unused parameters
        _ = sender;
        _ = certificate;
        _ = chain;

        if (sslPolicyErrors == SslPolicyErrors.None)
        {
            return true;
        }

        Logger.Warn("Broker TLS Certificate error: {0}", sslPolicyErrors);

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

        Logger.Trace($"Socket connected to {this.Socket.RemoteEndPoint}");
        return socketConnected;
    }

    /// <summary>
    /// Close the TCP connection.
    /// </summary>
    /// <param name="shutdownPipeline">A boolean indicating whether to shutdown the pipeline.</param>
    /// <returns>A boolean indicating whether the operation was successful.</returns>
    public override async Task<bool> CloseAsync(bool? shutdownPipeline = true)
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

        var source = new ReadOnlyMemory<byte>(buffer);
        var writeResult = await this.Writer.WriteAsync(source, cancellationToken).ConfigureAwait(false);

        if (writeResult.IsCompleted || writeResult.IsCanceled)
        {
            Logger.Debug($"-(TCP)- WriteAsync: The party is over. IsCompleted={writeResult.IsCompleted} IsCancelled={writeResult.IsCanceled}");
            return false;
        }

        return true;
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
