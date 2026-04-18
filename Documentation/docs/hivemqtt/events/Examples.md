---
sidebar_position: 2
---
# Usage Examples

The following serves as a few examples on how to utilize the built in event system.

## Pattern

All events are available on the client instance: `client.<event_name>`.

Handlers can be assigned to the event by simply adding your handler to the list.  Handlers can be either a pre-defined function or a lambda (illustrated in the next section).

```csharp
client.AfterDisconnect += MyAfterDisconnectHandler;
```

...where `MyAfterDisconnectHandler` is an already-defined function in my application with the following signature:

```csharp
static void MyAfterDisconnectHandler(object? sender, AfterDisconnectEventArgs eventArgs)
```

### Lambdas

An alternative to using a pre-defined function are lambdas:

```csharp
client.AfterUnsubscribe += (sender, args) =>
{
    // code
};
```

### Event Arguments

The definitions for all event arguments [can be found here](https://github.com/hivemq/hivemq-mqtt-client-dotnet/tree/main/Source/HiveMQtt/Client/Events).

## Display Options Prior to Connecting

This example simply prints out the `HiveMQClientOptions` prior to the connect command being sent to the broker.

```csharp
using HiveMQtt.Client.Events;

private static void BeforeConnectHandler(object? sender, BeforeConnectEventArgs eventArgs)
{
    if (sender is not null)
    {
        var client = (HiveMQClient)sender;
        Console.WriteLine($"Connecting to Broker with the Options: {eventArgs.Options}");
    }
}

// Later...

var client = new HiveMQClient();

client.BeforeConnect += BeforeConnectHandler;
var connectResult = await client.ConnectAsync().ConfigureAwait(false);
```

## Taking Action After a Subscribe

Suppose you wanted to take some global action after every subscribe call made by the client.  This example
illustrates the steps required.

```csharp
using HiveMQtt.Client.Events;

private static void AfterSubscribeHandler(object? sender, AfterSubscribeEventArgs eventArgs)
{
    if (sender is not null)
    {
        var client = (HiveMQClient)sender;

        // The result of the subscribe call
        // eventArgs.SubscribeResult
    }
}

// Later...

var client = new HiveMQClient();

client.AfterSubscribe += AfterSubscribeHandler;
var connectResult = await client.ConnectAsync().ConfigureAwait(false);
var subscribeResult = await client.SubscribeAsync("district/9/level", MQTT5.Types.QualityOfService.ExactlyOnceDelivery).ConfigureAwait(false);
```

## Monitoring outgoing Publish Packets

The following can be used to monitor when publish packets are transmitted from the client.  A potential debug vector in application development.

```csharp
using HiveMQtt.Client.Events;

private static void OnPublishSentHandler(object? sender, OnPublishSentEventArgs eventArgs)
{
    if (sender is not null)
    {
        var client = (HiveMQClient)sender;

        // The transmitted MQTT Publish packet
        // eventArgs.PublishPacket

        // and the MQTT5PublishMessage
        // eventArgs.PublishPacket.Message

    }
}

// Later...

var client = new HiveMQClient();
var connectResult = await client.ConnectAsync().ConfigureAwait(false);

client.OnPublishSent += OnPublishSentHandler;

var result = await client.PublishAsync("district/7/count", "82", MQTT5.Types.QualityOfService.AtLeastOnceDelivery).ConfigureAwait(false);
```

## Monitoring Subscribe Response Packets (SUBACK)

The following can be used to monitor SubAck responses from the broker

```csharp
using HiveMQtt.Client.Events;

private static void OnSubAckReceivedHandler(object? sender, OnSubAckReceivedEventArgs eventArgs)
{
    if (sender is not null)
    {
        var client = (HiveMQClient)sender;

        // The received SubAck packet
        // eventArgs.SubAckPacket
    }
}

// Later...

var client = new HiveMQClient();
client.OnSubAckReceived += OnSubAckReceivedHandler;
var connectResult = await client.ConnectAsync().ConfigureAwait(false);
var subResult = await client.SubscribeAsync("district/9/level", MQTT5.Types.QualityOfService.ExactlyOnceDelivery).ConfigureAwait(false);
```
