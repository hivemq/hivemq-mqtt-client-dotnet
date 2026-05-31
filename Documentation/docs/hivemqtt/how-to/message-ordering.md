---
sidebar_position: 10
---

# Message Ordering for OnMessageReceived

MQTT 5.0 [section 4.6 (Message Ordering)](https://docs.oasis-open.org/mqtt/mqtt/v5.0/os/mqtt-v5.0-os.html#_Toc3901252) requires that QoS 1 and QoS 2 application messages be delivered to a client in the order they were sent for a given Topic Filter.

HiveMQtt preserves that guarantee at the `OnMessageReceived` event boundary for **QoS 1 and QoS 2** messages.

## What is guaranteed

For QoS 1 and QoS 2 publishes:

- Handlers are **started** in FIFO order on a single per-client dispatch queue.
- Within one message, **global** `OnMessageReceived` handlers run first (in registration order), then **per-subscription** handlers (in subscription order, respecting `OverlappingSubscriptionBehavior`).
- Handlers run on a **dedicated message dispatch thread** (not arbitrary thread-pool workers).

The guarantee is **invocation (start) order**, not completion order.

## Async handlers

`OnMessageReceived` uses the standard .NET `EventHandler<T>` signature, which returns `void`. If you register an `async` lambda, it becomes **async void**:

```csharp
client.OnMessageReceived += async (sender, args) =>
{
    await SaveToDatabaseAsync(args);  // ordering does NOT extend past the first await
};
```

Only synchronous work **before the first `await`** and handler **start order** are ordered. Work that continues after `await` may overlap with later messages.

**Recommendation:** keep handlers thin and synchronous — parse the payload, enqueue to your own worker/channel, and return. Perform I/O and heavy logic on application-owned consumers.

## QoS 0

QoS 0 has no ordering requirement in the MQTT specification. HiveMQtt may invoke QoS 0 `OnMessageReceived` handlers concurrently via the thread pool.

## Disconnect behavior

When the client disconnects, the library **quiesces** the dispatch queue (similar to [Eclipse Paho](https://eclipse.dev/paho/files/javadoc/org/eclipse/paho/client/mqttv3/MqttClient.html#disconnect--)):

- The **in-flight** handler (if any) is allowed to finish.
- **Pending** handlers that have not started are **dropped**.
- New messages are not dispatched after quiesce begins.

After a successful reconnect, dispatch resumes for new messages.

## Manual acknowledgement

If you use [manual ack](/docs/hivemqtt/how-to/manual-ack), unacknowledged messages consume [Receive Maximum](https://www.hivemq.com/blog/mqtt5-essentials-part12-flow-control/) slots until you call `AckAsync`. Ordered dispatch with slow handlers can cause the broker to stop sending new messages sooner — that is expected back-pressure. See the manual ack guide for details.

## See also

* [Subscribing](/docs/hivemqtt/subscribing) — registering `OnMessageReceived` handlers
* [Manual Acknowledgement](/docs/hivemqtt/how-to/manual-ack) — ack timing and Receive Maximum
* [Lifecycle Events Reference](/docs/hivemqtt/events/Reference) — `OnMessageReceived` event args
