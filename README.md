<p align="center">
  <img src="https://www.hivemq.com/img/svg/hivemq-mqtt-client.svg" width="500">
</p>

<div align="center">

# The Spectacular (BETA) MQTT Client for .NET

[![Semantic Versions](https://img.shields.io/badge/%20%20%F0%9F%93%A6%F0%9F%9A%80-semantic--versions-e10079.svg)](https://github.com/hivemq/hivemq-mqtt-client-dotnet/releases)
[![License](https://img.shields.io/github/license/hivemq/hivemq-mqtt-client-dotnet)](https://github.com/hivemq/hivemq-mqtt-client-dotnet/blob/main/LICENSE)

This .NET MQTT client was put together with love from the HiveMQ team but is still in BETA.  As such some things may not work completely until it matures and although unlikely, APIs may change slightly before version 1.0.

We'd appreciate any feedback you have.  Happy MQTT adventures!

</div>

* **Easy-to-Install**: Available as a Nuget package.
* **Opensource**: No blackbox code.  Only trusted, tested and reviewed opensource code.
* **Easy to Use**: Smart defaults, excellent interfaces and intelligent automation makes implementing a breeze.
* **MQTT v5.0 compatible**: Backported versions 3.1.1 & 3.0 coming soon!
* **Globally Compatible**: Built to be a fully compliant client compatible with all reputable MQTT brokers.
* **Actively Maintained**: Built by the MQTT professionals that built HiveMQ (and do this for a living).
* **Extensively Documented**: What good is it without excellent documentation?
* **Supported**: Contact us anytime in [this repository](https://github.com/hivemq/hivemq-mqtt-client-dotnet/issues), in the [community forum](https://community.hivemq.com) or [through support](https://www.hivemq.com/support/).

_Do you have a success story with this client?  [Let us know]().  We'd love to feature your story in a blog post or video and you'll get some sweet HiveMQ swag (and publicity) along the way._

## Install

... via NuGet

## Quickstart

### Simple Connect

```c#
using HiveMQtt.Client;

// Connect
var client = new HiveMQClient();
var connectResult = await client.ConnectAsync().ConfigureAwait(false);
```

#### With Options

```c#
var options = new HiveMQClientOptions();
options.Host = 'candy.x39.eu.hivemq.cloud';
options.Port = 8883;

var client = new HiveMQClient(options);
var connectResult = await client.ConnectAsync().ConfigureAwait(false);
```

### Subscribing

```c#
using HiveMQtt.Client;

// Connect
var client = new HiveMQClient();
var connectResult = await client.ConnectAsync().ConfigureAwait(false);

// Message Handler
client.OnMessageReceived += (sender, args) =>
{
    Console.WriteLine("Message Received: {}", args.PublishMessage.PayloadAsString)
};

// Subscribe
var subResult = await client.SubscribeAsync("critical/message").ConfigureAwait(false);

// ???

// Profit
```

### Simple Publishing

```c#
await client.PublishAsync(
                "core/graph",    // Topic to publish to
                "{'2023': '👍'}" // Message to publish
                ).ConfigureAwait(false);

```

### General Events

| Event         | EventArgs Class          | Event Arguments      |
| ------------- | ------------------------ | -------------------- |
| BeforeConnect | `BeforeConnectEventArgs` | `HiveMQClientOptions`  |
| AfterConnect  | `AfterConnectEventArgs` | `ConnectResult` |
| BeforeSubscribe | `BeforeSubscribeEventArgs` | `SubscribeOptions`  |
| AfterSubscribe  | `AfterSubscribeEventArgs` |  `SubscribeResult` |
| BeforeUnsubscribe | `BeforeUnsubscribeEventArgs` | `UnsubscribeOptions`  |
| AfterUnsubscribe  | `AfterUnsubscribeEventArgs` |  `UnsubscribeResult` |
| OnMessageReceived | `OnMessageReceivedEventArgs` |  `MQTT5PublishMessage` |

#### Packet Level Events

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

## 🛡 License

[![License](https://img.shields.io/github/license/hivemq/hivemq-mqtt-client-dotnet)](https://github.com/hivemq/hivemq-mqtt-client-dotnet/blob/main/LICENSE)

This project is licensed under the terms of the `Apache Software License 2.0` license. See [LICENSE](https://github.com/hivemq/hivemq-mqtt-client-dotnet/blob/main/LICENSE) for more details.

## 📃 Citation

```bibtex
@misc{hivemq-mqtt-client-dotnet,
  author = {HiveMQ GmbH},
  title = {HiveMQ MQTT Client is an MQTT 5.0 and MQTT 3.1.1 compatible and feature-rich high-performance .NET client library with different API flavours and backpressure support.},
  year = {2022},
  publisher = {GitHub},
  journal = {GitHub repository},
  howpublished = {\url{https://github.com/hivemq/hivemq-mqtt-client-dotnet}}
}
```
