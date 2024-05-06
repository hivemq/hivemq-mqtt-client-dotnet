---
sidebar_position: 50
---

# Subscribing to Topics

In MQTT, "subscribe" is an operation that allows an MQTT client to request to receive messages published to specific topics from an MQTT broker.


## Simple

Use the SubscribeAsync method to subscribe to the desired topic by providing the topic string as a parameter.

```csharp
await client.SubscribeAsync("instrument/x9284/boston").ConfigureAwait(false);
```

Optionally, you can specify the desired quality of service (QoS) level for the subscription. By default, the QoS level is set to `QualityOfServiceLevel.AtMostOnce`.

```csharp
using HiveMQtt.MQTT5.Types; // For QualityOfService enum

string topic = "my/topic";
QualityOfService qos = QualityOfService.AtLeastOnceDelivery;

await client.SubscribeAsync(topic, qosLevel);
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

var options = new SubscribeOptionsBuilder().
                    .WithSubscription(
                        "topic1",                             // Topic
                        QualityOfService.ExactlyOnceDelivery, // Quality of Service Level
                        true,                                 // NoLocal
                        true,                                 // RetainAsPublished
                        RetainHandling.SendAtSubscribe        // RetainHandling
                    ).
                    WithUserProperty("property1", "value1").
                    WithUserProperties(
                        new Dictionary<string, string> {
                            { "property1", "value1" }, { "property2", "value2" } }).
                    Build();

```

In `WithSubscription`, the first two arguments are required.  The additional optional parameters are defined as:

* `NoLoca`l: The NoLocal option, when set to true, indicates that the subscriber does not want to receive messages published by itself. This option is useful in scenarios where a client is publishing and subscribing to the same topic. By setting NoLocal to true, the client can avoid receiving its own published messages.

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
    Console.WriteLine("Message Received: {}", args.PublishMessage.PayloadAsString)
};

await client.ConnectAsync();
```

or alternatively:

```csharp
private static void MessageHandler(object? sender, OnMessageReceivedEventArgs eventArgs)
{
    Console.WriteLine("Message Received: {}", eventArgs.PublishMessage.PayloadAsString)
}

client.OnMessageReceived += MessageHandler;
await client.ConnectAsync();
```

In this example, the message handler is defined as a lambda function that writes the received message to the console. Only after the message handler is set up do we connect to the broker using the ConnectAsync method.

Remember, prioritizing the setup of your message handler ensures that your application is ready to process incoming messages as soon as the connection to the broker is established.

* See Also: [OnMessageReceivedEventArgs.cs](https://github.com/hivemq/hivemq-mqtt-client-dotnet/blob/main/Source/HiveMQtt/Client/Events/OnMessageReceivedEventArgs.cs)

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
            Console.WriteLine($"Message received on topic {e.Topic}: {e.Message}");
        })
    .Build();
```

In this example, we first create an instance of `SubscribeOptionsBuilder`. Then we call the `WithSubscription` method to add a subscription with a topic filter and an event handler. The event handler is a lambda function that writes a message to the console whenever a message is received on the subscribed topic. Finally, we call the Build method to create the SubscribeOptions.

Alternatively the message handler can be independently defined:

```csharp
private static void MessageHandler(object? sender, OnMessageReceivedEventArgs eventArgs)
{
    Console.WriteLine("Message Received: {}", eventArgs.PublishMessage.PayloadAsString)
}

var builder = new SubscribeOptionsBuilder();
var options = builder.WithSubscription(
        new TopicFilter("test/topic", QualityOfService.AtLeastOnceDelivery),
        MessageHandler)
    .Build();

```

## See Also

* [MQTT Topics, Wildcards, & Best Practices](https://www.hivemq.com/blog/mqtt-essentials-part-5-mqtt-topics-best-practices/)
* [MQTT Topic Tree & Topic Matching](https://www.hivemq.com/article/mqtt-topic-tree-matching-challenges-best-practices-explained/)
* [TopicFilter.cs](https://github.com/hivemq/hivemq-mqtt-client-dotnet/blob/main/Source/HiveMQtt/MQTT5/Types/TopicFilter.cs)
* [SubscribeOptions.cs](https://github.com/hivemq/hivemq-mqtt-client-dotnet/blob/main/Source/HiveMQtt/Client/Options/SubscribeOptions.cs)
* [SubscribeResult.cs](https://github.com/hivemq/hivemq-mqtt-client-dotnet/blob/main/Source/HiveMQtt/Client/Results/SubscribeResult.cs)
* [SubscribeOptionsBuilder.cs](https://github.com/hivemq/hivemq-mqtt-client-dotnet/blob/main/Source/HiveMQtt/Client/SubscribeOptionsBuilder.cs)

