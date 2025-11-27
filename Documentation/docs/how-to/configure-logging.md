# Configure Logging

The HiveMQtt package uses [Microsoft.Extensions.Logging](https://docs.microsoft.com/en-us/dotnet/core/extensions/logging) (Abstractions) as its logging infrastructure. This allows you to use any logging provider that implements the Microsoft.Extensions.Logging interface, including NLog, Serilog, Console, and many others.

## Basic Usage

To enable logging, you need to provide an `ILoggerFactory` to the `HiveMQClientOptions`:

```csharp
using Microsoft.Extensions.Logging;
using HiveMQtt.Client;
using HiveMQtt.Client.Options;

// Create a logger factory with console output
var loggerFactory = LoggerFactory.Create(builder =>
{
    builder.AddConsole().SetMinimumLevel(LogLevel.Trace);
});

var options = new HiveMQClientOptions
{
    Host = "127.0.0.1",
    Port = 1883,
    LoggerFactory = loggerFactory  // Set the logger factory
};

var client = new HiveMQClient(options);
```

## Using Console Logging

The simplest way to enable logging is to use the built-in console logger:

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

## Using NLog (For Existing NLog Users)

If you're already using NLog in your application, you can continue using it by configuring NLog as a Microsoft.Extensions.Logging provider:

### Step 1: Install the NLog.Extensions.Logging Package

```bash
dotnet add package NLog.Extensions.Logging
```

### Step 2: Configure NLog with Microsoft.Extensions.Logging

```csharp
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using HiveMQtt.Client;
using HiveMQtt.Client.Options;

// Setup NLog with MEL - this will automatically read NLog.config if present
var loggerFactory = LoggerFactory.Create(builder =>
{
    builder.AddNLog();  // This reads NLog.config automatically
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

You can still use your existing `NLog.config` file:

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

## Using Serilog

Serilog is another popular logging framework that works seamlessly with Microsoft.Extensions.Logging:

### Step 1: Install Required Packages

```bash
dotnet add package Serilog.Extensions.Logging
dotnet add package Serilog.Sinks.Console
```

### Step 2: Configure Serilog

```csharp
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Extensions.Logging;
using HiveMQtt.Client;
using HiveMQtt.Client.Options;

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Verbose()  // Set to Verbose for Trace-level logging
    .WriteTo.Console()
    .CreateLogger();

var loggerFactory = new SerilogLoggerFactory();

var options = new HiveMQClientOptions
{
    Host = "127.0.0.1",
    Port = 1883,
    LoggerFactory = loggerFactory
};

var client = new HiveMQClient(options);
```

## Log Levels

The HiveMQtt client uses the following log levels:

- **Trace**: Most detailed logging, including packet-level details and internal state information
- **Debug**: Detailed diagnostic information useful for debugging
- **Information**: General informational messages about client operations
- **Warning**: Warning messages for potentially problematic situations
- **Error**: Error messages for failures and exceptions

### Setting Log Levels

You can control the log level when configuring your logger factory:

```csharp
var loggerFactory = LoggerFactory.Create(builder =>
{
    builder
        .AddConsole()
        .SetMinimumLevel(LogLevel.Trace);  // Change this to control verbosity
        // Options: Trace, Debug, Information, Warning, Error, Critical, None
});

var options = new HiveMQClientOptions
{
    Host = "127.0.0.1",
    Port = 1883,
    LoggerFactory = loggerFactory
};
```

## Disabling Logging

If you don't want any logging output, simply don't set the `LoggerFactory` property. The client will use a `NullLogger` which discards all log messages:

```csharp
var options = new HiveMQClientOptions
{
    Host = "127.0.0.1",
    Port = 1883
    // LoggerFactory not set - logging is disabled
};

var client = new HiveMQClient(options);
```

## Example: Trace Level Output

When configured with `LogLevel.Trace`, you'll see detailed output like:

```
trce: HiveMQtt.Client.HiveMQClient[0]
      New client created: Client ID: hmqcsabc123xyz
trce: HiveMQtt.Client.Connection.ConnectionManager[0]
      Trace Level Logging Legend:
trce: HiveMQtt.Client.Connection.ConnectionManager[0]
          -(W)-   == ConnectionWriter
trce: HiveMQtt.Client.Connection.ConnectionManager[0]
          -(PW)-  == ConnectionPublishWriter
trce: HiveMQtt.Client.Connection.ConnectionManager[0]
          -(R)-   == ConnectionReader
trce: HiveMQtt.Client.Connection.ConnectionManager[0]
          -(CM)-  == ConnectionMonitor
trce: HiveMQtt.Client.Connection.ConnectionManager[0]
          -(RPH)- == ReceivedPacketsHandler
info: HiveMQtt.Client.HiveMQClient[0]
      Connecting to broker at 127.0.0.1:1883
trce: HiveMQtt.Client.HiveMQClient[0]
      Queuing CONNECT packet for send.
```

## Integration with ASP.NET Core

If you're using ASP.NET Core, you can use the built-in dependency injection:

```csharp
using Microsoft.Extensions.Logging;
using HiveMQtt.Client;
using HiveMQtt.Client.Options;

public class MyService
{
    private readonly ILoggerFactory _loggerFactory;

    public MyService(ILoggerFactory loggerFactory)
    {
        _loggerFactory = loggerFactory;
    }

    public void CreateClient()
    {
        var options = new HiveMQClientOptions
        {
            Host = "127.0.0.1",
            Port = 1883,
            LoggerFactory = _loggerFactory  // Use injected logger factory
        };

        var client = new HiveMQClient(options);
    }
}
```

## See Also

* [Microsoft.Extensions.Logging Documentation](https://docs.microsoft.com/en-us/dotnet/core/extensions/logging)
* [NLog.Extensions.Logging](https://github.com/NLog/NLog.Extensions.Logging)
* [Serilog.Extensions.Logging](https://github.com/serilog/serilog-extensions-logging)
* [How to Debug](/docs/how-to/debug) - Learn more about using Trace-level logging for debugging
