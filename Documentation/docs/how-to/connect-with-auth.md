---
sidebar_position: 1
---

# Connect with Username/Password Authentication

Connect securely to an MQTT broker using basic authentication credentials.

## Basic Example

```csharp
using HiveMQtt.Client;
using HiveMQtt.MQTT5.ReasonCodes;

var options = new HiveMQClientOptionsBuilder()
    .WithBroker("your-broker.hivemq.cloud")
    .WithPort(8883)
    .WithUseTls(true)
    .WithUserName("my-username")
    .WithPassword("my-password")
    .Build();

var client = new HiveMQClient(options);
var connectResult = await client.ConnectAsync().ConfigureAwait(false);

if (connectResult.ReasonCode == ConnAckReasonCode.Success)
{
    Console.WriteLine("Connected successfully!");
}
```

## Security Best Practices

:::warning
Never hardcode credentials in your source code. Use environment variables, configuration files, or a secrets manager.
:::

### Using Environment Variables

```csharp
var options = new HiveMQClientOptionsBuilder()
    .WithBroker(Environment.GetEnvironmentVariable("MQTT_BROKER"))
    .WithPort(8883)
    .WithUseTls(true)
    .WithUserName(Environment.GetEnvironmentVariable("MQTT_USERNAME"))
    .WithPassword(Environment.GetEnvironmentVariable("MQTT_PASSWORD"))
    .Build();
```

### Using Configuration (appsettings.json)

```csharp
// In appsettings.json:
// {
//   "Mqtt": {
//     "Broker": "your-broker.hivemq.cloud",
//     "Username": "your-username",
//     "Password": "your-password"
//   }
// }

var mqttConfig = configuration.GetSection("Mqtt");
var options = new HiveMQClientOptionsBuilder()
    .WithBroker(mqttConfig["Broker"])
    .WithPort(8883)
    .WithUseTls(true)
    .WithUserName(mqttConfig["Username"])
    .WithPassword(mqttConfig["Password"])
    .Build();
```

## Handling Authentication Failures

```csharp
using HiveMQtt.MQTT5.ReasonCodes;

var connectResult = await client.ConnectAsync().ConfigureAwait(false);

switch (connectResult.ReasonCode)
{
    case ConnAckReasonCode.Success:
        Console.WriteLine("Connected!");
        break;
    case ConnAckReasonCode.BadUserNameOrPassword:
        Console.WriteLine("Invalid credentials");
        break;
    case ConnAckReasonCode.NotAuthorized:
        Console.WriteLine("Not authorized to connect");
        break;
    default:
        Console.WriteLine($"Connection failed: {connectResult.ReasonCode}");
        break;
}
```

## See Also

- [Authentication with Username and Password - MQTT Security Fundamentals](https://www.hivemq.com/blog/mqtt-security-fundamentals-authentication-username-password/)
- [Advanced Authentication Mechanisms - MQTT Security Fundamentals](https://www.hivemq.com/blog/mqtt-security-fundamentals-advanced-authentication-mechanisms/)
- [HiveMQ Cloud - Authentication and Authorization](https://docs.hivemq.com/hivemq-cloud/authn-authz.html)
- [Client Certificates](/docs/how-to/client-certificates) - Certificate-based authentication
