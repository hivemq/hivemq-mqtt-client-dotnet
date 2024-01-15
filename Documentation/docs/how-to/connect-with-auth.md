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

## See Also

* [Authentication with Username and Password - MQTT Security Fundamentals](https://www.hivemq.com/blog/mqtt-security-fundamentals-authentication-username-password/)
* [Advanced Authentication Mechanisms - MQTT Security Fundamentals](https://www.hivemq.com/blog/mqtt-security-fundamentals-advanced-authentication-mechanisms/)
* [HiveMQ Cloud / Authentication and Authorization](https://docs.hivemq.com/hivemq-cloud/authn-authz.html)
* [HiveMQClientOptionsBuilder.cs](https://github.com/hivemq/hivemq-mqtt-client-dotnet/blob/main/Source/HiveMQtt/Client/HiveMQClientOptionsBuilder.cs)
* [HiveMQClientOptions.cs](https://github.com/hivemq/hivemq-mqtt-client-dotnet/blob/main/Source/HiveMQtt/Client/Options/HiveMQClientOptions.cs)
