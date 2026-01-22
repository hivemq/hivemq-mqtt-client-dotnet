---
sidebar_position: 30
---

# Namespaces

The HiveMQ MQTT client library is organized into namespaces that group related functionality. This page helps you understand which namespaces to import for different use cases.

## Quick Reference

For most applications, you only need these two imports:

```csharp
using HiveMQtt.Client;       // Client classes and builders
using HiveMQtt.MQTT5.Types;  // QoS levels, topic filters, etc.
```

## Namespace Reference

### Core Namespaces

| Namespace | Purpose | Common Classes |
|-----------|---------|----------------|
| `HiveMQtt.Client` | Main client classes and builders | `HiveMQClient`, `RawClient`, `HiveMQClientOptionsBuilder`, `SubscribeOptionsBuilder`, `PublishMessageBuilder` |
| `HiveMQtt.MQTT5.Types` | MQTT protocol types and enums | `QualityOfService`, `RetainHandling`, `TopicFilter`, `MQTT5PublishMessage` |

### Additional Namespaces

| Namespace | Purpose | When to Use |
|-----------|---------|-------------|
| `HiveMQtt.Client.Options` | Options classes | When working directly with options classes instead of builders |
| `HiveMQtt.Client.Results` | Operation result classes | When inspecting detailed results from operations |
| `HiveMQtt.Client.Events` | Event argument classes | When creating typed event handlers |
| `HiveMQtt.Client.Exceptions` | Exception classes | When catching specific client exceptions |
| `HiveMQtt.Client.ReasonCodes` | Reason code enums | When checking specific QoS reason codes |
| `HiveMQtt.MQTT5.Packets` | Low-level packet classes | For packet-level event handling or debugging |
| `HiveMQtt.MQTT5.ReasonCodes` | Protocol-level reason codes | When checking specific MQTT reason codes |

## Usage Examples

### Basic Publishing and Subscribing

```csharp
using HiveMQtt.Client;
using HiveMQtt.MQTT5.Types;

var client = new HiveMQClient();
await client.ConnectAsync();
await client.SubscribeAsync("topic", QualityOfService.AtLeastOnceDelivery);
await client.PublishAsync("topic", "Hello!");
```

### Working with Events

```csharp
using HiveMQtt.Client;
using HiveMQtt.Client.Events;

client.OnMessageReceived += (sender, args) =>
{
    // args is OnMessageReceivedEventArgs
    Console.WriteLine(args.PublishMessage.PayloadAsString);
};
```

### Checking Results

```csharp
using HiveMQtt.Client;
using HiveMQtt.Client.Results;
using HiveMQtt.MQTT5.ReasonCodes;

var result = await client.ConnectAsync();
if (result.ReasonCode == ConnAckReasonCode.Success)
{
    Console.WriteLine("Connected successfully!");
}
```

### Exception Handling

```csharp
using HiveMQtt.Client;
using HiveMQtt.Client.Exceptions;

try
{
    await client.ConnectAsync();
}
catch (HiveMQttClientException ex)
{
    Console.WriteLine($"Client error: {ex.Message}");
}
```
