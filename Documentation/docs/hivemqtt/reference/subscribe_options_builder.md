
# SubscribeOptionsBuilder

The `SubscribeOptionsBuilder` class provides a fluent API for constructing `SubscribeOptions` instances in the HiveMQ MQTT client. It allows you to define topic subscriptions, user properties, and handlers in a structured way.

## Constructor

### SubscribeOptionsBuilder()
Initializes a new instance of the `SubscribeOptionsBuilder` class.

```csharp
SubscribeOptionsBuilder();
```

**Example**:
```csharp
var builder = new SubscribeOptionsBuilder();
```

---

## Methods

### WithSubscription(string topic, QualityOfService qos, bool? noLocal = null, bool? retainAsPublished = null, RetainHandling? retainHandling = RetainHandling.SendAtSubscribe, EventHandler<OnMessageReceivedEventArgs>? messageReceivedHandler = null)
Adds a subscription with detailed options.

```csharp
SubscribeOptionsBuilder WithSubscription(
    string topic,
    QualityOfService qos,
    bool? noLocal = null,
    bool? retainAsPublished = null,
    RetainHandling? retainHandling = RetainHandling.SendAtSubscribe,
    EventHandler<OnMessageReceivedEventArgs>? messageReceivedHandler = null
);
```

**Parameters**:  
- `topic` *(string)*: The topic name.  
- `qos` *(QualityOfService)*: The QoS level.  
- `noLocal` *(bool?)*: Indicates whether this client receives messages it publishes.  
- `retainAsPublished` *(bool?)*: Determines if the retain flag remains as published.  
- `retainHandling` *(RetainHandling?)*: Defines how retained messages are handled.  
- `messageReceivedHandler` *(EventHandler<OnMessageReceivedEventArgs>?)*: A handler for messages received on this subscription.

**Example**:
```csharp
builder.WithSubscription(
    "home/sensors/temperature",
    QualityOfService.AtLeastOnce,
    noLocal: true,
    retainAsPublished: false,
    retainHandling: RetainHandling.DoNotSendOnSubscribe,
    messageReceivedHandler: (sender, args) =>
    {
        Console.WriteLine($"Received message: {args.Message.PayloadAsString}");
    });
```

---

### WithSubscription(TopicFilter topicFilter, EventHandler<OnMessageReceivedEventArgs>? handler = null)
Adds a subscription using a `TopicFilter`.

```csharp
SubscribeOptionsBuilder WithSubscription(TopicFilter topicFilter, EventHandler<OnMessageReceivedEventArgs>? handler = null);
```

**Parameters**:  
- `topicFilter` *(TopicFilter)*: The topic filter for the subscription.  
- `handler` *(EventHandler<OnMessageReceivedEventArgs>?)*: A message handler for the subscription.

**Example**:
```csharp
builder.WithSubscription(
    new TopicFilter("home/sensors/humidity", QualityOfService.ExactlyOnce),
    (sender, args) =>
    {
        Console.WriteLine($"Humidity: {args.Message.PayloadAsString}");
    });
```

---

### WithSubscriptions(IEnumerable<TopicFilter> topicFilters)
Adds multiple subscriptions using a list of `TopicFilter` objects.

```csharp
SubscribeOptionsBuilder WithSubscriptions(IEnumerable<TopicFilter> topicFilters);
```

**Parameters**:  
- `topicFilters` *(IEnumerable<TopicFilter>)*: A collection of topic filters.

**Example**:
```csharp
builder.WithSubscriptions(new List<TopicFilter>
{
    new TopicFilter("home/sensors/temperature", QualityOfService.AtLeastOnce),
    new TopicFilter("home/sensors/humidity", QualityOfService.AtMostOnce)
});
```

---

### WithUserProperty(string key, string value)
Adds a single user property.

```csharp
SubscribeOptionsBuilder WithUserProperty(string key, string value);
```

**Parameters**:  
- `key` *(string)*: The key of the user property.  
- `value` *(string)*: The value of the user property.

**Example**:
```csharp
builder.WithUserProperty("key", "value");
```

---

### WithUserProperties(Dictionary<string, string> userProperties)
Adds multiple user properties.

```csharp
SubscribeOptionsBuilder WithUserProperties(Dictionary<string, string> userProperties);
```

**Parameters**:  
- `userProperties` *(Dictionary<string, string>)*: A dictionary of key-value pairs.

**Example**:
```csharp
builder.WithUserProperties(new Dictionary<string, string>
{
    { "key1", "value1" },
    { "key2", "value2" }
});
```

---

### Build()
Builds the `SubscribeOptions` object based on the provided settings.

```csharp
SubscribeOptions Build();
```

**Description**:  
Constructs the `SubscribeOptions` instance and validates the configuration.

**Example**:
```csharp
var options = builder
    .WithSubscription("home/sensors/temperature", QualityOfService.AtLeastOnce)
    .WithUserProperty("key", "value")
    .Build();

options.Validate();
```

