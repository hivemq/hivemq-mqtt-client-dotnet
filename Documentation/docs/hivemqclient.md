---
sidebar_position: 22
---

# HiveMQClient

## Overview

The `HiveMQClient` is the full-featured MQTT 5.0 client for .NET applications. It provides a rich set of features including subscription management, per-subscription callbacks, automatic reconnect, and a comprehensive event system.

### Key Features

| Feature | Description |
|---------|-------------|
| Subscription tracking | Automatically tracks active subscriptions in `client.Subscriptions` |
| Per-subscription callbacks | Route messages to specific handlers based on topic |
| Automatic reconnect | Built-in reconnection with exponential backoff |
| Message routing | Incoming messages are matched to subscriptions and routed accordingly |
| Full MQTT 5.0 support | All MQTT 5.0 features including user properties, topic aliases, and more |

### HiveMQClient vs RawClient

For most applications, `HiveMQClient` is the recommended choice. Use `RawClient` only when you need maximum performance with minimal overhead.

| Feature | HiveMQClient | RawClient (Beta) |
|---------|--------------|------------------|
| Subscription state tracking | Yes | No |
| Per-subscription callbacks | Yes | No |
| `client.Subscriptions` property | Yes | No |
| Message routing by subscription | Yes | No |
| Automatic reconnect | Yes | No |
| Unsubscribe by topic string | Yes | No |
| Performance | Standard | Optimized |

## Quick Start

```csharp
using HiveMQtt.Client;
using HiveMQtt.MQTT5.Types;

// Create a client
var client = new HiveMQClient();

// Setup message handler BEFORE subscribing
client.OnMessageReceived += (sender, args) =>
{
    Console.WriteLine($"Message received on {args.PublishMessage.Topic}: {args.PublishMessage.PayloadAsString}");
};

// Connect to the broker
var connectResult = await client.ConnectAsync().ConfigureAwait(false);

// Subscribe to a topic (tracked in client.Subscriptions)
await client.SubscribeAsync("my/topic", QualityOfService.AtLeastOnceDelivery).ConfigureAwait(false);

// Publish a message
await client.PublishAsync("my/topic", "Hello from HiveMQClient!").ConfigureAwait(false);

// Disconnect when done
await client.DisconnectAsync().ConfigureAwait(false);
client.Dispose();
```

## Connecting

### With Default Options

Without any options, the client will attempt to connect to `localhost` on port 1883.

```csharp
using HiveMQtt.Client;

var client = new HiveMQClient();
var connectResult = await client.ConnectAsync().ConfigureAwait(false);
```

### With Specific Options

Use `HiveMQClientOptionsBuilder` to configure the client:

```csharp
var options = new HiveMQClientOptionsBuilder()
    .WithBroker("broker.hivemq.com")
    .WithPort(8883)
    .WithUseTls(true)
    .WithClientId("my-client-id")
    .Build();

var client = new HiveMQClient(options);
var connectResult = await client.ConnectAsync().ConfigureAwait(false);
```

### With Automatic Reconnect

Enable automatic reconnection to handle network interruptions:

```csharp
var options = new HiveMQClientOptionsBuilder()
    .WithBroker("broker.hivemq.com")
    .WithAutomaticReconnect(true)
    .Build();

var client = new HiveMQClient(options);
var connectResult = await client.ConnectAsync().ConfigureAwait(false);
```

The automatic reconnect uses an exponential backoff strategy:
- Initial delay: 5 seconds
- Maximum delay: 1 minute
- Attempts: Indefinite until successful

### Using WebSockets

```csharp
var options = new HiveMQClientOptionsBuilder()
    .WithWebSocketServer("ws://broker.hivemq.com:8000/mqtt")
    .Build();

var client = new HiveMQClient(options);
var connectResult = await client.ConnectAsync().ConfigureAwait(false);
```

### With Connect Options Override

Override certain options at connect time:

