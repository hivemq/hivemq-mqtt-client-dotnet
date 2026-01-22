---
sidebar_position: 25
---

# RawClient (Beta)

:::caution Beta Feature
The `RawClient` is currently in **beta**. While fully functional, the API may change in future releases based on feedback. We encourage you to try it out and share your experiences.
:::

## Overview

The `RawClient` is a low-level, performance-oriented MQTT 5.0 client that provides direct access to the MQTT protocol without the subscription management features found in the standard `HiveMQClient`.

### Key Characteristics

| Feature | RawClient | HiveMQClient |
|---------|-----------|--------------|
| Subscription state tracking | No | Yes |
| Per-subscription callbacks | No | Yes |
| `client.Subscriptions` property | No | Yes |
| Message routing by subscription | No | Yes |
| Performance optimized | Yes | Standard |
| Memory footprint | Lower | Standard |

### When to Use RawClient

The `RawClient` is ideal for:

- **High-throughput scenarios** where minimal overhead is critical
- **Simple publish-only applications** that don't need subscription management
- **Custom subscription handling** where you want full control over message routing
- **Embedded or resource-constrained environments** where memory is limited
- **Protocol-level debugging** and testing

For most applications, we recommend using the standard `HiveMQClient` which provides subscription tracking, per-subscription callbacks, and a more feature-rich experience.

## Quick Start

```csharp
using HiveMQtt.Client;
using HiveMQtt.MQTT5.Types;

// Create a RawClient
var client = new RawClient();

// Setup a global message handler BEFORE connecting
client.OnMessageReceived += (sender, args) =>
{
    Console.WriteLine($"Message received on {args.PublishMessage.Topic}: {args.PublishMessage.PayloadAsString}");
};

// Connect to the broker
var connectResult = await client.ConnectAsync().ConfigureAwait(false);

// Subscribe to a topic (RawClient does NOT track this subscription)
await client.SubscribeAsync("my/topic", QualityOfService.AtLeastOnceDelivery).ConfigureAwait(false);

// Publish a message
await client.PublishAsync("my/topic", "Hello from RawClient!").ConfigureAwait(false);

// Disconnect when done
await client.DisconnectAsync().ConfigureAwait(false);
client.Dispose();
```

## Connecting

### With Default Options

Without any options, the `RawClient` will attempt to connect to `localhost` on port 1883.

```csharp
using HiveMQtt.Client;

var client = new RawClient();
var connectResult = await client.ConnectAsync().ConfigureAwait(false);
```

### With Specific Options

Use `HiveMQClientOptionsBuilder` to configure the client (same as `HiveMQClient`):

```csharp
var options = new HiveMQClientOptionsBuilder()
    .WithBroker("broker.hivemq.com")
    .WithPort(8883)
    .WithUseTls(true)
    .WithClientId("my-raw-client")
    .Build();

var client = new RawClient(options);
var connectResult = await client.ConnectAsync().ConfigureAwait(false);
```

### With Connect Options Override

You can override certain options at connect time:

```csharp
var client = new RawClient(options);

var connectOptions = new ConnectOptions
{
    KeepAlive = 120,
    SessionExpiryInterval = 600,
    CleanStart = false,
};

var connectResult = await client.ConnectAsync(connectOptions).ConfigureAwait(false);
```

## Publishing Messages

### Simple Publish

```csharp
// Publish with string payload (default QoS 0)
var result = await client.PublishAsync("topic/example", "Hello World!").ConfigureAwait(false);

// Publish with specific QoS
var result = await client.PublishAsync("topic/example", "Hello World!", QualityOfService.AtLeastOnceDelivery).ConfigureAwait(false);

// Publish with byte array payload
var payload = Encoding.UTF8.GetBytes("Binary payload");
var result = await client.PublishAsync("topic/example", payload, QualityOfService.ExactlyOnceDelivery).ConfigureAwait(false);
```

### With MQTT5PublishMessage

For full control over the publish message:

```csharp
var message = new MQTT5PublishMessage
{
    Topic = "topic/example",
    Payload = Encoding.UTF8.GetBytes("Advanced message"),
    QoS = QualityOfService.AtLeastOnceDelivery,
    Retain = true,
    ContentType = "application/json",
    ResponseTopic = "response/topic",
};

message.UserProperties.Add("Client-Geo", "-33.8688, 151.2093");

var result = await client.PublishAsync(message).ConfigureAwait(false);
```

### Performance Optimization

The `RawClient` includes a fast path for simple QoS 0 messages. When publishing QoS 0 messages without topic aliases or retain flags, the client skips validation overhead for maximum performance:

```csharp
// This takes the fast path - minimal overhead
var message = new MQTT5PublishMessage
{
    Topic = "sensor/data",
    Payload = sensorData,
    QoS = QualityOfService.AtMostOnceDelivery,
};

var result = await client.PublishAsync(message).ConfigureAwait(false);
```

## Subscribing to Topics

