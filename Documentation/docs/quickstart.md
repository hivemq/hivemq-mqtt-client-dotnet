---
sidebar_position: 20
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
using HiveMQtt.MQTT5.Types;

// Setup Client options and instantiate
var options = new HiveMQClientOptionsBuilder().
                    WithBroker("candy.x39.eu.hivemq.cloud").
                    WithPort(8883).
                    WithUseTls(true).
                    Build();
var client = new HiveMQClient(options);

// Setup an application message handlers BEFORE subscribing to a topic
client.OnMessageReceived += (sender, args) =>
{
    Console.WriteLine("Message Received: {}", args.PublishMessage.PayloadAsString);
};

// Connect to the MQTT broker
var connectResult = await client.ConnectAsync().ConfigureAwait(false);

// Configure the subscriptions we want and subscribe
var builder = new SubscribeOptionsBuilder();
builder.WithSubscription("topic1", QualityOfService.AtLeastOnceDelivery)
       .WithSubscription("topic2", QualityOfService.ExactlyOnceDelivery);
var subscribeOptions = builder.Build();
var subscribeResult = await client.SubscribeAsync(subscribeOptions);

// Publish a message
var publishResult = await client.PublishAsync("topic1/example", "Hello Payload");
```


