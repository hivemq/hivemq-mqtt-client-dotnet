namespace HiveMQtt.Client.Internal;

using System;
using Microsoft.Extensions.Logging;

#pragma warning disable CS1710 // XML comment has a duplicate typeparam tag - typeparams are documented in main partial class
/// <summary>
/// Source-generated logging methods for BoundedDictionaryX using LoggerMessage pattern.
/// This provides zero-overhead logging when logging is disabled.
/// </summary>
/// <typeparam name="TKey">The type of items to index with.</typeparam>
/// <typeparam name="TVal">The type of items to store as values.</typeparam>
public partial class BoundedDictionaryX<TKey, TVal>
    where TKey : notnull
{
    [LoggerMessage(
        EventId = 10001,
        Level = LogLevel.Trace,
        Message = "Adding item {Key}")]
    private static partial void LogAddingItem(ILogger logger, TKey key);

    [LoggerMessage(
        EventId = 10002,
        Level = LogLevel.Trace,
        Message = "Open slots: {OpenSlots}  Dictionary Count: {Count}")]
    private static partial void LogOpenSlots(ILogger logger, int openSlots, int count);

    [LoggerMessage(
        EventId = 10003,
        Level = LogLevel.Warning,
        Message = "Duplicate key: {Key}")]
    private static partial void LogDuplicateKey(ILogger logger, TKey key);

    [LoggerMessage(
        EventId = 10004,
        Level = LogLevel.Warning,
        Message = "ArgumentNull Exception")]
    private static partial void LogArgumentNullException(ILogger logger, ArgumentNullException ex);

    [LoggerMessage(
        EventId = 10005,
        Level = LogLevel.Warning,
        Message = "Overflow Exception")]
    private static partial void LogOverflowException(ILogger logger, OverflowException ex);

    [LoggerMessage(
        EventId = 10008,
        Level = LogLevel.Warning,
        Message = "Exception")]
    private static partial void LogException(ILogger logger, Exception ex);

    [LoggerMessage(
        EventId = 10006,
        Level = LogLevel.Trace,
        Message = "Removing item {Key}")]
    private static partial void LogRemovingItem(ILogger logger, TKey key);

    [LoggerMessage(
        EventId = 10007,
        Level = LogLevel.Warning,
        Message = "Key not found: {Key}")]
    private static partial void LogKeyNotFound(ILogger logger, TKey key);
}
