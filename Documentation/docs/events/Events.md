# Events

This HiveMQ client has a large number of built in events to allow users to hook into any part of the client.

These events can be used to modify behavior, monitor activity or extend functionality.

## Examples

The following serves as a few examples on how to utilize the built in event system.

### Display Options Prior to Connecting

This one simply prints out the HiveMQClientOptions prior to the connect command being sent to the broker.

```csharp
using HiveMQtt.Client.Events;

private static void BeforeConnectHandler(object? sender, BeforeConnectEventArgs eventArgs)
{
    if (sender is not null)
    {
        var client = (HiveMQClient)sender;
        Console.WriteLine("Connecting to Broker with the Options: {}", eventArgs.Options)

    }
}

// Later...

var client = new HiveMQClient();

client.BeforeConnect += BeforeConnectHandler;
var connectResult = await client.ConnectAsync().ConfigureAwait(false);
```

### Taking Action After a Subscribe

Suppose you wanted to take some global action after every subscribe call made by the client.

```csharp
using HiveMQtt.Client.Events;

private static void AfterSubscribeHandler(object? sender, AfterSubscribeEventArgs eventArgs)
{
    if (sender is not null)
    {
        var client = (HiveMQClient)sender;

        // The result of the subscribe call
        // eventArgs.SubcribeResult

    }
}

// Later...

var client = new HiveMQClient();

client.BeforeConnect += BeforeConnectHandler;
var connectResult = await client.ConnectAsync().ConfigureAwait(false);
var subscribeResult = await client.SubscribeAsync("district/9/level", MQTT5.Types.QualityOfService.ExactlyOnceDelivery).ConfigureAwait(false);
```

### Monitoring outgoing Publish Packets

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

### Monitoring Subscribe Response Packets (SUBACK)

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
var connectResult = await client.ConnectAsync().ConfigureAwait(false);
var subResult = await client.SubscribeAsync("district/9/level", MQTT5.Types.QualityOfService.ExactlyOnceDelivery).ConfigureAwait(false);
```

## List of Supported Events

### General

| Event         | EventArgs Class          | Event Arguments      |
| ------------- | ------------------------ | -------------------- |
| BeforeConnect | `BeforeConnectEventArgs` | `HiveMQClientOptions`  |
| AfterConnect  | `AfterConnectEventArgs` | `ConnectResult` |
| BeforeSubscribe | `BeforeSubscribeEventArgs` | `SubscribeOptions`  |
| AfterSubscribe  | `AfterSubscribeEventArgs` |  `SubscribeResult` |
| BeforeUnsubscribe | `BeforeUnsubscribeEventArgs` | `UnsubscribeOptions`  |
| AfterUnsubscribe  | `AfterUnsubscribeEventArgs` |  `UnsubscribeResult` |
| OnMessageReceived | `OnMessageReceivedEventArgs` |  `MQTT5PublishMessage` |

### Packet Level

These events happen based on MQTT packet activity.

| Event         | EventArgs Class          | Event Arguments      |
| ------------- | ------------------------ | -------------------- |
| OnConnectSent        | `OnConnectSentEventArgs`     |  `ConnectPacket` |
| OnConnAckReceived    | `OnConnAckReceivedEventArgs` |  `ConnAckPacket` |
| OnConnectSent        | `OnConnectSentEventArgs`     |  `ConnectPacket` |
| OnDisconnectReceived | `OnDisconnectReceivedEventArgs` |  `DisconnectPacket` |
| OnDisconnectSent     | `OnDisconnectSentEventArgs`  |  `DisconnectPacket` |
| OnPingReqSent        | `OnPingReqSentEventArgs`     |  `PingReqPacket` |
| OnPingRespReceived   | `OnPingRespReceivedEventArgs` |  `PingRespPacket` |
| OnPublishSent        | `OnPublishSentEventArgs`     |  `PublishPacket` |
| OnPublishReceived    | `OnPublishReceivedEventArgs` |  `PublishPacket` |
| OnPubAckSent         | `OnPubAckSentEventArgs`      |  `PubAckPacket` |
| OnPubAckReceived     | `OnPubAckReceivedEventArgs` |  `PubAckPacket` |
| OnPubRecSent         | `OnPubRecSentEventArgs`     |  `PubRecPacket` |
| OnPubRecReceived     | `OnPubRecReceivedEventArgs` |  `PubRecPacket` |
| OnPubRelSent         | `OnPubRelSentEventArgs`     |  `PubRelPacket` |
| OnPubRelReceived     | `OnPubRelReceivedEventArgs` |  `PubRelPacket` |
| OnPubCompSent        | `OnPubCompSentEventArgs`    |  `PubCompPacket` |
| OnPubCompReceived    | `OnPubCompReceivedEventArgs` |  `PubCompPacket` |
| OnSubscribeSent      | `OnSubscribeSentEventArgs`    |  `SubscribePacket` |
| OnSubAckSent         | `OnSubAckSentEventArgs`    |  `SubAckPacket` |
| OnUnsubscribeSent    | `OnUnsubscribeSentEventArgs`    |  `UnsubscribePacket` |
| OnUnsubAckSent       | `OnUnsubAckSentEventArgs`    |  `UnsubAckPacket` |

# See Also

* [Examples](https://github.com/hivemq/hivemq-mqtt-client-dotnet/blob/main/Documentation/Examples.md)
