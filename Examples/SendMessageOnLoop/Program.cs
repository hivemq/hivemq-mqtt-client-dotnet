using System.Text;
using System.Text.Json;
using HiveMQtt.Client;
using HiveMQtt.Client.Options;
using HiveMQtt.MQTT5.Types;
using HiveMQtt.Client.Exceptions;

// Connect to localhost:1883.  Change for brokers elsewhere.
// Run HiveMQ CE locally: docker run --name hivemq-ce -d -p 1883:1883 hivemq/hivemq-ce
//
var options = new HiveMQClientOptions
{
    Host = "127.0.0.1",
    Port = 1883,
    CleanStart = false,
    ClientId = "SendMessageOnLoop",
};

var client = new HiveMQClient(options);


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

        // Start with a small delay and double it on each retry up to a maximum value
        var delay = 5000;
        var reconnectAttempts = 0;
        var maxReconnectAttempts = 15;

        while (true)
        {
            await Task.Delay(delay).ConfigureAwait(false);
            reconnectAttempts++;

            try
            {
                Console.WriteLine($"--> Attempting to reconnect to broker.  This is attempt #{reconnectAttempts}.");
                var connectResult = await client.ConnectAsync().ConfigureAwait(false);

                if (connectResult.ReasonCode != HiveMQtt.MQTT5.ReasonCodes.ConnAckReasonCode.Success)
                {
                    Console.WriteLine($"--> Failed to connect: {connectResult.ReasonString}");

                    // Double the delay with each failed retry to a maximum
                    delay = Math.Min(delay * 2, 60000);
                    Console.WriteLine($"--> Will delay for {delay / 1000} seconds until next try.");
                }
                else
                {
                    Console.WriteLine("--> Reconnected successfully.");
                    break;
                }
            }
            catch (HiveMQttClientException ex)
            {
                Console.WriteLine($"--> Failed to reconnect: {ex.Message}");

                if (reconnectAttempts > maxReconnectAttempts)
                {
                    Console.WriteLine("--> Maximum reconnect attempts exceeded.  Exiting.");
                    break;
                }

                // Double the delay with each failed retry to a maximum
                delay = Math.Min(delay * 2, 60000);
                Console.WriteLine($"--> Will delay for {delay / 1000} seconds until next try.");
            }
        }
    } // if (args.CleanDisconnect)

    Console.WriteLine("--> Exiting AfterDisconnect handler.");
};

// Connect to the broker
var connectResult = await client.ConnectAsync().ConfigureAwait(false);
if (connectResult.ReasonCode != HiveMQtt.MQTT5.ReasonCodes.ConnAckReasonCode.Success)
{
    throw new Exception($"Failed to connect: {connectResult.ReasonString}");
}

// Example Settings
var wait = 1000;  // The delay between each message
Console.WriteLine($"Starting {wait / 1000} second loop... (press q to exit)");

// Topic to send messages to
var topic = "hivemqtt/sendmessageonloop";

// Counter for messages sent
var message_number = 0;

// Main loop - press q to exit
// Send QoS 2 messages to the broker
//
while (true)
{
    if (client.IsConnected())
    {
        message_number++;
        var payload = JsonSerializer.Serialize(new
        {
            Content = "SendMessageOnLoop",
            MessageNumber = message_number,
        });

        var message = new MQTT5PublishMessage
        {
            Topic = topic,
            Payload = Encoding.ASCII.GetBytes(payload),
            QoS = QualityOfService.ExactlyOnceDelivery,
        };

        var resultPublish = await client.PublishAsync(message).ConfigureAwait(false);
        Console.WriteLine($"Published message {message_number} to topic {topic}: {resultPublish.QoS2ReasonCode}");
    }
    else
    {
        Console.WriteLine("Client is not connected.  Standing by...");
    }

    await Task.Delay(wait).ConfigureAwait(false);
}

Console.WriteLine("Disconnecting gracefully...");
await client.DisconnectAsync().ConfigureAwait(false);
