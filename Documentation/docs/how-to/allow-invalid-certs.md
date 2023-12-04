# Connect with TLS but allow Invalid TLS Certificates

Use the `AllowInvalidBrokerCertificates` option in `HiveMQClientOptions` to disable the TLS certificate check upon connect.

```csharp
var options = new HiveMQClientOptions()
{
    Host = "broker-with-invalid-tls-cert.localhost.dev",
    Port = 8883,
    UseTLS = true,
    AllowInvalidBrokerCertificates = true,
};

var client = new HiveMQClient(options);
var connectResult = await client.ConnectAsync().ConfigureAwait(false);
```
