---
sidebar_position: 2
---
# Quickstart

## Install

This package is [available on NuGet.org](https://www.nuget.org/packages/HiveMQtt/) and can be installed with:

```sh
dotnet add package HiveMQtt
```

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

#### HiveMQClientOptionsBuilder

To illustrate _each and every possible call_ with `HiveMQClientOptionsBuilder`, see the following example:

```csharp
var options = new HiveMQClientOptionsBuilder()
    .WithBroker("mqtt.example.com")
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
    .WithLastWill(new LastWillAndTestament { Topic = "lwt/topic", Message = "LWT message", QualityOfServiceLevel = MqttQualityOfServiceLevel.AtLeastOnce, Retain = true })
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

### Simple

Use the SubscribeAsync method to subscribe to the desired topic by providing the topic string as a parameter.

```csharp
await client.SubscribeAsync("instrument/x9284/boston").ConfigureAwait(false);
```

Optionally, you can specify the desired quality of service (QoS) level for the subscription. By default, the QoS level is set to MqttQualityOfServiceLevel.AtMostOnce.

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
builder.WithSubscription("topic1", MqttQualityOfServiceLevel.AtLeastOnce)
       .WithSubscription("topic2", MqttQualityOfServiceLevel.ExactlyOnce);
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


## Publishing

```csharp
// Message Handler
client.OnMessageReceived += (sender, args) =>
{
    Console.WriteLine("Message Received: {}", args.PublishMessage.PayloadAsString)
};

// Subscribe
await client.SubscribeAsync("instrument/x9284/boston").ConfigureAwait(false);

var publishMessage = new PublishMessageBuilder().
                            WithTopic("core/dynamic_graph/entity/227489").
                            WithPayload("{'2023': '👍'}").
                            Build();

await client.PublishAsync(publishMessage).ConfigureAwait(false);
```


