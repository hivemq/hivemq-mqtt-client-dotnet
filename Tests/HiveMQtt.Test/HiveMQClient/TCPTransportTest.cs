namespace HiveMQtt.Test.HiveMQClient;

using System.Net;
using FluentAssertions;
using HiveMQtt.Client;
using HiveMQtt.Client.Exceptions;
using HiveMQtt.Client.Transport;
using Xunit;

/// <summary>
/// Unit tests for TCPTransport.
/// These tests verify the transport configuration and error handling
/// without requiring a live MQTT broker or proxy server.
/// </summary>
public class TCPTransportTest
{
    [Fact]
    public void Constructor_WithOptions_CreatesTransport()
    {
        // Arrange
        var options = new HiveMQClientOptionsBuilder()
            .WithBroker("broker.example.com")
            .WithPort(1883)
            .Build();

        // Act
        var transport = new TCPTransport(options);

        // Assert
        transport.Should().NotBeNull();
        transport.Options.Should().Be(options);
    }

    [Fact]
    public void Constructor_WithProxyOption_CreatesTransport()
    {
        // Arrange
        var proxy = new WebProxy("http://proxy.example.com:8080");
        var options = new HiveMQClientOptionsBuilder()
            .WithBroker("broker.example.com")
            .WithPort(1883)
            .WithProxy(proxy)
            .Build();

        // Act
        var transport = new TCPTransport(options);

        // Assert
        transport.Should().NotBeNull();
        transport.Options.Should().Be(options);
        transport.Options.Proxy.Should().Be(proxy);
    }

    [Fact]
    public void Constructor_WithProxyAndTLS_CreatesTransport()
    {
        // Arrange
        var proxy = new WebProxy("http://proxy.example.com:8080");
        var options = new HiveMQClientOptionsBuilder()
            .WithBroker("broker.example.com")
            .WithPort(8883)
            .WithUseTls(true)
            .WithProxy(proxy)
            .Build();

        // Act
        var transport = new TCPTransport(options);

        // Assert
        transport.Should().NotBeNull();
        transport.Options.Proxy.Should().Be(proxy);
        transport.Options.UseTLS.Should().BeTrue();
    }

    [Fact]
    public void Constructor_WithProxyCredentials_CreatesTransport()
    {
        // Arrange
        var proxy = new WebProxy("http://proxy.example.com:8080")
        {
            Credentials = new NetworkCredential("user", "pass"),
        };
        var options = new HiveMQClientOptionsBuilder()
            .WithBroker("broker.example.com")
            .WithPort(1883)
            .WithProxy(proxy)
            .Build();

        // Act
        var transport = new TCPTransport(options);

        // Assert
        transport.Should().NotBeNull();
        transport.Options.Proxy.Should().Be(proxy);

        var targetUri = new Uri("http://broker.example.com:1883");
        var creds = transport.Options.Proxy?.Credentials?.GetCredential(targetUri, "Basic");
        creds.Should().NotBeNull();
        creds!.UserName.Should().Be("user");
    }

    [Fact]
    public async Task ConnectAsync_WithInvalidProxy_ThrowsExceptionAsync()
    {
        // Arrange - Use a proxy that doesn't exist
        var proxy = new WebProxy("http://192.0.2.1:9999"); // Non-routable IP
        var options = new HiveMQClientOptionsBuilder()
            .WithBroker("broker.example.com")
            .WithPort(1883)
            .WithProxy(proxy)
            .Build();

        var transport = new TCPTransport(options);

        // Act & Assert - Should throw when unable to connect to proxy
        await Assert.ThrowsAsync<HiveMQttClientException>(async () =>
            await transport.ConnectAsync().ConfigureAwait(false)).ConfigureAwait(false);
    }

    [Fact]
    public async Task ConnectAsync_WithInvalidBroker_WithoutProxy_ThrowsExceptionAsync()
    {
        // Arrange - Use a broker that doesn't exist (no proxy)
        var options = new HiveMQClientOptionsBuilder()
            .WithBroker("192.0.2.1") // Non-routable IP
            .WithPort(1883)
            .Build();

        var transport = new TCPTransport(options);

        // Act & Assert - Should throw when unable to connect directly
        await Assert.ThrowsAsync<HiveMQttClientException>(() => transport.ConnectAsync()).ConfigureAwait(false);
    }

