# Securely Connect to a Broker with Basic Authentication Credentials

To securely connect to an MQTT Broker with basic authentication credentials, use the `UserName` and `Password` fields in `HiveMQClientOptions`:

```csharp
var options = new HiveMQClientOptionsBuilder()
    .WithBroker("b273h09193b.s1.eu.hivemq.cloud")
    .WithPort(8883)
    .WithUseTls(true)
    .WithUserName("my-username")
    .WithPassword("my-password")
    .Build();

var client = new HiveMQClient(options);
var connectResult = await client.ConnectAsync().ConfigureAwait(false);
```
