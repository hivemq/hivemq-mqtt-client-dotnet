# How to set a Last Will & Testament

The Last Will and Testament support of MQTT can be used to notify subscribers that your client is offline.

For a more in-depth explanation, see [What is MQTT Last Will and Testament (LWT)? – MQTT Essentials: Part 9](https://www.hivemq.com/blog/mqtt-essentials-part-9-last-will-and-testament/).

```csharp
// Specify the Last Will and Testament specifics in HiveMQClientOptions
var options = new HiveMQClientOptions
{
    LastWillAndTestament = new LastWillAndTestament("custom/lastwill/topic", QualityOfService.AtLeastOnceDelivery, "last will message"),
};

// Optionally set extended properties on the Last Will and Testament message
options.LastWillAndTestament.WillDelayInterval = 1;
options.LastWillAndTestament.PayloadFormatIndicator = 1;
options.LastWillAndTestament.MessageExpiryInterval = 100;
options.LastWillAndTestament.ContentType = "application/text";
options.LastWillAndTestament.ResponseTopic = "response/topic";
options.LastWillAndTestament.CorrelationData = new byte[] { 1, 2, 3, 4, 5 };
options.LastWillAndTestament.UserProperties.Add("userPropertyKey", "userPropertyValue");

// ConnectAsync will transmit the Last Will and Testament configuration.
var client = new HiveMQClient(options);
connectResult = await client.ConnectAsync().ConfigureAwait(false);

// The Last Will and Testament message will be sent to the "custom/lastwill/topic" topic if your clients get
// unexpectedly disconnected or alternatively, if your client disconnects with `DisconnectWithWillMessage`
var disconnectOptions = new DisconnectOptions { ReasonCode = DisconnectReasonCode.DisconnectWithWillMessage };
var disconnectResult = await client.DisconnectAsync(disconnectOptions).ConfigureAwait(false);
``````

Because the client above disconnected with `DisconnectReasonCode.DisconnectWithWillMessage`, subscribers to the `last/will` topic will receive the Last Will and Testament message as specified above.
