# PublishMessageBuilder

The `PublishMessageBuilder` class is a builder for constructing MQTT 5 publish messages in the HiveMQ client. 
This class provides a fluent API for setting various properties of the publish message.

## Constructor

```csharp
PublishMessageBuilder()
```
Initializes a new instance of the `PublishMessageBuilder` class.

---

## Methods

### WithPayload(byte[] payload)
Sets the payload of the publish message as a byte array.

```csharp
PublishMessageBuilder WithPayload(byte[] payload);
```

**Description**:  
Sets the payload to be published as a byte array.

**Example**:
```csharp
var builder = new PublishMessageBuilder()
    .WithPayload(new byte[] { 0x01, 0x02, 0x03 });
```

---

### WithPayload(string payload)
Sets the payload of the publish message as a UTF-8 encoded string.

```csharp
PublishMessageBuilder WithPayload(string payload);
```

**Description**:  
Sets the payload to be published as a string.

**Example**:
```csharp
var builder = new PublishMessageBuilder()
    .WithPayload("Hello, MQTT!");
```

---

### WithTopic(string topic)
Sets the topic of the publish message.

```csharp
PublishMessageBuilder WithTopic(string topic);
```

**Description**:  
Specifies the topic where the message will be published. Topics must be valid MQTT topic strings.

**Example**:
```csharp
var builder = new PublishMessageBuilder()
    .WithTopic("home/sensors/temperature");
```

**Exceptions**:  
- `ArgumentException`: If the topic is null, empty, or invalid.

---

### WithQualityOfService(QualityOfService qos)
Sets the Quality of Service (QoS) level of the publish message.

```csharp
PublishMessageBuilder WithQualityOfService(QualityOfService qos);
```

**Description**:  
Sets the reliability level for message delivery.

**Example**:
```csharp
var builder = new PublishMessageBuilder()
    .WithQualityOfService(QualityOfService.AtLeastOnce);
```

---

### WithRetain(bool retain)
Sets the retain flag of the publish message.

```csharp
PublishMessageBuilder WithRetain(bool retain);
```

**Description**:  
Determines whether the broker should retain the message for new subscribers.

**Example**:
```csharp
var builder = new PublishMessageBuilder()
    .WithRetain(true);
```

---

### WithDuplicate(bool duplicate)
Sets the duplicate flag of the publish message.

```csharp
PublishMessageBuilder WithDuplicate(bool duplicate);
```

**Description**:  
Indicates whether the message is a duplicate of a previous message.

**Example**:
```csharp
var builder = new PublishMessageBuilder()
    .WithDuplicate(true);
```

---

### WithResponseTopic(string responseTopic)
Sets the response topic for the publish message.

```csharp
PublishMessageBuilder WithResponseTopic(string responseTopic);
```

**Description**:  
Defines the response topic for the message.

**Example**:
```csharp
var builder = new PublishMessageBuilder()
    .WithResponseTopic("responses/temperature");
```

---

### WithCorrelationData(byte[] correlationData)
Sets the correlation data for the publish message as a byte array.

```csharp
PublishMessageBuilder WithCorrelationData(byte[] correlationData);
```

**Description**:  
Adds correlation data to the message.

**Example**:
```csharp
var builder = new PublishMessageBuilder()
    .WithCorrelationData(new byte[] { 0x01, 0x02 });
```

---

### WithCorrelationData(string correlationData)
Sets the correlation data for the publish message as a UTF-8 encoded string.

```csharp
PublishMessageBuilder WithCorrelationData(string correlationData);
```

**Description**:  
Adds correlation data as a string.

**Example**:
```csharp
var builder = new PublishMessageBuilder()
    .WithCorrelationData("session-1234");
```

---

### WithContentType(string contentType)
Sets the content type of the publish message.

```csharp
PublishMessageBuilder WithContentType(string contentType);
```

**Description**:  
Specifies the content type of the payload.

**Example**:
```csharp
var builder = new PublishMessageBuilder()
    .WithContentType("application/json");
```

---

### WithMessageExpiryInterval(int messageExpiryInterval)
Sets the message expiry interval of the publish message.

```csharp
PublishMessageBuilder WithMessageExpiryInterval(int messageExpiryInterval);
```

**Description**:  
Sets the expiry time for the message in seconds.

**Example**:
```csharp
var builder = new PublishMessageBuilder()
    .WithMessageExpiryInterval(3600);
```

---

### WithSubscriptionIdentifier(int subscriptionIdentifier)
Adds a subscription identifier to the publish message.

```csharp
PublishMessageBuilder WithSubscriptionIdentifier(int subscriptionIdentifier);
```

**Description**:  
Associates the message with a specific subscription.

**Example**:
```csharp
var builder = new PublishMessageBuilder()
    .WithSubscriptionIdentifier(1);
```

---

### WithSubscriptionIdentifiers(List<int> subscriptionIdentifiers)
Adds multiple subscription identifiers to the publish message.

```csharp
PublishMessageBuilder WithSubscriptionIdentifiers(List<int> subscriptionIdentifiers);
```

**Description**:  
Associates the message with multiple subscriptions.

**Example**:
```csharp
var builder = new PublishMessageBuilder()
    .WithSubscriptionIdentifiers(new List<int> { 1, 2, 3 });
```

---

### WithTopicAlias(int topicAlias)
Sets the topic alias of the publish message.

```csharp
PublishMessageBuilder WithTopicAlias(int topicAlias);
```

**Description**:  
Assigns a topic alias for efficient transmission.

**Example**:
```csharp
var builder = new PublishMessageBuilder()
    .WithTopicAlias(42);
```

---

### WithUserProperty(string key, string value)
Adds a user property to the publish message.

```csharp
PublishMessageBuilder WithUserProperty(string key, string value);
```

**Description**:  
Adds a custom key-value pair to the message.

**Example**:
```csharp
var builder = new PublishMessageBuilder()
    .WithUserProperty("key", "value");
```

---

### WithUserProperties(Dictionary<string, string> properties)
Adds multiple user properties to the publish message.

```csharp
PublishMessageBuilder WithUserProperties(Dictionary<string, string> properties);
```

**Description**:  
Adds multiple custom key-value pairs.

**Example**:
```csharp
var builder = new PublishMessageBuilder()
    .WithUserProperties(new Dictionary<string, string>
    {
        { "key1", "value1" },
        { "key2", "value2" }
    });
```

---

### WithPayloadFormatIndicator(MQTT5PayloadFormatIndicator indicator)
Sets the payload format indicator of the publish message.

```csharp
PublishMessageBuilder WithPayloadFormatIndicator(MQTT5PayloadFormatIndicator indicator);
```

**Description**:  
Specifies the format of the payload.

**Example**:
```csharp
var builder = new PublishMessageBuilder()
    .WithPayloadFormatIndicator(MQTT5PayloadFormatIndicator.Text);
```

---

## Build
Builds the publish message.

```csharp
MQTT5PublishMessage Build();
```

**Description**:  
Creates the final `MQTT5PublishMessage` instance.

**Example**:
```csharp
var message = new PublishMessageBuilder()
    .WithTopic("home/sensors/temperature")
    .WithPayload("22.5°C")
    .Build();
```
