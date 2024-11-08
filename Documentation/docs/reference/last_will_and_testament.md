
# LastWillAndTestament

The `LastWillAndTestament` class represents a Last Will and Testament message in the MQTT protocol. This message is sent by the broker on behalf of a client if the client disconnects unexpectedly. It contains details such as the topic, payload, Quality of Service (QoS), and other optional properties.

## Constructors

### LastWillAndTestament(string topic, string payload, QualityOfService qos = QualityOfService.AtMostOnceDelivery, bool retain = false)
Initializes a new instance of the `LastWillAndTestament` class with a UTF-8 encoded payload.

```csharp
LastWillAndTestament(string topic, string payload, QualityOfService qos = QualityOfService.AtMostOnceDelivery, bool retain = false);
```

**Parameters**:  
- `topic` *(string)*: The topic of the message.  
- `payload` *(string)*: The payload in UTF-8 encoded format.  
- `qos` *(QualityOfService)*: The QoS level (default: AtMostOnceDelivery).  
- `retain` *(bool)*: Whether the message should be retained by the broker.

**Example**:
```csharp
var lwt = new LastWillAndTestament("disconnect/topic", "Client disconnected", QualityOfService.AtLeastOnce, true);
```

---

### LastWillAndTestament(string topic, byte[] payload, QualityOfService qos = QualityOfService.AtMostOnceDelivery, bool retain = false)
Initializes a new instance of the `LastWillAndTestament` class with a binary payload.

```csharp
LastWillAndTestament(string topic, byte[] payload, QualityOfService qos = QualityOfService.AtMostOnceDelivery, bool retain = false);
```

**Parameters**:  
- `topic` *(string)*: The topic of the message.  
- `payload` *(byte[])*: The payload in binary format.  
- `qos` *(QualityOfService)*: The QoS level (default: AtMostOnceDelivery).  
- `retain` *(bool)*: Whether the message should be retained by the broker.

**Example**:
```csharp
var lwt = new LastWillAndTestament("disconnect/topic", Encoding.UTF8.GetBytes("Client disconnected"), QualityOfService.ExactlyOnce, true);
```

---

## Properties

### Topic
Gets or sets the topic of the message.

```csharp
string Topic { get; set; }
```

**Example**:
```csharp
lwt.Topic = "status/update";
```

---

### Payload
Gets or sets the payload as a byte array.

```csharp
byte[]? Payload { get; set; }
```

**Example**:
```csharp
lwt.Payload = Encoding.UTF8.GetBytes("Disconnected");
```

---

### PayloadAsString
Gets or sets the payload as a UTF-8 encoded string.

```csharp
string PayloadAsString { get; set; }
```

**Example**:
```csharp
lwt.PayloadAsString = "Disconnected";
```

---

### QoS
Gets or sets the Quality of Service level.

```csharp
QualityOfService QoS { get; set; }
```

**Example**:
```csharp
lwt.QoS = QualityOfService.ExactlyOnce;
```

---

### Retain
Gets or sets whether the message should be retained by the broker.

```csharp
bool Retain { get; set; }
```

**Example**:
```csharp
lwt.Retain = true;
```

---

### WillDelayInterval
Gets or sets the delay before the Last Will and Testament is sent.

```csharp
long? WillDelayInterval { get; set; }
```

**Example**:
```csharp
lwt.WillDelayInterval = 60; // Delay of 60 seconds
```

---

### MessageExpiryInterval
Gets or sets the lifetime of the message in seconds.

```csharp
long? MessageExpiryInterval { get; set; }
```

**Example**:
```csharp
lwt.MessageExpiryInterval = 3600; // Expires in 1 hour
```

---

### ContentType
Gets or sets the content type of the payload.

```csharp
string? ContentType { get; set; }
```

**Example**:
```csharp
lwt.ContentType = "text/plain";
```

---

### ResponseTopic
Gets or sets the topic to which the client should publish a response.

```csharp
string? ResponseTopic { get; set; }
```

**Example**:
```csharp
lwt.ResponseTopic = "response/topic";
```

---

### CorrelationData
Gets or sets the correlation data for the message.

```csharp
byte[]? CorrelationData { get; set; }
```

**Example**:
```csharp
lwt.CorrelationData = Encoding.UTF8.GetBytes("correlation-id");
```

---

### UserProperties
Gets or sets a dictionary of user properties for the message.

```csharp
Dictionary<string, string> UserProperties { get; set; }
```

**Example**:
```csharp
lwt.UserProperties["key"] = "value";
```

---

## Methods

### Validate()
Validates the `LastWillAndTestament` instance for correctness.

```csharp
bool Validate();
```

**Exceptions**:  
- `HiveMQttClientException`: Thrown if the topic or payload is invalid.

**Example**:
```csharp
try
{
    lwt.Validate();
}
catch (HiveMQttClientException ex)
{
    Console.WriteLine($"Validation failed: {ex.Message}");
}
```

