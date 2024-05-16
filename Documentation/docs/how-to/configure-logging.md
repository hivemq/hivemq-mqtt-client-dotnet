# Configure Logging

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
2024-03-14 15:40:18.2252|TRACE|HiveMQtt.Client.HiveMQClient|Trace Level Logging Legend:
2024-03-14 15:40:18.2312|TRACE|HiveMQtt.Client.HiveMQClient|    -(W)-   == ConnectionWriter
2024-03-14 15:40:18.2312|TRACE|HiveMQtt.Client.HiveMQClient|    -(PW)-   == ConnectionPublishWriter
2024-03-14 15:40:18.2312|TRACE|HiveMQtt.Client.HiveMQClient|    -(R)-   == ConnectionReader
2024-03-14 15:40:18.2312|TRACE|HiveMQtt.Client.HiveMQClient|    -(RPH)- == ReceivedPacketsHandler
2024-03-14 15:40:18.2320|INFO|HiveMQtt.Client.HiveMQClient|Connecting to broker at 127.0.0.1:1883
2024-03-14 15:40:18.2343|TRACE|HiveMQtt.Client.HiveMQClient|BeforeConnectEventLauncher
2024-03-14 15:40:18.2460|TRACE|HiveMQtt.Client.HiveMQClient|Socket connected to 127.0.0.1:1883
2024-03-14 15:40:18.2464|TRACE|HiveMQtt.Client.HiveMQClient|Queuing packet for send: HiveMQtt.MQTT5.Packets.ConnectPacket
2024-03-14 15:40:18.2464|TRACE|HiveMQtt.Client.HiveMQClient|-(RPH)- Starting...Connecting
2024-03-14 15:40:18.2464|TRACE|HiveMQtt.Client.HiveMQClient|-(R)- ConnectionReader Starting...Connecting
2024-03-14 15:40:18.2464|TRACE|HiveMQtt.Client.HiveMQClient|5: ConnectionMonitor Starting...Connecting
2024-03-14 15:40:18.2464|TRACE|HiveMQtt.Client.HiveMQClient|-(RPH)- 0 received packets currently waiting to be processed.
2024-03-14 15:40:18.2464|TRACE|HiveMQtt.Client.HiveMQClient|-(W)- ConnectionWriter Starting...Connecting
2024-03-14 15:40:18.2464|TRACE|HiveMQtt.Client.HiveMQClient|-(W)- ConnectionWriter: 1 packets waiting to be sent.
2024-03-14 15:40:18.2476|TRACE|HiveMQtt.Client.HiveMQClient|-(W)- --> Sending ConnectPacket id=0
2024-03-14 15:40:18.2529|TRACE|HiveMQtt.Client.HiveMQClient|OnConnectSentEventLauncher
2024-03-14 15:40:18.2529|TRACE|HiveMQtt.Client.HiveMQClient|-(W)- ConnectionWriter: 0 packets waiting to be sent.
2024-03-14 15:40:18.2732|TRACE|HiveMQtt.Client.HiveMQClient|ReadAsync: Read Buffer Length 11
2024-03-14 15:40:18.2765|TRACE|HiveMQtt.MQTT5.PacketDecoder|PacketDecoder: Decoded Packet: consumed=11, packet=HiveMQtt.MQTT5.Packets.ConnAckPacket id=0
2024-03-14 15:40:18.2765|TRACE|HiveMQtt.Client.HiveMQClient|-(R)- <-- Received ConnAckPacket.  Adding to receivedQueue.
2024-03-14 15:40:18.2765|TRACE|HiveMQtt.Client.HiveMQClient|-(RPH)- <-- Received ConnAck id=0
2024-03-14 15:40:18.2765|TRACE|HiveMQtt.Client.HiveMQClient|OnConnAckReceivedEventLauncher
2024-03-14 15:40:18.2775|TRACE|HiveMQtt.Client.HiveMQClient|AfterConnectEventLauncher
```

## See Also

* [NLog](https://github.com/NLog/NLog)
