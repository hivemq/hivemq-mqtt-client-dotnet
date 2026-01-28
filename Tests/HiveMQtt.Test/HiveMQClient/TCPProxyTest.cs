namespace HiveMQtt.Test.HiveMQClient;

using System.Net;
using FluentAssertions;
using HiveMQtt.Client;
using Xunit;

/// <summary>
/// Unit tests for TCP proxy configuration.
/// These tests verify the proxy options are correctly configured without requiring
/// a live proxy server or MQTT broker.
/// </summary>
public class TCPProxyTest
{
    [Fact]
    public void WithProxy_ConfiguresProxyOption()
    {
        // Arrange
        var proxyUri = new Uri("http://proxy.example.com:8080");
        var proxy = new WebProxy(proxyUri);

        // Act
        var options = new HiveMQClientOptionsBuilder()
            .WithBroker("broker.example.com")
            .WithPort(1883)
            .WithProxy(proxy)
            .Build();

        // Assert
        options.Proxy.Should().NotBeNull();
        options.Proxy.Should().Be(proxy);
    }

    [Fact]
    public void WithProxy_WithCredentials_ConfiguresProxyWithCredentials()
    {
        // Arrange
        var proxyUri = new Uri("http://proxy.example.com:8080");
        var proxy = new WebProxy(proxyUri)
        {
            Credentials = new NetworkCredential("proxyUser", "proxyPassword"),
        };

        // Act
        var options = new HiveMQClientOptionsBuilder()
            .WithBroker("broker.example.com")
            .WithPort(1883)
            .WithProxy(proxy)
            .Build();

        // Assert
        options.Proxy.Should().NotBeNull();
        options.Proxy.Should().Be(proxy);

        // Verify credentials are set
        var targetUri = new Uri("http://broker.example.com:1883");
        var credentials = options.Proxy!.Credentials?.GetCredential(targetUri, "Basic");
        credentials.Should().NotBeNull();
        credentials!.UserName.Should().Be("proxyUser");
        credentials.Password.Should().Be("proxyPassword");
    }

    [Fact]
    public void WithProxy_WithDomainCredentials_ConfiguresProxyWithDomainCredentials()
    {
        // Arrange
        var proxyUri = new Uri("http://proxy.example.com:8080");
        var proxy = new WebProxy(proxyUri)
        {
            Credentials = new NetworkCredential("proxyUser", "proxyPassword", "DOMAIN"),
        };

        // Act
        var options = new HiveMQClientOptionsBuilder()
            .WithBroker("broker.example.com")
            .WithPort(1883)
            .WithProxy(proxy)
            .Build();

        // Assert
        options.Proxy.Should().NotBeNull();

        var targetUri = new Uri("http://broker.example.com:1883");
        var credentials = options.Proxy!.Credentials?.GetCredential(targetUri, "Basic");
        credentials.Should().NotBeNull();
        credentials!.Domain.Should().Be("DOMAIN");
    }

    [Fact]
    public void WithProxy_WithTLS_ConfiguresProxyAndTLS()
    {
        // Arrange
        var proxy = new WebProxy("http://proxy.example.com:8080");

        // Act
        var options = new HiveMQClientOptionsBuilder()
            .WithBroker("broker.example.com")
            .WithPort(8883)
            .WithUseTls(true)
            .WithProxy(proxy)
            .Build();

        // Assert
        options.Proxy.Should().NotBeNull();
        options.UseTLS.Should().BeTrue();
        options.Port.Should().Be(8883);
    }

    [Fact]
    public void WithProxy_MultipleProxyTypes_ConfiguresBothIndependently()
    {
        // Arrange
        var tcpProxy = new WebProxy("http://tcp-proxy.example.com:8080");
        var wsProxy = new WebProxy("http://ws-proxy.example.com:3128");

        // Act - Configure both TCP and WebSocket proxies
        var options = new HiveMQClientOptionsBuilder()
            .WithBroker("broker.example.com")
            .WithPort(1883)
            .WithProxy(tcpProxy)
            .WithWebSocketProxy(wsProxy)
            .Build();

        // Assert - Both should be configured independently
        options.Proxy.Should().NotBeNull();
        options.Proxy.Should().Be(tcpProxy);
        options.WebSocketProxy.Should().NotBeNull();
        options.WebSocketProxy.Should().Be(wsProxy);

        // Verify they are different proxy instances
        options.Proxy.Should().NotBe(options.WebSocketProxy);
    }

    [Fact]
    public void WithoutProxy_ProxyIsNull()
    {
        // Arrange & Act
        var options = new HiveMQClientOptionsBuilder()
            .WithBroker("broker.example.com")
            .WithPort(1883)
            .Build();

        // Assert
        options.Proxy.Should().BeNull();
    }

    [Fact]
    public void Options_Proxy_CanBeSetDirectly()
    {
        // Arrange
        var options = new HiveMQClientOptionsBuilder()
            .WithBroker("broker.example.com")
            .Build();

        var proxy = new WebProxy("http://proxy.example.com:8080");

        // Act
        options.Proxy = proxy;

        // Assert
        options.Proxy.Should().NotBeNull();
        options.Proxy.Should().Be(proxy);
    }

