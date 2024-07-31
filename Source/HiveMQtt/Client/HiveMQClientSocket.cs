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
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;

using System.Threading;
using System.Threading.Tasks;

using HiveMQtt.Client.Exceptions;

/// <inheritdoc />
public partial class HiveMQClient : IDisposable, IHiveMQClient
{
    internal Socket? Socket { get; set; }

    internal Stream? Stream { get; set; }

    internal PipeReader? Reader { get; set; }

    internal PipeWriter? Writer { get; set; }

    private CancellationTokenSource cancellationTokenSource;

    internal Task? ConnectionPublishWriterTask { get; set; }

    internal Task? ConnectionWriterTask { get; set; }

    internal Task? ConnectionReaderTask { get; set; }

    internal Task? ReceivedPacketsHandlerTask { get; set; }

    internal Task? ConnectionMonitorTask { get; set; }

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
        if (sslPolicyErrors == SslPolicyErrors.None)
        {
            return true;
        }

        Logger.Warn("Broker TLS Certificate error: {0}", sslPolicyErrors);

        // Do not allow this client to communicate with unauthenticated servers.
        return false;
    }

    /// <summary>
    /// Make a TCP connection to a remote broker.
    /// </summary>
    /// <returns>A boolean representing the success or failure of the operation.</returns>
    internal async Task<bool> ConnectTCPSocketAsync()
    {
        IPEndPoint ipEndPoint;
        var ipAddress = await this.LookupHostNameAsync(this.Options.Host).ConfigureAwait(false);

        // Create the IPEndPoint depending on whether it is a host name or IP address.
        if (ipAddress == null)
        {
            ipEndPoint = new IPEndPoint(IPAddress.Parse(this.Options.Host), this.Options.Port);
        }
        else
        {
            ipEndPoint = new IPEndPoint(ipAddress, this.Options.Port);
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

        // Reset the CancellationTokenSource in case this is a reconnect
        this.cancellationTokenSource.Dispose();
        this.cancellationTokenSource = new CancellationTokenSource();

        // Start the traffic processors
        this.ConnectionPublishWriterTask = this.ConnectionPublishWriterAsync(this.cancellationTokenSource.Token);
        this.ConnectionWriterTask = this.ConnectionWriterAsync(this.cancellationTokenSource.Token);
        this.ConnectionReaderTask = this.ConnectionReaderAsync(this.cancellationTokenSource.Token);
        this.ReceivedPacketsHandlerTask = this.ReceivedPacketsHandlerAsync(this.cancellationTokenSource.Token);
        this.ConnectionMonitorTask = this.ConnectionMonitorAsync(this.cancellationTokenSource.Token);

        Logger.Trace($"Socket connected to {this.Socket.RemoteEndPoint}");
        return socketConnected;
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

    internal async Task<bool> CloseSocketAsync(bool? shutdownPipeline = true)
    {
        await this.CancelBackgroundTasksAsync().ConfigureAwait(false);

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

    private async Task<IPAddress?> LookupHostNameAsync(string host)
    {
        try
        {
            IPAddress? ipAddress = null;
            var ipHostInfo = await Dns.GetHostEntryAsync(host).ConfigureAwait(false);

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
            return ipAddress;
        }
        catch (SocketException socketException)
        {
            Logger.Debug(socketException.Message);
            return null;
        }
    }
}
