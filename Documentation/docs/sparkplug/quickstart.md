---
sidebar_position: 2
---

# Quickstart

Get a Sparkplug Host Application or Edge Node running in minutes using `HiveMQtt.Sparkplug`.

## Install

```bash
dotnet add package HiveMQtt.Sparkplug
```

## Required Namespaces

### Host Application

```csharp
using HiveMQtt.Client.Options;
using HiveMQtt.Sparkplug.HostApplication;
```

### Edge Node

```csharp
using HiveMQtt.Client.Options;
using HiveMQtt.Sparkplug.EdgeNode;
using HiveMQtt.Sparkplug.Payload;
```

## Host Application Example

```csharp
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
await host.PublishRebirthCommandAsync("myGroup", "myNode");
```

## Edge Node Example

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
edgeNode.NodeCommandReceived += (_, e) => Console.WriteLine($"NCMD: {e.Payload.Metrics.Count} metrics");

await edgeNode.StartAsync();

var metrics = new[] { SparkplugMetricBuilder.Create("temperature").WithFloatValue(22.5).Build() };
await edgeNode.PublishNodeDataAsync(metrics);
```

## Next Steps

- [Host Application Guide](/docs/sparkplug/host-application)
- [Edge Node Guide](/docs/sparkplug/edge-node)
- [Payloads and Metrics](/docs/sparkplug/payloads-and-metrics)
- [Security Best Practices](/docs/sparkplug/security-best-practices)
- [MQTT Core Quickstart](/docs/hivemqtt/quickstart)
