
# UnsubscribeOptionsBuilder

The `UnsubscribeOptionsBuilder` class provides a fluent API for constructing `UnsubscribeOptions` instances in the HiveMQ MQTT client. This builder allows you to define which subscriptions to remove and to include additional user properties.

## Constructor

### UnsubscribeOptionsBuilder()
Initializes a new instance of the `UnsubscribeOptionsBuilder` class.

```csharp
UnsubscribeOptionsBuilder();
```

**Example**:
```csharp
var builder = new UnsubscribeOptionsBuilder();
```

---

## Methods

### WithSubscription(Subscription subscription)
Adds a single subscription to the `UnsubscribeOptions`.

```csharp
UnsubscribeOptionsBuilder WithSubscription(Subscription subscription);
```

**Parameters**:  
- `subscription` *(Subscription)*: The topic subscription to unsubscribe from.

**Example**:
```csharp
builder.WithSubscription(new Subscription("home/sensors/temperature", QualityOfService.AtLeastOnce));
```

---

### WithSubscriptions(IEnumerable<Subscription> subscriptions)
Adds multiple subscriptions to the `UnsubscribeOptions`.

```csharp
UnsubscribeOptionsBuilder WithSubscriptions(IEnumerable<Subscription> subscriptions);
```

**Parameters**:  
- `subscriptions` *(IEnumerable<Subscription>)*: A collection of topic subscriptions to unsubscribe from.

**Example**:
```csharp
builder.WithSubscriptions(new List<Subscription>
{
    new Subscription("home/sensors/temperature", QualityOfService.AtLeastOnce),
    new Subscription("home/sensors/humidity", QualityOfService.AtMostOnce)
});
```

---

### WithUserProperty(string key, string value)
Adds a single user property to the unsubscribe request.

```csharp
UnsubscribeOptionsBuilder WithUserProperty(string key, string value);
```

**Parameters**:  
- `key` *(string)*: The key of the user property.  
- `value` *(string)*: The value of the user property.

**Example**:
```csharp
builder.WithUserProperty("reason", "No longer needed");
```

---

### WithUserProperties(Dictionary<string, string> userProperties)
Adds multiple user properties to the unsubscribe request.

```csharp
UnsubscribeOptionsBuilder WithUserProperties(Dictionary<string, string> userProperties);
```

**Parameters**:  
- `userProperties` *(Dictionary<string, string>)*: A dictionary of key-value pairs representing user properties.

**Example**:
```csharp
builder.WithUserProperties(new Dictionary<string, string>
{
    { "reason", "Cleaning up subscriptions" },
    { "requester", "Admin" }
});
```

---

### Build()
Constructs the `UnsubscribeOptions` instance based on the provided settings.

```csharp
UnsubscribeOptions Build();
```

**Description**:  
Validates the builder configuration and creates an instance of `UnsubscribeOptions`.

**Example**:
```csharp
var options = builder
    .WithSubscription(new Subscription("home/sensors/temperature", QualityOfService.AtLeastOnce))
    .WithUserProperty("reason", "No longer needed")
    .Build();

options.Validate();
```

