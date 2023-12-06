# Subscribe to Multiple Topics At Once With Varying QoS Levels

```csharp
using HiveMQtt.Client.Options;
using HiveMQtt.Client.Results;

var options = new SubscribeOptions();
options.TopicFilters.Add(new TopicFilter { Topic = "foo/boston", QoS = QualityOfService.AtLeastOnceDelivery });
options.TopicFilters.Add(new TopicFilter { Topic = "bar/landshut", QoS = QualityOfService.AtMostOnceDelivery });

var result = await client.SubscribeAsync(options);
```

* `result.Subscriptions` contains the list of subscriptions made with this call
* `client.Subscriptions` is updated with complete list of subscriptions made up to this point
* each `Subscription` object has a resulting `ReasonCode` that represents the Subscribe result in `result.Subscriptions[0].ReasonCode`
