![](https://i.imgur.com/YmafvGk.png)
![](https://i.imgur.com/Tnl78V6.png)

# The Spectacular (BETA) C# MQTT Client for .NET

[![GitHub release (latest by date)](https://img.shields.io/github/v/release/hivemq/hivemq-mqtt-client-dotnet?style=for-the-badge)](https://github.com/hivemq/hivemq-mqtt-client-dotnet/releases)
[![GitHub Workflow Status](https://img.shields.io/github/actions/workflow/status/hivemq/hivemq-mqtt-client-dotnet/build.yml?style=for-the-badge)](https://github.com/hivemq/hivemq-mqtt-client-dotnet/actions)
[![Nuget](https://img.shields.io/nuget/dt/HiveMQtt?style=for-the-badge)](https://www.nuget.org/packages/HiveMQtt)
[![GitHub](https://img.shields.io/github/license/hivemq/hivemq-mqtt-client-dotnet?style=for-the-badge)](https://github.com/hivemq/hivemq-mqtt-client-dotnet/blob/main/LICENSE)

_This .NET MQTT client was put together with love from the HiveMQ team but is still in DEVELOPMENT.  As such some things may not work completely until it matures and although unlikely, APIs may change slightly before version 1.0._

We'd appreciate any feedback you have.  Happy MQTT adventures!

* **Easy-to-Install**: Available as a Nuget package.
* **Opensource**: No blackbox code.  Only trusted, tested and reviewed opensource code.
* **Easy to Use**: Smart defaults, excellent interfaces and intelligent automation makes implementing a breeze.
* **MQTT v5.0 compatible**: Backported versions 3.1.1 & 3.0 coming soon!
* **Extensive Event System**: Hook into all parts of the client down to the packet level with [built in events](https://github.com/hivemq/hivemq-mqtt-client-dotnet/blob/main/Documentation/Events.md).
* **Globally Compatible**: Built to be a fully compliant client compatible with all reputable MQTT brokers.
* **Actively Maintained**: Built by the MQTT professionals that built HiveMQ (and do this for a living).
* **Extensively Documented**: What good is it without excellent documentation?
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

### Last Will and Testament

The Last Will and Testament support of MQTT can be used to notify subscribers that your client is offline.

For a more in-depth explanation, see [What is MQTT Last Will and Testament (LWT)? – MQTT Essentials: Part 9](https://www.hivemq.com/blog/mqtt-essentials-part-9-last-will-and-testament/).

```C#
// Specify the Last Will and Testament specifics in HiveMQClientOptions
var options = new HiveMQClientOptions
{
    LastWillAndTestament = new LastWillAndTestament("last/will", QualityOfService.AtLeastOnceDelivery, "last will message"),
};

// Optionally set extended properties on the Last Will and Testament message
options.LastWillAndTestament.WillDelayInterval = 1;
options.LastWillAndTestament.PayloadFormatIndicator = 1;
options.LastWillAndTestament.MessageExpiryInterval = 100;
options.LastWillAndTestament.ContentType = "application/text";
options.LastWillAndTestament.ResponseTopic = "response/topic";
options.LastWillAndTestament.CorrelationData = new byte[] { 1, 2, 3, 4, 5 };
options.LastWillAndTestament.UserProperties.Add("userPropertyKey", "userPropertyValue");

// ConnectAsync will transmit the Last Will and Testament configuration.
var client = new HiveMQClient(options);
connectResult = await client.ConnectAsync().ConfigureAwait(false);

// The Last Will and Testament message will be sent to the "last/will" topic if your clients get
// unexpectedly disconnected or alternatively, if your client disconnects with `DisconnectWithWillMessage`
var disconnectOptions = new DisconnectOptions { ReasonCode = DisconnectReasonCode.DisconnectWithWillMessage };
var disconnectResult = await client.DisconnectAsync(disconnectOptions).ConfigureAwait(false);
``````

Because the client above disconnected with `DisconnectReasonCode.DisconnectWithWillMessage`, subscribers to the `last/will` topic will receive the Last Will and Testament message as specified above.

### More

For more examples that you can easily copy/paste, see our [Examples](https://github.com/hivemq/hivemq-mqtt-client-dotnet/blob/main/Documentation/Examples.md).

There is even an https://github.com/hivemq/hivemq-mqtt-client-dotnet/tree/main/Examples/HiveMQtt-CLI to demonstrate usage of the package.

## Configuration

### Logging

The HiveMQtt package uses [NLog](https://github.com/NLog/NLog) and can be configured with a configuration file (`NLog.config`).  Having this file in the same directory of your executable will configure the HiveMQtt logger to output as configured:

```xml
<?xml version="1.0" encoding="utf-8" ?>
<nlog xmlns="http://www.nlog-project.org/schemas/NLog.xsd"
      xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance">

  <targets>
    <target name="logfile" xsi:type="File" fileName="HiveMQtt.log" />
    <target name="logconsole" xsi:type="Console" />
  </targets>

  <rules>
     <!-- minlevel can be Debug, Info, Warn, Error and Fatal or Trace -->
    <logger name="HiveMQtt.*" minlevel="Error" writeTo="logconsole" />
  </rules>
</nlog>

```

Setting `minlevel` to `Trace` will output all activity in the HiveMQtt package down to packet and event handling.  Using this level will produce a lot of output such as the following:

```log
2023-10-04 16:56:54.9373|TRACE|HiveMQtt.Client.HiveMQClient|BeforeConnectEventLauncher
2023-10-04 16:56:55.0081|TRACE|HiveMQtt.Client.HiveMQClient|7: TrafficInflowProcessor Starting...Connecting
2023-10-04 16:56:55.0081|TRACE|HiveMQtt.Client.HiveMQClient|9: TrafficOutflowProcessor Starting...Connecting
2023-10-04 16:56:55.0081|TRACE|HiveMQtt.Client.HiveMQClient|--> ConnectPacket
2023-10-04 16:56:55.0128|TRACE|HiveMQtt.Client.HiveMQClient|OnConnectSentEventLauncher
2023-10-04 16:56:55.0374|TRACE|HiveMQtt.Client.HiveMQClient|<-- ConnAck
2023-10-04 16:56:55.0374|TRACE|HiveMQtt.Client.HiveMQClient|OnConnAckReceivedEventLauncher
2023-10-04 16:56:55.0379|TRACE|HiveMQtt.Client.HiveMQClient|AfterConnectEventLauncher
```

## Other MQTT Clients

* [Java](https://github.com/hivemq/hivemq-mqtt-client)
* [Javascript](https://github.com/hivemq/hivemq-mqtt-web-client)

## 🛡 License

[![License](https://img.shields.io/github/license/hivemq/hivemq-mqtt-client-dotnet)](https://github.com/hivemq/hivemq-mqtt-client-dotnet/blob/main/LICENSE)

This project is licensed under the terms of the `Apache Software License 2.0` license. See [LICENSE](https://github.com/hivemq/hivemq-mqtt-client-dotnet/blob/main/LICENSE) for more details.

## 📃 Citation

```bibtex
@misc{hivemq-mqtt-client-dotnet,
  author = {HiveMQ GmbH},
  title = {The HiveMQ C# MQTT client for .NET},
  year = {2023},
  publisher = {GitHub},
  journal = {GitHub repository},
  howpublished = {\url{https://github.com/hivemq/hivemq-mqtt-client-dotnet}}
}
```
