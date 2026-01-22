---
sidebar_position: 20
---

# Quickstart

Get up and running with the HiveMQ MQTT Client for .NET in minutes.

## Installation

Install the [HiveMQtt NuGet package](https://www.nuget.org/packages/HiveMQtt/):

```bash
dotnet add package HiveMQtt
```

## Required Namespaces

```csharp
using HiveMQtt.Client;
using HiveMQtt.MQTT5.Types;
```

## Key Classes Reference

| Purpose | Class | Builder |
|---------|-------|---------|
| Main Client | [`HiveMQClient`](/docs/hivemqclient) | — |
| Performance Client | [`RawClient`](/docs/rawclient) (Beta) | — |
| Client Configuration | `HiveMQClientOptions` | `HiveMQClientOptionsBuilder` |
| Subscribe Configuration | `SubscribeOptions` | `SubscribeOptionsBuilder` |
| Unsubscribe Configuration | `UnsubscribeOptions` | `UnsubscribeOptionsBuilder` |
| Publish Message | `MQTT5PublishMessage` | `PublishMessageBuilder` |

## Complete Example

This example demonstrates the typical workflow: configure, connect, subscribe, and publish.

```csharp
using HiveMQtt.Client;
using HiveMQtt.MQTT5.Types;

// 1. Configure client options
var options = new HiveMQClientOptionsBuilder()
    .WithBroker("broker.hivemq.com")
    .WithPort(1883)
    .Build();

var client = new HiveMQClient(options);

// 2. Set up message handler BEFORE subscribing
client.OnMessageReceived += (sender, args) =>
{
    Console.WriteLine($"Message Received: {args.PublishMessage.PayloadAsString}");
};

// 3. Connect to the broker
var connectResult = await client.ConnectAsync().ConfigureAwait(false);

// 4. Subscribe to topics
var subscribeOptions = new SubscribeOptionsBuilder()
    .WithSubscription("topic1", QualityOfService.AtLeastOnceDelivery)
    .WithSubscription("topic2", QualityOfService.ExactlyOnceDelivery)
    .Build();

var subscribeResult = await client.SubscribeAsync(subscribeOptions).ConfigureAwait(false);

// 5. Publish a message
var publishResult = await client.PublishAsync("topic1/example", "Hello, MQTT!").ConfigureAwait(false);

// 6. Disconnect when done
await client.DisconnectAsync().ConfigureAwait(false);
client.Dispose();
```

:::tip Important
Always set up your message handlers **before** subscribing to topics. The broker may send retained messages immediately upon subscription.
:::

## Minimal Example

For quick testing with a local broker:

```csharp
using HiveMQtt.Client;

// Connect to localhost:1883 with defaults
var client = new HiveMQClient();
await client.ConnectAsync();

// Publish a message
await client.PublishAsync("test/topic", "Hello World!");

// Clean up
await client.DisconnectAsync();
client.Dispose();
```

## Next Steps

- [Connecting](/docs/connecting) - Connection options and TLS configuration
- [Publishing](/docs/publishing) - Message publishing options
- [Subscribing](/docs/subscribing) - Subscription management and callbacks
- [Events](/docs/events) - Hook into client lifecycle events
- [HiveMQClient Reference](/docs/hivemqclient) - Complete client documentation
