---
sidebar_position: 30
---
# Namespaces

The HiveMQtt client is broken in to a few key namespaces that provides various functionality.  This page serves as a reference to these namespaces and what they encapsulate.

# Definition

In C#, a namespace is a way to organize and group related classes, interfaces, structs, enums, and other types. It provides a hierarchical naming structure to avoid naming conflicts and to provide logical separation of code elements. Namespaces help in organizing code into logical units, improving code readability, and facilitating code reuse.

# Example

```csharp
using HiveMQtt.Client;
using HiveMQtt.MQTT5.Types;

/// code
```

# List of Namespaces


| Namespace              | Description | Classes |
|-----------------------|----------|----------|
| `HiveMQtt.Client`       | Base namespace for the HiveMQClient, RawClient and related classes. | `HiveMQClient`, `RawClient` (Beta), `LastWillAndTestament`, etc...|
| `HiveMQtt.Client.Options`| Class that encapsulate options. | `ConnectOptions`, `DisconnectOptions`, `SubscribeOptions` etc...|
| `HiveMQtt.Client.Events` | Classes related to the event subsystem. | _See events reference._|
| `HiveMQtt.Client.Exceptions` | HiveMQtt Exceptions. | `HiveMQttClientException` |
| `HiveMQtt.Client.ReasonCodes` | Reason code exceptions. | `QoS1ReasonCode`, `QoS2ReasonCode`|
| `HiveMQtt.Client.Results` | Result classes.| `ConnectResult`, `PublishResult`, `SubscribeResult`, `UnsubscribeResult`|
| `HiveMQtt.MQTT5.Types`   | MQTT protocol types.| `QualityOfService`, `RetainHandling`, `TopicFilter` etc...|
| `HiveMQtt.MQTT5.Packets` | MQTT packet classes. | `ConnectPacket`, `PingReqPacket` etc...|
| `HiveMQtt.MQTT5.ReasonCodes` | Packet level reason codes. | `ConnAckReasonCode`, `PubRecReasonCode`|
