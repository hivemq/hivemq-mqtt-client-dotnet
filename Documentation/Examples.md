
## How to Wait on an Event

Use a `TaskCompletionSource` in your event handlers to wait for events to complete.

```c#
// The TaskCompletionSource setup
var taskCompletionSource = new TaskCompletionSource<bool>();

// Event handler that sets the result of the `TaskCompletionSource`
client.AfterDisconnect += (sender, args) =>
{
    // Do the AfterDisconnect work for your application
    //
    // Mark the taskCompletionSource as completed
    taskCompletionSource.SetResult(true);
};

// Connect
var result = await client.ConnectAsync().ConfigureAwait(false);
//
// Insert application work here...
//
// and Disconnect
await client.DisconnectAsync().ConfigureAwait(false);

// Wait for the AfterDisconnect event handler to finish
// ...with a 5 second timeout as a hang safety
await taskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(5)).ConfigureAwait(false);
```

#### Subscribe to Multiple Topics At Once With Varying QoS Levels

```c#
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

