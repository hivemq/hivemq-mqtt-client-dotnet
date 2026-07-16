![HiveMQtt logo](https://i.imgur.com/YmafvGk.png)
![HiveMQ C# MQTT Client banner](https://i.imgur.com/Tnl78V6.png)

# The Sparkplug Client Extension for .NET

[![NuGet Version](https://img.shields.io/nuget/v/HiveMQtt.Sparkplug?style=for-the-badge)](https://www.nuget.org/packages/HiveMQtt.Sparkplug)
[![GitHub release (latest by date)](https://img.shields.io/github/v/release/hivemq/hivemq-mqtt-client-dotnet?style=for-the-badge)](https://github.com/hivemq/hivemq-mqtt-client-dotnet/releases)
[![GitHub Workflow Status](https://img.shields.io/github/actions/workflow/status/hivemq/hivemq-mqtt-client-dotnet/build.yml?style=for-the-badge)](https://github.com/hivemq/hivemq-mqtt-client-dotnet/actions)
[![NuGet](https://img.shields.io/nuget/dt/HiveMQtt.Sparkplug?style=for-the-badge)](https://www.nuget.org/packages/HiveMQtt.Sparkplug)
[![GitHub](https://img.shields.io/github/license/hivemq/hivemq-mqtt-client-dotnet?style=for-the-badge)](https://github.com/hivemq/hivemq-mqtt-client-dotnet/blob/main/LICENSE)

![Static Badge](https://img.shields.io/badge/.NET-6.0-%23512BD4?style=for-the-badge)
![Static Badge](https://img.shields.io/badge/.NET-7.0-%23512BD4?style=for-the-badge)
![Static Badge](https://img.shields.io/badge/.NET-8.0-%23512BD4?style=for-the-badge)
![Static Badge](https://img.shields.io/badge/.NET-9.0-%23512BD4?style=for-the-badge)
![Static Badge](https://img.shields.io/badge/.NET-10.0-%23512BD4?style=for-the-badge)

Sparkplug B 3.0 extension for the [HiveMQ MQTT Client for .NET](https://github.com/hivemq/hivemq-mqtt-client-dotnet). Build both **Host Applications** and **Edge Nodes** for industrial IoT (IIoT) with the official [Eclipse Sparkplug B 3.0](https://sparkplug.eclipse.org/specification/version/3.0) topic and payload model (`spBv1.0`).

> Using this package together with the core client? See the [main README](../../README.md) for MQTT fundamentals and broader client features.

### 💽 Installation & Compatibility
- **Easy-to-Install**: Available as a [NuGet package](https://www.nuget.org/packages/HiveMQtt.Sparkplug).
- **Built for IIoT**: Adds Sparkplug Host Application and Edge Node workflows on top of `HiveMQtt`.
- **Multi-Targeted**: Supports .NET 6.0, 7.0, 8.0, 9.0 and 10.0.
- **Standards-Based**: Aligned with [Eclipse Sparkplug B 3.0](https://sparkplug.eclipse.org/specification/version/3.0) and `sparkplug_b.proto`.
- **Status**: This extension is in **beta**. APIs may change before stable release.

### 🚀 Features
- **Host Application**: Subscribe to Sparkplug topics (`spBv1.0/#` or scoped), track Edge Node and Device online/offline state, publish NCMD/DCMD, and optionally use STATE messages and LWT.
- **Edge Node**: Publish NBIRTH/NDATA/NDEATH and DBIRTH/DDATA/DDEATH, subscribe to NCMD/DCMD, manage birth/death lifecycle with `bdSeq`, and optionally follow a Primary Host Application STATE (wait for online before NBIRTH; NDEATH + disconnect on offline).
- **Payload Tooling**: Encode and decode Sparkplug B protobuf payloads, build metrics via `SparkplugMetricBuilder`, and build/parse topics safely.
- **Spec-Aware Lifecycle**: Optional NDEATH Last Will and Testament (LWT) for spec-compliant session awareness on ungraceful disconnect.
- **Validation Controls**: Optional strict identifier validation to reject invalid topic IDs (`#`, `+`) for Group, Edge Node, Device, and Host IDs.

**📍 Get Started Today**

Install the package, connect to your broker, and start publishing Sparkplug-compliant data and commands for modern industrial telemetry at scale.

## Install

This package is [available on NuGet.org](https://www.nuget.org/packages/HiveMQtt.Sparkplug/) and can be installed with:

```sh
dotnet add package HiveMQtt.Sparkplug
```

See the [HiveMQtt.Sparkplug NuGet page](https://www.nuget.org/packages/HiveMQtt.Sparkplug/) for more installation options.

## Quick start

### Host Application example

```csharp
using HiveMQtt.Client.Options;
using HiveMQtt.Sparkplug.HostApplication;

var clientOptions = new HiveMQClientOptionsBuilder()
    .WithBroker("localhost")
    .WithPort(1883)
    .WithClientId("my-host")
    .Build();
var sparkplugOptions = new SparkplugHostApplicationOptions
{
    SparkplugTopicFilter = "spBv1.0/#",
    UseStateMessages = false
};
var host = new SparkplugHostApplication(clientOptions, sparkplugOptions);

host.NodeBirthReceived += (_, e) => Console.WriteLine($"Node birth: {e.Topic.GroupId}/{e.Topic.EdgeNodeId}");
host.DeviceBirthReceived += (_, e) => Console.WriteLine($"Device birth: {e.Topic.DeviceId}");
await host.StartAsync();

// Later: send a Rebirth command to an edge node
await host.PublishRebirthCommandAsync("myGroup", "myNode");
```

### Edge Node example

```csharp
using HiveMQtt.Client.Options;
using HiveMQtt.Sparkplug.EdgeNode;
using HiveMQtt.Sparkplug.Payload;

var clientOptions = new HiveMQClientOptionsBuilder()
    .WithBroker("localhost")
    .WithPort(1883)
    .WithClientId("my-edge-node")
    .Build();
var sparkplugOptions = new SparkplugEdgeNodeOptions
{
    GroupId = "myGroup",
    EdgeNodeId = "myNode"
};
var edgeNode = new SparkplugEdgeNode(clientOptions, sparkplugOptions);

edgeNode.NodeCommandReceived += (_, e) => Console.WriteLine($"NCMD: {e.Payload.Metrics.Count} metrics");
await edgeNode.StartAsync();

// Publish node data
var metrics = new[] { SparkplugMetricBuilder.Create("temperature").WithFloatValue(22.5).Build() };
await edgeNode.PublishNodeDataAsync(metrics);
```

See the [Examples](../../Examples) folder for runnable Host and Edge Node samples.

## Operational guidance

### Rebirth handling

When a Host sends a **Rebirth** command (NCMD with a `Node Control/Rebirth` boolean metric set to true), the Edge Node should publish a fresh Node Birth so the Host receives an up-to-date NBIRTH. In your `NodeCommandReceived` handler, check for the Rebirth metric and call **`PublishNodeBirthAsync`**; if the node has devices, re-publish their DBIRTHs as well.

```csharp
edgeNode.NodeCommandReceived += async (_, e) =>
{
    var isRebirth = e.Payload.Metrics.Any(m => m.Name == SparkplugPayloadEncoder.NodeControlRebirthMetricName && m.BooleanValue);
    if (isRebirth)
    {
        await edgeNode.PublishNodeBirthAsync(null); // add node metrics if needed
        // If you have devices, re-publish each with PublishDeviceBirthAsync(...)
    }
};
```

### Scoped topic filter and STATE messages

STATE topics use the form `spBv1.0/STATE/{primary_host_id}` (no group segment). A scoped filter such as `spBv1.0/myGroup/#` therefore does **not** match STATE messages, so the Host will not receive STATE from other Host Applications when using a scoped filter. To receive STATE from other Hosts, use **`spBv1.0/#`** (the default) or add a separate subscription to **`spBv1.0/STATE/#`** on the same client and handle those messages in your application.

### DataSet, Template, and File metrics

`SparkplugMetricBuilder` supports scalar and simple types (integers, float, string, bytes, UUID, etc.). Sparkplug B also defines **DataSet**, **Template**, and **File** metric types. For those, build the metric via the protobuf API:

1. Use the builder for name/alias/timestamp if needed, call `Build()`, then set `Datatype` and the complex value on the returned `Payload.Types.Metric`.
2. Or construct a new `Payload.Types.Metric` and set `Name`, `Datatype = (uint)DataType.DataSet` (or `DataType.Template` / `DataType.File`), and the corresponding `DatasetValue`, `TemplateValue`, or other field from the generated `HiveMQtt.Sparkplug.Protobuf` types.

Example (DataSet): set `metric.Datatype = (uint)DataType.DataSet` and `metric.DatasetValue = new Payload.Types.DataSet { NumOfColumns = 2, Columns = { "col1", "col2" }, Types = { (uint)DataType.String, (uint)DataType.Int32 }, Rows = { ... } }`. See `sparkplug_b.proto` and the generated `Payload.Types` for the full structure.

## TCK compatibility

The [Eclipse Sparkplug TCK](https://sparkplug.eclipse.org/specification/tck-process) validates implementations for Sparkplug compatibility. The TCK is typically run against **brokers** (Sparkplug Compliant / Sparkplug Aware). HiveMQtt.Sparkplug is a **client** library (Host Application and Edge Node).

- **Spec alignment:** This extension is designed to the [Sparkplug B 3.0 specification](https://sparkplug.eclipse.org/specification/version/3.0) and `sparkplug_b.proto` payload format. Topics follow `spBv1.0/{group_id}/{message_type}/{edge_node_id}[/{device_id}]`.
- **Interoperability:** Use this client with any MQTT broker that supports Sparkplug (e.g. [HiveMQ](https://www.hivemq.com/)). To validate broker compliance, run the official Eclipse Sparkplug TCK with your broker; the Host and Edge Node roles implemented here can act as the Sparkplug clients in that setup.

## Package

- **NuGet:** `HiveMQtt.Sparkplug`
- **Dependencies:** `HiveMQtt`, `Google.Protobuf`
- **Targets:** .NET 6.0 through .NET 10.0

## References

- [Eclipse Sparkplug B 3.0](https://sparkplug.eclipse.org/specification/version/3.0)
- [sparkplug_b.proto (Eclipse Tahu)](https://github.com/eclipse-tahu/tahu/blob/master/sparkplug_b/sparkplug_b.proto)
- [Sparkplug TCK process](https://sparkplug.eclipse.org/specification/tck-process)