:::warning Important Difference
Unlike `HiveMQClient`, the `RawClient` does **not** maintain subscription state. Subscriptions are sent to the broker and acknowledged, but the client does not track them. There is no `client.Subscriptions` property and no per-subscription callbacks.
:::

### Simple Subscribe

```csharp
// Subscribe with default options (QoS 0)
await client.SubscribeAsync("my/topic").ConfigureAwait(false);

// Subscribe with specific QoS
await client.SubscribeAsync("my/topic", QualityOfService.AtLeastOnceDelivery).ConfigureAwait(false);

// Subscribe with additional options
await client.SubscribeAsync(
    "my/topic",
    QualityOfService.ExactlyOnceDelivery,
    noLocal: true,
    retainAsPublished: true,
    retainHandling: RetainHandling.SendAtSubscribe
).ConfigureAwait(false);
```

### With SubscribeOptions

```csharp
var subscribeOptions = new SubscribeOptionsBuilder()
    .WithSubscription("topic1", QualityOfService.AtLeastOnceDelivery)
    .WithSubscription("topic2", QualityOfService.ExactlyOnceDelivery)
    .WithUserProperty("Client-Type", "RawClient")
    .Build();

var result = await client.SubscribeAsync(subscribeOptions).ConfigureAwait(false);

// Check results
foreach (var subscription in result.Subscriptions)
{
    Console.WriteLine($"Subscribed to {subscription.TopicFilter.Topic}: {subscription.SubscribeReasonCode}");
}
```

## Receiving Messages

### Global Message Handler

The `RawClient` fires `OnMessageReceived` for **all** incoming PUBLISH packets. There is no subscription matching or per-subscription routing:

```csharp
client.OnMessageReceived += (sender, args) =>
{
    var message = args.PublishMessage;

    Console.WriteLine($"Topic: {message.Topic}");
    Console.WriteLine($"Payload: {message.PayloadAsString}");
    Console.WriteLine($"QoS: {message.QoS}");
    Console.WriteLine($"Retain: {message.Retain}");

    // You must implement your own topic routing if needed
    if (message.Topic.StartsWith("sensors/"))
    {
        HandleSensorMessage(message);
    }
    else if (message.Topic.StartsWith("commands/"))
    {
        HandleCommandMessage(message);
    }
};
```

### Implementing Custom Message Routing

Since `RawClient` doesn't provide per-subscription callbacks, you can implement your own routing:

```csharp
// Simple dictionary-based router
var handlers = new Dictionary<string, Action<MQTT5PublishMessage>>
{
    { "sensors/temperature", HandleTemperature },
    { "sensors/humidity", HandleHumidity },
    { "commands/restart", HandleRestart },
};

client.OnMessageReceived += (sender, args) =>
{
    var message = args.PublishMessage;

    if (handlers.TryGetValue(message.Topic, out var handler))
    {
        handler(message);
    }
    else
    {
        Console.WriteLine($"No handler for topic: {message.Topic}");
    }
};
```

## Unsubscribing

```csharp
var unsubscribeOptions = new UnsubscribeOptionsBuilder()
    .WithSubscription("topic1")
    .WithSubscription("topic2")
    .Build();

var result = await client.UnsubscribeAsync(unsubscribeOptions).ConfigureAwait(false);
```

## Disconnecting

```csharp
// Simple disconnect
await client.DisconnectAsync().ConfigureAwait(false);

// Disconnect with options
var disconnectOptions = new DisconnectOptions
{
    ReasonCode = DisconnectReasonCode.NormalDisconnection,
};

await client.DisconnectAsync(disconnectOptions).ConfigureAwait(false);
```

## Events

The `RawClient` supports the same lifecycle and packet-level events as `HiveMQClient`:

### Lifecycle Events

```csharp
client.BeforeConnect += (sender, args) => Console.WriteLine("About to connect...");
client.AfterConnect += (sender, args) => Console.WriteLine($"Connected: {args.ConnectResult.ReasonCode}");
client.BeforeDisconnect += (sender, args) => Console.WriteLine("About to disconnect...");
client.AfterDisconnect += (sender, args) => Console.WriteLine($"Disconnected (clean: {args.CleanDisconnect})");
client.BeforeSubscribe += (sender, args) => Console.WriteLine("About to subscribe...");
client.AfterSubscribe += (sender, args) => Console.WriteLine("Subscribed!");
client.BeforeUnsubscribe += (sender, args) => Console.WriteLine("About to unsubscribe...");
client.AfterUnsubscribe += (sender, args) => Console.WriteLine("Unsubscribed!");
```

### Packet-Level Events

For protocol-level debugging and monitoring:

