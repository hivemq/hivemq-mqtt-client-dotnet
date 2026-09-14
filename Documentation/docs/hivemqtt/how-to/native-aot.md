---
sidebar_position: 11
---

# Native AOT

HiveMQtt and HiveMQtt.Sparkplug are marked [`IsAotCompatible`](https://learn.microsoft.com/en-us/dotnet/core/deploying/native-aot/) for **.NET 8.0 and later**. That enables trim, single-file, and AOT analyzers during the library build and advertises Native AOT readiness to consumers.

## Publish your app with Native AOT

In your **application** project (not the library):

```xml
<PropertyGroup>
  <TargetFramework>net8.0</TargetFramework>
  <PublishAot>true</PublishAot>
</PropertyGroup>
```

Then publish for a specific runtime identifier:

```bash
dotnet publish -c Release -r osx-arm64
# or linux-x64, win-x64, etc.
```

You do **not** set `IsAotCompatible` in your app — that property is for libraries.

## Logging under Native AOT

HiveMQtt depends only on `Microsoft.Extensions.Logging.Abstractions` and holds no static logging
state: there is no configuration-file discovery and no reflection-based target loading to trim away.
You pass in an `ILoggerFactory` and HiveMQtt uses it — nothing more.

```csharp
using var loggerFactory = LoggerFactory.Create(builder =>
    builder.AddSimpleConsole().SetMinimumLevel(LogLevel.Information));

var options = new HiveMQClientOptionsBuilder()
    .WithBroker("127.0.0.1")
    .WithLoggerFactory(loggerFactory)
    .Build();
```

Whatever AOT caveats apply are your logging provider's, not HiveMQtt's — follow that provider's AOT
guidance.  See [Configure Logging](/docs/hivemqtt/how-to/configure-logging).

## Sparkplug and Google.Protobuf

`HiveMQtt.Sparkplug` is also marked `IsAotCompatible` (trim/AOT analyzers are clean on net8.0+). [Google.Protobuf is not officially AOT-certified](https://github.com/protocolbuffers/protobuf/issues/25574), so treat Sparkplug AOT support as **best-effort** relative to that dependency. Validate `PublishAot` in your own application.

## See Also

* [Native AOT deployment overview](https://learn.microsoft.com/en-us/dotnet/core/deploying/native-aot/)
* [Configure Logging](/docs/hivemqtt/how-to/configure-logging)
* [Creating AOT-compatible libraries](https://devblogs.microsoft.com/dotnet/creating-aot-compatible-libraries/)
