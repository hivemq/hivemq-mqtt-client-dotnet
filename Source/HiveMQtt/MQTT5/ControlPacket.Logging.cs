namespace HiveMQtt.MQTT5;

using System;
using Microsoft.Extensions.Logging;

/// <summary>
/// Source-generated logging methods for ControlPacket using LoggerMessage pattern.
/// This provides zero-overhead logging when logging is disabled.
/// </summary>
internal static partial class ControlPacketLogging
{
    [LoggerMessage(
        EventId = 15001,
        Level = LogLevel.Trace,
        Message = "OnPublishQoS1CompleteEventLauncher")]
    internal static partial void LogOnPublishQoS1CompleteEventLauncher(ILogger logger);

    [LoggerMessage(
        EventId = 15002,
        Level = LogLevel.Error,
        Message = "OnPublishQoS1CompleteEventLauncher exception")]
    internal static partial void LogOnPublishQoS1CompleteEventLauncherException(ILogger logger, Exception? ex);

    [LoggerMessage(
        EventId = 15003,
        Level = LogLevel.Trace,
        Message = "OnPublishQoS2CompleteEventLauncher")]
    internal static partial void LogOnPublishQoS2CompleteEventLauncher(ILogger logger);

    [LoggerMessage(
        EventId = 15004,
        Level = LogLevel.Error,
        Message = "OnPublishQoS2CompleteEventLauncher exception")]
    internal static partial void LogOnPublishQoS2CompleteEventLauncherException(ILogger logger, Exception? ex);

    [LoggerMessage(
        EventId = 15005,
        Level = LogLevel.Trace,
        Message = "SubscribePacket.OnCompleteEventLauncher")]
    internal static partial void LogSubscribePacketOnCompleteEventLauncher(ILogger logger);

    [LoggerMessage(
        EventId = 15006,
        Level = LogLevel.Error,
        Message = "SubscribePacket.OnCompleteEventLauncher exception")]
    internal static partial void LogSubscribePacketOnCompleteEventLauncherException(ILogger logger, Exception? ex);

    [LoggerMessage(
        EventId = 15007,
        Level = LogLevel.Error,
        Message = "SubscribePacket.OnCompleteEventLauncher inner exception")]
    internal static partial void LogSubscribePacketOnCompleteEventLauncherInnerException(ILogger logger, Exception ex);
}
