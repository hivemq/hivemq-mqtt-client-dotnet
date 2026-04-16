# MQTT5PublishMessage

The `MQTT5PublishMessage` class represents an MQTT 5 publish message. This class provides various properties and methods for defining the message details, such as the topic, payload, Quality of Service (QoS), and additional metadata.

## Constructor

### MQTT5PublishMessage()
Initializes a new instance of the `MQTT5PublishMessage` class.

```csharp
MQTT5PublishMessage();
```

**Example**:
```csharp
var message = new MQTT5PublishMessage();
```

### MQTT5PublishMessage(string topic, QualityOfService? qos)
Initializes a new instance of the `MQTT5PublishMessage` class with the specified topic and Quality of Service level.

```csharp
MQTT5PublishMessage(string topic, QualityOfService? qos);
```

**Parameters**:
- `topic` *(string)*: The topic name.
- `qos` *(QualityOfService?)*: The QoS level.

**Example**:
```csharp
var message = new MQTT5PublishMessage("home/sensors/temperature", QualityOfService.AtLeastOnce);
```

---

## Properties

### Topic
Gets or sets the topic of the publish message.

```csharp
string? Topic { get; set; }
```

**Description**:  
The topic where the message will be published.

**Example**:
```csharp
message.Topic = "home/sensors/humidity";
```

---

### QoS
Gets or sets the Quality of Service level for the publish message.

```csharp
QualityOfService? QoS { get; set; }
```

**Description**:  
Specifies the QoS level for message delivery.

**Example**:
```csharp
message.QoS = QualityOfService.ExactlyOnce;
```

---

### PayloadFormatIndicator
Gets or sets the payload format indicator.

```csharp
MQTT5PayloadFormatIndicator? PayloadFormatIndicator { get; set; }
```

**Description**:  
Indicates the format of the payload.

**Example**:
```csharp
message.PayloadFormatIndicator = MQTT5PayloadFormatIndicator.Text;
```

---

### MessageExpiryInterval
Gets or sets the message expiry interval in seconds.

```csharp
int? MessageExpiryInterval { get; set; }
```

**Description**:  
The time after which the message expires.

**Example**:
```csharp
message.MessageExpiryInterval = 3600;
```

---

### TopicAlias
Gets or sets the topic alias.

```csharp
int? TopicAlias { get; set; }
```

**Description**:  
An alias for the topic to reduce message size.

**Example**:
```csharp
message.TopicAlias = 42;
```

---

### ResponseTopic
Gets or sets the response topic for the publish message.

```csharp
string? ResponseTopic { get; set; }
```

**Description**:  
Specifies the topic for response messages.

**Example**:
```csharp
message.ResponseTopic = "home/responses/humidity";
```

---

### CorrelationData
Gets or sets the correlation data for the publish message.

```csharp
byte[]? CorrelationData { get; set; }
```

**Description**:  
Data used to correlate the message with a request.

**Example**:
```csharp
message.CorrelationData = new byte[] { 0x01, 0x02 };
```

---

### UserProperties
Gets or sets the user properties of the publish message.

```csharp
Dictionary<string, string> UserProperties { get; set; }
```

**Description**:  
Custom key-value pairs to add metadata.

**Example**:
```csharp
message.UserProperties["key1"] = "value1";
```

---

### SubscriptionIdentifiers
Gets or sets the subscription identifiers associated with the message.

```csharp
List<int> SubscriptionIdentifiers { get; set; }
```

**Description**:  
Identifies the subscriptions matching this message.

**Example**:
```csharp
message.SubscriptionIdentifiers.Add(12345);
```

---

### ContentType
Gets or sets the content type of the payload.

```csharp
string? ContentType { get; set; }
```

**Description**:  
Indicates the type of the message payload.

**Example**:
```csharp
message.ContentType = "application/json";
```

---

### Payload
Gets or sets the payload as a byte array.

```csharp
byte[]? Payload { get; set; }
```

**Description**:  
The message payload in binary format.

**Example**:
```csharp
message.Payload = Encoding.UTF8.GetBytes("Hello, MQTT!");
```

---

### PayloadAsString
Gets or sets the payload as a string.

```csharp
string PayloadAsString { get; set; }
```

**Description**:  
The payload in string format.

**Example**:
```csharp
message.PayloadAsString = "Hello, MQTT!";
```

---

### Retain
Gets or sets the retain flag of the message.

```csharp
bool Retain { get; set; }
```

**Description**:  
Indicates whether the message should be retained by the broker.

**Example**:
```csharp
message.Retain = true;
```

---

### Duplicate
Gets or sets the duplicate flag of the message.

```csharp
bool Duplicate { get; set; }
```

**Description**:  
Indicates whether this message is a retransmission.

**Example**:
```csharp
message.Duplicate = true;
```

---

## Methods

### Validate()
Validates the properties of the message.

```csharp
void Validate();
```

**Description**:  
Checks if the message properties are valid.

**Example**:
```csharp
try
{
    message.Validate();
}
catch (HiveMQttClientException ex)
{
    Console.WriteLine($"Validation failed: {ex.Message}");
}
```
