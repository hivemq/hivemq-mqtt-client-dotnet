namespace HiveMQtt.Client;

using Microsoft.Extensions.Logging;

/// <summary>
/// Source-generated logging methods for DisconnectOptionsBuilder using LoggerMessage pattern.
/// This provides zero-overhead logging when logging is disabled.
/// </summary>
public partial class DisconnectOptionsBuilder
{
    [LoggerMessage(
        EventId = 13001,
        Level = LogLevel.Error,
        Message = "Reason string cannot be null.")]
    internal static partial void LogReasonStringCannotBeNull(ILogger logger);

    [LoggerMessage(
        EventId = 13002,
        Level = LogLevel.Error,
        Message = "Reason string must be between 1 and 65535 characters.")]
    internal static partial void LogReasonStringInvalidLength(ILogger logger);

    [LoggerMessage(
        EventId = 13003,
        Level = LogLevel.Error,
        Message = "User property key cannot be null.")]
    internal static partial void LogUserPropertyKeyCannotBeNull(ILogger logger);

    [LoggerMessage(
        EventId = 13004,
        Level = LogLevel.Error,
        Message = "User property value cannot be null.")]
    internal static partial void LogUserPropertyValueCannotBeNull(ILogger logger);

    [LoggerMessage(
        EventId = 13005,
        Level = LogLevel.Error,
        Message = "User property key must be between 1 and 65535 characters.")]
    internal static partial void LogUserPropertyKeyInvalidLength(ILogger logger);

    [LoggerMessage(
        EventId = 13006,
        Level = LogLevel.Error,
        Message = "User property value must be between 1 and 65535 characters.")]
    internal static partial void LogUserPropertyValueInvalidLength(ILogger logger);
}
