---
sidebar_position: 8
---

# Configure a Proxy Server

When deploying applications in corporate environments or certain hosting providers, network traffic may be routed through a proxy server. The HiveMQ MQTT .NET client supports connecting to MQTT brokers through HTTP proxy servers.

:::info Version Note
Proxy support for TCP connections (`WithProxy`) was added in **v0.38.0**. WebSocket proxy support (`WithWebSocketProxy`) has been available since earlier versions.
:::

## Choosing a Transport: WebSocket vs TCP

The HiveMQ MQTT client supports two transport protocols, each with different proxy capabilities:

| Transport | Proxy Support | Recommended |
|-----------|---------------|-------------|
| **WebSocket** | Native, well-tested | ✅ Preferred |
| **TCP** | HTTP CONNECT tunneling | Backup option |

### WebSocket Transport (Recommended)

**When to use:** WebSocket transport should be your first choice when proxy support is required.

**Why it's preferred:**
- Native proxy support through .NET's `ClientWebSocket` implementation
- Better compatibility with enterprise proxy servers and firewalls
- HTTP-based protocol that proxies are designed to handle
- Works seamlessly with most proxy configurations

### TCP Transport with HTTP CONNECT

**When to use:** Use TCP proxy support as a fallback when:
- WebSocket transport is not available on your broker
- You have specific requirements that prevent WebSocket usage
- Your infrastructure only supports raw TCP connections

**How it works:** The client establishes a connection to the proxy server, then sends an HTTP CONNECT request to create a tunnel to the MQTT broker. Once the tunnel is established, MQTT traffic flows through this connection as if it were a direct connection.

```
┌─────────┐       ┌─────────────┐       ┌─────────────┐
│  Client │──────▶│ HTTP Proxy  │──────▶│ MQTT Broker │
└─────────┘       └─────────────┘       └─────────────┘
     │                   │                     │
     │  HTTP CONNECT     │                     │
     │  broker:port      │                     │
     │──────────────────▶│                     │
     │                   │  TCP Connection     │
     │                   │────────────────────▶│
     │  200 OK           │                     │
     │◀──────────────────│                     │
     │                   │                     │
     │         MQTT Traffic (tunneled)         │
     │◀───────────────────────────────────────▶│
```

## WebSocket Proxy Configuration (Recommended)

