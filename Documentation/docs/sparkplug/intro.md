---
sidebar_position: 1
---

# HiveMQtt.Sparkplug (Beta)

The Sparkplug client extension for .NET built on top of [HiveMQtt](/docs/hivemqtt/intro). Use it to build both Sparkplug **Host Applications** and **Edge Nodes** with the [Eclipse Sparkplug B 3.0](https://sparkplug.eclipse.org/specification/version/3.0) topic and payload model.

:::caution Beta Feature
`HiveMQtt.Sparkplug` is currently in **beta**. While fully functional, APIs may evolve in upcoming releases as the package matures.
:::

:::tip Quick Start
Install the package and jump into examples:
```bash
dotnet add package HiveMQtt.Sparkplug
```
Then continue with the [Sparkplug Quickstart](/docs/sparkplug/quickstart).
:::

## Installation & Compatibility

| Feature | Details |
|---------|---------|
| **NuGet Package** | [HiveMQtt.Sparkplug](https://www.nuget.org/packages/HiveMQtt.Sparkplug) |
| **Depends On** | [HiveMQtt](https://www.nuget.org/packages/HiveMQtt) |
| **Sparkplug Version** | [Sparkplug B 3.0](https://sparkplug.eclipse.org/specification/version/3.0) |
| **.NET Support** | .NET 6.0, 7.0, 8.0, 9.0 and 10.0 |
| **Status** | Beta |

## What You Can Build

- **Host Applications**: Subscribe to Sparkplug topics, track Edge Node and Device lifecycle events, and publish NCMD/DCMD commands.
- **Edge Nodes**: Publish NBIRTH/NDATA/NDEATH and DBIRTH/DDATA/DDEATH, process NCMD/DCMD, and maintain Sparkplug lifecycle state.
- **Payload Workflows**: Encode/decode Sparkplug protobuf payloads and create metrics with `SparkplugMetricBuilder`.

## Client Separation

This repository has two client tracks:

- **MQTT Client**: Core MQTT 5.0 functionality via `HiveMQtt`. Start at [MQTT Intro](/docs/hivemqtt/intro) and [Quickstart](/docs/hivemqtt/quickstart).
- **Sparkplug Client**: Sparkplug B 3.0 workflows via `HiveMQtt.Sparkplug`. Continue with [Sparkplug Quickstart](/docs/sparkplug/quickstart).

## Next Steps

- [Sparkplug Quickstart](/docs/sparkplug/quickstart)
- [Host Application Guide](/docs/sparkplug/host-application)
- [Edge Node Guide](/docs/sparkplug/edge-node)
- [Payloads and Metrics](/docs/sparkplug/payloads-and-metrics)
- [Security Best Practices](/docs/sparkplug/security-best-practices)
- [TCK Compatibility](/docs/sparkplug/tck-compatibility)
