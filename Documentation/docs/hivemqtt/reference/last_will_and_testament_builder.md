
# LastWillAndTestamentBuilder

The `LastWillAndTestamentBuilder` class provides a fluent API for constructing instances of `LastWillAndTestament`. It allows you to set various properties for creating a Last Will and Testament message, such as the topic, payload, Quality of Service (QoS), and additional metadata.

## Constructor

### LastWillAndTestamentBuilder()
Initializes a new instance of the `LastWillAndTestamentBuilder` class.

```csharp
LastWillAndTestamentBuilder();
```

**Example**:
```csharp
var builder = new LastWillAndTestamentBuilder();
```

---

## Methods

### WithTopic(string topic)
Sets the topic for the Last Will and Testament message.

```csharp
LastWillAndTestamentBuilder WithTopic(string topic);
```

**Parameters**:  
- `topic` *(string)*: The topic name.

**Example**:
```csharp
builder.WithTopic("client/disconnect");
```

---

### WithPayload(byte[] payload)
Sets the payload for the Last Will and Testament message as a byte array.

```csharp
LastWillAndTestamentBuilder WithPayload(byte[] payload);
```

**Parameters**:  
- `payload` *(byte[])*: The payload in bytes.

**Example**:
```csharp
builder.WithPayload(Encoding.UTF8.GetBytes("Disconnected unexpectedly"));
```

---

### WithPayload(string payload)
Sets the payload for the Last Will and Testament message as a string.

```csharp
LastWillAndTestamentBuilder WithPayload(string payload);
```

**Parameters**:  
- `payload` *(string)*: The payload as a string.

**Example**:
```csharp
builder.WithPayload("Disconnected unexpectedly");
```

---

### WithQualityOfServiceLevel(QualityOfService qos)
Sets the Quality of Service (QoS) level for the Last Will and Testament message.

```csharp
LastWillAndTestamentBuilder WithQualityOfServiceLevel(QualityOfService qos);
```

**Parameters**:  
- `qos` *(QualityOfService)*: The QoS level.

**Example**:
```csharp
builder.WithQualityOfServiceLevel(QualityOfService.AtLeastOnce);
```

---

### WithRetain(bool retain)
Sets the retain flag for the Last Will and Testament message.

```csharp
LastWillAndTestamentBuilder WithRetain(bool retain);
```

**Parameters**:  
- `retain` *(bool)*: Whether the message should be retained by the broker.

**Example**:
```csharp
builder.WithRetain(true);
```

---

### WithWillDelayInterval(long willDelayInterval)
Sets the delay before the Last Will and Testament message is sent.

```csharp
LastWillAndTestamentBuilder WithWillDelayInterval(long willDelayInterval);
```

**Parameters**:  
- `willDelayInterval` *(long)*: The delay in seconds.

**Example**:
```csharp
builder.WithWillDelayInterval(60); // Delay of 60 seconds
```

---

### WithPayloadFormatIndicator(MQTT5PayloadFormatIndicator payloadFormatIndicator)
Sets the payload format indicator.

```csharp
LastWillAndTestamentBuilder WithPayloadFormatIndicator(MQTT5PayloadFormatIndicator payloadFormatIndicator);
```

**Parameters**:  
- `payloadFormatIndicator` *(MQTT5PayloadFormatIndicator)*: The payload format indicator.

**Example**:
```csharp
builder.WithPayloadFormatIndicator(MQTT5PayloadFormatIndicator.Text);
```

---

### WithMessageExpiryInterval(long messageExpiryInterval)
Sets the expiry interval for the Last Will and Testament message.

```csharp
LastWillAndTestamentBuilder WithMessageExpiryInterval(long messageExpiryInterval);
```

**Parameters**:  
- `messageExpiryInterval` *(long)*: The expiry interval in seconds.

**Example**:
```csharp
builder.WithMessageExpiryInterval(3600); // 1 hour expiry
```

---

### WithContentType(string contentType)
Sets the content type for the Last Will and Testament message.

```csharp
LastWillAndTestamentBuilder WithContentType(string contentType);
```

**Parameters**:  
- `contentType` *(string)*: The content type.

**Example**:
```csharp
builder.WithContentType("text/plain");
```

---

### WithResponseTopic(string responseTopic)
Sets the response topic for the Last Will and Testament message.

```csharp
LastWillAndTestamentBuilder WithResponseTopic(string responseTopic);
```

**Parameters**:  
- `responseTopic` *(string)*: The response topic.

**Example**:
```csharp
builder.WithResponseTopic("response/topic");
```

---

### WithCorrelationData(byte[] correlationData)
Sets the correlation data for the Last Will and Testament message.

```csharp
LastWillAndTestamentBuilder WithCorrelationData(byte[] correlationData);
```

**Parameters**:  
- `correlationData` *(byte[])*: The correlation data.

**Example**:
```csharp
builder.WithCorrelationData(Encoding.UTF8.GetBytes("12345"));
```

---

### WithUserProperty(string key, string value)
Adds a single user property to the Last Will and Testament message.

```csharp
LastWillAndTestamentBuilder WithUserProperty(string key, string value);
```

**Parameters**:  
- `key` *(string)*: The key of the user property.  
- `value` *(string)*: The value of the user property.

**Example**:
```csharp
builder.WithUserProperty("reason", "client disconnect");
```

---

### WithUserProperties(Dictionary<string, string> properties)
Adds multiple user properties to the Last Will and Testament message.

```csharp
LastWillAndTestamentBuilder WithUserProperties(Dictionary<string, string> properties);
```

**Parameters**:  
- `properties` *(Dictionary<string, string>)*: A dictionary of user properties.

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
Builds the `LastWillAndTestament` instance.

```csharp
LastWillAndTestament Build();
```

**Description**:  
Constructs the `LastWillAndTestament` instance and validates the configuration.

**Example**:
```csharp
var lwt = builder
    .WithTopic("client/disconnect")
    .WithPayload("Disconnected unexpectedly")
    .WithQualityOfServiceLevel(QualityOfService.ExactlyOnce)
    .WithRetain(true)
    .Build();
```

