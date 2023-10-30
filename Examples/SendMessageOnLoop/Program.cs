using System.Text;
using System.Text.Json;
using HiveMQtt.Client;
using HiveMQtt.Client.Options;
using HiveMQtt.MQTT5.Types;

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

// Connect to the broker
var connectResult = await client.ConnectAsync().ConfigureAwait(false);
if (connectResult.ReasonCode != HiveMQtt.MQTT5.ReasonCodes.ConnAckReasonCode.Success)
{
    throw new Exception($"Failed to connect: {connectResult.ReasonString}");
}

// Example Settings
var wait = 50;  // The delay between each message
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

    await Task.Delay(wait).ConfigureAwait(false);

    if (Console.KeyAvailable)
    {
        if (Console.ReadKey().Key == ConsoleKey.Q)
        {
            break;
        }
    }
}

Console.WriteLine("Disconnecting gracefully...");
await client.DisconnectAsync().ConfigureAwait(false);
