namespace HiveMQtt.MQTT5;

using System;
using Microsoft.Extensions.Logging;

/// <summary>
/// Source-generated logging methods for PacketDecoder using LoggerMessage pattern.
/// This provides zero-overhead logging when logging is disabled.
/// </summary>
internal static partial class PacketDecoder
{
    [LoggerMessage(
        EventId = 12001,
        Level = LogLevel.Trace,
        Message = "PacketDecoder.TryDecode: Waiting on more data: {BufferLength} < {PacketLength} - Returning PartialPacket.")]
    internal static partial void LogWaitingOnMoreData(ILogger logger, long bufferLength, int packetLength);

    [LoggerMessage(
        EventId = 12002,
        Level = LogLevel.Error,
        Message = "PacketDecoder.Decode: Exception caught.  Returning MalformedPacket.")]
    internal static partial void LogDecodeException(ILogger logger, Exception ex);
}
