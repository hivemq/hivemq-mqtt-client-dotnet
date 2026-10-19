/*
 * Copyright 2026-present HiveMQ and the HiveMQ Community
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

namespace HiveMQtt.Test.Logging;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using HiveMQtt.Client;
using HiveMQtt.Client.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

public class LoggerFactoryTest
{
    [Fact]
    public void Options_DefaultsToNullLoggerFactory()
    {
        var options = new HiveMQClientOptions();
        Assert.Same(NullLoggerFactory.Instance, options.LoggerFactory);
    }

    [Fact]
    public void Options_NullAssignmentFallsBackToNullLoggerFactory()
    {
        var options = new HiveMQClientOptions { LoggerFactory = null! };
        Assert.Same(NullLoggerFactory.Instance, options.LoggerFactory);
    }

    [Fact]
    public void Builder_WithLoggerFactory_SetsOptions()
    {
        var recorder = new RecordingLoggerFactory();
        var options = new HiveMQClientOptionsBuilder()
            .WithBroker("127.0.0.1")
            .WithLoggerFactory(recorder)
            .Build();

        Assert.Same(recorder, options.LoggerFactory);
    }

    [Fact]
    public void Builder_WithNullLoggerFactory_FallsBackToNullLoggerFactory()
    {
        var options = new HiveMQClientOptionsBuilder()
            .WithBroker("127.0.0.1")
            .WithLoggerFactory(null)
            .Build();

        Assert.Same(NullLoggerFactory.Instance, options.LoggerFactory);
    }

    [Fact]
    public void Builder_ValidationFailure_IsLoggedToInjectedFactory()
    {
        var recorder = new RecordingLoggerFactory();
        var builder = new HiveMQClientOptionsBuilder().WithLoggerFactory(recorder);

        Assert.Throws<ArgumentException>(() => builder.WithUserName(new string('x', 65536)));

        var entry = Assert.Single(recorder.Entries);
        Assert.Equal("HiveMQtt.Client.HiveMQClientOptionsBuilder", entry.Category);
        Assert.Equal(LogLevel.Error, entry.Level);
        Assert.Contains("Username must be between", entry.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Clients_DoNotShareLoggers()
    {
        // There is no static/global logger left: each client logs only to the factory it was
        // given, and a client built without one stays silent.
        var recorder = new RecordingLoggerFactory();

        using var logged = new HiveMQClient(new HiveMQClientOptionsBuilder()
            .WithBroker("127.0.0.1").WithClientId("logged-client").WithLoggerFactory(recorder).Build());

        var countAfterFirstClient = recorder.Entries.Count();
        Assert.NotEqual(0, countAfterFirstClient);

        using var silent = new HiveMQClient(new HiveMQClientOptionsBuilder()
            .WithBroker("127.0.0.1").WithClientId("silent-client").Build());

        Assert.Equal(countAfterFirstClient, recorder.Entries.Count());
        Assert.DoesNotContain(recorder.Entries, e => e.Message.Contains("silent-client", StringComparison.Ordinal));
    }

    [Fact]
    public void Client_Construction_LogsUnderClientCategory()
    {
        var recorder = new RecordingLoggerFactory();
        var options = new HiveMQClientOptionsBuilder()
            .WithBroker("127.0.0.1")
            .WithClientId("logger-factory-test")
            .WithLoggerFactory(recorder)
            .Build();

        using var client = new HiveMQClient(options);

        Assert.Contains(
            recorder.Entries,
            e => e.Category == "HiveMQtt.Client.HiveMQClient"
                 && e.Level == LogLevel.Trace
                 && e.Message.Contains("New client created", StringComparison.Ordinal));
    }

    [Fact]
    public void Client_Construction_LogsConnectionManagerLegendUnderItsOwnCategory()
    {
        var recorder = new RecordingLoggerFactory();
        var options = new HiveMQClientOptionsBuilder()
            .WithBroker("127.0.0.1")
            .WithClientId("logger-factory-category-test")
            .WithLoggerFactory(recorder)
            .Build();

        using var client = new HiveMQClient(options);

        // Categories are per-type, so consumers can filter them independently.
        Assert.Contains(
            recorder.Entries,
            e => e.Category == "HiveMQtt.Client.Connection.ConnectionManager"
                 && e.Message.Contains("Trace Level Logging Legend", StringComparison.Ordinal));
    }

    private sealed record LogEntry(string Category, LogLevel Level, string Message, Exception? Exception);

    private sealed class RecordingLoggerFactory : ILoggerFactory
    {
        private readonly ConcurrentQueue<LogEntry> entries = new();

        public IEnumerable<LogEntry> Entries => this.entries.ToArray();

        public ILogger CreateLogger(string categoryName) => new RecordingLogger(categoryName, this.entries);

        public void AddProvider(ILoggerProvider provider)
        {
        }

        public void Dispose()
        {
        }

        private sealed class RecordingLogger : ILogger
        {
            private readonly string category;
            private readonly ConcurrentQueue<LogEntry> sink;

            public RecordingLogger(string category, ConcurrentQueue<LogEntry> sink)
            {
                this.category = category;
                this.sink = sink;
            }

            public IDisposable? BeginScope<TState>(TState state)
                where TState : notnull => null;

            public bool IsEnabled(LogLevel logLevel) => true;

            public void Log<TState>(
                LogLevel logLevel,
                EventId eventId,
                TState state,
                Exception? exception,
                Func<TState, Exception?, string> formatter) =>
                this.sink.Enqueue(new LogEntry(this.category, logLevel, formatter(state, exception), exception));
        }
    }
}
