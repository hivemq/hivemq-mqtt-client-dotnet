# Connecting with TLS and Allowing Invalid TLS Certificates

In certain development or testing scenarios, you might need to connect to an MQTT broker that uses TLS but has an invalid or self-signed certificate. The HiveMQtt client library provides an option to disable the TLS certificate check upon connection, which can be useful in these situations.

The `AllowInvalidBrokerCertificates` option in the `HiveMQClientOptions` class allows you to disable the TLS certificate check. 

Here's an example of how to use this option:

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

In this example, we first create an instance of HiveMQClientOptionsBuilder. We then set the broker address, port, and enable TLS. The WithAllowInvalidBrokerCertificates(true) method call disables the TLS certificate check. Finally, we build the options and use them to create a new HiveMQClient.

:::note

Disabling the TLS certificate check can expose your application to security risks, such as man-in-the-middle attacks. Therefore, this option should only be used in controlled environments for development or testing purposes.

::: 

## See Also

* [HiveMQClientOptionsBuilder.cs](https://github.com/hivemq/hivemq-mqtt-client-dotnet/blob/main/Source/HiveMQtt/Client/HiveMQClientOptionsBuilder.cs)
* [HiveMQClientOptions.cs](https://github.com/hivemq/hivemq-mqtt-client-dotnet/blob/main/Source/HiveMQtt/Client/Options/HiveMQClientOptions.cs)
* [TLS/SSL - MQTT Security Fundamentals](https://www.hivemq.com/blog/mqtt-security-fundamentals-tls-ssl/)
* [HiveMQ Documentation on Security](https://docs.hivemq.com/hivemq/latest/user-guide/security.html)
