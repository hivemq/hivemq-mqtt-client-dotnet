![](./Images/hivemq-mqtt-client.png)
![](./Images/csharp-logo.png)

# The Spectacular (BETA) C# MQTT Client for .NET

[![GitHub release (latest by date)](https://img.shields.io/github/v/release/hivemq/hivemq-mqtt-client-dotnet?style=for-the-badge)](https://github.com/hivemq/hivemq-mqtt-client-dotnet/releases)
[![GitHub Workflow Status](https://img.shields.io/github/actions/workflow/status/hivemq/hivemq-mqtt-client-dotnet/build.yml?style=for-the-badge)](https://github.com/hivemq/hivemq-mqtt-client-dotnet/actions)
[![GitHub](https://img.shields.io/github/license/hivemq/hivemq-mqtt-client-dotnet?style=for-the-badge)](https://github.com/hivemq/hivemq-mqtt-client-dotnet/blob/main/LICENSE)

_This .NET MQTT client was put together with love from the HiveMQ team but is still in DEVELOPMENT.  As such some things may not work completely until it matures and although unlikely, APIs may change slightly before version 1.0._

We'd appreciate any feedback you have.  Happy MQTT adventures!

* **Easy-to-Install**: Available as a Nuget package.
* **Opensource**: No blackbox code.  Only trusted, tested and reviewed opensource code.
* **Easy to Use**: Smart defaults, excellent interfaces and intelligent automation makes implementing a breeze.
* **MQTT v5.0 compatible**: Backported versions 3.1.1 & 3.0 coming soon!
* **Globally Compatible**: Built to be a fully compliant client compatible with all reputable MQTT brokers.
* **Actively Maintained**: Built by the MQTT professionals that built HiveMQ (and do this for a living).
* **Extensively Documented**: What good is it without excellent documentation?
* **Supported**: Contact us anytime in [this repository](https://github.com/hivemq/hivemq-mqtt-client-dotnet/issues), in the [community forum](https://community.hivemq.com) or [through support](https://www.hivemq.com/support/).

_Do you have a success story with this client?  [Let us know]().  We'd love to feature your story in a blog post or video and you'll get some sweet HiveMQ swag (and publicity) along the way._

## What is this?

MQTT is an [open standard protocol](https://mqtt.org) for publishing and consuming messages from IoT devices all the way up to mainframes.  It's binary, massively performant and easy to use.

This client library is used to publish and consume messages over MQTT.  So you can get a the temperature from a remote sensor, send a control message to a factory robot, tunnel MQTT messages to a Twitter account or anything else you can imagine.

This is the client library that speaks with an MQTT broker that delivers messages to their final destination.  

Need a broker? Sign up for a free broker at [HiveMQ Cloud](https://www.hivemq.com/mqtt-cloud-broker/) and be up and running in a couple minutes.  Connect up to 100 devices - no credit card required.

## Install

This package is [available on NuGet.org](https://www.nuget.org/packages/HiveMQtt/) and can be installed with:

```sh
dotnet add package HiveMQtt
```

See the [HiveMQtt NuGet page](https://www.nuget.org/packages/HiveMQtt/) for more installation options.

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

### Basic Subscribe & Publish

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
await client.SubscribeAsync("instrument/x9284/boston").ConfigureAwait(false);

await client.PublishAsync(
                "core/dynamic_graph/entity/227489", // Topic to publish to
                "{'2023': '👍'}"                    // Message to publish
                ).ConfigureAwait(false);

```

#### Subscribe to Multiple Topics At Once With Varying QoS Levels

```c#
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


For more examples that you can easily copy/paste, see our [Examples](./Examples/).

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
