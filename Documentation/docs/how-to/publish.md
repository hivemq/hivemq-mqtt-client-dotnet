# Publish

# Simple

The simple way to publish a message is to use the following API:

```csharp
await client.PublishAsync(
                "core/dynamic_graph/entity/227489", // Topic to publish to
                "{'2023': '👍'}",                   // Message to publish
                QualityOfService.AtMostOnceDelivery
                ).ConfigureAwait(false);

```

The 3 arguments are:

1. The topic to publish to (`string`)
2. The message to publish (`string` or `byte[]`)
3. Quality of Service level (defaults to `AtMostOnceDelivery`)

For the Quality of Service, see the [QualityOfService enum](https://github.com/hivemq/hivemq-mqtt-client-dotnet/blob/main/Source/HiveMQtt/MQTT5/Types/QualityOfService.cs).

But if you want more control and extended options for a publish, see the next section.

# MQTT5PublishMessage

The [MQTT5PublishMessage](https://github.com/hivemq/hivemq-mqtt-client-dotnet/blob/main/Source/HiveMQtt/MQTT5/Types/MQTT5PublishMessage.cs) class represents the entirety of a publish message in MQTT.  If you construct this class directly, you can access all of the MQTT publish options such as `Retain`, `PayloadFormatIndicator`, `UserProperties` and so forth.

```csharp
var message = new MQTT5PublishMessage
{
    Topic = topic,
    Payload = Encoding.ASCII.GetBytes(payload),
    QoS = qos,
};

message.Retain = True
message.UserProperties.Add("Client-Geo", "-33.8688, 151.2093");

var result = await client.PublishAsync(message);
```

For the full details, see the source code on [MQTT5PublishMessage](https://github.com/hivemq/hivemq-mqtt-client-dotnet/blob/main/Source/HiveMQtt/MQTT5/Types/MQTT5PublishMessage.cs).

# PublishMessageBuilder

The `PublishMessageBuilder` class provides a convenient way to construct MQTT publish messages with various options and properties. It allows you to customize the topic, payload, quality of service (QoS) level, retain flag, and other attributes of the message.

```csharp
var publishMessage = new PublishMessageBuilder().
                            WithTopic("topic1/example").
                            WithPayload("{'HiveMQ': '👍'}").
                            WithContentType("application/json")
                            WithResponseTopic("response/topic")
                            Build();

await client.PublishAsync(publishMessage).ConfigureAwait(false);
```

By using `PublishMessageBuilder`, you can easily construct MQTT publish messages with the desired properties and options. It provides a fluent and intuitive way to customize the topic, payload, QoS level, retain flag, and other attributes of the message.

# Publish Return Value: `PublishResult`

The `PublishAsync` method returns a `PublishResult` object.

For `QualityOfService.AtMostOnceDelivery`, since it's a "fire-and-forget" method, it doesn't contain any useful information.

For `QualityOfService.AtLeastOnceDelivery` (QoS level 1) and `QualityOfService.ExactlyOnceDelivery` (QoS level 2), the `PublishResult` contains `PublishResult.QoS1ReasonCode` and `PublishResult.QoS2ReasonCode` respectfully.

For ease of use, you can call `PublishResult.ReasonCode()` to retrieve the appropriate result code automatically.

# See Also

* [MQTT5PublishMessage](https://github.com/hivemq/hivemq-mqtt-client-dotnet/blob/main/Source/HiveMQtt/MQTT5/Types/MQTT5PublishMessage.cs)
* [QualityOfService](https://github.com/hivemq/hivemq-mqtt-client-dotnet/blob/main/Source/HiveMQtt/MQTT5/Types/QualityOfService.cs)
* [PublishResult](https://github.com/hivemq/hivemq-mqtt-client-dotnet/blob/main/Source/HiveMQtt/Client/Results/PublishResult.cs)