```csharp
var client = new HiveMQClient(options);

var connectOptions = new ConnectOptions
{
    KeepAlive = 120,
    SessionExpiryInterval = 600,
    CleanStart = false,
};

var connectResult = await client.ConnectAsync(connectOptions).ConfigureAwait(false);
```

### Checking Connection State

```csharp
if (client.IsConnected())
{
    Console.WriteLine("Client is connected");
}
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

### With PublishMessageBuilder

```csharp
var publishMessage = new PublishMessageBuilder()
    .WithTopic("topic/example")
    .WithPayload("{'HiveMQ': 'rocks'}")
    .WithContentType("application/json")
    .WithQualityOfServiceLevel(QualityOfService.AtLeastOnceDelivery)
    .WithRetainFlag(true)
    .WithResponseTopic("response/topic")
    .WithUserProperty("property1", "value1")
    .Build();

var result = await client.PublishAsync(publishMessage).ConfigureAwait(false);
```

### With Cancellation Token

```csharp
var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));

try
{
    var result = await client.PublishAsync(message, cts.Token).ConfigureAwait(false);
}
catch (OperationCanceledException)
{
    Console.WriteLine("Publish operation was cancelled");
}
```

## Subscribing to Topics

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
    .WithUserProperty("Client-Type", "HiveMQClient")
    .Build();

var result = await client.SubscribeAsync(subscribeOptions).ConfigureAwait(false);

// Check results
foreach (var subscription in result.Subscriptions)
{
    Console.WriteLine($"Subscribed to {subscription.TopicFilter.Topic}: {subscription.SubscribeReasonCode}");
}
```

### Accessing Tracked Subscriptions

The client automatically tracks all active subscriptions:

```csharp
// View all current subscriptions
foreach (var subscription in client.Subscriptions)
{
    Console.WriteLine($"Active subscription: {subscription.TopicFilter.Topic} (QoS: {subscription.TopicFilter.QoS})");
}

// Get a specific subscription by topic
var subscription = client.GetSubscriptionByTopic("my/topic");
if (subscription != null)
{
    Console.WriteLine($"Found subscription: {subscription.TopicFilter.Topic}");
}
```

### Per-Subscription Callbacks

Route messages to specific handlers based on topic:

```csharp
// Define a handler for a specific topic
void SensorHandler(object? sender, OnMessageReceivedEventArgs args)
{
    Console.WriteLine($"Sensor data: {args.PublishMessage.PayloadAsString}");
}

// Subscribe with a per-subscription callback
var options = new SubscribeOptionsBuilder()
    .WithSubscription(
        new TopicFilter("sensors/temperature", QualityOfService.AtLeastOnceDelivery),
        SensorHandler)
    .Build();

await client.SubscribeAsync(options).ConfigureAwait(false);
```

Or with inline lambda:

```csharp
var options = new SubscribeOptionsBuilder()
    .WithSubscription(
        new TopicFilter("sensors/temperature", QualityOfService.AtLeastOnceDelivery),
        (sender, args) =>
        {
            Console.WriteLine($"Temperature: {args.PublishMessage.PayloadAsString}");
        })
    .WithSubscription(
        new TopicFilter("sensors/humidity", QualityOfService.AtLeastOnceDelivery),
        (sender, args) =>
        {
            Console.WriteLine($"Humidity: {args.PublishMessage.PayloadAsString}");
        })
    .Build();

await client.SubscribeAsync(options).ConfigureAwait(false);
```

## Receiving Messages

### Global Message Handler

Set up a global handler for all incoming messages:

```csharp
client.OnMessageReceived += (sender, args) =>
{
    var message = args.PublishMessage;
    
    Console.WriteLine($"Topic: {message.Topic}");
    Console.WriteLine($"Payload: {message.PayloadAsString}");
    Console.WriteLine($"QoS: {message.QoS}");
    Console.WriteLine($"Retain: {message.Retain}");
};
```

:::tip Important
Set your message handlers **before** subscribing to topics. Once connected, the broker may start sending messages immediately, especially retained messages.
:::

### Message Handler as Method

