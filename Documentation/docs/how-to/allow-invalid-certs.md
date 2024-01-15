# Connect with TLS but allow Invalid TLS Certificates

Use the `AllowInvalidBrokerCertificates` option in `HiveMQClientOptions` to disable the TLS certificate check upon connect.

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

## See Also

* [HiveMQClientOptionsBuilder.cs](https://github.com/hivemq/hivemq-mqtt-client-dotnet/blob/main/Source/HiveMQtt/Client/HiveMQClientOptionsBuilder.cs)
* [HiveMQClientOptions.cs](https://github.com/hivemq/hivemq-mqtt-client-dotnet/blob/main/Source/HiveMQtt/Client/Options/HiveMQClientOptions.cs)
* [TLS/SSL - MQTT Security Fundamentals](https://www.hivemq.com/blog/mqtt-security-fundamentals-tls-ssl/)
* [HiveMQ Documentation on Security](https://docs.hivemq.com/hivemq/latest/user-guide/security.html)
