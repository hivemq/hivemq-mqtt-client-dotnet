# Logging Example

The HiveMQtt package uses [Microsoft.Extensions.Logging](https://docs.microsoft.com/en-us/dotnet/core/extensions/logging) (Abstractions) for logging. This example demonstrates how to configure logging for the HiveMQtt client.

## Overview

This example shows how to:
- Configure console logging with different log levels
- Use NLog with Microsoft.Extensions.Logging (for existing NLog users)
- Use Serilog for structured logging
- Disable logging entirely

## Basic Console Logging

The simplest way to enable logging is using the built-in console logger:

```csharp
using Microsoft.Extensions.Logging;
using HiveMQtt.Client;
using HiveMQtt.Client.Options;

var loggerFactory = LoggerFactory.Create(builder =>
{
    builder
        .AddConsole()
        .SetMinimumLevel(LogLevel.Trace);  // Set to Trace for maximum detail
});

var options = new HiveMQClientOptions
{
    Host = "127.0.0.1",
    Port = 1883,
    LoggerFactory = loggerFactory
};

var client = new HiveMQClient(options);
```

## Using NLog (Migration Path)

If you were previously using NLog, you can continue using it with `NLog.Extensions.Logging`:

### Step 1: Install NLog.Extensions.Logging

```bash
dotnet add package NLog.Extensions.Logging
```

### Step 2: Configure NLog

```csharp
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using HiveMQtt.Client;
using HiveMQtt.Client.Options;

var loggerFactory = LoggerFactory.Create(builder =>
{
    builder.AddNLog();  // Reads NLog.config automatically
});

var options = new HiveMQClientOptions
{
    Host = "127.0.0.1",
    Port = 1883,
    LoggerFactory = loggerFactory
};

var client = new HiveMQClient(options);
```

### Step 3: Create NLog.config (Optional)

You can create an `NLog.config` file in your project root:

```xml
<?xml version="1.0" encoding="utf-8" ?>
<nlog xmlns="http://www.nlog-project.org/schemas/NLog.xsd"
      xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance">

  <targets>
    <target name="logfile" xsi:type="File" fileName="HiveMQtt.log" />
    <target name="logconsole" xsi:type="Console" />
  </targets>

  <rules>
     <!-- minlevel can be Debug, Info, Warn, Error and Fatal or Trace -->
    <logger name="HiveMQtt.*" minlevel="Trace" writeTo="logconsole" />
  </rules>
</nlog>
```

## Log Levels

The client supports the following log levels:

- **Trace**: Most detailed logging (packet-level details, internal state)
- **Debug**: Detailed diagnostic information
- **Information**: General informational messages
- **Warning**: Warning messages
- **Error**: Error messages and exceptions

Set the minimum log level when configuring your logger factory:

```csharp
var loggerFactory = LoggerFactory.Create(builder =>
{
    builder
        .AddConsole()
        .SetMinimumLevel(LogLevel.Trace);  // Change this to control verbosity
});
```

## Disabling Logging

If you don't need logging, simply don't set the `LoggerFactory` property:

```csharp
var options = new HiveMQClientOptions
{
    Host = "127.0.0.1",
    Port = 1883
    // LoggerFactory not set - logging is disabled (NullLogger used)
};

var client = new HiveMQClient(options);
```

## See Also

* [Configure Logging Documentation](/docs/how-to/configure-logging) - Complete logging configuration guide
* [How to Debug](/docs/how-to/debug) - Using Trace-level logging for debugging
* [Microsoft.Extensions.Logging](https://docs.microsoft.com/en-us/dotnet/core/extensions/logging) - Official Microsoft documentation
