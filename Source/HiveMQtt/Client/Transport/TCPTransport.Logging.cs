namespace HiveMQtt.Client.Transport;

using Microsoft.Extensions.Logging;
using System.Net.Security;

/// <summary>
/// Source-generated logging methods for TCPTransport using LoggerMessage pattern.
/// This provides zero-overhead logging when logging is disabled.
/// </summary>
public partial class TCPTransport
{
    [LoggerMessage(
        EventId = 9001,
        Level = LogLevel.Warning,
        Message = "Broker TLS Certificate error: {SslPolicyErrors}")]
    private static partial void LogBrokerTLSCertificateError(ILogger logger, SslPolicyErrors sslPolicyErrors);

    [LoggerMessage(
        EventId = 9002,
        Level = LogLevel.Debug,
        Message = "Certificate Subject: {Subject}")]
    private static partial void LogCertificateSubject(ILogger logger, string subject);

    [LoggerMessage(
        EventId = 9003,
        Level = LogLevel.Debug,
        Message = "Certificate Issuer: {Issuer}")]
    private static partial void LogCertificateIssuer(ILogger logger, string issuer);

    [LoggerMessage(
        EventId = 9004,
        Level = LogLevel.Debug,
        Message = "Certificate Serial Number: {SerialNumber}")]
    private static partial void LogCertificateSerialNumber(ILogger logger, string serialNumber);

    [LoggerMessage(
        EventId = 9005,
        Level = LogLevel.Debug,
        Message = "Certificate chain validation status: {ChainStatus}")]
    private static partial void LogCertificateChainStatus(ILogger logger, string chainStatus);

    [LoggerMessage(
        EventId = 9006,
        Level = LogLevel.Trace,
        Message = "Creating TLS connection")]
    private static partial void LogCreatingTLSConnection(ILogger logger);

    [LoggerMessage(
        EventId = 9007,
        Level = LogLevel.Trace,
        Message = "Allowing invalid broker certificates")]
    private static partial void LogAllowingInvalidBrokerCertificates(ILogger logger);

    [LoggerMessage(
        EventId = 9008,
        Level = LogLevel.Trace,
        Message = "Authenticating TLS connection")]
    private static partial void LogAuthenticatingTLSConnection(ILogger logger);

    [LoggerMessage(
        EventId = 9009,
        Level = LogLevel.Information,
        Message = "Connected via TLS: {IsEncrypted}")]
    private static partial void LogConnectedViaTLS(ILogger logger, bool isEncrypted);

    [LoggerMessage(
        EventId = 9010,
        Level = LogLevel.Debug,
        Message = "Cipher Algorithm: {CipherAlgorithm}")]
    private static partial void LogCipherAlgorithm(ILogger logger, System.Security.Authentication.CipherAlgorithmType cipherAlgorithm);

    [LoggerMessage(
        EventId = 9011,
        Level = LogLevel.Debug,
        Message = "Cipher Strength: {CipherStrength}")]
    private static partial void LogCipherStrength(ILogger logger, int cipherStrength);

    [LoggerMessage(
        EventId = 9012,
        Level = LogLevel.Debug,
        Message = "Hash Algorithm: {HashAlgorithm}")]
    private static partial void LogHashAlgorithm(ILogger logger, System.Security.Authentication.HashAlgorithmType hashAlgorithm);

    [LoggerMessage(
        EventId = 9013,
        Level = LogLevel.Debug,
        Message = "Hash Strength: {HashStrength}")]
    private static partial void LogHashStrength(ILogger logger, int hashStrength);

    [LoggerMessage(
        EventId = 9014,
        Level = LogLevel.Debug,
        Message = "Key Exchange Algorithm: {KeyExchangeAlgorithm}")]
    private static partial void LogKeyExchangeAlgorithm(ILogger logger, System.Security.Authentication.ExchangeAlgorithmType keyExchangeAlgorithm);

