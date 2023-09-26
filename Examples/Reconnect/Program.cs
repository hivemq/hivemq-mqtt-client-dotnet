using HiveMQtt.Client;
using HiveMQtt.Client.Options;
using System.Text.Json;

var topic = "hivemqtt/waiting/game";

var options = new HiveMQClientOptions();
options.Host = "127.0.0.1";
options.Port = 1883;

var client = new HiveMQClient(options);

// Add handlers
// Message handler
client.OnMessageReceived += (sender, args) =>
{
    // Handle Message in args.PublishMessage
    Console.WriteLine($"--> Message Received: {args.PublishMessage.PayloadAsString}");
};

// This handler is called when the client is disconnected
client.AfterDisconnect += async (sender, args) =>
{
    var client = (HiveMQClient)sender;

    Console.WriteLine($"AfterDisconnect Handler called with args.CleanDisconnect={args.CleanDisconnect}.");

    // We've been disconnected
    if (args.CleanDisconnect)
    {
        Console.WriteLine("--> AfterDisconnectEventArgs indicate a clean disconnect.");
        Console.WriteLine("--> A clean disconnect was requested by either the client or the broker.");
    }
    else
    {
        Console.WriteLine("--> AfterDisconnectEventArgs indicate an unexpected disconnect.");
        Console.WriteLine("--> This could be due to a network outage, broker outage, or other issue.");
        Console.WriteLine("--> In this case we will attempt to reconnect periodically.");

        // We could have been disconnected for any number of reasons: network outage, broker outage, etc.
        // Here we loop with a backing off delay until we reconnect

        // Start with a 1 second delay and double each retry to a maximum of 10 seconds.
        var delay = 5000;
        var reconnectAttempts = 0;

        while (true)
        {
            await Task.Delay(delay).ConfigureAwait(false);
            reconnectAttempts++;

            if (reconnectAttempts > 3)
            {
                Console.WriteLine("--> Maximum reconnect attempts exceeded.  Exiting.");
                break;
            }

            try
            {
                Console.WriteLine($"--> Attempting to reconnect to broker.  This is attempt #{reconnectAttempts}.");
                var connectResult = await client.ConnectAsync().ConfigureAwait(false);

                if (connectResult.ReasonCode != HiveMQtt.MQTT5.ReasonCodes.ConnAckReasonCode.Success)
                {
                    Console.WriteLine($"--> Failed to connect: {connectResult.ReasonString}");

                    // Double the delay with each failed retry to a maximum of 30 seconds.
                    delay = Math.Min(delay * 2, 30000);
                    Console.WriteLine($"--> Will delay for {delay / 1000} seconds until next try.");
                }
                else
                {
                    Console.WriteLine("--> Reconnected successfully.");
                    break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"--> Failed to connect: {ex.Message}");

                // Double the delay with each failed retry to a maximum of 10 seconds.
                delay = Math.Min(delay * 2, 10000);
                Console.WriteLine($"--> Will delay for {delay / 1000} seconds until next try.");
            }
        }
    } // if (args.CleanDisconnect)

    Console.WriteLine("--> Exiting AfterDisconnect handler.");
};

// Attempt to connect to the broker
try
{
    var connectResult = await client.ConnectAsync().ConfigureAwait(false);
    if (connectResult.ReasonCode != HiveMQtt.MQTT5.ReasonCodes.ConnAckReasonCode.Success)
    {
        throw new Exception($"Failed to connect to broker: {connectResult.ReasonString}");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Failed to connect to broker: {ex.Message}");
    return;
}

// Subscribe to a topic
Console.WriteLine($"Subscribing to {topic}...");
await client.SubscribeAsync(topic).ConfigureAwait(false);

Console.WriteLine($"We are connected to the broker and will be waiting indefinitely for messages or a disconnect.");
Console.WriteLine($"--> Publish messages to {topic} and they will be printed.");
Console.WriteLine($"--> Shutdown/disconnect the broker and see the AfterDisconnect code execute.");

await Task.Delay(1000).ConfigureAwait(false);

// Publish a message
Console.WriteLine("Publishing a test message...");
var resultPublish = await client.PublishAsync(
    topic,
    JsonSerializer.Serialize(new
    {
        Command = "Hello",
    })
).ConfigureAwait(false);

while (true)
{
    await Task.Delay(2000).ConfigureAwait(false);
    Console.WriteLine("Press q exit...");
    if (Console.ReadKey().Key == ConsoleKey.Q)
    {
        Console.WriteLine("\n");
        break;
    }
}

Console.WriteLine("Disconnecting gracefully...");
await client.DisconnectAsync().ConfigureAwait(false);