```csharp
private static void MessageHandler(object? sender, OnMessageReceivedEventArgs args)
{
    Console.WriteLine($"Message received: {args.PublishMessage.PayloadAsString}");
}

// Register the handler
client.OnMessageReceived += MessageHandler;
await client.ConnectAsync();
```

## Unsubscribing

The `HiveMQClient` provides multiple ways to unsubscribe:

### By Topic String

```csharp
// Unsubscribe using the topic string
var result = await client.UnsubscribeAsync("my/topic").ConfigureAwait(false);
```

### By Subscription Object

```csharp
// Get a subscription from the tracked list
var subscription = client.GetSubscriptionByTopic("my/topic");

if (subscription != null)
{
    var result = await client.UnsubscribeAsync(subscription).ConfigureAwait(false);
}
```

### By List of Subscriptions

```csharp
// Unsubscribe from multiple topics at once
var subscriptionsToRemove = client.Subscriptions
    .Where(s => s.TopicFilter.Topic.StartsWith("sensors/"))
    .ToList();

var result = await client.UnsubscribeAsync(subscriptionsToRemove).ConfigureAwait(false);
```

### With UnsubscribeOptions

```csharp
var unsubscribeOptions = new UnsubscribeOptionsBuilder()
    .WithSubscription(subscription1)
    .WithSubscription(subscription2)
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

The `HiveMQClient` provides a comprehensive event system for monitoring and customizing client behavior.

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

The client provides a `LocalStore` dictionary for storing client-specific data:

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
await using var client = new HiveMQClient(options);
await client.ConnectAsync();
// ... use client ...
await client.DisconnectAsync();

// Or explicit dispose
var client = new HiveMQClient(options);
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
    .WithClientId("hivemqclient-example")
    .WithAutomaticReconnect(true)
    .Build();

var client = new HiveMQClient(options);

// Setup global message handler
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

    // Subscribe with per-subscription callbacks
    var subscribeOptions = new SubscribeOptionsBuilder()
        .WithSubscription(
            new TopicFilter("sensors/temperature", QualityOfService.AtLeastOnceDelivery),
            (sender, args) => Console.WriteLine($"Temperature: {args.PublishMessage.PayloadAsString}"))
        .WithSubscription(
            new TopicFilter("sensors/humidity", QualityOfService.AtLeastOnceDelivery),
            (sender, args) => Console.WriteLine($"Humidity: {args.PublishMessage.PayloadAsString}"))
        .Build();
    
    await client.SubscribeAsync(subscribeOptions).ConfigureAwait(false);

    // View tracked subscriptions
    Console.WriteLine($"Active subscriptions: {client.Subscriptions.Count}");

    // Publish messages
    for (var i = 0; i < 10; i++)
    {
        await client.PublishAsync("sensors/temperature", $"{20 + i}°C").ConfigureAwait(false);
        await Task.Delay(100);
    }

    // Wait for messages
    await Task.Delay(1000);

    // Unsubscribe by topic
    await client.UnsubscribeAsync("sensors/temperature").ConfigureAwait(false);

    // Disconnect
    await client.DisconnectAsync().ConfigureAwait(false);
}
finally
{
    client.Dispose();
}
```

## See Also

* [HiveMQClient.cs](https://github.com/hivemq/hivemq-mqtt-client-dotnet/blob/main/Source/HiveMQtt/Client/HiveMQClient.cs)
* [IHiveMQClient.cs](https://github.com/hivemq/hivemq-mqtt-client-dotnet/blob/main/Source/HiveMQtt/Client/IHiveMQClient.cs)
* [RawClient](/docs/rawclient) - Low-level client for performance-critical scenarios
* [Connecting](/docs/connecting) - Detailed connection documentation
* [Publishing Messages](/docs/publishing) - Detailed publishing documentation
* [Subscribing to Topics](/docs/subscribing) - Detailed subscription documentation
* [Events](/docs/events) - Event system documentation
* [Automatic Reconnect](/docs/reference/automatic_reconnect) - Reconnection behavior details
* [HiveMQClientOptionsBuilder Reference](/docs/reference/client_options_builder)
