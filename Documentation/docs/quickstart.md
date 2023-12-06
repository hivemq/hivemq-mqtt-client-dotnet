---
sidebar_position: 2
---
# Quickstart

## Install

This package is [available on NuGet.org](https://www.nuget.org/packages/HiveMQtt/) and can be installed with:

```sh
dotnet add package HiveMQtt
```

See the [HiveMQtt NuGet page](https://www.nuget.org/packages/HiveMQtt/) for more installation options.

## Connect & Publish

### Simple Connect

```csharp
using HiveMQtt.Client;

// Connect
var client = new HiveMQClient();
var connectResult = await client.ConnectAsync().ConfigureAwait(false);
```

#### With Options

```csharp
var options = new HiveMQClientOptions();
options.Host = 'candy.x39.eu.hivemq.cloud';
options.Port = 8883;

var client = new HiveMQClient(options);
var connectResult = await client.ConnectAsync().ConfigureAwait(false);
```

#### Subscribe & Publish

```csharp
// Message Handler
client.OnMessageReceived += (sender, args) =>
{
    Console.WriteLine("Message Received: {}", args.PublishMessage.PayloadAsString)
};

// Subscribe
await client.SubscribeAsync("instrument/x9284/boston").ConfigureAwait(false);

await client.PublishAsync(
                "core/dynamic_graph/entity/227489", // Topic to publish to
                "{'2023': '👍'}"                    // Message to publish
                ).ConfigureAwait(false);
```


