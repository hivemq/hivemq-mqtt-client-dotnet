# Subscribe & Publish

Once you set a message handler in `OnMessageReceived`, you can call `SubscribeAsync` to subscribe to one or more topics.

Note that you should always set the handler before subscribing when possible.  This avoids the case of lost messages as the broker can send messages immediately after `SubscribeAsync`.

```csharp
using HiveMQtt.Client;

// Connect
var client = new HiveMQClient();
var connectResult = await client.ConnectAsync().ConfigureAwait(false);

// Message Handler
client.OnMessageReceived += (sender, args) =>
{
    Console.WriteLine("Message Received: {}", args.PublishMessage.PayloadAsString)
};

// Subscribe
await client.SubscribeAsync("instrument/x9284/boston").ConfigureAwait(false);

await client.PublishAsync(
                "core/dynamic_graph/entity/227489", // Topic to publish to
                "{'2023': '👍'}"                    // Message to publish
                ).ConfigureAwait(false);
```
