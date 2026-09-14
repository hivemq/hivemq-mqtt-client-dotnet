---
sidebar_position: 6
---

# Debug the Client

When troubleshooting issues with the HiveMQ client, TRACE level logging provides detailed insight into the client's internal operations.

## Enable TRACE Logging

TRACE logging shows comprehensive information about all client operations, including:

- Connection establishment and teardown
- Packet transmission and reception
- Event handling
- Queue processing

This level of detail is invaluable for debugging complex issues.

## Quick Setup

Hand the client an `ILoggerFactory` configured for `Trace`:

```csharp
using Microsoft.Extensions.Logging;

using var loggerFactory = LoggerFactory.Create(builder =>
    builder.AddSimpleConsole().SetMinimumLevel(LogLevel.Trace));

var options = new HiveMQClientOptionsBuilder()
    .WithBroker("127.0.0.1")
    .WithLoggerFactory(loggerFactory)
    .Build();

var client = new HiveMQClient(options);
```

See [Configure Logging](/docs/hivemqtt/how-to/configure-logging) for more detailed configuration options.
