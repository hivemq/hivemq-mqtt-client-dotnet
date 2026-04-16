
# UnsubscribeOptions

The `UnsubscribeOptions` class defines the options for unsubscribing from topics in the HiveMQ MQTT client. This class allows you to specify the subscriptions to remove and include additional user properties for metadata.

## Constructor

### UnsubscribeOptions()
Initializes a new instance of the `UnsubscribeOptions` class.

```csharp
UnsubscribeOptions();
```

**Example**:
```csharp
var options = new UnsubscribeOptions();
```

---

## Properties

### Subscriptions
Gets or sets the list of subscriptions for this unsubscribe operation.

```csharp
List<Subscription> Subscriptions { get; set; }
```

**Description**:  
Specifies the topics and associated details to unsubscribe from.

**Example**:
```csharp
options.Subscriptions.Add(new Subscription("home/sensors/temperature", QualityOfService.AtLeastOnce));
```

---

### UserProperties
Gets or sets the user properties for the unsubscribe operation.

```csharp
Dictionary<string, string> UserProperties { get; set; }
```

**Description**:  
Defines custom key-value pairs to include in the unsubscribe request.

**Example**:
```csharp
options.UserProperties["reason"] = "No longer needed";
```

---

## Methods

### Validate()
Validates the options for correctness.

```csharp
void Validate();
```

**Description**:  
Ensures that the `UnsubscribeOptions` instance includes at least one valid subscription.

**Exceptions**:
- `HiveMQttClientException`: Thrown if no subscriptions are specified.

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