For WebSocket connections (ws:// or wss://), use the `WithWebSocketProxy` method:

```csharp
using System.Net;
using HiveMQtt.Client;

// Basic proxy without authentication
var options = new HiveMQClientOptionsBuilder()
    .WithWebSocketServer("wss://broker.example.com:8884/mqtt")
    .WithWebSocketProxy(new WebProxy("http://proxy.example.com:8080"))
    .Build();

var client = new HiveMQClient(options);
await client.ConnectAsync();
```

### WebSocket Proxy with Authentication

```csharp
using System.Net;
using HiveMQtt.Client;

var proxy = new WebProxy("http://proxy.example.com:8080");
proxy.Credentials = new NetworkCredential("proxyUsername", "proxyPassword");

var options = new HiveMQClientOptionsBuilder()
    .WithWebSocketServer("wss://broker.example.com:8884/mqtt")
    .WithWebSocketProxy(proxy)
    .Build();

var client = new HiveMQClient(options);
await client.ConnectAsync();
```

## TCP Proxy Configuration (Fallback Option)

For direct TCP connections, use the `WithProxy` method. This uses the HTTP CONNECT method to tunnel TCP traffic through the proxy.

:::note
TCP proxy support requires a proxy server that supports the HTTP CONNECT method for TCP tunneling. Most enterprise HTTP proxies support this feature.
:::

```csharp
using System.Net;
using HiveMQtt.Client;

// Basic proxy without authentication
var options = new HiveMQClientOptionsBuilder()
    .WithBroker("broker.example.com")
    .WithPort(1883)
    .WithProxy(new WebProxy("http://proxy.example.com:8080"))
    .Build();

var client = new HiveMQClient(options);
await client.ConnectAsync();
```

### TCP Proxy with Authentication

```csharp
using System.Net;
using HiveMQtt.Client;

var proxy = new WebProxy("http://proxy.example.com:8080");
proxy.Credentials = new NetworkCredential("proxyUsername", "proxyPassword");

var options = new HiveMQClientOptionsBuilder()
    .WithBroker("broker.example.com")
    .WithPort(8883)
    .WithUseTls(true)
    .WithProxy(proxy)
    .Build();

var client = new HiveMQClient(options);
await client.ConnectAsync();
```

### TCP Proxy with TLS

The proxy tunnel is established first, then TLS negotiation occurs through the tunnel. This means the proxy only sees encrypted traffic after the initial CONNECT handshake.

```csharp
using System.Net;
using HiveMQtt.Client;

var proxy = new WebProxy("http://proxy.example.com:8080");

var options = new HiveMQClientOptionsBuilder()
    .WithBroker("broker.example.com")
    .WithPort(8883)
    .WithUseTls(true)
    .WithProxy(proxy)
    .Build();

var client = new HiveMQClient(options);
await client.ConnectAsync();
```

## Using with RawClient

Both `HiveMQClient` and `RawClient` support proxy configuration through the same options:

```csharp
using System.Net;
using HiveMQtt.Client;

var proxy = new WebProxy("http://proxy.example.com:8080");

var options = new HiveMQClientOptionsBuilder()
    .WithBroker("broker.example.com")
    .WithProxy(proxy)
    .Build();

// Works with both HiveMQClient and RawClient
var rawClient = new RawClient(options);
await rawClient.ConnectAsync();
```

## Environment-Based Proxy Configuration

In production environments, avoid hardcoding proxy settings. Use environment variables or configuration files:

### Using Environment Variables

```csharp
using System.Net;
using HiveMQtt.Client;

var proxyUrl = Environment.GetEnvironmentVariable("HTTP_PROXY");
var builder = new HiveMQClientOptionsBuilder()
    .WithBroker("broker.example.com")
    .WithPort(1883);

if (!string.IsNullOrEmpty(proxyUrl))
{
    var proxy = new WebProxy(proxyUrl);
    
    // Optional: Add credentials from environment
    var proxyUser = Environment.GetEnvironmentVariable("PROXY_USER");
    var proxyPass = Environment.GetEnvironmentVariable("PROXY_PASS");
    
    if (!string.IsNullOrEmpty(proxyUser))
    {
        proxy.Credentials = new NetworkCredential(proxyUser, proxyPass);
    }
    
    builder.WithProxy(proxy);
}

var options = builder.Build();
var client = new HiveMQClient(options);
```

### Using appsettings.json

```json
{
  "MqttSettings": {
    "Broker": "broker.example.com",
    "Port": 1883,
    "Proxy": {
      "Enabled": true,
      "Url": "http://proxy.example.com:8080",
      "Username": "",
      "Password": ""
    }
  }
}
```

```csharp
using System.Net;
using HiveMQtt.Client;
using Microsoft.Extensions.Configuration;

var config = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .Build();

var mqttSettings = config.GetSection("MqttSettings");
var builder = new HiveMQClientOptionsBuilder()
    .WithBroker(mqttSettings["Broker"])
    .WithPort(int.Parse(mqttSettings["Port"]));

var proxySection = mqttSettings.GetSection("Proxy");
if (proxySection.GetValue<bool>("Enabled"))
{
    var proxy = new WebProxy(proxySection["Url"]);
    
    var username = proxySection["Username"];
    if (!string.IsNullOrEmpty(username))
    {
        proxy.Credentials = new NetworkCredential(
            username, 
            proxySection["Password"]);
    }
    
    builder.WithProxy(proxy);
}

var options = builder.Build();
```

## Troubleshooting

### Common Issues

1. **Proxy Connection Refused**
   - Verify the proxy URL and port are correct
   - Check if the proxy server is running and accessible
   - Ensure your network allows connections to the proxy

2. **HTTP 407 Proxy Authentication Required**
   - The proxy requires authentication
   - Add credentials using `proxy.Credentials = new NetworkCredential(...)`

3. **HTTP 403 Forbidden**
   - The proxy may be blocking connections to the MQTT broker port
   - Contact your network administrator to allow the destination

4. **Connection Timeout**
   - The proxy may not support the HTTP CONNECT method
   - Try using WebSocket transport with `WithWebSocketProxy` instead

### Logging

Enable debug logging to troubleshoot proxy connection issues:

```csharp
// In your NLog.config or logging configuration
<logger name="HiveMQtt.*" minlevel="Trace" writeTo="console" />
```

## Proxy Server Requirements

For TCP proxy support (HTTP CONNECT tunneling), the proxy server must:

- Support the HTTP CONNECT method (RFC 7231)
- Allow connections to the MQTT broker's port (typically 1883 or 8883)
- Support HTTP/1.1

Most enterprise HTTP proxies (Squid, nginx, Apache, etc.) support these requirements.

## See Also

* [Connecting to a Broker](/docs/connecting) - Basic connection setup
* [Security Best Practices](/docs/security) - TLS and authentication
* [HiveMQClientOptionsBuilder Reference](/docs/reference/client_options_builder) - All builder methods
* [HiveMQClientOptions Reference](/docs/reference/client_options) - All configuration options
