# Logging

HiveMQtt logs through the standard [`Microsoft.Extensions.Logging`](https://learn.microsoft.com/dotnet/core/extensions/logging)
abstractions.  The package depends only on `Microsoft.Extensions.Logging.Abstractions` — it does not
pull in, or configure, any particular logging backend.

**By default HiveMQtt logs nothing.**  To see its internal activity, hand it an `ILoggerFactory`:

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
await client.ConnectAsync().ConfigureAwait(false);
```

Because it is just `ILoggerFactory`, any backend works — Serilog, NLog, ZLogger, or the built-in
console/debug providers.  In an ASP.NET Core or generic-host application, resolve the factory
from DI:

```csharp
var options = new HiveMQClientOptionsBuilder()
    .WithBroker("127.0.0.1")
    .WithLoggerFactory(serviceProvider.GetRequiredService<ILoggerFactory>())
    .Build();
```

## Log categories

Each HiveMQtt type logs under its own full type name, so you can filter by category:

| Category | What it covers |
|---|---|
| `HiveMQtt.Client.HiveMQClient` | Client lifecycle, publish/subscribe, event launchers |
| `HiveMQtt.Client.RawClient` | Same, for `RawClient` |
| `HiveMQtt.Client.Connection.ConnectionManager` | Connect/disconnect, reader/writer tasks, keep-alive |
| `HiveMQtt.Client.Transport.BaseTransport` | TCP / WebSocket / TLS transport |
| `HiveMQtt.MQTT5.PacketDecoder` | Inbound packet decoding |
| `HiveMQtt.MQTT5.ControlPacket` | Packet-level event launchers |

For example, to see everything except the very chatty transport layer:

```csharp
using var loggerFactory = LoggerFactory.Create(builder => builder
    .AddSimpleConsole()
    .SetMinimumLevel(LogLevel.Trace)
    .AddFilter("HiveMQtt.Client.Transport", LogLevel.Warning));
```

See [this section](https://github.com/hivemq/hivemq-mqtt-client-dotnet#logging) in the repository
README for more details.