    [Fact]
    public void Options_Proxy_CanBeSetToNull()
    {
        // Arrange
        var proxy = new WebProxy("http://proxy.example.com:8080");
        var options = new HiveMQClientOptionsBuilder()
            .WithBroker("broker.example.com")
            .WithProxy(proxy)
            .Build();

        // Act
        options.Proxy = null;

        // Assert
        options.Proxy.Should().BeNull();
    }

    [Fact]
    public void WithProxy_ChainedWithOtherOptions_WorksCorrectly()
    {
        // Arrange
        var proxy = new WebProxy("http://proxy.example.com:8080");

        // Act - Verify fluent API chaining works with proxy
        var options = new HiveMQClientOptionsBuilder()
            .WithBroker("broker.example.com")
            .WithPort(1883)
            .WithClientId("test-client")
            .WithProxy(proxy)
            .WithKeepAlive(60)
            .WithCleanStart(true)
            .WithUseTls(false)
            .Build();

        // Assert
        options.Host.Should().Be("broker.example.com");
        options.Port.Should().Be(1883);
        options.ClientId.Should().Be("test-client");
        options.Proxy.Should().Be(proxy);
        options.KeepAlive.Should().Be(60);
        options.CleanStart.Should().BeTrue();
        options.UseTLS.Should().BeFalse();
    }

    [Fact]
    public void WithProxy_DifferentProxyPorts_WorksCorrectly()
    {
        // Arrange & Act - Test common proxy ports
        var proxy3128 = new WebProxy("http://proxy.example.com:3128");
        var options3128 = new HiveMQClientOptionsBuilder()
            .WithBroker("broker.example.com")
            .WithProxy(proxy3128)
            .Build();

        var proxy8080 = new WebProxy("http://proxy.example.com:8080");
        var options8080 = new HiveMQClientOptionsBuilder()
            .WithBroker("broker.example.com")
            .WithProxy(proxy8080)
            .Build();

        var proxy80 = new WebProxy("http://proxy.example.com:80");
        var options80 = new HiveMQClientOptionsBuilder()
            .WithBroker("broker.example.com")
            .WithProxy(proxy80)
            .Build();

        // Assert
        options3128.Proxy.Should().Be(proxy3128);
        options8080.Proxy.Should().Be(proxy8080);
        options80.Proxy.Should().Be(proxy80);
    }

    [Fact]
    public void WithProxy_HttpsProxy_WorksCorrectly()
    {
        // Arrange - Some proxies use HTTPS
        var httpsProxy = new WebProxy("https://secure-proxy.example.com:8443");

        // Act
        var options = new HiveMQClientOptionsBuilder()
            .WithBroker("broker.example.com")
            .WithProxy(httpsProxy)
            .Build();

        // Assert
        options.Proxy.Should().Be(httpsProxy);
    }

    [Fact]
    public void WithProxy_ProxyWithBypassList_PreservesConfiguration()
    {
        // Arrange - BypassList expects regex patterns, not glob patterns
#pragma warning disable IDE0300 // Collection initialization can be simplified
        var proxy = new WebProxy("http://proxy.example.com:8080")
        {
            BypassProxyOnLocal = true,
            BypassList = new[] { @".*\.local$", @"192\.168\..*" },
        };
#pragma warning restore IDE0300 // Collection initialization can be simplified

        // Act
        var options = new HiveMQClientOptionsBuilder()
            .WithBroker("broker.example.com")
            .WithProxy(proxy)
            .Build();

        // Assert
        options.Proxy.Should().Be(proxy);
        (options.Proxy as WebProxy)?.BypassProxyOnLocal.Should().BeTrue();
        (options.Proxy as WebProxy)?.BypassList.Should().Contain(@".*\.local$");
        (options.Proxy as WebProxy)?.BypassList.Should().Contain(@"192\.168\..*");
    }

    [Fact]
    public void Client_WithProxy_CanBeCreated()
    {
        // Arrange
        var proxy = new WebProxy("http://proxy.example.com:8080");
        var options = new HiveMQClientOptionsBuilder()
            .WithBroker("broker.example.com")
            .WithProxy(proxy)
            .Build();

        // Act - Verify client can be created with proxy options
        var client = new HiveMQClient(options);

        // Assert
        client.Should().NotBeNull();
        client.Options.Proxy.Should().Be(proxy);
    }

    [Fact]
    public void RawClient_WithProxy_CanBeCreated()
    {
        // Arrange
        var proxy = new WebProxy("http://proxy.example.com:8080");
        var options = new HiveMQClientOptionsBuilder()
            .WithBroker("broker.example.com")
            .WithProxy(proxy)
            .Build();

        // Act - Verify RawClient can be created with proxy options
        var rawClient = new RawClient(options);

        // Assert
        rawClient.Should().NotBeNull();
        rawClient.Options.Proxy.Should().Be(proxy);
    }
}
