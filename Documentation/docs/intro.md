---
sidebar_position: 10
---

# The HiveMQ MQTT Client for .NET

The official HiveMQ MQTT client library for .NET applications. A fully-featured, high-performance MQTT 5.0 client designed for modern .NET development.

:::tip Quick Start
Install via NuGet and connect in minutes:
```bash
dotnet add package HiveMQtt
```
Then check out the [Quickstart Guide](/docs/quickstart) to begin.
:::

## Installation & Compatibility

| Feature | Details |
|---------|---------|
| **NuGet Package** | [HiveMQtt](https://www.nuget.org/packages/HiveMQtt) |
| **MQTT Version** | Full [MQTT 5.0](https://docs.oasis-open.org/mqtt/mqtt/v5.0/mqtt-v5.0.html) compliance |
| **.NET Support** | .NET 6.0, 7.0, 8.0 & 9.0 |
| **License** | Apache 2.0 (Open Source) |

## Key Features

### Core Capabilities
- **MQTT 5.0 Support**: Fully compliant with the latest specification, ensuring compatibility with all modern MQTT brokers
- **Asynchronous Design**: High-performance, low-latency communication with concurrent message processing
- **Back Pressure Management**: Automatically manages flow to prevent overwhelming the broker or client
- **Automatic Reconnect**: Built-in reconnection with exponential backoff strategy

### Message Handling
- **Global Message Handlers**: Process all incoming messages in one place
- **Per-Subscription Callbacks**: Route messages to specific handlers based on topic
- **Full QoS Support**: QoS 0, 1, and 2 for reliable message delivery
- **Last Will & Testament**: Automatic notification of client disconnections

### Security
- **TLS/SSL Encryption**: Secure connections out of the box
- **X.509 Client Certificates**: [Full support](/docs/how-to/client-certificates) for certificate-based authentication
- **Username/Password Auth**: Simple credential-based authentication

### Developer Experience
- **Extensive Event System**: [Hook into all operations](/docs/events) from connection to packet level
- **Detailed Logging**: [TRACE level logging](/docs/how-to/debug) for debugging
- **Builder Pattern APIs**: Fluent interfaces for easy configuration
- **Smart Defaults**: Works out of the box with sensible defaults

### Performance
- **Optimized & Benchmarked**: See [benchmark results](/docs/benchmarks)
- **RawClient (Beta)**: [Low-level client](/docs/rawclient) for maximum performance scenarios

## Client Options

This library provides two client implementations:

| Client | Best For |
|--------|----------|
| [`HiveMQClient`](/docs/hivemqclient) | Most applications - full features including subscription tracking, per-subscription callbacks, and automatic reconnect |
| [`RawClient`](/docs/rawclient) | Performance-critical scenarios - minimal overhead, no subscription tracking |

## Support & Maintenance

- **Actively Maintained**: Built by the MQTT professionals at HiveMQ
- **Community Support**: [GitHub Issues](https://github.com/hivemq/hivemq-mqtt-client-dotnet/issues) and [Community Forum](https://community.hivemq.com)
- **Professional Support**: [HiveMQ Support](https://www.hivemq.com/support/)
- **Open Source**: Apache 2.0 licensed - transparent, trusted code

## What is MQTT?

MQTT is an [open standard protocol](https://mqtt.org) for publishing and consuming messages from IoT devices all the way up to mainframes. It's binary, massively performant, and easy to use.

This client library enables you to publish and consume messages over MQTT. Use cases include:
- Reading sensor data from remote IoT devices
- Sending control commands to industrial equipment
- Building real-time messaging applications
- Implementing event-driven architectures

The client communicates with an MQTT broker that routes messages to their destinations.

## Need an MQTT Broker?

### Quick Start with Docker

Run HiveMQ Community Edition locally:

```bash
docker run --name hivemq-ce -d -p 1883:1883 hivemq/hivemq-ce
```

This starts a broker on `localhost:1883`.

### Cloud Option

[HiveMQ Cloud](https://www.hivemq.com/mqtt-cloud-broker/) offers a free tier with up to 100 device connections - no credit card required. For advanced features, see [HiveMQ Editions](https://www.hivemq.com/hivemq/editions/).

## Learning Resources

### MQTT Fundamentals
- [MQTT Essentials](https://www.hivemq.com/mqtt-essentials/) - Comprehensive introduction to MQTT
- [MQTT Toolbox](https://www.hivemq.com/mqtt-toolbox/) - Useful tools for MQTT development
- [HiveMQ Public Broker](http://www.mqtt-dashboard.com) - Free public broker for testing

### Code Examples
- [GitHub Examples](https://github.com/hivemq/hivemq-mqtt-client-dotnet/tree/main/Examples) - Sample applications
- [CLI Example](https://github.com/hivemq/hivemq-mqtt-client-dotnet/tree/main/Examples/HiveMQtt-CLI) - Command-line interface demo

## Other HiveMQ MQTT Clients

- [Java Client](https://github.com/hivemq/hivemq-mqtt-client)
- [JavaScript Client](https://github.com/hivemq/hivemq-mqtt-web-client)

## License

[![License](https://img.shields.io/github/license/hivemq/hivemq-mqtt-client-dotnet)](https://github.com/hivemq/hivemq-mqtt-client-dotnet/blob/main/LICENSE)

This project is licensed under the **Apache Software License 2.0**. See [LICENSE](https://github.com/hivemq/hivemq-mqtt-client-dotnet/blob/main/LICENSE) for details.

## Citation

```bibtex
@misc{hivemq-mqtt-client-dotnet,
  author = {HiveMQ GmbH},
  title = {The HiveMQ C# MQTT client for .NET},
  year = {2026},
  publisher = {GitHub},
  journal = {GitHub repository},
  howpublished = {\url{https://github.com/hivemq/hivemq-mqtt-client-dotnet}}
}
```
