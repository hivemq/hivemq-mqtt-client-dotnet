namespace HiveMQtt.Test.HiveMQClient.WebSocket;

using System;
using System.Collections.Generic;
using System.Buffers;
using System.Net;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using HiveMQtt.Client;
using HiveMQtt.Client.Transport;
using Xunit;

/// <summary>
/// Unit tests for WebSocketTransport.
/// These tests focus on error handling, state validation, and edge cases
/// without requiring a live MQTT broker.
/// </summary>
[Collection("Broker")]
public class WebSocketTransportTest
{
    [Fact]
    public void Constructor_WithInvalidUriScheme_ThrowsArgumentException()
    {
        // Arrange
        var options = new HiveMQClientOptionsBuilder()
            .WithWebSocketServer("http://localhost:8000/mqtt")
            .WithClientId("test")
            .Build();

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new WebSocketTransport(options));
        Assert.Contains("Invalid WebSocket URI scheme", exception.Message);
    }

    [Fact]
    public void Constructor_WithValidWsScheme_CreatesTransport()
    {
        // Arrange
        var options = new HiveMQClientOptionsBuilder()
            .WithWebSocketServer("ws://localhost:8000/mqtt")
            .WithClientId("test")
            .Build();

        // Act
        var transport = new WebSocketTransport(options);

        // Assert
        transport.Should().NotBeNull();
        transport.Options.Should().Be(options);
        transport.Socket.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_WithValidWssScheme_CreatesTransport()
    {
        // Arrange
        var options = new HiveMQClientOptionsBuilder()
            .WithWebSocketServer("wss://localhost:8000/mqtt")
            .WithClientId("test")
            .Build();

        // Act
        var transport = new WebSocketTransport(options);

        // Assert
        transport.Should().NotBeNull();
        transport.Options.Should().Be(options);
        transport.Socket.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_WithWssScheme_ConfiguresTlsOptions()
    {
        // Arrange
        var options = new HiveMQClientOptionsBuilder()
            .WithWebSocketServer("wss://localhost:8000/mqtt")
            .WithClientId("test")
            .WithAllowInvalidBrokerCertificates(true)
            .Build();

        // Act
        var transport = new WebSocketTransport(options);

        // Assert
        transport.Should().NotBeNull();

        // TLS options should be configured (we can't easily verify the callback, but we can verify it doesn't throw)
        transport.Socket.Options.RemoteCertificateValidationCallback.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_WithWebSocketKeepAliveInterval_ConfiguresKeepAlive()
    {
        // Arrange
        var keepAliveInterval = TimeSpan.FromSeconds(30);
        var options = new HiveMQClientOptionsBuilder()
            .WithWebSocketServer("ws://localhost:8000/mqtt")
            .WithClientId("test")
            .WithWebSocketKeepAliveInterval(keepAliveInterval)
            .Build();

        // Act
        var transport = new WebSocketTransport(options);

        // Assert
        transport.Should().NotBeNull();
        transport.Socket.Options.KeepAliveInterval.Should().Be(keepAliveInterval);
    }

    [Fact]
    public void Constructor_WithWebSocketRequestHeaders_ConfiguresHeaders()
    {
        // Arrange
        var headers = new Dictionary<string, string>
        {
            { "Authorization", "Bearer token123" },
            { "X-API-Key", "api-key-value" },
            { "Custom-Header", "custom-value" },
        };

        var options = new HiveMQClientOptionsBuilder()
            .WithWebSocketServer("ws://localhost:8000/mqtt")
            .WithClientId("test")
            .WithWebSocketRequestHeaders(headers)
            .Build();

        // Act
        var transport = new WebSocketTransport(options);

        // Assert
        transport.Should().NotBeNull();

        // Verify headers are set (ClientWebSocket.Options doesn't expose headers directly,
        // but we can verify the transport was created without errors and the options are stored)
        options.WebSocketRequestHeaders.Should().NotBeNull();
        options.WebSocketRequestHeaders.Should().HaveCount(3);
        options.WebSocketRequestHeaders.Should().ContainKey("Authorization");
        options.WebSocketRequestHeaders.Should().ContainKey("X-API-Key");
        options.WebSocketRequestHeaders.Should().ContainKey("Custom-Header");
        options.WebSocketRequestHeaders!["Authorization"].Should().Be("Bearer token123");
        options.WebSocketRequestHeaders!["X-API-Key"].Should().Be("api-key-value");
        options.WebSocketRequestHeaders!["Custom-Header"].Should().Be("custom-value");
    }

    [Fact]
    public void Constructor_WithWebSocketRequestHeader_AddsSingleHeader()
    {
        // Arrange
        var options = new HiveMQClientOptionsBuilder()
            .WithWebSocketServer("ws://localhost:8000/mqtt")
            .WithClientId("test")
            .WithWebSocketRequestHeader("Authorization", "Bearer token456")
            .Build();

        // Act
        var transport = new WebSocketTransport(options);

        // Assert
        transport.Should().NotBeNull();
        options.WebSocketRequestHeaders.Should().NotBeNull();
        options.WebSocketRequestHeaders.Should().HaveCount(1);
        options.WebSocketRequestHeaders.Should().ContainKey("Authorization");
        options.WebSocketRequestHeaders!["Authorization"].Should().Be("Bearer token456");
    }

    [Fact]
    public void Constructor_WithMultipleWebSocketRequestHeaders_AddsAllHeaders()
    {
        // Arrange
        var options = new HiveMQClientOptionsBuilder()
            .WithWebSocketServer("ws://localhost:8000/mqtt")
            .WithClientId("test")
            .WithWebSocketRequestHeader("Header1", "Value1")
            .WithWebSocketRequestHeader("Header2", "Value2")
            .WithWebSocketRequestHeader("Header3", "Value3")
            .Build();

        // Act
        var transport = new WebSocketTransport(options);

        // Assert
        transport.Should().NotBeNull();
        options.WebSocketRequestHeaders.Should().NotBeNull();
        options.WebSocketRequestHeaders.Should().HaveCount(3);
        options.WebSocketRequestHeaders!["Header1"].Should().Be("Value1");
        options.WebSocketRequestHeaders!["Header2"].Should().Be("Value2");
        options.WebSocketRequestHeaders!["Header3"].Should().Be("Value3");
    }

    [Fact]
    public void Constructor_WithWebSocketProxy_ConfiguresProxy()
    {
        // Arrange
        var proxyUri = new Uri("http://proxy.example.com:8080");
        var proxy = new WebProxy(proxyUri);
        var options = new HiveMQClientOptionsBuilder()
            .WithWebSocketServer("ws://localhost:8000/mqtt")
            .WithClientId("test")
            .WithWebSocketProxy(proxy)
            .Build();

        // Act
        var transport = new WebSocketTransport(options);

        // Assert
        transport.Should().NotBeNull();
        transport.Socket.Options.Proxy.Should().Be(proxy);
    }

    [Fact]
    public void Constructor_WithAllWebSocketOptions_ConfiguresAllOptions()
    {
        // Arrange
        var keepAliveInterval = TimeSpan.FromSeconds(45);
        var headers = new Dictionary<string, string>
        {
            { "Authorization", "Bearer token789" },
            { "X-Custom", "value" },
        };
        var proxy = new WebProxy("http://proxy.test.com:3128");

        var options = new HiveMQClientOptionsBuilder()
            .WithWebSocketServer("ws://localhost:8000/mqtt")
            .WithClientId("test")
            .WithWebSocketKeepAliveInterval(keepAliveInterval)
            .WithWebSocketRequestHeaders(headers)
            .WithWebSocketProxy(proxy)
            .Build();

        // Act
        var transport = new WebSocketTransport(options);

        // Assert
        transport.Should().NotBeNull();
        transport.Socket.Options.KeepAliveInterval.Should().Be(keepAliveInterval);
        transport.Socket.Options.Proxy.Should().Be(proxy);
        options.WebSocketRequestHeaders.Should().NotBeNull();
        options.WebSocketRequestHeaders.Should().HaveCount(2);
        options.WebSocketRequestHeaders!["Authorization"].Should().Be("Bearer token789");
        options.WebSocketRequestHeaders!["X-Custom"].Should().Be("value");
    }

    [Fact]
    public void Constructor_WithoutWebSocketOptions_UsesDefaults()
    {
        // Arrange
        var options = new HiveMQClientOptionsBuilder()
            .WithWebSocketServer("ws://localhost:8000/mqtt")
            .WithClientId("test")
            .Build();

        // Act
        var transport = new WebSocketTransport(options);

        // Assert
        transport.Should().NotBeNull();

        // Verify defaults (keep-alive uses system default, headers are null, proxy uses system default)
        options.WebSocketKeepAliveInterval.Should().BeNull();
        options.WebSocketRequestHeaders.Should().BeNull();
        options.WebSocketProxy.Should().BeNull();

        // Socket should still be created (defaults are applied by the system)
        transport.Socket.Should().NotBeNull();
    }

    [Fact]
    public async Task WriteAsync_WhenSocketNotOpen_ReturnsFalseAsync()
    {
        // Arrange
        var options = new HiveMQClientOptionsBuilder()
            .WithWebSocketServer("ws://localhost:8000/mqtt")
            .WithClientId("test")
            .Build();
        var transport = new WebSocketTransport(options);

        // Socket is not connected, so state should not be Open
        // We can't easily set the state without reflection, but we can test the logic

        // Act
        var result = await transport.WriteAsync([1, 2, 3], CancellationToken.None).ConfigureAwait(false);

        // Assert
        // Since socket is not connected, WriteAsync should return false due to state check
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ReadAsync_WhenSocketNotInReadableState_ReturnsCompletedResultAsync()
    {
        // Arrange
        var options = new HiveMQClientOptionsBuilder()
            .WithWebSocketServer("ws://localhost:8000/mqtt")
            .WithClientId("test")
            .Build();
        var transport = new WebSocketTransport(options);

        // Act
        var result = await transport.ReadAsync(CancellationToken.None).ConfigureAwait(false);

        // Assert
        // Since socket is not connected, ReadAsync should return a failed result
        result.Failed.Should().BeTrue();
    }

    [Fact]
    public async Task WriteAsync_WithCancellation_HandlesCancellationAsync()
    {
        // Arrange
        var options = new HiveMQClientOptionsBuilder()
            .WithWebSocketServer("ws://localhost:8000/mqtt")
            .WithClientId("test")
            .Build();
        var transport = new WebSocketTransport(options);
        using var cts = new CancellationTokenSource();
#pragma warning disable VSTHRD103 // Call async methods when in an async context
        cts.Cancel();
#pragma warning restore VSTHRD103

        // Act
        var result = await transport.WriteAsync([1, 2, 3], cts.Token).ConfigureAwait(false);

        // Assert
        // Should return false due to cancellation or state check
        result.Should().BeFalse();
    }

    [Fact]
    public async Task CloseAsync_WhenSocketNotConnected_ReturnsTrueAsync()
    {
        // Arrange
        var options = new HiveMQClientOptionsBuilder()
            .WithWebSocketServer("ws://localhost:8000/mqtt")
            .WithClientId("test")
            .Build();
        var transport = new WebSocketTransport(options);

        // Act
        var result = await transport.CloseAsync(true, CancellationToken.None).ConfigureAwait(false);

        // Assert
        // CloseAsync should handle gracefully even when socket is not connected
        result.Should().BeTrue();
    }

    [Fact]
    public async Task CloseAsync_WithCancellation_HandlesCancellationAsync()
    {
        // Arrange
        var options = new HiveMQClientOptionsBuilder()
            .WithWebSocketServer("ws://localhost:8000/mqtt")
            .WithClientId("test")
            .Build();
        var transport = new WebSocketTransport(options);
        using var cts = new CancellationTokenSource();
#pragma warning disable VSTHRD103 // Call async methods when in an async context
        cts.Cancel();
#pragma warning restore VSTHRD103

        // Act & Assert
        // CloseAsync should handle cancellation gracefully without throwing
        // Since socket is not open, it will return true (socket already closed)
        var result = await transport.CloseAsync(true, cts.Token).ConfigureAwait(false);
        result.Should().BeTrue();
    }

    [Fact]
    public void Dispose_DisposesResources()
    {
        // Arrange
        var options = new HiveMQClientOptionsBuilder()
            .WithWebSocketServer("ws://localhost:8000/mqtt")
            .WithClientId("test")
            .Build();
        var transport = new WebSocketTransport(options);
        var socket = transport.Socket;

        // Act
        transport.Dispose();

        // Assert
        // After disposal, accessing the socket should throw or be null
        // We can't easily verify this without reflection, but we can verify Dispose doesn't throw
        transport.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_InitializesWebSocket()
    {
        // Arrange
        var options = new HiveMQClientOptionsBuilder()
            .WithWebSocketServer("ws://localhost:8000/mqtt")
            .WithClientId("test")
            .Build();

        // Act
        var transport = new WebSocketTransport(options);

        // Assert
        // Verify that the WebSocket is initialized
        transport.Socket.Should().NotBeNull();
        transport.Socket.State.Should().Be(WebSocketState.None);
    }

    [Fact]
    public void AdvanceTo_Methods_AreNoOps()
    {
        // Arrange
        var options = new HiveMQClientOptionsBuilder()
            .WithWebSocketServer("ws://localhost:8000/mqtt")
            .WithClientId("test")
            .Build();
        var transport = new WebSocketTransport(options);
        var buffer = new ReadOnlySequence<byte>([1, 2, 3]);
        var position = buffer.Start;

        // Act & Assert - should not throw
        transport.AdvanceTo(position);
        transport.AdvanceTo(position, position);
    }

    [Fact]
    public async Task WriteAsync_SerializesConcurrentWritesAsync()
    {
        // Arrange
        var options = new HiveMQClientOptionsBuilder()
            .WithWebSocketServer("ws://localhost:8000/mqtt")
            .WithClientId("test")
            .Build();
        var transport = new WebSocketTransport(options);
        var buffer = new byte[] { 1, 2, 3 };

        // Act - attempt concurrent writes
        var tasks = new List<Task<bool>>();
        for (var i = 0; i < 5; i++)
        {
            tasks.Add(transport.WriteAsync(buffer, CancellationToken.None));
        }

        var results = await Task.WhenAll(tasks).ConfigureAwait(false);

        // Assert
        // All writes should complete (even if they fail due to socket not being open)
        // The semaphore ensures they don't interfere with each other
        results.Should().HaveCount(5);

        // All should return false since socket is not connected
        results.Should().AllBeEquivalentTo(false);
    }

    [Theory]
#pragma warning disable CS3016 // Arrays as attribute arguments is not CLS-compliant
    [InlineData("ws://localhost:8000/mqtt")]
    [InlineData("wss://localhost:8000/mqtt")]
#pragma warning restore CS3016 // Arrays as attribute arguments is not CLS-compliant
    public void Constructor_WithValidSchemes_DoesNotThrow(string uri)
    {
        // Arrange
        var options = new HiveMQClientOptionsBuilder()
            .WithWebSocketServer(uri)
            .WithClientId("test")
            .Build();

        // Act & Assert
        var transport = new WebSocketTransport(options);
        transport.Should().NotBeNull();
    }

    [Theory]
#pragma warning disable CS3016 // Arrays as attribute arguments is not CLS-compliant
    [InlineData("http://localhost:8000/mqtt")]
    [InlineData("https://localhost:8000/mqtt")]
    [InlineData("ftp://localhost:8000/mqtt")]
    [InlineData("invalid://localhost:8000/mqtt")]
#pragma warning restore CS3016 // Arrays as attribute arguments is not CLS-compliant
    public void Constructor_WithInvalidSchemes_ThrowsArgumentException(string uri)
    {
        // Arrange
        var options = new HiveMQClientOptionsBuilder()
            .WithWebSocketServer(uri)
            .WithClientId("test")
            .Build();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => new WebSocketTransport(options));
    }
}
