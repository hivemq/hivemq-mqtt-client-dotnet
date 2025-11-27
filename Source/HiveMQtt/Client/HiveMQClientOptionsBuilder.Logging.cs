namespace HiveMQtt.Client;

using Microsoft.Extensions.Logging;

/// <summary>
/// Source-generated logging methods for HiveMQClientOptionsBuilder using LoggerMessage pattern.
/// This provides zero-overhead logging when logging is disabled.
/// </summary>
public partial class HiveMQClientOptionsBuilder
{
    [LoggerMessage(
        EventId = 14001,
        Level = LogLevel.Error,
        Message = "Client Id must be between 0 and 65535 characters.")]
    internal static partial void LogClientIdInvalidLength(ILogger logger);

    [LoggerMessage(
        EventId = 14002,
        Level = LogLevel.Error,
        Message = "WithClientCertificate: File exists but is not readable due to access permissions.")]
    internal static partial void LogClientCertificateFileNotReadable(ILogger logger);

    [LoggerMessage(
        EventId = 14003,
        Level = LogLevel.Error,
        Message = "WithClientCertificate: An I/O error occurred while trying to read the file.")]
    internal static partial void LogClientCertificateIOError(ILogger logger);

    [LoggerMessage(
        EventId = 14004,
        Level = LogLevel.Error,
        Message = "WithClientCertificate: The specified client certificate file does not exist.")]
    internal static partial void LogClientCertificateFileNotFound(ILogger logger);

    [LoggerMessage(
        EventId = 14005,
        Level = LogLevel.Error,
        Message = "Authentication method must be between 1 and 65535 characters.")]
    internal static partial void LogAuthenticationMethodInvalidLength(ILogger logger);

    [LoggerMessage(
        EventId = 14006,
        Level = LogLevel.Error,
        Message = "User property key must be between 1 and 65535 characters.")]
    internal static partial void LogUserPropertyKeyInvalidLength(ILogger logger);

    [LoggerMessage(
        EventId = 14007,
        Level = LogLevel.Error,
        Message = "User property value must be between 1 and 65535 characters.")]
    internal static partial void LogUserPropertyValueInvalidLength(ILogger logger);

    [LoggerMessage(
        EventId = 14008,
        Level = LogLevel.Error,
        Message = "Username must be between 0 and 65535 characters.")]
    internal static partial void LogUsernameInvalidLength(ILogger logger);

    [LoggerMessage(
        EventId = 14009,
        Level = LogLevel.Error,
        Message = "Password must be between 0 and 65535 characters.")]
    internal static partial void LogPasswordInvalidLength(ILogger logger);
}
