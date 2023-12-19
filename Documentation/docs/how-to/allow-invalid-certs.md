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
