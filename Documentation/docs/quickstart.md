---
sidebar_position: 2
---
# Quickstart

## Install

This package is [available on NuGet.org](https://www.nuget.org/packages/HiveMQtt/) and can be installed with:

```sh
dotnet add package HiveMQtt
```

This client is 100% open-source.  Find the source code in the [Github repository](https://github.com/hivemq/hivemq-mqtt-client-dotnet). 

## Overview

This C# client provides a user-friendly builder pattern interface for simplified usage.

The following table serves as a handy reference for the most frequently utilized classes within the library:

| Description          | Core Class         | Builder Class      |
|-----------------------|---------------|---------------------|
| The Client | `HiveMQClient` ([Docs](/docs/quickstart#hivemqclient-connect-with-defaults), [Source](https://github.com/hivemq/hivemq-mqtt-client-dotnet/tree/main/Source/HiveMQtt/Client)) | None |
| Client Options | [`HiveMQClientOptions`](https://github.com/hivemq/hivemq-mqtt-client-dotnet/blob/main/Source/HiveMQtt/Client/Options/HiveMQClientOptions.cs) | [`HiveMQClientOptionsBuilder`](https://github.com/hivemq/hivemq-mqtt-client-dotnet/blob/main/Source/HiveMQtt/Client/HiveMQClientOptionsBuilder.cs) |
| Subscribe Options | [`SubscribeOptions`](https://github.com/hivemq/hivemq-mqtt-client-dotnet/blob/main/Source/HiveMQtt/Client/Options/SubscribeOptions.cs) | [`SubscribeOptionsBuilder`](https://github.com/hivemq/hivemq-mqtt-client-dotnet/blob/builder/Source/HiveMQtt/Client/SubscribeOptionsBuilder.cs) |
| Unsubscribe Options | [`UnsubscribeOptions`](https://github.com/hivemq/hivemq-mqtt-client-dotnet/blob/main/Source/HiveMQtt/Client/Options/UnsubscribeOptions.cs) | [`UnsubscribeOptionsBuilder`](https://github.com/hivemq/hivemq-mqtt-client-dotnet/blob/builder/Source/HiveMQtt/Client/UnsubscribeOptionsBuilder.cs) |
| An Application Message | [`MQTT5PublishMessage`](https://github.com/hivemq/hivemq-mqtt-client-dotnet/blob/main/Source/HiveMQtt/MQTT5/Types/MQTT5PublishMessage.cs) | [`PublishMessageBuilder`](https://github.com/hivemq/hivemq-mqtt-client-dotnet/blob/main/Source/HiveMQtt/Client/PublishMessageBuilder.cs) |

## Common Usage: Complete Example

The example presented below illustrates the prevalent usage pattern for our client, offering a solid foundation from which you can build upon. This serves as a practical starting point, showcasing the most common workflows to guide and assist you in efficiently implementing and customizing the client based on your specific needs.

```csharp
using HiveMQtt.Client;
using HiveMQtt.MQTT5.Types; // For QualityOfService enum

var options = new HiveMQClientOptionsBuilder().
                    WithBroker('candy.x39.eu.hivemq.cloud').
                    WithPort(8883).
                    WithUseTLS(true).
                    Build();

// Instantiate the HiveMQtt client
var client = new HiveMQClient(options);

// Setup application message handlers FIRST
client.OnMessageReceived += (sender, args) =>
{
    Console.WriteLine("Message Received: {}", args.PublishMessage.PayloadAsString)
};

// Connect
var connectResult = await client.ConnectAsync().ConfigureAwait(false);

// Create subscribe options for topics we want to subscribe to
var builder = new SubscribeOptionsBuilder();
builder.WithSubscription("topic1", QualityOfService.AtLeastOnceDelivery)
       .WithSubscription("topic2", QualityOfService.ExactlyOnceDelivery);
var subscribeOptions = builder.Build();

// Subscribe
var subscribeResult = await client.SubscribeAsync(subscribeOptions);

// Publish
var publishResult = await client.PublishAsync("topic1/example", "Hello Payload")
```

## Connecting

### with Defaults

Without any options given, the `HiveMQClient` will search on `localhost` port 1883 for an unsecured broker.

If you don't have a broker at this location, see the next sections.

```csharp
using HiveMQtt.Client;

// Connect
var client = new HiveMQClient();
var connectResult = await client.ConnectAsync().ConfigureAwait(false);
```

### With Options

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

### Reference

To illustrate _each and every possible call_ with `HiveMQClientOptionsBuilder`, see the following example:

```csharp
using HiveMQtt.MQTT5.Types; // For QualityOfService enum

var options = new HiveMQClientOptionsBuilder()
    .WithBroker("broker.hivemq.com")
    .WithPort(1883)
    .WithClientId("myClientId")
    .WithAllowInvalidBrokerCertificates(true)
    .WithUseTls(true)
    .WithCleanStart(true)
    .WithKeepAlive(60)
    .WithAuthenticationMethod("UsernamePassword")
    .WithAuthenticationData(Encoding.UTF8.GetBytes("authenticationData"))
    .WithUserProperty("property1", "value1")
    .WithUserProperties(new Dictionary<string, string> { { "property1", "value1" }, { "property2", "value2" } })
    .WithLastWill(new LastWillAndTestament {
                            Topic = "lwt/topic",
                            PayloadAsString = "LWT message",
                            QoS = QualityOfService.AtLeastOnceDelivery,
                            Retain = true })
    .WithMaximumPacketSize(1024)
    .WithReceiveMaximum(100)
    .WithSessionExpiryInterval(3600)
    .WithUserName("myUserName")
    .WithPassword("myPassword")
    .WithPreferIPv6(true)
    .WithTopicAliasMaximum(10)
    .WithRequestProblemInformation(true)
    .WithRequestResponseInformation(true)
    .Build();
```

## Subscribing

In MQTT, "subscribe" is an operation that allows an MQTT client to request to receive messages published to specific topics from an MQTT broker.

### Simple

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

### With Options

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

### Reference

To illustrate _each and every possible call_ with `SubscribeOptionsBuilder`, see the following example:

```csharp
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

* NoLocal: The NoLocal option, when set to true, indicates that the subscriber does not want to receive messages published by itself. This option is useful in scenarios where a client is publishing and subscribing to the same topic. By setting NoLocal to true, the client can avoid receiving its own published messages.

* RetainAsPublished: The RetainAsPublished option, when set to false, indicates that the broker should not send retained messages to the subscriber when it first subscribes to a topic. Retained messages are those that are stored by the broker and sent to new subscribers upon subscription. By setting RetainAsPublished to false, the subscriber will not receive any retained messages for that topic.

* Retain handling: Retain handling refers to the behavior of the broker when it receives a subscription request for a topic that has retained messages. In MQTT 5, there are three options for retain handling:

  * `RetainHandling.SendAtSubscribe`: The broker sends any retained messages for the topic to the subscriber immediately upon subscription.
  * `RetainHandling.SendAtSubscribeIfNewSubscription`: The broker sends retained messages to new subscribers only if there are no existing subscriptions for that topic.
  * `RetainHandling.DoNotSendAtSubscribe`: The broker does not send any retained messages to the subscriber upon subscription.

These options provide flexibility and control over the behavior of the subscription process in MQTT 5, allowing subscribers to customize their experience based on their specific requirements.


## Publishing

In MQTT, "publish" is an operation that allows an MQTT client to send a message to an MQTT broker, which then distributes the message to all subscribed clients interested in the topic of the message.

### Simple

Use the PublishAsync method to publish a payload to the desired topic by providing the topic string and payload as parameters.

```csharp
var publishResult = await client.PublishAsync("topic1/example", "Hello Payload")
```

Optionally, you can specify the desired quality of service (QoS) level for the publish. By default, the QoS level is set to `QualityOfService.AtMostOnceDelivery`.

```csharp
using HiveMQtt.MQTT5.Types; // For the QualityOfService enum

var publishResult = await client.PublishAsync("topic1/example", "Hello Payload", QualityOfService.ExactlyOnceDelivery)
```

### With Options

The `PublishMessageBuilder` class provides a convenient way to construct MQTT publish messages with various options and properties. It allows you to customize the topic, payload, quality of service (QoS) level, retain flag, and other attributes of the message.

```csharp
var publishMessage = new PublishMessageBuilder().
                            WithTopic("topic1/example").
                            WithPayload("{'HiveMQ': '👍'}").
                            WithContentType("application/json")
                            WithResponseTopic("response/topic")
                            Build();

await client.PublishAsync(publishMessage).ConfigureAwait(false);
```

By using `PublishMessageBuilder`, you can easily construct MQTT publish messages with the desired properties and options. It provides a fluent and intuitive way to customize the topic, payload, QoS level, retain flag, and other attributes of the message.

### Reference

To illustrate _each and every possible call_ with `PublishMessageBuilder`, see the following example:

```csharp
var publishMessage = new PublishMessageBuilder()
    .WithTopic("topic1/example")
    .WithPayload("Hello, HiveMQtt!")
    .WithQualityOfServiceLevel(QualityOfService.AtLeastOnceDelivery)
    .WithRetainFlag(true)
    .WithPayloadFormatIndicator(MQTT5PayloadFormatIndicator.UTF8Encoded)
    .WithContentType("application/json")
    .WithResponseTopic("response/topic")
    .WithCorrelationData(Encoding.UTF8.GetBytes("correlation-data"))
    .WithUserProperty("property1", "value1")
    .WithUserProperties(new Dictionary<string, string> { { "property1", "value1" }, { "property2", "value2" } });
    .WithMessageExpiryInterval(3600)
    .WithSubscriptionIdentifier(123)
    .WithSubscriptionIdentifiers(1, 2, 3)
    .WithTopicAlias(456)
    .WithContentTypeAlias(789)
    .WithResponseTopicAlias(987)
    .Build()
```
