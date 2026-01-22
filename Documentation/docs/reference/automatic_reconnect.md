---
sidebar_position: 1
---

# Automatic Reconnect

The HiveMQ MQTT client provides automatic reconnection functionality that allows the client to automatically recover from unexpected disconnections. This feature is **disabled by default** and must be explicitly enabled.

## Enabling Automatic Reconnect

```csharp
var options = new HiveMQClientOptionsBuilder()
    .WithBroker("broker.hivemq.com")
    .WithAutomaticReconnect(true)
    .Build();

var client = new HiveMQClient(options);
await client.ConnectAsync();
```

## Backoff Strategy

The automatic reconnect uses an **exponential backoff** strategy:

| Attempt | Delay |
|---------|-------|
| 1st | 5 seconds |
| 2nd | 10 seconds |
| 3rd | 20 seconds |
| 4th | 40 seconds |
| 5th+ | 60 seconds (maximum) |

The client will continue attempting to reconnect **indefinitely** until successful.

## Monitoring Reconnection

Use events to track reconnection status:

```csharp
var client = new HiveMQClient(options);

// Triggered when reconnected successfully
client.AfterConnect += (sender, args) =>
{
    Console.WriteLine($"Connected/Reconnected: {args.ConnectResult.ReasonCode}");
};

// Triggered when disconnected (before reconnect attempts begin)
client.AfterDisconnect += (sender, args) =>
{
    if (!args.CleanDisconnect)
    {
        Console.WriteLine("Unexpected disconnection - reconnection will be attempted");
    }
};

await client.ConnectAsync();
```

## Re-subscribing After Reconnect

:::warning Important
After reconnection, you may need to re-subscribe to topics depending on your session configuration. If using `CleanStart = true`, subscriptions are not preserved across reconnections.
:::

```csharp
client.AfterConnect += async (sender, args) =>
{
    // Re-subscribe after reconnection
    await client.SubscribeAsync("my/topic", QualityOfService.AtLeastOnceDelivery);
};
```

## When to Use

| Scenario | Recommendation |
|----------|----------------|
| IoT devices with unstable networks | Enable |
| Long-running services | Enable |
| Short-lived connections | Usually not needed |
| Custom reconnect logic required | Disable and implement your own |

## See Also

- [Connecting to a Broker](/docs/connecting)
- [Events](/docs/events) - Monitor connection state changes
- [HiveMQClientOptionsBuilder Reference](/docs/reference/client_options_builder)