    [LoggerMessage(
        EventId = 9015,
        Level = LogLevel.Debug,
        Message = "Key Exchange Strength: {KeyExchangeStrength}")]
    private static partial void LogKeyExchangeStrength(ILogger logger, int keyExchangeStrength);

    [LoggerMessage(
        EventId = 9016,
        Level = LogLevel.Information,
        Message = "Remote Certificate Subject: {Subject}")]
    private static partial void LogRemoteCertificateSubject(ILogger logger, string subject);

    [LoggerMessage(
        EventId = 9017,
        Level = LogLevel.Information,
        Message = "Remote Certificate Issuer: {Issuer}")]
    private static partial void LogRemoteCertificateIssuer(ILogger logger, string issuer);

    [LoggerMessage(
        EventId = 9018,
        Level = LogLevel.Information,
        Message = "Remote Certificate Serial Number: {SerialNumber}")]
    private static partial void LogRemoteCertificateSerialNumber(ILogger logger, string serialNumber);

    [LoggerMessage(
        EventId = 9019,
        Level = LogLevel.Information,
        Message = "TLS Protocol: {SslProtocol}")]
    private static partial void LogTLSProtocol(ILogger logger, System.Security.Authentication.SslProtocols sslProtocol);

    [LoggerMessage(
        EventId = 9020,
        Level = LogLevel.Error,
        Message = "TLS connection error")]
    private static partial void LogTLSConnectionError(ILogger logger, System.Exception ex);

    [LoggerMessage(
        EventId = 9021,
        Level = LogLevel.Error,
        Message = "TLS connection inner exception")]
    private static partial void LogTLSConnectionInnerException(ILogger logger, System.Exception ex);

    [LoggerMessage(
        EventId = 9022,
        Level = LogLevel.Error,
        Message = "Error while establishing TLS connection - closing the connection.")]
    private static partial void LogTLSEstablishmentError(ILogger logger);

    [LoggerMessage(
        EventId = 9023,
        Level = LogLevel.Trace,
        Message = "Socket connected to {RemoteEndPoint}")]
    private static partial void LogSocketConnected(ILogger logger, System.Net.EndPoint? remoteEndPoint);

    [LoggerMessage(
        EventId = 9024,
        Level = LogLevel.Debug,
        Message = "-(TCP)- WriteAsync: The party is over. IsCompleted={IsCompleted} IsCancelled={IsCancelled}")]
    private static partial void LogTCPWriteAsyncPartyOver(ILogger logger, bool isCompleted, bool isCancelled);

    [LoggerMessage(
        EventId = 9025,
        Level = LogLevel.Debug,
        Message = "-(TCP)- ReadAsync: The party is over. IsCompleted={IsCompleted} IsCancelled={IsCancelled}")]
    private static partial void LogTCPReadAsyncPartyOver(ILogger logger, bool isCompleted, bool isCancelled);

    [LoggerMessage(
        EventId = 9026,
        Level = LogLevel.Debug,
        Message = "SocketException in Read: {Message}")]
    private static partial void LogSocketExceptionInRead(ILogger logger, System.Exception ex, string message);

    [LoggerMessage(
        EventId = 9027,
        Level = LogLevel.Debug,
        Message = "IOException in Read: {Message}")]
    private static partial void LogIOExceptionInRead(ILogger logger, System.Exception ex, string message);

    [LoggerMessage(
        EventId = 9028,
        Level = LogLevel.Trace,
        Message = "Disposing TCPTransport")]
    private static partial void LogDisposingTCPTransport(ILogger logger);

    [LoggerMessage(
        EventId = 9029,
        Level = LogLevel.Warning,
        Message = "Error closing stream: {Message}")]
    private static partial void LogErrorClosingStream(ILogger logger, System.Exception ex, string message);

    [LoggerMessage(
        EventId = 9030,
        Level = LogLevel.Warning,
        Message = "Error shutting down socket: {Message}")]
    private static partial void LogErrorShuttingDownSocket(ILogger logger, System.Exception ex, string message);
}
