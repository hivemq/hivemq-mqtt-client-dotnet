---
sidebar_position: 4
---

# Connecting to an MQTT Broker

## with Defaults

Without any options given, the `HiveMQClient` will search on `localhost` port 1883 for an unsecured broker.

If you don't have a broker at this location, see the next sections.

```csharp
using HiveMQtt.Client;

// Connect
var client = new HiveMQClient();
var connectResult = await client.ConnectAsync().ConfigureAwait(false);
```

## With Options

The `HiveMQClientOptions` class provides a set of options that can be used to configure various aspects of the `HiveMQClient`.

The easiest way to construct this class is to use `HiveMQClientOptionsBuilder`.

```csharp
var options = new HiveMQClientOptionsBuilder().
                    WithBroker('candy.x39.eu.hivemq.cloud').
                    WithPort(8883).
                    WithUseTLS(true).
                    Build();

var client = new HiveMQClient(options);
var connectResult = await client.ConnectAsync().ConfigureAwait(false);
```

## `HiveMQClientOptionsBuilder` Reference

To illustrate _each and every possible call_ with `HiveMQClientOptionsBuilder`, see the following example:

```csharp
using HiveMQtt.MQTT5.Types; // For QualityOfService enum

var options = new HiveMQClientOptionsBuilder()
    .WithBroker("broker.hivemq.com")
    .WithPort(1883)
    .WithClientId("myClientId")
    .WithAllowInvalidBrokerCertificates(true)
    .WithUseTls(true)
    .WithCleanStart(true)
    .WithKeepAlive(60)
    .WithAuthenticationMethod("UsernamePassword")
    .WithAuthenticationData(Encoding.UTF8.GetBytes("authenticationData"))
    .WithUserProperty("property1", "value1")
    .WithUserProperties(new Dictionary<string, string> { { "property1", "value1" }, { "property2", "value2" } })
    .WithLastWill(new LastWillAndTestament {
                            Topic = "lwt/topic",
                            PayloadAsString = "LWT message",
                            QoS = QualityOfService.AtLeastOnceDelivery,
                            Retain = true })
    .WithMaximumPacketSize(1024)
    .WithReceiveMaximum(100)
    .WithSessionExpiryInterval(3600)
    .WithUserName("myUserName")
    .WithPassword("myPassword")
    .WithPreferIPv6(true)
    .WithTopicAliasMaximum(10)
    .WithRequestProblemInformation(true)
    .WithRequestResponseInformation(true)
    .Build();
```

## See Also

* [How to Set a Last Will & Testament](/docs/how-to/set-lwt)
* [Connect with TLS but allow Invalid TLS Certificates](/docs/how-to/allow-invalid-certs)
* [Securely Connect to a Broker with Basic Authentication Credentials](/docs/how-to/connect-with-auth)
* [Custom Client Certificates](/docs/how-to/client-certificates)
* [HiveMQClientOptionsBuilder.cs](https://github.com/hivemq/hivemq-mqtt-client-dotnet/blob/main/Source/HiveMQtt/Client/HiveMQClientOptionsBuilder.cs)
