---
sidebar_position: 1
---

# The MQTT Client

* **Easy-to-Install**: Available as a Nuget package.
* **Opensource**: No blackbox code.  Only trusted, tested and reviewed opensource code.
* **Easy to Use**: Smart defaults, excellent interfaces and intelligent automation makes implementing a breeze.
* **MQTT v5.0 compatible**: Backported versions 3.1.1 & 3.0 coming soon!
* **Extensive Event System**: Hook into all parts of the client down to the packet level with [built in events](https://github.com/hivemq/hivemq-mqtt-client-dotnet/blob/main/Documentation/Events.md).
* **Globally Compatible**: Built to be a fully compliant client compatible with all reputable MQTT brokers.
* **Actively Maintained**: Built by the MQTT professionals that built HiveMQ (and do this for a living).
* **Extensively Documented**: What good is it without excellent documentation?
* **Supported**: Contact us anytime in [this repository](https://github.com/hivemq/hivemq-mqtt-client-dotnet/issues), in the [community forum](https://community.hivemq.com) or [through support](https://www.hivemq.com/support/).

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
  year = {2024},
  publisher = {GitHub},
  journal = {GitHub repository},
  howpublished = {\url{https://github.com/hivemq/hivemq-mqtt-client-dotnet}}
}
```
