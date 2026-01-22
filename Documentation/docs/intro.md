---
sidebar_position: 10
---

# The MQTT Client

### 💽 Installation & Compatibility
* **Easy-to-Install**: Available as a [Nuget package](https://www.nuget.org/packages/HiveMQtt).
* **Globally Compatible**: Built to be a fully compliant MQTT 5.0 client compatible with all modern MQTT brokers.
* **Multi-Targeted**: Supports .NET 6.0, 7.0 & 8.0

### 🚀 Features
* **MQTT 5.0 Support**: Fully compliant with the latest [MQTT 5.0 specification](https://docs.oasis-open.org/mqtt/mqtt/v5.0/mqtt-v5.0.html), ensuring compatibility with modern MQTT brokers.
* **Back Pressure Management**: Automatically manages back pressure to prevent overwhelming the broker (or client), ensuring reliable and efficient communication.
* **Asynchronous Design**: Designed for high-performance and low-latency communication, allowing your application to process multiple messages concurrently.
* **Extensive Event System**: Hook into all parts of the client down to the packet level with [built in events](https://hivemq.github.io/hivemq-mqtt-client-dotnet/docs/events).
* **Global and Per-Subscription Message Handling**: Use multiple targeted handlers for more targeted and specialized message processing.
* **Full Last Will & Testament Support**: Reliable message delivery and notification of client disconnections.
* **Secure Client Identification**: Full support for [X.509 client certificates](https://hivemq.github.io/hivemq-mqtt-client-dotnet/docs/how-to/client-certificates) and TLS connections.
* **Observable**: Configure up to [TRACE level logging](https://hivemq.github.io/hivemq-mqtt-client-dotnet/docs/how-to/debug) for package internals.
* **Fast**: Optimized & benchmarked.  See the benchmark results [here](https://github.com/hivemq/hivemq-mqtt-client-dotnet/blob/main/Benchmarks/ClientBenchmarkApp/README.md).
* **RawClient (Beta)**: A low-level, performance-oriented client for scenarios requiring minimal overhead. See [RawClient documentation](https://hivemq.github.io/hivemq-mqtt-client-dotnet/docs/rawclient).

### 🏝️ Ease of Use
* **Easy to Use**: Smart defaults, excellent interfaces and intelligent automation makes implementing a breeze.
* **Easy Integration**: Simple and intuitive API makes it easy to integrate with your .NET applications.

### 🛟 Maintenance and Support
* **Actively Maintained**: Built by the MQTT professionals that built HiveMQ (and do this for a living).
* **Supported**: Contact us anytime in [this repository](https://github.com/hivemq/hivemq-mqtt-client-dotnet/issues), in the [community forum](https://community.hivemq.com) or [through support](https://www.hivemq.com/support/).
* **Extensively Documented**: What good is it without [excellent documentation](https://hivemq.github.io/hivemq-mqtt-client-dotnet/)?

### 🐧 Opensource
* **Opensource**: No blackbox code.  Only trusted, tested and reviewed opensource code.

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

### More

For more examples that you can easily copy/paste, see our [Examples](https://github.com/hivemq/hivemq-mqtt-client-dotnet/blob/main/Documentation/Examples.md).

There is even an https://github.com/hivemq/hivemq-mqtt-client-dotnet/tree/main/Examples/HiveMQtt-CLI to demonstrate usage of the package.


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
  year = {2025},
  publisher = {GitHub},
  journal = {GitHub repository},
  howpublished = {\url{https://github.com/hivemq/hivemq-mqtt-client-dotnet}}
}
```