    [Fact]
    public async Task ConnectAsync_WithInvalidHostname_ThrowsExceptionAsync()
    {
        // Arrange - Use an invalid hostname
        var options = new HiveMQClientOptionsBuilder()
            .WithBroker("this-hostname-does-not-exist.invalid")
            .WithPort(1883)
            .Build();

        var transport = new TCPTransport(options);

        // Act & Assert - Should throw when unable to resolve hostname
        await Assert.ThrowsAsync<HiveMQttClientException>(() => transport.ConnectAsync()).ConfigureAwait(false);
    }

    [Fact]
    public void Dispose_WithoutConnection_DoesNotThrow()
    {
        // Arrange
        var options = new HiveMQClientOptionsBuilder()
            .WithBroker("broker.example.com")
            .WithPort(1883)
            .Build();

        var transport = new TCPTransport(options);

        // Act & Assert - Should not throw when disposing unconnected transport
        var exception = Record.Exception(transport.Dispose);
        exception.Should().BeNull();
    }

    [Fact]
    public void Dispose_MultipleTimes_DoesNotThrow()
    {
        // Arrange
        var options = new HiveMQClientOptionsBuilder()
            .WithBroker("broker.example.com")
            .WithPort(1883)
            .Build();

        var transport = new TCPTransport(options);

        // Act & Assert - Should handle multiple dispose calls gracefully
        transport.Dispose();
        var exception = Record.Exception(transport.Dispose);
        exception.Should().BeNull();
    }

    [Fact]
    public async Task WriteAsync_BeforeConnect_ThrowsExceptionAsync()
    {
        // Arrange
        var options = new HiveMQClientOptionsBuilder()
            .WithBroker("broker.example.com")
            .WithPort(1883)
            .Build();

        var transport = new TCPTransport(options);

        // Act & Assert - Writing before connecting should throw
        await Assert.ThrowsAsync<HiveMQttClientException>(() => transport.WriteAsync([1, 2, 3])).ConfigureAwait(false);
    }

    [Fact]
    public async Task ReadAsync_BeforeConnect_ThrowsExceptionAsync()
    {
        // Arrange
        var options = new HiveMQClientOptionsBuilder()
            .WithBroker("broker.example.com")
            .WithPort(1883)
            .Build();

        var transport = new TCPTransport(options);

        // Act & Assert - Reading before connecting should throw
        await Assert.ThrowsAsync<HiveMQttClientException>(() => transport.ReadAsync()).ConfigureAwait(false);
    }

    [Fact]
    public async Task CloseAsync_BeforeConnect_ReturnsTrueAsync()
    {
        // Arrange
        var options = new HiveMQClientOptionsBuilder()
            .WithBroker("broker.example.com")
            .WithPort(1883)
            .Build();

        var transport = new TCPTransport(options);

        // Act
        var result = await transport.CloseAsync().ConfigureAwait(false);

        // Assert - Should return true even when not connected
        result.Should().BeTrue();
    }

    [Fact]
    public void Options_ProxyDifferentPorts_ArePreserved()
    {
        // Arrange - Test various proxy port configurations
        var ports = new[] { 80, 443, 3128, 8080, 8443 };

        foreach (var port in ports)
        {
            var proxy = new WebProxy($"http://proxy.example.com:{port}");
            var options = new HiveMQClientOptionsBuilder()
                .WithBroker("broker.example.com")
                .WithProxy(proxy)
                .Build();

            // Act
            var transport = new TCPTransport(options);

            // Assert
            transport.Options.Proxy.Should().Be(proxy);
        }
    }

    [Fact]
    public void Options_IPv6Broker_WithProxy_IsSupported()
    {
        // Arrange
        var proxy = new WebProxy("http://proxy.example.com:8080");
        var options = new HiveMQClientOptionsBuilder()
            .WithBroker("::1") // IPv6 localhost
            .WithPort(1883)
            .WithProxy(proxy)
            .Build();

        // Act
        var transport = new TCPTransport(options);

        // Assert
        transport.Should().NotBeNull();
        transport.Options.Host.Should().Be("::1");
        transport.Options.Proxy.Should().Be(proxy);
    }

    [Fact]
    public void Options_PreferIPv6_WithProxy_IsSupported()
    {
        // Arrange
        var proxy = new WebProxy("http://proxy.example.com:8080");
        var options = new HiveMQClientOptionsBuilder()
            .WithBroker("broker.example.com")
            .WithPort(1883)
            .WithPreferIPv6(true)
            .WithProxy(proxy)
            .Build();

        // Act
        var transport = new TCPTransport(options);

        // Assert
        transport.Options.PreferIPv6.Should().BeTrue();
        transport.Options.Proxy.Should().Be(proxy);
    }
}
