---
sidebar_position: 5
---

# Subscribing to Topics

In MQTT, "subscribe" is an operation that allows an MQTT client to request to receive messages published to specific topics from an MQTT broker.


## Simple

Use the SubscribeAsync method to subscribe to the desired topic by providing the topic string as a parameter.

```csharp
await client.SubscribeAsync("instrument/x9284/boston").ConfigureAwait(false);
```

Optionally, you can specify the desired quality of service (QoS) level for the subscription. By default, the QoS level is set to `QualityOfService.AtMostOnceDelivery`.

```csharp
using HiveMQtt.MQTT5.Types; // For QualityOfService enum

string topic = "my/topic";
QualityOfService qos = QualityOfService.AtLeastOnceDelivery;

await client.SubscribeAsync(topic, qos).ConfigureAwait(false);
```

:::tip

Make sure to set your message handlers **before subscribing**.  See [this section](#important-tip-prioritize-setting-your-message-handlers) below for more details.

:::

## With Options

The `SubscribeOptionsBuilder` class provides a convenient way to construct subscription options for MQTT subscriptions. It allows you to configure various aspects of the subscription(s), including the topic filter, quality of service (QoS) level, user properties, and more.

To use the SubscribeOptionsBuilder:

Create an instance of the SubscribeOptionsBuilder class.

```csharp
var builder = new SubscribeOptionsBuilder();
```

Use the `WithSubscription` method to specify the topic filter and QoS level for the subscription. This method can be called multiple times to create multiple subscriptions at once.

```csharp
builder.WithSubscription("topic1", QualityOfService.AtLeastOnceDelivery)
       .WithSubscription("topic2", QualityOfService.ExactlyOnceDelivery);
```

Optionally, you can use the `WithUserProperties` method to add custom user properties to the subscription. User properties are key-value pairs that provide additional metadata or application-specific information.

```csharp
var userProperties = new Dictionary<string, string>
{
    { "property1", "value1" },
    { "property2", "value2" }
};

builder.WithUserProperties(userProperties);
```

There also exists a singular `WithUserProperty` if you just need to send one key-value pair:

```csharp
builder.WithUserProperty("property1", "value1")
```

Call the Build method to create the SubscribeOptions object.

```csharp
var options = builder.Build();
```

Use the created `SubscribeOptions` object when subscribing to MQTT topics using the MQTT client library.

```csharp
await client.SubscribeAsync(options);
```

By using the `SubscribeOptionsBuilder`, you can easily configure multiple subscriptions with different topic filters and QoS levels. Additionally, you have the flexibility to include custom user properties to provide additional information or metadata for the subscriptions.

### `SubscribeOptionsBuilder` Reference

To illustrate _each and every possible call_ with `SubscribeOptionsBuilder`, see the following example:

```csharp
using HiveMQtt.MQTT5.Types;

var options = new SubscribeOptionsBuilder()
    .WithSubscription(
        "topic1",                             // Topic
        QualityOfService.ExactlyOnceDelivery, // Quality of Service Level
        true,                                 // NoLocal
        true,                                 // RetainAsPublished
        RetainHandling.SendAtSubscribe)       // RetainHandling
    .WithUserProperty("property1", "value1")
    .WithUserProperties(
        new Dictionary<string, string> {
            { "property1", "value1" }, { "property2", "value2" } })
    .Build();
```

In `WithSubscription`, the first two arguments are required.  The additional optional parameters are defined as:

* `NoLocal`: The NoLocal option, when set to true, indicates that the subscriber does not want to receive messages published by itself. This option is useful in scenarios where a client is publishing and subscribing to the same topic. By setting NoLocal to true, the client can avoid receiving its own published messages.

* `RetainAsPublished`: The RetainAsPublished option, when set to false, indicates that the broker should not send retained messages to the subscriber when it first subscribes to a topic. Retained messages are those that are stored by the broker and sent to new subscribers upon subscription. By setting RetainAsPublished to false, the subscriber will not receive any retained messages for that topic.

* `Retain handling`: Retain handling refers to the behavior of the broker when it receives a subscription request for a topic that has retained messages. In MQTT 5, there are three options for retain handling:

  * `RetainHandling.SendAtSubscribe`: The broker sends any retained messages for the topic to the subscriber immediately upon subscription.
  * `RetainHandling.SendAtSubscribeIfNewSubscription`: The broker sends retained messages to new subscribers only if there are no existing subscriptions for that topic.
  * `RetainHandling.DoNotSendAtSubscribe`: The broker does not send any retained messages to the subscriber upon subscription.

These options provide flexibility and control over the behavior of the subscription process in MQTT 5, allowing subscribers to customize their experience based on their specific requirements.

## Important Tip: Prioritize Setting Your Message Handlers

In MQTT communication, the message handler is responsible for processing incoming messages from the broker. It's crucial to set up your message handler before establishing a connection to the MQTT broker.

Why is this order important? Once a connection is established, the broker may start sending messages immediately, especially if there are retained messages for the topics you're subscribing to. If the message handler is not set up in advance, these incoming messages might not be processed, leading to potential data loss or unexpected behavior.



```csharp
// Message Handler
client.OnMessageReceived += (sender, args) =>
{
    Console.WriteLine($"Message Received: {args.PublishMessage.PayloadAsString}");
};

await client.ConnectAsync();
```

or alternatively:

```csharp
private static void MessageHandler(object? sender, OnMessageReceivedEventArgs eventArgs)
{
    Console.WriteLine($"Message Received: {eventArgs.PublishMessage.PayloadAsString}");
}

client.OnMessageReceived += MessageHandler;
await client.ConnectAsync();
```

In this example, the message handler is defined as a lambda function that writes the received message to the console. Only after the message handler is set up do we connect to the broker using the ConnectAsync method.

Remember, prioritizing the setup of your message handler ensures that your application is ready to process incoming messages as soon as the connection to the broker is established.

* See Also: [OnMessageReceivedEventArgs.cs](https://github.com/hivemq/hivemq-mqtt-client-dotnet/blob/main/Source/HiveMQtt/Client/Events/OnMessageReceivedEventArgs.cs)
* See Also: [Message Ordering](/docs/hivemqtt/how-to/message-ordering) — QoS 1/2 handler start order and async handler limits

## Subscribe: Multiple Topics At Once

```csharp
using HiveMQtt.Client.Options;
using HiveMQtt.Client.Results;

var options = new SubscribeOptions();
options.TopicFilters.Add(new TopicFilter { Topic = "foo/boston", QoS = QualityOfService.AtLeastOnceDelivery });
options.TopicFilters.Add(new TopicFilter { Topic = "bar/landshut", QoS = QualityOfService.AtMostOnceDelivery });

var result = await client.SubscribeAsync(options);
```

* `result.Subscriptions` contains the list of subscriptions made with this call
* `client.Subscriptions` is updated with complete list of subscriptions made up to this point
* each `Subscription` object has a resulting `ReasonCode` that represents the Subscribe result in `result.Subscriptions[0].ReasonCode`

### Using `SubscribeOptionsBuilder`

```csharp
var subscribeOptions = new SubscribeOptionsBuilder()
            .WithSubscription("my/topic1", MQTT5.Types.QualityOfService.AtLeastOnceDelivery)
            .WithSubscription("my/topic/2", MQTT5.Types.QualityOfService.AtLeastOnceDelivery, true, true, MQTT5.Types.RetainHandling.SendAtSubscribe)
            .WithUserProperty("Client-Geo", "38.115662, 13.361470")
            .Build();

var subResult = await subClient.SubscribeAsync(subscribeOptions).ConfigureAwait(false);
```

## Per Subscription Callbacks

### Introduction

The `SubscribeOptionsBuilder` class in the HiveMQtt client library provides a convenient way to configure subscription options for MQTT subscriptions. One of the key features of the `SubscribeOptionsBuilder` is the ability to specify per subscription callbacks using the `WithSubscription` method. This allows you to define custom event handlers that will be invoked when messages are received for specific topics.

### The Problem

In MQTT communication, it is common to have different subscriptions for different topics, each requiring specific handling or processing of the received messages. The challenge is to associate a specific callback or event handler with each subscription, so that the appropriate logic can be executed when messages are received for those topics.

### Per Subscription Callbacks with `WithSubscription``

The `WithSubscription` method of the `SubscribeOptionsBuilder` class provides a solution to this problem. It allows you to specify a topic filter and an event handler that will be associated with that topic filter. The event handler will be invoked whenever a message is received for the subscribed topic.

The signature of the `WithSubscription` method is as follows:

```csharp
public SubscribeOptionsBuilder WithSubscription(TopicFilter topicFilter, EventHandler<OnMessageReceivedEventArgs>? handler = null)
```

Here's an example of how you might use the `WithSubscription` method to set up a per subscription callback:

```csharp
var builder = new SubscribeOptionsBuilder();
var options = builder.WithSubscription(
        new TopicFilter("test/topic", QualityOfService.AtLeastOnceDelivery),
        (sender, e) =>
        {
            Console.WriteLine($"Message received on topic {e.PublishMessage.Topic}: {e.PublishMessage.PayloadAsString}");
        })
    .Build();
```

In this example, we first create an instance of `SubscribeOptionsBuilder`. Then we call the `WithSubscription` method to add a subscription with a topic filter and an event handler. The event handler is a lambda function that writes a message to the console whenever a message is received on the subscribed topic. Finally, we call the Build method to create the SubscribeOptions.

Alternatively the message handler can be independently defined:

```csharp
private static void MessageHandler(object? sender, OnMessageReceivedEventArgs eventArgs)
{
    Console.WriteLine($"Message Received: {eventArgs.PublishMessage.PayloadAsString}");
}

var builder = new SubscribeOptionsBuilder();
var options = builder.WithSubscription(
        new TopicFilter("test/topic", QualityOfService.AtLeastOnceDelivery),
        MessageHandler)
    .Build();
```

## Overlapping Subscriptions

### The Problem

When you subscribe to overlapping topic patterns, a single published message may match multiple subscriptions. For example:

```csharp
await client.SubscribeAsync("sensors/#");           // Matches: sensors/temperature/livingroom
await client.SubscribeAsync("sensors/+/temperature"); // Matches: sensors/livingroom/temperature
```

A message published to `sensors/livingroom/temperature` matches **both** subscriptions.

Depending on your MQTT broker, this can result in:
- **Single delivery**: One PUBLISH packet sent by the broker
- **Multiple deliveries**: Multiple PUBLISH packets (one per matching subscription)

The `OverlappingSubscriptionBehavior` setting controls how the client handles this.

### Behavior Options

The `HiveMQClientOptions.OverlappingSubscriptionBehavior` property controls this behavior:

| Option | Description | When to Use |
|--------|-------------|-------------|
| `FireAllMatchingHandlers` (default) | Fires a handler for **each** matching subscription | Backward compatibility, different logic per subscription |
| `FireFirstMatchingHandler` | Fires only the **first** matching subscription handler | Single message processing, simpler handling |

### Option 1: `FireAllMatchingHandlers` (Default)

**Behavior**: The `OnMessageReceived` event fires **once for each matching subscription** that has a handler registered.

**Example**:

```csharp
var options = new HiveMQClientOptionsBuilder()
    .WithOverlappingSubscriptionBehavior(OverlappingSubscriptionBehavior.FireAllMatchingHandlers) // Default
    .Build();

var client = new HiveMQClient(options);

var opts1 = new SubscribeOptions();
opts1.TopicFilters.Add(new TopicFilter("sensors/#", QualityOfService.AtLeastOnceDelivery));
opts1.Handlers["sensors/#"] = (s, e) => Console.WriteLine("Handler 1: Wildcard match");
await client.SubscribeAsync(opts1);

var opts2 = new SubscribeOptions();
opts2.TopicFilters.Add(new TopicFilter("sensors/+/temperature", QualityOfService.AtLeastOnceDelivery));
opts2.Handlers["sensors/+/temperature"] = (s, e) => Console.WriteLine("Handler 2: Specific match");
await client.SubscribeAsync(opts2);

// Publish to sensors/livingroom/temperature
// Output:
// Handler 1: Wildcard match
// Handler 2: Specific match
```

:::note

Only subscriptions **with handlers** fire events. The global `OnMessageReceived` event always fires separately.

:::

### Option 2: `FireFirstMatchingHandler` (Recommended)

**Behavior**: The `OnMessageReceived` event fires **only once** for the first matching subscription.

**Example**:

```csharp
var options = new HiveMQClientOptionsBuilder()
    .WithOverlappingSubscriptionBehavior(OverlappingSubscriptionBehavior.FireFirstMatchingHandler)
    .Build();

var client = new HiveMQClient(options);

var opts1 = new SubscribeOptions();
opts1.TopicFilters.Add(new TopicFilter("sensors/#", QualityOfService.AtLeastOnceDelivery));
opts1.Handlers["sensors/#"] = (s, e) => Console.WriteLine("Handler 1: Wildcard match");
await client.SubscribeAsync(opts1);  // First subscription

var opts2 = new SubscribeOptions();
opts2.TopicFilters.Add(new TopicFilter("sensors/+/temperature", QualityOfService.AtLeastOnceDelivery));
opts2.Handlers["sensors/+/temperature"] = (s, e) => Console.WriteLine("Handler 2: Specific match");
await client.SubscribeAsync(opts2);  // Second subscription

// Publish to sensors/livingroom/temperature
// Output:
// Handler 1: Wildcard match
// (Handler 2 does NOT fire because it's not the first match)
```

:::tip

"First" means **first in subscription order** (the order you called `SubscribeAsync`). The global `OnMessageReceived` event always fires regardless of this setting.

:::

### Edge Cases

#### First Subscription Has No Handler

If the first matching subscription has no handler, but a later one does:

```csharp
await client.SubscribeAsync("sensors/#");  // No per-subscription handler

var opts = new SubscribeOptions();
opts.TopicFilters.Add(new TopicFilter("sensors/+/temperature", QualityOfService.AtLeastOnceDelivery));
opts.Handlers["sensors/+/temperature"] = MyHandler;
await client.SubscribeAsync(opts);

// Publish to sensors/livingroom/temperature
// With FireFirstMatchingHandler:
//   - NO per-subscription handler fires (sensors/# was first but has no handler)
//   - Global OnMessageReceived still fires
```

**Recommendation**: Either register handlers on all overlapping subscriptions, use the global `OnMessageReceived` event, or ensure your most specific subscription is added first.

#### Mixed Global and Per-Subscription Handlers

The global `OnMessageReceived` event is **not affected** by the behavior setting:

```csharp
var options = new HiveMQClientOptionsBuilder()
    .WithOverlappingSubscriptionBehavior(OverlappingSubscriptionBehavior.FireFirstMatchingHandler)
    .Build();

var client = new HiveMQClient(options);

// Global handler
client.OnMessageReceived += (s, e) => Console.WriteLine("Global handler");

// Per-subscription handler
var opts = new SubscribeOptions();
opts.TopicFilters.Add(new TopicFilter("sensors/#", QualityOfService.AtLeastOnceDelivery));
opts.Handlers["sensors/#"] = (s, e) => Console.WriteLine("Per-subscription handler");
await client.SubscribeAsync(opts);

// Publish to sensors/temperature
// Output:
// Global handler
// Per-subscription handler
// (Both fire - global is always independent)
```

### Quick Reference Table

| Scenario | `FireAllMatchingHandlers` | `FireFirstMatchingHandler` |
|----------|---------------------------|---------------------------|
| 3 overlapping subs, all with handlers | 3 handler calls | 1 handler call (first only) |
| 3 overlapping subs, only 2nd has handler | 1 handler call (2nd only) | 0 handler calls (1st has none) |
| Global + 2 per-sub handlers | Global + 2 handlers | Global + 1 handler (first only) |
| Global only (no per-sub handlers) | Global fires once | Global fires once |

### Migration Guide

#### For New Users

We recommend using `FireFirstMatchingHandler` for new applications:

```csharp
var options = new HiveMQClientOptionsBuilder()
    .WithOverlappingSubscriptionBehavior(OverlappingSubscriptionBehavior.FireFirstMatchingHandler)
    .Build();

var client = new HiveMQClient(options);
```

#### For Existing Users

**No changes required** - the default remains `FireAllMatchingHandlers` for backward compatibility.

## See Also

* [MQTT Topics, Wildcards, & Best Practices](https://www.hivemq.com/blog/mqtt-essentials-part-5-mqtt-topics-best-practices/)
* [MQTT Topic Tree & Topic Matching](https://www.hivemq.com/article/mqtt-topic-tree-matching-challenges-best-practices-explained/)
* [TopicFilter.cs](https://github.com/hivemq/hivemq-mqtt-client-dotnet/blob/main/Source/HiveMQtt/MQTT5/Types/TopicFilter.cs)
* [SubscribeOptions.cs](https://github.com/hivemq/hivemq-mqtt-client-dotnet/blob/main/Source/HiveMQtt/Client/Options/SubscribeOptions.cs)
* [SubscribeResult.cs](https://github.com/hivemq/hivemq-mqtt-client-dotnet/blob/main/Source/HiveMQtt/Client/Results/SubscribeResult.cs)
* [SubscribeOptionsBuilder.cs](https://github.com/hivemq/hivemq-mqtt-client-dotnet/blob/main/Source/HiveMQtt/Client/SubscribeOptionsBuilder.cs)
