# How to set a Last Will & Testament

The Last Will and Testament support of MQTT can be used to notify subscribers that your client is offline.

For a more in-depth explanation, see [What is MQTT Last Will and Testament (LWT)? – MQTT Essentials: Part 9](https://www.hivemq.com/blog/mqtt-essentials-part-9-last-will-and-testament/).

## Using LastWillAndTestament

This example instantiates the `LastWillAndTestament` in the `HiveMQClientOption` class.  This is then sent to the broker in the `connect` operation.

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
```

Because the client above disconnected with `DisconnectReasonCode.DisconnectWithWillMessage`, subscribers to the `last/will` topic will receive the Last Will and Testament message as specified above.

## The LastWillAndTestament Builder Class

As an ease-of-use alternative, the HiveMQtt client offers a `LastWillAndTestamentBuilder` class to more easily instantiate a `LastWillAndTestament` class.

```csharp
var lwt = new LastWillAndTestamentBuilder()
            .WithTopic("last/will")
            .WithPayload("last will message")
            .WithQualityOfServiceLevel(QualityOfService.AtLeastOnceDelivery)
            .WithContentType("application/text")
            .WithResponseTopic("response/topic")
            .WithCorrelationData(new byte[] { 1, 2, 3, 4, 5 })
            .WithPayloadFormatIndicator(MQTT5PayloadFormatIndicator.UTF8Encoded)
            .WithMessageExpiryInterval(100)
            .WithUserProperty("userPropertyKey", "userPropertyValue")
            .WithWillDelayInterval(1)
            .Build();

// Setup & Connect the client with LWT
var options = new HiveMQClientOptions
{
    LastWillAndTestament = lwt,
};

var client = new HiveMQClient(options);
connectResult = await client.ConnectAsync().ConfigureAwait(false);
```

## See Also

  * [What is MQTT Last Will and Testament (LWT)? – MQTT Essentials: Part 9](https://www.hivemq.com/blog/mqtt-essentials-part-9-last-will-and-testament/)
  * [LastWillAndTestamentBuilder.cs](https://github.com/hivemq/hivemq-mqtt-client-dotnet/blob/main/Source/HiveMQtt/Client/LastWillAndTestamentBuilder.cs)
  * [LastWillAndTestament.cs](https://github.com/hivemq/hivemq-mqtt-client-dotnet/blob/main/Source/HiveMQtt/Client/LastWillAndTestament.cs)
