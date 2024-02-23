![](https://i.imgur.com/YmafvGk.png)
![](https://i.imgur.com/Tnl78V6.png)

# The Spectacular (BETA) C# MQTT Client for .NET

[![GitHub release (latest by date)](https://img.shields.io/github/v/release/hivemq/hivemq-mqtt-client-dotnet?style=for-the-badge)](https://github.com/hivemq/hivemq-mqtt-client-dotnet/releases)
[![GitHub Workflow Status](https://img.shields.io/github/actions/workflow/status/hivemq/hivemq-mqtt-client-dotnet/build.yml?style=for-the-badge)](https://github.com/hivemq/hivemq-mqtt-client-dotnet/actions)
[![Nuget](https://img.shields.io/nuget/dt/HiveMQtt?style=for-the-badge)](https://www.nuget.org/packages/HiveMQtt)
[![GitHub](https://img.shields.io/github/license/hivemq/hivemq-mqtt-client-dotnet?style=for-the-badge)](https://github.com/hivemq/hivemq-mqtt-client-dotnet/blob/main/LICENSE)

_This .NET MQTT client was put together with love from the HiveMQ team but is still in BETA.  While it's mostly stable, it is still under active development as we add new features.  If you have a feature request, [let us know](https://github.com/hivemq/hivemq-mqtt-client-dotnet/issues/new/choose)!._

Happy MQTT adventures!

* **Easy-to-Install**: Available as a [Nuget package](https://www.nuget.org/packages/HiveMQtt).
* **Opensource**: No blackbox code.  Only trusted, tested and reviewed opensource code.
* **Works with Any MQTT 5.0 Broker**: Built by HiveMQ _but not only for_ HiveMQ.
* **Easy to Use**: Smart defaults, excellent interfaces and intelligent automation makes implementing a breeze.
* **Compliant**: Fully compliant with the [MQTT 5.0 specification](https://docs.oasis-open.org/mqtt/mqtt/v5.0/mqtt-v5.0.html).
* **Extensive Event System**: Hook into all parts of the client down to the packet level with [built in events](https://hivemq.github.io/hivemq-mqtt-client-dotnet/docs/events).
* **Global and Per-Subscription Message Handling**: Use multiple targeted handlers for more targeted and specialized message processing.
* **Full Last Will & Testament Support**: Reliable message delivery and notification of client disconnections.
* **Observable**: Configure up to [TRACE level logging](https://hivemq.github.io/hivemq-mqtt-client-dotnet/docs/how-to/debug) for package internals.
* **Secure Client Identification**: Full support for [X.509 client certificates](https://hivemq.github.io/hivemq-mqtt-client-dotnet/docs/how-to/client-certificates) and TLS connections.
* **Globally Compatible**: Built to be a fully compliant MQTT 5.0 client compatible with all reputable MQTT brokers.
* **Actively Maintained**: Built by the MQTT professionals that built HiveMQ (and do this for a living).
* **Extensively Documented**: What good is it without [excellent documentation](https://hivemq.github.io/hivemq-mqtt-client-dotnet/)?
* **Supported**: Contact us anytime in [this repository](https://github.com/hivemq/hivemq-mqtt-client-dotnet/issues), in the [community forum](https://community.hivemq.com) or [through support](https://www.hivemq.com/support/).

_Do you have a success story with this client?  [Let us know]().  We'd love to feature your story in a blog post or video and you'll get some sweet HiveMQ swag (and publicity) along the way._

## What is this?

MQTT is an [open standard protocol](https://mqtt.org) for publishing and consuming messages from IoT devices all the way up to mainframes.  It's binary, massively performant and easy to use.

This client library is used to publish and consume messages over MQTT.  So you can get a the temperature from a remote sensor, send a control message to a factory robot, tunnel WhatsApp messages to a Twitter account or anything else you can imagine.

This is the client library that speaks with an MQTT broker that delivers messages to their final destination.  

Need a broker? Sign up for a free broker at [HiveMQ Cloud](https://www.hivemq.com/mqtt-cloud-broker/) and be up and running in a couple minutes.  Connect up to 100 devices - no credit card required.

## MQTT Resources

* [MQTT Essentials](https://www.hivemq.com/mqtt-essentials/) (Great for beginners wanting an introduction)
* [MQTT Toolbox](https://www.hivemq.com/mqtt-toolbox/)
* [MQTT Client Library Encyclopedia](https://www.hivemq.com/mqtt-client-library-encyclopedia/)
* HiveMQ [Public Broker](http://www.mqtt-dashboard.com)
* HiveMQ [Support](https://www.hivemq.com/support/)

## Need an MQTT Broker?

This client communicates with an MQTT broker to publish and consume messages.  It's built to be compatible with all major MQTT brokers but if you need a broker now run the HiveMQ Community Edition:

```bash
docker run --name hivemq-ce -d -p 1883:1883 hivemq/hivemq-ce
```

This will run the HiveMQ Community Edition broker on localhost port 1883.

If you need advanced features, checkout our [premium editions](https://www.hivemq.com/hivemq/editions/) or alternatively [HiveMQ Cloud](https://www.hivemq.com/mqtt-cloud-broker/) which is free to connect up to 100 devices (no credit card required).

## Install

This package is [available on NuGet.org](https://www.nuget.org/packages/HiveMQtt/) and can be installed with:

```sh
dotnet add package HiveMQtt
```

See the [HiveMQtt NuGet page](https://www.nuget.org/packages/HiveMQtt/) for more installation options.

## Example

The following illustrates the client pattern to connect, subscribe and publish messages.

```csharp
using HiveMQtt.Client;
using HiveMQtt.MQTT5.Types;

// Setup Client options and instantiate
var options = new HiveMQClientOptionsBuilder().
                    WithBroker('candy.x39.eu.hivemq.cloud').
                    WithPort(8883).
                    WithUseTLS(true).
                    Build();
var client = new HiveMQClient(options);

// Setup an application message handlers BEFORE subscribing to a topic
client.OnMessageReceived += (sender, args) =>
{
    Console.WriteLine("Message Received: {}", args.PublishMessage.PayloadAsString)
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
var publishResult = await client.PublishAsync("topic1/example", "Hello Payload")
```

For a Quickstart, more examples and walkthroughs, see [the documentation](https://hivemq.github.io/hivemq-mqtt-client-dotnet/docs/quickstart).

## Other Other MQTT Clients

* [Java](https://github.com/hivemq/hivemq-mqtt-client)
* [Javascript](https://github.com/hivemq/hivemq-mqtt-web-client)

For a list of all known MQTT clients, see [MQTT.org](https://mqtt.org/software/).

## 🛡 License

[![License](https://img.shields.io/github/license/hivemq/hivemq-mqtt-client-dotnet)](https://github.com/hivemq/hivemq-mqtt-client-dotnet/blob/main/LICENSE)

This project is licensed under the terms of the `Apache Software License 2.0` license. See [LICENSE](https://github.com/hivemq/hivemq-mqtt-client-dotnet/blob/main/LICENSE) for more details.

## 📃 Citation

```bibtex
@misc{hivemq-mqtt-client-dotnet,
  author = {HiveMQ GmbH},
  title = {The HiveMQ C# MQTT client for .NET},
  year = {2024},
  publisher = {GitHub},
  journal = {GitHub repository},
  howpublished = {\url{https://github.com/hivemq/hivemq-mqtt-client-dotnet}}
}
```
