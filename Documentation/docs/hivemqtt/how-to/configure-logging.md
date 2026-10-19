---
sidebar_position: 5
---

# Configure Logging

HiveMQtt logs through the standard [`Microsoft.Extensions.Logging`](https://learn.microsoft.com/dotnet/core/extensions/logging)
abstractions.  The package depends only on `Microsoft.Extensions.Logging.Abstractions`; it does not
reference or configure any particular logging backend.

**By default, HiveMQtt logs nothing.**  Supply an `ILoggerFactory` to route its internal logs into
your application's logging pipeline.

## Supplying a logger factory

```csharp
using HiveMQtt.Client;
using HiveMQtt.Client.Options;
using Microsoft.Extensions.Logging;

using var loggerFactory = LoggerFactory.Create(builder =>
    builder.AddSimpleConsole().SetMinimumLevel(LogLevel.Trace));

var options = new HiveMQClientOptionsBuilder()
    .WithBroker("127.0.0.1")
    .WithPort(1883)
    .WithLoggerFactory(loggerFactory)
    .Build();

var client = new HiveMQClient(options);
```

You can also set it directly on the options object:

```csharp
var options = new HiveMQClientOptions { LoggerFactory = loggerFactory };
```

In an application using the generic host or ASP.NET Core, resolve the factory from DI:

```csharp
var options = new HiveMQClientOptionsBuilder()
    .WithBroker("127.0.0.1")
    .WithLoggerFactory(serviceProvider.GetRequiredService<ILoggerFactory>())
    .Build();
```

Because this is just `ILoggerFactory`, any backend works: Serilog, NLog, ZLogger, log4net, or the
built-in console/debug providers.

## Log categories

Each HiveMQtt type logs under its own full type name, so categories can be filtered independently:

| Category | What it covers |
|---|---|
| `HiveMQtt.Client.HiveMQClient` | Client lifecycle, publish/subscribe, event launchers |
| `HiveMQtt.Client.RawClient` | Same, for `RawClient` |
| `HiveMQtt.Client.Connection.ConnectionManager` | Connect/disconnect, reader/writer tasks, keep-alive |
| `HiveMQtt.Client.Transport.BaseTransport` | TCP / WebSocket / TLS transport |
| `HiveMQtt.MQTT5.PacketDecoder` | Inbound packet decoding |
| `HiveMQtt.MQTT5.ControlPacket` | Packet-level event launchers |
| `HiveMQtt.Client.HiveMQClientOptionsBuilder` | Option validation messages |

```csharp
using var loggerFactory = LoggerFactory.Create(builder => builder
    .AddSimpleConsole()
    .SetMinimumLevel(LogLevel.Trace)
    .AddFilter("HiveMQtt.Client.Transport", LogLevel.Warning));
```

## Trace level output

Setting the minimum level to `Trace` outputs all activity in the HiveMQtt package down to packet and
event handling.  This produces a lot of output, such as:

```log
trce: HiveMQtt.Client.HiveMQClient[0]
      Trace Level Logging Legend:
trce: HiveMQtt.Client.HiveMQClient[0]
          -(W)-   == ConnectionWriter
trce: HiveMQtt.Client.HiveMQClient[0]
          -(R)-   == ConnectionReader
info: HiveMQtt.Client.HiveMQClient[0]
      Connecting to broker at 127.0.0.1:1883
trce: HiveMQtt.Client.HiveMQClient[0]
      BeforeConnectEventLauncher
trce: HiveMQtt.Client.Connection.ConnectionManager[0]
      -(W)- --> Sending ConnectPacket id=0
trce: HiveMQtt.Client.Connection.ConnectionManager[0]
      -(RPH)- <-- Received ConnAck id=0
```

## Native AOT

There is nothing extra to do: HiveMQtt holds no static logging state and performs no configuration
file discovery or reflection-based target loading.  Configure your logging provider as that provider
documents for AOT.  See [Native AOT](/docs/hivemqtt/how-to/native-aot).

## Migrating from NLog (HiveMQtt 0.47 and earlier)

Earlier versions of HiveMQtt depended on NLog directly and were configured by dropping an
`NLog.config` file next to your executable.  That is no longer the case — the `NLog` dependency and
the bundled `NLog.config` have been removed.

To keep using NLog as your backend, add
[`NLog.Extensions.Logging`](https://www.nuget.org/packages/NLog.Extensions.Logging) to *your*
application and bridge it in:

```csharp
using var loggerFactory = LoggerFactory.Create(builder => builder.AddNLog());

var options = new HiveMQClientOptionsBuilder()
    .WithBroker("127.0.0.1")
    .WithLoggerFactory(loggerFactory)
    .Build();
```

Your existing `NLog.config` rules (including `<logger name="HiveMQtt.*" .../>`) continue to work,
because the category names are unchanged.

## See Also

* [Microsoft.Extensions.Logging](https://learn.microsoft.com/dotnet/core/extensions/logging)
* [Native AOT](/docs/hivemqtt/how-to/native-aot)
