# Subscribe

# Set Your Message Handlers First

You can subscribe to one or many topics in MQTT.  But to do so, you must first set a message handler.

_Tip: Set your message handler before connecting to the MQTT broker as it may send messages before your handler is setup!_

```csharp
// Message Handler
client.OnMessageReceived += (sender, args) =>
{
    Console.WriteLine("Message Received: {}", args.PublishMessage.PayloadAsString)
};
```

or alternatively:

```csharp
private static void MessageHandler(object? sender, OnMessageReceivedEventArgs eventArgs)
{
    Console.WriteLine("Message Received: {}", eventArgs.PublishMessage.PayloadAsString)
}

client.OnMessageReceived += MessageHandler;
```

* See: [OnMessageReceivedEventArgs](https://github.com/hivemq/hivemq-mqtt-client-dotnet/blob/main/Source/HiveMQtt/Client/Events/OnMessageReceivedEventArgs.cs)

# Basic

To subscribe to a single topic with a Quality of Service level, use `SubscribeAsync` as follows.

```csharp
// Subscribe
var subscribeResult = await client.SubscribeAsync("instrument/x9284/boston", QualityOfService.AtLeastOnceDelivery).ConfigureAwait(false);

assert subscribeResult.Subscriptions.Length() == 1
assert subscribeResult.Subscriptions[0].SubscribeReasonCode == SubAckReasonCode.GrantedQoS1
```

# Using `SubscribeOptions`

To utilize the complete set of options for `SubscribeAsync`, create a `SubscribeOptions` object.

```csharp
var topic1 = "instrument/x9284/boston"
var topic2 = "instrument/x9284/austin"
var qos = QualityOfService.AtLeastOnceDelivery;

var subscribeOptions = new SubscribeOptions();

var tf1 = new TopicFilter(topic1, qos);
var tf2 = new TopicFilter(topic2, qos);

subscribeOptions.TopicFilters.Add(tf1);
subscribeOptions.TopicFilters.Add(tf2);

subscribeOptions.UserProperties.Add("Client-Geo", "-33.8688, 151.2093");

var result = await client.SubscribeAsync(subscribeOptions).ConfigureAwait(false);
```

# See Also

* [TopicFilter](https://github.com/hivemq/hivemq-mqtt-client-dotnet/blob/main/Source/HiveMQtt/MQTT5/Types/TopicFilter.cs)
* [SubscribeOptions](https://github.com/hivemq/hivemq-mqtt-client-dotnet/blob/main/Source/HiveMQtt/Client/Options/SubscribeOptions.cs)
* [SubscribeResult](https://github.com/hivemq/hivemq-mqtt-client-dotnet/blob/main/Source/HiveMQtt/Client/Results/SubscribeResult.cs)
* [MQTT Topics, Wildcards, & Best Practices](https://www.hivemq.com/blog/mqtt-essentials-part-5-mqtt-topics-best-practices/)
* [MQTT Topic Tree & Topic Matching](https://www.hivemq.com/article/mqtt-topic-tree-matching-challenges-best-practices-explained/)
