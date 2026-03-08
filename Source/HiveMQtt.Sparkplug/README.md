# HiveMQtt.Sparkplug

Sparkplug B 3.0 extension for the [HiveMQ MQTT Client for .NET](https://github.com/hivemq/hivemq-mqtt-client-dotnet). Adds **Host Application** and **Edge Node** support for industrial IoT (IIoT) using the [Eclipse Sparkplug B 3.0](https://sparkplug.eclipse.org/specification/version/3.0) specification and `spBv1.0` topic namespace.

**Status:** This extension is in **alpha**. The first release was in March 2026. APIs may change before a stable release.

## Features

- **Host Application** — Subscribe to Sparkplug topics (`spBv1.0/#` or scoped), track Edge Node and Device online/offline state, publish NCMD/DCMD (node and device commands), optional STATE messages and LWT.
- **Edge Node** — Publish NBIRTH/NDATA/NDEATH, DBIRTH/DDATA/DDEATH; subscribe to NCMD/DCMD; sequence and birth/death lifecycle. Optional NDEATH Last Will and Testament (LWT) for spec-compliant session awareness when the node disconnects ungracefully. Birth/death sequence (bdSeq) is maintained and included in NBIRTH, NDEATH, DBIRTH, and DDEATH so Host Applications can correlate deaths to births.
- **Payloads** — Encode/decode Sparkplug B protobuf payloads; `SparkplugMetricBuilder` for metrics; topic build/parse.
- **Validation** — Optional strict identifier validation (reject `#` and `+` in Group ID, Edge Node ID, Device ID, Host Application ID).

## Quick start

### Host Application

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

### Edge Node

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

### Rebirth handling

When a Host sends a **Rebirth** command (NCMD with a `Rebirth` boolean metric set to true), the Edge Node should publish a fresh Node Birth so the Host receives an up-to-date NBIRTH. In your `NodeCommandReceived` handler, check for the Rebirth metric and call **`PublishNodeBirthAsync`**; if the node has devices, re-publish their DBIRTHs as well.

```csharp
edgeNode.NodeCommandReceived += async (_, e) =>
{
    var isRebirth = e.Payload.Metrics.Any(m => m.Name == "Rebirth" && m.BooleanValue);
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
- **Interoperability:** Use this client with any MQTT broker that supports Sparkplug (e.g. [HiveMQ](https://www.hivemq.com/). To validate broker compliance, run the official Eclipse Sparkplug TCK with your broker; the Host and Edge Node roles implemented here can act as the Sparkplug clients in that setup.

## Package

- **NuGet:** (when published) `HiveMQtt.Sparkplug`
- **Dependencies:** `HiveMQtt`, `Google.Protobuf`
- **Targets:** .NET 6.0 through .NET 10.0

## References

- [Eclipse Sparkplug B 3.0](https://sparkplug.eclipse.org/specification/version/3.0)
- [sparkplug_b.proto (Eclipse Tahu)](https://github.com/eclipse-tahu/tahu/blob/master/sparkplug_b/sparkplug_b.proto)
- [Sparkplug TCK process](https://sparkplug.eclipse.org/specification/tck-process)
