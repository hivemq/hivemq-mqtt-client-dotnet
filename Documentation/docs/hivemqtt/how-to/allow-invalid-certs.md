---
sidebar_position: 4
---

# Allow Invalid TLS Certificates

In development or testing scenarios, you may need to connect to an MQTT broker with a self-signed or invalid TLS certificate. This guide shows how to bypass certificate validation.

:::danger Security Warning
Disabling certificate validation exposes your application to man-in-the-middle attacks. **Only use this in controlled development environments, never in production.**
:::

## Example

```csharp
var options = new HiveMQClientOptionsBuilder()
    .WithBroker("broker-with-invalid-tls-cert.localhost.dev")
    .WithPort(8883)
    .WithUseTls(true)
    .WithAllowInvalidBrokerCertificates(true)
    .Build();

var client = new HiveMQClient(options);
var connectResult = await client.ConnectAsync().ConfigureAwait(false);
```

The `WithAllowInvalidBrokerCertificates(true)` call disables certificate validation, allowing connections to brokers with self-signed or expired certificates.

## See Also

* [HiveMQClientOptionsBuilder.cs](https://github.com/hivemq/hivemq-mqtt-client-dotnet/blob/main/Source/HiveMQtt/Client/HiveMQClientOptionsBuilder.cs)
* [HiveMQClientOptions.cs](https://github.com/hivemq/hivemq-mqtt-client-dotnet/blob/main/Source/HiveMQtt/Client/Options/HiveMQClientOptions.cs)
* [TLS/SSL - MQTT Security Fundamentals](https://www.hivemq.com/blog/mqtt-security-fundamentals-tls-ssl/)
* [HiveMQ Documentation on Security](https://docs.hivemq.com/hivemq/latest/user-guide/security.html)
