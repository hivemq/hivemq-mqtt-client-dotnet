
# SubscribeOptions

The `SubscribeOptions` class is used to define options for subscribing to topics in the HiveMQ MQTT client. It allows you to specify topic filters, user properties, subscription identifiers, and handlers for processing messages.

## Constructor

### SubscribeOptions()
Initializes a new instance of the `SubscribeOptions` class.

```csharp
SubscribeOptions();
```

**Example**:
```csharp
var options = new SubscribeOptions();
```

---

## Properties

### SubscriptionIdentifier
Gets or sets the subscription identifier.

```csharp
int? SubscriptionIdentifier { get; set; }
```

**Description**:  
A unique identifier for the subscription.

**Example**:
```csharp
options.SubscriptionIdentifier = 12345;
```

---

### UserProperties
Gets or sets the user properties for the subscription.

```csharp
Dictionary<string, string> UserProperties { get; set; }
```

**Description**:  
Custom key-value pairs for metadata associated with the subscription.

**Example**:
```csharp
options.UserProperties["key"] = "value";
```

---

### TopicFilters
Gets or sets the topic filters for the subscription.

```csharp
List<TopicFilter> TopicFilters { get; set; }
```

**Description**:  
Specifies the list of topic filters to subscribe to.

**Example**:
```csharp
options.TopicFilters.Add(new TopicFilter { Topic = "home/sensors/temperature", QoS = QualityOfService.AtLeastOnce });
```

---

### Handlers
Gets or sets the handlers for message processing.

```csharp
Dictionary<string, EventHandler<OnMessageReceivedEventArgs>> Handlers { get; set; }
```

**Description**:  
Defines per-topic message processing callbacks. The key is the topic filter, and the value is the handler function.

**Example**:
```csharp
options.Handlers["home/sensors/temperature"] = (sender, args) =>
{
    Console.WriteLine($"Received message: {args.Message.PayloadAsString}");
};
```

---

## Methods

### Validate()
Validates the options for correctness.

```csharp
void Validate();
```

**Description**:  
Ensures that the `SubscribeOptions` instance contains at least one valid topic filter.

**Exceptions**:
- `HiveMQttClientException`: Thrown if no topic filters are specified.

**Example**:
```csharp
try
{
    options.Validate();
}
catch (HiveMQttClientException ex)
{
    Console.WriteLine($"Validation failed: {ex.Message}");
}
```

