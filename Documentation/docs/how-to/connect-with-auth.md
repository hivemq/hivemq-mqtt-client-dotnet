# Securely Connect to a Broker with Basic Authentication Credentials

To securely connect to an MQTT Broker with basic authentication credentials, use the `UserName` and `Password` fields in `HiveMQClientOptions`:

```csharp
var options = new HiveMQClientOptions()
{
    Host = "b8293h09193b.s1.eu.hivemq.cloud",
    Port = 8883,
    UseTLS = true,
    UserName = "my-username",
    Password = "my-password",
};

var client = new HiveMQClient(options);
var connectResult = await client.ConnectAsync().ConfigureAwait(false);
```
