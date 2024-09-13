---
sidebar_position: 40
---

# Connecting to an MQTT Broker

## with Defaults

Without any options given, the `HiveMQClient` will search on `localhost` port 1883 for an unsecured broker.

If you don't have a broker at this location, see the next sections.

```csharp
using HiveMQtt.Client;

// Connect
var client = new HiveMQClient();
var connectResult = await client.ConnectAsync().ConfigureAwait(false);
```

## With Specific Options

The `HiveMQClientOptions` class provides a set of options that can be used to configure various aspects of the `HiveMQClient`.

The easiest way to construct this class is to use `HiveMQClientOptionsBuilder`.

```csharp
var options = new HiveMQClientOptionsBuilder().
                    WithBroker('candy.x39.eu.hivemq.cloud').
                    WithPort(8883).
                    WithUseTLS(true).
                    Build();

var client = new HiveMQClient(options);
var connectResult = await client.ConnectAsync().ConfigureAwait(false);
```

## With Automatic Reconnect 

```csharp
var options = new HiveMQClientOptionsBuilder().
                    WithBroker('candy.x39.eu.hivemq.cloud').
                    WithAutomaticReconnect(true)
                    Build();

var client = new HiveMQClient(options);
var connectResult = await client.ConnectAsync().ConfigureAwait(false);
```

## Using WebSockets

```csharp
var options = new HiveMQClientOptionsBuilder()
    .WithWebSocketServer("ws://broker.hivemq.com:8000/mqtt")
    .Build();

var client = new HiveMQClient(options);
var connectResult = await client.ConnectAsync().ConfigureAwait(false);
```

## Using an IP address

Some embedded devices don't have DNS support and can only connect to static IPs.  Here's how you can connect:

```csharp
var options = new HiveMQClientOptionsBuilder()
    .WithBroker("10.0.12.222:8000")
    .Build();

var client = new HiveMQClient(options);
var connectResult = await client.ConnectAsync().ConfigureAwait(false);
```

## See Also

* [HiveMQClientOptionsBuilder Reference](/docs/reference/client_options_builder)
* [Automatic Reconnect](/docs/reference/automatic_reconnect)
* [How to Set a Last Will & Testament](/docs/how-to/set-lwt)
* [Connect with TLS but allow Invalid TLS Certificates](/docs/how-to/allow-invalid-certs)
* [Securely Connect to a Broker with Basic Authentication Credentials](/docs/how-to/connect-with-auth)
* [Custom Client Certificates](/docs/how-to/client-certificates)
* [HiveMQClientOptionsBuilder.cs](https://github.com/hivemq/hivemq-mqtt-client-dotnet/blob/main/Source/HiveMQtt/Client/HiveMQClientOptionsBuilder.cs)
