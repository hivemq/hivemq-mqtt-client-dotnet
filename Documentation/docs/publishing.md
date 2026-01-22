---
sidebar_position: 60
---

# Publishing Messages

In MQTT, "publish" is an operation that allows an MQTT client to send a message to an MQTT broker, which then distributes the message to all subscribed clients interested in the topic of the message.

## Simple

Use the PublishAsync method to publish a payload to the desired topic by providing the topic string and payload as parameters.

```csharp
var publishResult = await client.PublishAsync("topic1/example", "Hello Payload").ConfigureAwait(false);
```

Optionally, you can specify the desired quality of service (QoS) level for the publish. By default, the QoS level is set to `QualityOfService.AtMostOnceDelivery`.

```csharp
using HiveMQtt.MQTT5.Types; // For the QualityOfService enum

var publishResult = await client.PublishAsync("topic1/example", "Hello Payload", QualityOfService.ExactlyOnceDelivery);
```

## With Options

The `PublishMessageBuilder` class provides a convenient way to construct MQTT publish messages with various options and properties. It allows you to customize the topic, payload, quality of service (QoS) level, retain flag, and other attributes of the message.

```csharp
var publishMessage = new PublishMessageBuilder()
    .WithTopic("topic1/example")
    .WithPayload("{\"HiveMQ\": \"rocks\"}")
    .WithContentType("application/json")
    .WithResponseTopic("response/topic")
    .Build();

await client.PublishAsync(publishMessage).ConfigureAwait(false);
```

By using `PublishMessageBuilder`, you can easily construct MQTT publish messages with the desired properties and options. It provides a fluent and intuitive way to customize the topic, payload, QoS level, retain flag, and other attributes of the message.

### `PublishMessagebuilder` Reference

To illustrate _each and every possible call_ with `PublishMessageBuilder`, see the following example:

```csharp
var publishMessage = new PublishMessageBuilder()
    .WithTopic("topic1/example")
    .WithPayload("Hello, HiveMQtt!")
    .WithQualityOfServiceLevel(QualityOfService.AtLeastOnceDelivery)
    .WithRetainFlag(true)
    .WithPayloadFormatIndicator(MQTT5PayloadFormatIndicator.UTF8Encoded)
    .WithContentType("application/json")
    .WithResponseTopic("response/topic")
    .WithCorrelationData(Encoding.UTF8.GetBytes("correlation-data"))
    .WithUserProperty("property1", "value1")
    .WithUserProperties(new Dictionary<string, string> { { "property1", "value1" }, { "property2", "value2" } })
    .WithMessageExpiryInterval(3600)
    .WithSubscriptionIdentifier(123)
    .WithSubscriptionIdentifiers(1, 2, 3)
    .WithTopicAlias(456)
    .WithContentTypeAlias(789)
    .WithResponseTopicAlias(987)
    .Build();
```

## MQTT5PublishMessage

The [MQTT5PublishMessage](https://github.com/hivemq/hivemq-mqtt-client-dotnet/blob/main/Source/HiveMQtt/MQTT5/Types/MQTT5PublishMessage.cs) class represents the entirety of a publish message in MQTT.  If you construct this class directly, you can access all of the MQTT publish options such as `Retain`, `PayloadFormatIndicator`, `UserProperties` and so forth.

```csharp
var message = new MQTT5PublishMessage
{
    Topic = "topic1/example",
    Payload = Encoding.UTF8.GetBytes("Hello, World!"),
    QoS = QualityOfService.AtLeastOnceDelivery,
};

message.Retain = true;
message.UserProperties.Add("Client-Geo", "-33.8688, 151.2093");

var result = await client.PublishAsync(message);
```

For the full details, see the source code on [MQTT5PublishMessage](https://github.com/hivemq/hivemq-mqtt-client-dotnet/blob/main/Source/HiveMQtt/MQTT5/Types/MQTT5PublishMessage.cs).


## Return Value of Publish: `PublishResult`

The `PublishAsync` method in the HiveMQtt client library returns an instance of the `PublishResult` class. This object provides detailed information about the outcome of the publish operation.

## Quality of Service (QoS) and `PublishResult`

The information contained in the `PublishResult` object varies depending on the Quality of Service (QoS) level used for the publish operation.

* QoS Level 0 (`QualityOfService.AtMostOnceDelivery`): This level is often referred to as "fire-and-forget". It does not provide any acknowledgement of delivery, and as such, the `PublishResult` object does not contain any meaningful information.

* QoS Level 1 (`QualityOfService.AtLeastOnceDelivery`): This level ensures that the message is delivered at least once. The `PublishResult` object for this QoS level contains a `QoS1ReasonCode` property, which provides information about the outcome of the publish operation.

* QoS Level 2 (`QualityOfService.ExactlyOnceDelivery`): This level ensures that the message is delivered exactly once. The `PublishResult` object for this QoS level contains a `QoS2ReasonCode` property, which provides information about the outcome of the publish operation.

## Retrieving the Reason Code

For convenience, the `PublishResult` class provides a `ReasonCode` method. This method automatically retrieves the appropriate reason code (`QoS1ReasonCode` or `QoS2ReasonCode`) based on the QoS level of the publish operation. This allows you to easily access the outcome of the publish operation without having to check the QoS level manually.

## See Also

* [PublishMessageBuilder.cs](https://github.com/hivemq/hivemq-mqtt-client-dotnet/blob/main/Source/HiveMQtt/Client/PublishMessageBuilder.cs)
* [MQTT5PublishMessage.cs](https://github.com/hivemq/hivemq-mqtt-client-dotnet/blob/main/Source/HiveMQtt/MQTT5/Types/MQTT5PublishMessage.cs)
* [QualityOfService.cs](https://github.com/hivemq/hivemq-mqtt-client-dotnet/blob/main/Source/HiveMQtt/MQTT5/Types/QualityOfService.cs)
* [PublishResult.cs](https://github.com/hivemq/hivemq-mqtt-client-dotnet/blob/main/Source/HiveMQtt/Client/Results/PublishResult.cs)
