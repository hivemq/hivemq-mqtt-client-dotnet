---
sidebar_position: 3
---

# Host Application

Use `SparkplugHostApplication` to consume Sparkplug lifecycle/data messages and publish node/device commands.

## Core Responsibilities

- Subscribe to Sparkplug topics, usually `spBv1.0/#`.
- Track edge and device state using birth/death events.
- Publish NCMD/DCMD commands, including rebirth requests.
- Optionally publish/consume STATE messages.

## Typical Setup

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
```

## Handling Birth Events

```csharp
host.NodeBirthReceived += (_, e) =>
{
    Console.WriteLine($"Node birth: {e.Topic.GroupId}/{e.Topic.EdgeNodeId}");
};

host.DeviceBirthReceived += (_, e) =>
{
    Console.WriteLine($"Device birth: {e.Topic.DeviceId}");
};
```

## Sending Rebirth Commands

```csharp
await host.StartAsync();
await host.PublishRebirthCommandAsync("myGroup", "myNode");
```

## Scoped Topic Filters and STATE

STATE messages use `spBv1.0/STATE/{primary_host_id}` and do not include a group segment.  
If you use a scoped filter like `spBv1.0/myGroup/#`, you will not receive STATE from other hosts.

Use one of these patterns:

- Keep default broad filter: `spBv1.0/#`
- Add a separate STATE subscription: `spBv1.0/STATE/#`

## See Also

- [Sparkplug Quickstart](/docs/sparkplug/quickstart)
- [Edge Node Guide](/docs/sparkplug/edge-node)
- [MQTT Events](/docs/hivemqtt/events)
