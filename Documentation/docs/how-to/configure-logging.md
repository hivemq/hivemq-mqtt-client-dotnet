# Configure HiveMQtt Logging

The HiveMQtt package uses [NLog](https://github.com/NLog/NLog) and can be configured with a configuration file (`NLog.config`).  Having this file in the same directory of your executable will configure the HiveMQtt logger to output as configured:

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
    <logger name="HiveMQtt.*" minlevel="Error" writeTo="logconsole" />
  </rules>
</nlog>

```

Setting `minlevel` to `Trace` will output all activity in the HiveMQtt package down to packet and event handling.  Using this level will produce a lot of output such as the following:

```log
2023-10-04 16:56:54.9373|TRACE|HiveMQtt.Client.HiveMQClient|BeforeConnectEventLauncher
2023-10-04 16:56:55.0081|TRACE|HiveMQtt.Client.HiveMQClient|7: TrafficInflowProcessor Starting...Connecting
2023-10-04 16:56:55.0081|TRACE|HiveMQtt.Client.HiveMQClient|9: TrafficOutflowProcessor Starting...Connecting
2023-10-04 16:56:55.0081|TRACE|HiveMQtt.Client.HiveMQClient|--> ConnectPacket
2023-10-04 16:56:55.0128|TRACE|HiveMQtt.Client.HiveMQClient|OnConnectSentEventLauncher
2023-10-04 16:56:55.0374|TRACE|HiveMQtt.Client.HiveMQClient|<-- ConnAck
2023-10-04 16:56:55.0374|TRACE|HiveMQtt.Client.HiveMQClient|OnConnAckReceivedEventLauncher
2023-10-04 16:56:55.0379|TRACE|HiveMQtt.Client.HiveMQClient|AfterConnectEventLauncher
```
