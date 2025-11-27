namespace HiveMQtt.Client.Transport;

using System;
using System.Net.Security;
using System.Net.WebSockets;
using Microsoft.Extensions.Logging;

/// <summary>
/// Source-generated logging methods for WebSocketTransport using LoggerMessage pattern.
/// This provides zero-overhead logging when logging is disabled.
/// </summary>
public partial class WebSocketTransport
{
    [LoggerMessage(
        EventId = 11001,
        Level = LogLevel.Warning,
        Message = "Broker TLS Certificate error for WebSocket: {SslPolicyErrors}")]
    private static partial void LogBrokerTLSCertificateError(ILogger logger, SslPolicyErrors sslPolicyErrors);

    [LoggerMessage(
        EventId = 11002,
        Level = LogLevel.Debug,
        Message = "WebSocket Certificate Subject: {Subject}")]
    private static partial void LogWebSocketCertificateSubject(ILogger logger, string subject);

    [LoggerMessage(
        EventId = 11003,
        Level = LogLevel.Debug,
        Message = "WebSocket Certificate Issuer: {Issuer}")]
    private static partial void LogWebSocketCertificateIssuer(ILogger logger, string issuer);

    [LoggerMessage(
        EventId = 11004,
        Level = LogLevel.Debug,
        Message = "WebSocket Certificate Serial Number: {SerialNumber}")]
    private static partial void LogWebSocketCertificateSerialNumber(ILogger logger, string serialNumber);

    [LoggerMessage(
        EventId = 11005,
        Level = LogLevel.Debug,
        Message = "WebSocket Certificate chain validation status: {ChainStatus}")]
    private static partial void LogWebSocketCertificateChainStatus(ILogger logger, string chainStatus);

    [LoggerMessage(
        EventId = 11006,
        Level = LogLevel.Trace,
        Message = "WebSocket keep-alive interval set to {KeepAliveInterval}")]
    private static partial void LogWebSocketKeepAliveInterval(ILogger logger, TimeSpan keepAliveInterval);

    [LoggerMessage(
        EventId = 11007,
        Level = LogLevel.Trace,
        Message = "Added {HeaderCount} custom header(s) for WebSocket connection")]
    private static partial void LogWebSocketCustomHeadersAdded(ILogger logger, int headerCount);

    [LoggerMessage(
        EventId = 11008,
        Level = LogLevel.Trace,
        Message = "WebSocket proxy configured: {Proxy}")]
    private static partial void LogWebSocketProxyConfigured(ILogger logger, System.Net.IWebProxy proxy);

    [LoggerMessage(
        EventId = 11009,
        Level = LogLevel.Trace,
        Message = "Added {CertificateCount} client certificate(s) for WebSocket connection")]
    private static partial void LogWebSocketClientCertificatesAdded(ILogger logger, int certificateCount);

    [LoggerMessage(
        EventId = 11010,
        Level = LogLevel.Trace,
        Message = "Allowing invalid broker certificates for WebSocket connection")]
    private static partial void LogWebSocketAllowingInvalidCertificates(ILogger logger);

    [LoggerMessage(
        EventId = 11011,
        Level = LogLevel.Debug,
        Message = "Failed to connect to the WebSocket server")]
    private static partial void LogWebSocketConnectFailed(ILogger logger, Exception ex);

    [LoggerMessage(
        EventId = 11012,
        Level = LogLevel.Debug,
        Message = "WebSocket is already aborted")]
    private static partial void LogWebSocketAlreadyAborted(ILogger logger);

    [LoggerMessage(
        EventId = 11013,
        Level = LogLevel.Debug,
        Message = "WebSocket is already closed")]
    private static partial void LogWebSocketAlreadyClosed(ILogger logger);

    [LoggerMessage(
        EventId = 11014,
        Level = LogLevel.Debug,
        Message = "WebSocket is in state: {State}")]
    private static partial void LogWebSocketState(ILogger logger, WebSocketState state);

    [LoggerMessage(
        EventId = 11015,
        Level = LogLevel.Warning,
        Message = "Error closing WebSocket connection")]
    private static partial void LogWebSocketCloseError(ILogger logger, Exception ex);

