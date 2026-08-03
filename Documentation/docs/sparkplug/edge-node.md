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
- Optionally follow a Primary Host Application STATE (wait for online, terminate on offline).

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

## Primary Host Application

When `PrimaryHostApplicationId` is set, the Edge Node uses single-broker Primary Host Application behavior:

- Subscribes to the exact topic `spBv1.0/STATE/{PrimaryHostApplicationId}` (not `STATE/#`).
- Waits for an online STATE (including a retained one) before publishing NBIRTH.
- Raises `StateMessageReceived` for STATE updates.
- Exposes `IsPrimaryHostOnline`.
- On a valid offline STATE (`online=false` and timestamp ≥ last online timestamp), publishes NDEATH and disconnects. Call `StartAsync` again after the Host returns online.

When `PrimaryHostApplicationId` is unset, behavior is unchanged (no STATE subscription).

```csharp
var sparkplugOptions = new SparkplugEdgeNodeOptions
{
    GroupId = "myGroup",
    EdgeNodeId = "myNode",
    PrimaryHostApplicationId = "myPrimaryHost",
};

var edgeNode = new SparkplugEdgeNode(clientOptions, sparkplugOptions);
edgeNode.StateMessageReceived += (_, e) =>
{
    Console.WriteLine($"Primary Host online={e.StatePayload!.Online}");
};
await edgeNode.StartAsync();
```

Multi-MQTT-server cycling when the Primary Host goes offline is not implemented yet.

## Rebirth Handling

When a Host sends an NCMD with `Node Control/Rebirth=true`, republish a fresh NBIRTH and related DBIRTH messages.

```csharp
edgeNode.NodeCommandReceived += async (_, e) =>
{
    var isRebirth = e.Payload.Metrics.Any(m => m.Name == SparkplugPayloadEncoder.NodeControlRebirthMetricName && m.BooleanValue);
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
