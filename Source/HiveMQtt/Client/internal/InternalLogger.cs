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
namespace HiveMQtt.Client.Internal;

using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

/// <summary>
/// A thin internal adapter over <see cref="ILogger"/>.
///
/// <para>
/// HiveMQtt logs from a large number of call sites.  This adapter keeps those call sites
/// terse (<c>Logger.Trace(...)</c>, <c>Logger.Warn(...)</c>) while routing every message to
/// the <see cref="ILoggerFactory"/> supplied by the consumer through
/// <see cref="HiveMQtt.Client.Options.HiveMQClientOptions.LoggerFactory"/>.  When no factory
/// is supplied, <see cref="NullLogger.Instance"/> is used and logging is a no-op.
/// </para>
/// </summary>
internal readonly struct InternalLogger
{
    private readonly ILogger logger;

    internal InternalLogger(ILogger? logger) => this.logger = logger ?? NullLogger.Instance;

    /// <summary>
    /// Gets the underlying <see cref="ILogger"/>.
    /// </summary>
    internal ILogger Logger => this.logger ?? NullLogger.Instance;

    /// <summary>
    /// Creates an <see cref="InternalLogger"/> for <typeparamref name="T"/> from the given factory.
    /// </summary>
    /// <typeparam name="T">The type the logger category is named after.</typeparam>
    /// <param name="factory">The factory to create the logger from.  May be <see langword="null"/>.</param>
    /// <returns>A new <see cref="InternalLogger"/>.</returns>
    internal static InternalLogger For<T>(ILoggerFactory? factory) =>
        new((factory ?? NullLoggerFactory.Instance).CreateLogger(CategoryName(typeof(T))));

    /// <summary>
    /// Produces the log category for a type, matching what NLog's <c>GetCurrentClassLogger()</c>
    /// previously produced: the full type name, without constructed generic arguments.
    /// </summary>
    /// <param name="type">The type to name.</param>
    /// <returns>The category name.</returns>
    private static string CategoryName(Type type)
    {
        if (type.IsGenericType && !type.IsGenericTypeDefinition)
        {
            type = type.GetGenericTypeDefinition();
        }

        return type.FullName ?? type.Name;
    }

    /// <summary>
    /// Gets a value indicating whether trace level logging is enabled.
    /// </summary>
    internal bool IsTraceEnabled => this.Logger.IsEnabled(LogLevel.Trace);

    /// <summary>
    /// Gets a value indicating whether debug level logging is enabled.
    /// </summary>
    internal bool IsDebugEnabled => this.Logger.IsEnabled(LogLevel.Debug);

#pragma warning disable CA2254 // Template should be a static expression: call sites pass pre-formatted text.
#pragma warning disable CA1848 // Use LoggerMessage delegates: this adapter forwards free-form messages.
    internal void Trace(string message) => this.Logger.LogTrace(message);

    internal void Trace(string message, params object?[] args) => this.Logger.LogTrace(message, args);

    internal void Trace(Exception exception, string message) => this.Logger.LogTrace(exception, message);

    internal void Debug(string message) => this.Logger.LogDebug(message);

    internal void Debug(string message, params object?[] args) => this.Logger.LogDebug(message, args);

    internal void Debug(Exception exception, string message) => this.Logger.LogDebug(exception, message);

    internal void Info(string message) => this.Logger.LogInformation(message);

    internal void Info(string message, params object?[] args) => this.Logger.LogInformation(message, args);

    internal void Info(Exception exception, string message) => this.Logger.LogInformation(exception, message);

    internal void Warn(string message) => this.Logger.LogWarning(message);

    internal void Warn(string message, params object?[] args) => this.Logger.LogWarning(message, args);

    internal void Warn(Exception exception, string message) => this.Logger.LogWarning(exception, message);

    internal void Error(string message) => this.Logger.LogError(message);

    internal void Error(string message, params object?[] args) => this.Logger.LogError(message, args);

    internal void Error(Exception exception, string message) => this.Logger.LogError(exception, message);
#pragma warning restore CA1848
#pragma warning restore CA2254
}