    [LoggerMessage(
        EventId = 11016,
        Level = LogLevel.Debug,
        Message = "WebSocket has already been disposed")]
    private static partial void LogWebSocketAlreadyDisposed(ILogger logger, Exception ex);

    [LoggerMessage(
        EventId = 11017,
        Level = LogLevel.Debug,
        Message = "WebSocket is already closed or closing")]
    private static partial void LogWebSocketAlreadyClosedOrClosing(ILogger logger, Exception ex);

    [LoggerMessage(
        EventId = 11018,
        Level = LogLevel.Debug,
        Message = "Close operation was cancelled")]
    private static partial void LogWebSocketCloseCancelled(ILogger logger, Exception ex);

    [LoggerMessage(
        EventId = 11019,
        Level = LogLevel.Debug,
        Message = "Write operation was cancelled while waiting for semaphore")]
    private static partial void LogWebSocketWriteCancelled(ILogger logger);

    [LoggerMessage(
        EventId = 11020,
        Level = LogLevel.Debug,
        Message = "WebSocket is not in a writable state: {State}")]
    private static partial void LogWebSocketNotWritable(ILogger logger, WebSocketState state);

    [LoggerMessage(
        EventId = 11021,
        Level = LogLevel.Debug,
        Message = "Failed to write to the WebSocket server")]
    private static partial void LogWebSocketWriteFailed(ILogger logger, Exception ex);

    [LoggerMessage(
        EventId = 11022,
        Level = LogLevel.Debug,
        Message = "WebSocket is not in a readable state: {State}")]
    private static partial void LogWebSocketNotReadable(ILogger logger, WebSocketState state);

    [LoggerMessage(
        EventId = 11023,
        Level = LogLevel.Debug,
        Message = "WebSocket received close message: {CloseStatus}")]
    private static partial void LogWebSocketReceivedCloseMessage(ILogger logger, WebSocketCloseStatus? closeStatus);

    [LoggerMessage(
        EventId = 11024,
        Level = LogLevel.Debug,
        Message = "WebSocket connection closed during read: {CloseStatus}")]
    private static partial void LogWebSocketConnectionClosedDuringRead(ILogger logger, WebSocketCloseStatus? closeStatus);

    [LoggerMessage(
        EventId = 11025,
        Level = LogLevel.Trace,
        Message = "Received {Count} bytes (single read)")]
    private static partial void LogWebSocketReceivedBytesSingle(ILogger logger, int count);

    [LoggerMessage(
        EventId = 11026,
        Level = LogLevel.Trace,
        Message = "Received {Length} bytes (fragmented)")]
    private static partial void LogWebSocketReceivedBytesFragmented(ILogger logger, long length);

    [LoggerMessage(
        EventId = 11027,
        Level = LogLevel.Warning,
        Message = "Received unexpected WebSocket message type: {MessageType}")]
    private static partial void LogWebSocketUnexpectedMessageType(ILogger logger, WebSocketMessageType messageType);

    [LoggerMessage(
        EventId = 11028,
        Level = LogLevel.Debug,
        Message = "Failed to read from the WebSocket server")]
    private static partial void LogWebSocketReadFailed(ILogger logger, Exception ex);

    [LoggerMessage(
        EventId = 11029,
        Level = LogLevel.Debug,
        Message = "Read operation was canceled")]
    private static partial void LogWebSocketReadCancelled(ILogger logger, Exception ex);

    [LoggerMessage(
        EventId = 11030,
        Level = LogLevel.Debug,
        Message = "WebSocket operation is invalid - socket may be closed")]
    private static partial void LogWebSocketOperationInvalid(ILogger logger, Exception ex);

    [LoggerMessage(
        EventId = 11031,
        Level = LogLevel.Trace,
        Message = "Disposing WebSocketTransport")]
    private static partial void LogDisposingWebSocketTransport(ILogger logger);

    [LoggerMessage(
        EventId = 11032,
        Level = LogLevel.Warning,
        Message = "Error closing WebSocket: {Message}")]
    private static partial void LogWebSocketDisposeCloseError(ILogger logger, Exception ex, string message);
}
