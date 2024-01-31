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
    private Socket? socket;
    private Stream? stream;
    private PipeReader? reader;
    private PipeWriter? writer;
    private CancellationTokenSource cancellationSource;
    private CancellationToken outFlowCancellationToken;
    private CancellationToken inFlowCancellationToken;
    private CancellationToken receivedPacketsCancellationToken;

#pragma warning disable IDE0052
    private Task? trafficOutflowProcessorTask;
    private Task? trafficInflowProcessorTask;
    private Task? receivedPacketsProcessorAsync;
#pragma warning restore IDE0052

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
    internal async Task<bool> ConnectSocketAsync()
    {
        IPAddress? ipAddress = null;
        var ipHostInfo = await Dns.GetHostEntryAsync(this.Options.Host).ConfigureAwait(false);

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

        IPEndPoint ipEndPoint = new(ipAddress, this.Options.Port);

        this.socket = new(ipEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

        try
        {
            await this.socket.ConnectAsync(ipEndPoint).ConfigureAwait(false);
        }
        catch (SocketException socketException)
        {
            throw new HiveMQttClientException("Failed to connect to broker", socketException);
        }

        var socketConnected = this.socket.Connected;
        if (!socketConnected || this.socket == null)
        {
            throw new HiveMQttClientException("Failed to connect socket");
        }

        // Setup the stream
        this.stream = new NetworkStream(this.socket);

        if (this.Options.UseTLS)
        {
            var result = await this.CreateTLSConnectionAsync(this.stream).ConfigureAwait(false);
            if (!result)
            {
                throw new HiveMQttClientException("Failed to create TLS connection");
            }
        }

        // Setup the Pipeline
        this.reader = PipeReader.Create(this.stream);
        this.writer = PipeWriter.Create(this.stream);

        // Reset the CancellationTokenSource in case this is a reconnect
        this.cancellationSource.Dispose();
        this.cancellationSource = new CancellationTokenSource();

        // Setup the cancellation tokens
        this.outFlowCancellationToken = this.cancellationSource.Token;
        this.inFlowCancellationToken = this.cancellationSource.Token;
        this.receivedPacketsCancellationToken = this.cancellationSource.Token;

        // Start the traffic processors
        this.trafficOutflowProcessorTask = this.TrafficOutflowProcessorAsync(this.outFlowCancellationToken);
        this.trafficInflowProcessorTask = this.TrafficInflowProcessorAsync(this.inFlowCancellationToken);
        this.receivedPacketsProcessorAsync = this.ReceivedPacketsProcessorAsync(this.receivedPacketsCancellationToken);

        Logger.Trace($"Socket connected to {this.socket.RemoteEndPoint}");
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
            tlsOptions.RemoteCertificateValidationCallback = HiveMQClient.ValidateServerCertificate;
        }

        try
        {
            Logger.Trace("Authenticating TLS connection");
            this.stream = new SslStream(stream);
            await ((SslStream)this.stream).AuthenticateAsClientAsync(tlsOptions).ConfigureAwait(false);

            Logger.Info($"Connected via TLS: {((SslStream)this.stream).IsEncrypted}");
            Logger.Debug($"Cipher Algorithm: {((SslStream)this.stream).CipherAlgorithm}");
            Logger.Debug($"Cipher Strength: {((SslStream)this.stream).CipherStrength}");
            Logger.Debug($"Hash Algorithm: {((SslStream)this.stream).HashAlgorithm}");
            Logger.Debug($"Hash Strength: {((SslStream)this.stream).HashStrength}");
            Logger.Debug($"Key Exchange Algorithm: {((SslStream)this.stream).KeyExchangeAlgorithm}");
            Logger.Debug($"Key Exchange Strength: {((SslStream)this.stream).KeyExchangeStrength}");

            var remoteCertificate = ((SslStream)this.stream).RemoteCertificate;
            if (remoteCertificate != null)
            {
                Logger.Info($"Remote Certificate Subject: {remoteCertificate.Subject}");
                Logger.Info($"Remote Certificate Issuer: {remoteCertificate.Issuer}");
                Logger.Info($"Remote Certificate Serial Number: {remoteCertificate.GetSerialNumberString()}");
            }

            Logger.Info($"TLS Protocol: {((SslStream)this.stream).SslProtocol}");
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

    internal bool CloseSocket(bool? shutdownPipeline = true)
    {
        if (shutdownPipeline == true)
        {
            // Shutdown the pipeline
            this.reader = null;
            this.writer = null;
        }

        // Shutdown the socket
        this.socket?.Shutdown(SocketShutdown.Both);
        this.socket?.Close();

        this.cancellationSource.Cancel();

        return true;
    }
}