```csharp
// Connection packets
client.OnConnectSent += (sender, args) => Console.WriteLine("CONNECT sent");
client.OnConnAckReceived += (sender, args) => Console.WriteLine("CONNACK received");
client.OnDisconnectSent += (sender, args) => Console.WriteLine("DISCONNECT sent");
client.OnDisconnectReceived += (sender, args) => Console.WriteLine("DISCONNECT received from broker");

// Keep-alive packets
client.OnPingReqSent += (sender, args) => Console.WriteLine("PINGREQ sent");
client.OnPingRespReceived += (sender, args) => Console.WriteLine("PINGRESP received");

// Subscription packets
client.OnSubscribeSent += (sender, args) => Console.WriteLine("SUBSCRIBE sent");
client.OnSubAckReceived += (sender, args) => Console.WriteLine("SUBACK received");
client.OnUnsubscribeSent += (sender, args) => Console.WriteLine("UNSUBSCRIBE sent");
client.OnUnsubAckReceived += (sender, args) => Console.WriteLine("UNSUBACK received");

// Publish packets
client.OnPublishSent += (sender, args) => Console.WriteLine($"PUBLISH sent: {args.PublishPacket.Message.Topic}");
client.OnPublishReceived += (sender, args) => Console.WriteLine($"PUBLISH received: {args.PublishPacket.Message.Topic}");

// QoS 1 acknowledgment
client.OnPubAckSent += (sender, args) => Console.WriteLine("PUBACK sent");
client.OnPubAckReceived += (sender, args) => Console.WriteLine("PUBACK received");

// QoS 2 handshake
client.OnPubRecSent += (sender, args) => Console.WriteLine("PUBREC sent");
client.OnPubRecReceived += (sender, args) => Console.WriteLine("PUBREC received");
client.OnPubRelSent += (sender, args) => Console.WriteLine("PUBREL sent");
client.OnPubRelReceived += (sender, args) => Console.WriteLine("PUBREL received");
client.OnPubCompSent += (sender, args) => Console.WriteLine("PUBCOMP sent");
client.OnPubCompReceived += (sender, args) => Console.WriteLine("PUBCOMP received");
```

## LocalStore

Like `HiveMQClient`, the `RawClient` provides a `LocalStore` dictionary for storing client-specific data:

```csharp
// Store application-specific data
client.LocalStore["device-id"] = "sensor-001";
client.LocalStore["location"] = "building-a";

// Retrieve later
var deviceId = client.LocalStore["device-id"];
```

## Resource Cleanup

Always dispose of the client when done:

```csharp
// Using pattern (recommended)
await using var client = new RawClient(options);
await client.ConnectAsync();
// ... use client ...
await client.DisconnectAsync();

// Or explicit dispose
var client = new RawClient(options);
try
{
    await client.ConnectAsync();
    // ... use client ...
    await client.DisconnectAsync();
}
finally
{
    client.Dispose();
}
```

## Complete Example

```csharp
using HiveMQtt.Client;
using HiveMQtt.MQTT5.Types;

// Configure the client
var options = new HiveMQClientOptionsBuilder()
    .WithBroker("broker.hivemq.com")
    .WithPort(1883)
    .WithClientId("raw-client-example")
    .Build();

var client = new RawClient(options);

// Setup message handler BEFORE connecting
client.OnMessageReceived += (sender, args) =>
{
    Console.WriteLine($"[{args.PublishMessage.Topic}]: {args.PublishMessage.PayloadAsString}");
};

// Track connection state changes
client.AfterConnect += (sender, args) => Console.WriteLine("Connected to broker");
client.AfterDisconnect += (sender, args) => Console.WriteLine("Disconnected from broker");

try
{
    // Connect
    var connectResult = await client.ConnectAsync().ConfigureAwait(false);

    if (connectResult.ReasonCode != ConnAckReasonCode.Success)
    {
        Console.WriteLine($"Connection failed: {connectResult.ReasonCode}");
        return;
    }

    // Subscribe to topics
    await client.SubscribeAsync("test/topic1", QualityOfService.AtLeastOnceDelivery).ConfigureAwait(false);
    await client.SubscribeAsync("test/topic2", QualityOfService.ExactlyOnceDelivery).ConfigureAwait(false);

    // Publish messages
    for (var i = 0; i < 10; i++)
    {
        await client.PublishAsync("test/topic1", $"Message {i}").ConfigureAwait(false);
        await Task.Delay(100);
    }

    // Wait for messages
    await Task.Delay(1000);

    // Disconnect
    await client.DisconnectAsync().ConfigureAwait(false);
}
finally
{
    client.Dispose();
}
```

## See Also

* [RawClient.cs](https://github.com/hivemq/hivemq-mqtt-client-dotnet/blob/main/Source/HiveMQtt/Client/RawClient.cs)
* [IRawClient.cs](https://github.com/hivemq/hivemq-mqtt-client-dotnet/blob/main/Source/HiveMQtt/Client/IRawClient.cs)
* [HiveMQClient](/docs/quickstart) - The standard client with full subscription management
* [Events](/docs/events) - Event system documentation
* [Publishing Messages](/docs/publishing) - Detailed publishing documentation
* [Subscribing to Topics](/docs/subscribing) - Detailed subscription documentation
