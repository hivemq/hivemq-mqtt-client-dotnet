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

HiveMQtt uses [NLog 6](https://nlog-project.org/2025/06/21/nlog-6-0-released.html), which supports AOT. If you load logging from an XML `NLog.config`, the AOT/trimmer cannot see target types referenced only by `xsi:type`. Register the targets your config uses before logging starts:

```csharp
NLog.LogManager.Setup().SetupExtensions(ext =>
{
    ext.RegisterTarget<NLog.Targets.FileTarget>();
    ext.RegisterTarget<NLog.Targets.ConsoleTarget>();
});
```

See [Configure Logging](/docs/hivemqtt/how-to/configure-logging) for the full NLog setup.

## Sparkplug and Google.Protobuf

`HiveMQtt.Sparkplug` is also marked `IsAotCompatible` (trim/AOT analyzers are clean on net8.0+). [Google.Protobuf is not officially AOT-certified](https://github.com/protocolbuffers/protobuf/issues/25574), so treat Sparkplug AOT support as **best-effort** relative to that dependency. Validate `PublishAot` in your own application.

## See Also

* [Native AOT deployment overview](https://learn.microsoft.com/en-us/dotnet/core/deploying/native-aot/)
* [Configure Logging](/docs/hivemqtt/how-to/configure-logging)
* [Creating AOT-compatible libraries](https://devblogs.microsoft.com/dotnet/creating-aot-compatible-libraries/)
