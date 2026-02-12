---
sidebar_position: 9
---

# Manual Acknowledgement of Incoming Publishes

:::info Version Note
The ability to manually acknowledge incoming publishes was added in **v0.40.0**.
:::

When **manual ack** is enabled, the client does not send PubAck (QoS 1) or PubRec (QoS 2) to the broker until your application explicitly acknowledges each received message. This gives you control over when the broker considers a message delivered—for example, after persisting it or completing business logic.

:::info When to use manual ack
Use manual ack when you need to process or store messages before the broker is told they were received. Unacknowledged messages consume slots in the [Receive Maximum](https://www.hivemq.com/blog/mqtt5-essentials-part12-flow-control/) window until you call `AckAsync` or the connection closes.
:::

## Enabling manual ack

Enable manual acknowledgement when building client options:

```csharp
using HiveMQtt.Client;

var options = new HiveMQClientOptionsBuilder()
    .WithBroker("broker.example.com")
    .WithPort(1883)
    .WithManualAck()   // or .WithManualAck(true)
    .Build();

var client = new HiveMQClient(options);
await client.ConnectAsync();
```

To disable (default): `.WithManualAck(false)` or omit the call.

## Acknowledging messages

Two overloads are available on both `IHiveMQClient` and `IRawClient`:

### By packet identifier

For QoS 1 and QoS 2, each received publish has a packet identifier. Use it to acknowledge:

```csharp
client.OnMessageReceived += (sender, args) =>
{
    // Only QoS 1 and 2 have a packet identifier
    if (args.PacketIdentifier.HasValue)
    {
        _ = client.AckAsync(args.PacketIdentifier.Value);
    }
};
```

### By event args (recommended when mixing QoS levels)

When your subscription receives both QoS 0 and QoS 1/2, use the event-args overload. It no-ops for QoS 0 (no packet identifier) and acknowledges for QoS 1 and 2:

```csharp
client.OnMessageReceived += async (sender, args) =>
{
    // Process the message (e.g. persist, forward)...
    await ProcessMessageAsync(args.PublishMessage);

    // Safe for any QoS: no-op for QoS 0, sends PubAck/PubRec for QoS 1/2
    await client.AckAsync(args);
};
```

This avoids having to check `PacketIdentifier` and prevents accidentally using it when it is null (QoS 0).

## QoS behavior

| QoS | Packet identifier | Manual ack behavior |
|-----|--------------------|----------------------|
| **0** | `null` | No ack is sent. `AckAsync(args)` is a no-op. |
| **1** | Set | Call `AckAsync` to send PubAck to the broker. |
| **2** | Set | Call `AckAsync` to send PubRec; client completes QoS 2 flow when broker sends PubRel. |

For QoS 0, `OnMessageReceivedEventArgs.PacketIdentifier` is always `null`. The client does not send any acknowledgement to the broker for QoS 0.

## Receive Maximum and unacked messages

The broker limits how many QoS 1 and QoS 2 publishes can be “in flight” to the client (Receive Maximum). With manual ack enabled, each message you have not yet acknowledged consumes one of those slots. If you receive more messages than the window size without acknowledging, the broker will stop delivering until you call `AckAsync` (or disconnect). Configure a larger window if you need more in-flight messages:

```csharp
var options = new HiveMQClientOptionsBuilder()
    .WithBroker("broker.example.com")
    .WithManualAck()
    .WithReceiveMaximum(100)   // Allow more unacked messages
    .Build();
```

## Exceptions

- **Manual ack not enabled:** Calling `AckAsync` when `ManualAckEnabled` is `false` throws `HiveMQttClientException`.
- **Invalid packet identifier:** If no pending incoming publish exists for the given packet id (e.g. wrong id or already acked), `AckAsync` throws `HiveMQttClientException`.
- **Double ack:** Acknowledging the same packet identifier more than once throws `HiveMQttClientException` (e.g. “Packet identifier X was already acknowledged.”).
- **Not connected:** Calling `AckAsync` when the client is not connected throws `HiveMQttClientException`.
- **Null event args:** `AckAsync(OnMessageReceivedEventArgs eventArgs)` throws `ArgumentNullException` if `eventArgs` is null.

## Thread safety

`AckAsync` may be called from any thread, including directly from your `OnMessageReceived` handler (which may run on a thread-pool thread). You do not need to marshal the call back to a specific thread.

## Full example (HiveMQClient)

```csharp
using HiveMQtt.Client;
using HiveMQtt.MQTT5.Types;

var options = new HiveMQClientOptionsBuilder()
    .WithBroker("broker.example.com")
    .WithPort(1883)
    .WithManualAck()
    .Build();

var client = new HiveMQClient(options);
client.OnMessageReceived += async (sender, args) =>
{
    try
    {
        // Your processing (e.g. save to DB, forward)
        await SaveToDatabaseAsync(args.PublishMessage);
    }
    finally
    {
        // Always ack when manual ack is enabled (no-op for QoS 0)
        await client.AckAsync(args);
    }
};

await client.ConnectAsync();
await client.SubscribeAsync("orders/#", QualityOfService.AtLeastOnceDelivery);
```

## RawClient

Manual ack works the same with `RawClient`: enable with `WithManualAck()` and use `AckAsync(packetIdentifier)` or `AckAsync(eventArgs)` on the `RawClient` instance. Use the packet identifier from your receive path (e.g. from the publish packet or from higher-level event args if you build them).

## See also

* [HiveMQClientOptions Reference](/docs/reference/client_options) — `ManualAckEnabled`
* [HiveMQClientOptionsBuilder Reference](/docs/reference/client_options_builder) — `WithManualAck`
* [Lifecycle Events](/docs/events) — `OnMessageReceived` and event args
* [MQTT 5 Essentials – Receive Maximum](https://www.hivemq.com/blog/mqtt5-essentials-part12-flow-control/)
