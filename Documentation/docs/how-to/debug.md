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

Create an `NLog.config` file in your application directory:

```xml
<?xml version="1.0" encoding="utf-8" ?>
<nlog xmlns="http://www.nlog-project.org/schemas/NLog.xsd"
      xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance">
  <targets>
    <target name="logconsole" xsi:type="Console" />
  </targets>
  <rules>
    <logger name="HiveMQtt.*" minlevel="Trace" writeTo="logconsole" />
  </rules>
</nlog>
```

See [Configure Logging](/docs/how-to/configure-logging) for more detailed configuration options.
