---
sidebar_position: 4
---

# Edge Node

Use `SparkplugEdgeNode` to publish Sparkplug node/device lifecycle data and process host commands.

## Core Responsibilities

- Publish NBIRTH/NDATA/NDEATH and DBIRTH/DDATA/DDEATH.
- Subscribe to NCMD/DCMD commands from Host Applications.
- Maintain proper birth/death lifecycle sequencing (`bdSeq`).
- Optionally configure NDEATH LWT for ungraceful disconnect awareness.

## Typical Setup

```csharp
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
await edgeNode.StartAsync();
```

## Publishing Node Data

```csharp
var metrics = new[]
{
    SparkplugMetricBuilder.Create("temperature").WithFloatValue(22.5).Build()
};

await edgeNode.PublishNodeDataAsync(metrics);
```

## Rebirth Handling

When a Host sends an NCMD with `Node Control/Rebirth=true`, republish a fresh NBIRTH and related DBIRTH messages.

```csharp
edgeNode.NodeCommandReceived += async (_, e) =>
{
    var isRebirth = e.Payload.Metrics.Any(m => m.Name == "Node Control/Rebirth" && m.BooleanValue);
    if (isRebirth)
    {
        await edgeNode.PublishNodeBirthAsync(null);
        // Re-publish each device birth with PublishDeviceBirthAsync(...)
    }
};
```

## See Also

- [Sparkplug Quickstart](/docs/sparkplug/quickstart)
- [Host Application Guide](/docs/sparkplug/host-application)
- [Payloads and Metrics](/docs/sparkplug/payloads-and-metrics)
