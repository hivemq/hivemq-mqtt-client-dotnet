using System.Text;
using System.Text.Json;
using HiveMQtt.Client;
using HiveMQtt.Client.Options;
using HiveMQtt.MQTT5.Types;

var options = new HiveMQClientOptions
{
    Host = "127.0.0.1",
    Port = 1883,
    CleanStart = false,  // <--- Set to false to receive messages queued on the broker
    ClientId = "ConnectReceiveAndPublish",
};

var client = new HiveMQClient(options);

// Message Handler
//
// It's important that this is setup before we connect to the broker
// otherwise queued messages that are sent down may be lost.
//
client.OnMessageReceived += (sender, args) =>
{
    var jsonString = args.PublishMessage.PayloadAsString;
    var jsonDocument = JsonDocument.Parse(jsonString);

    // Traverse the JSON document using the JsonElement API
    var root = jsonDocument.RootElement;
    var message_number = root.GetProperty("MessageNumber").GetInt32();

    Console.WriteLine($"Message Received; topic={args.PublishMessage.Topic}, message number={message_number}");
};

// Connect to the broker
var connectResult = await client.ConnectAsync().ConfigureAwait(false);
if (connectResult.ReasonCode != HiveMQtt.MQTT5.ReasonCodes.ConnAckReasonCode.Success)
{
    throw new Exception($"Failed to connect: {connectResult.ReasonString}");
}

// Subscribe to a topic
var topic = "hivemqtt/sendmessageonloop";
var subscribeResult = await client.SubscribeAsync(topic, QualityOfService.ExactlyOnceDelivery).ConfigureAwait(false);
Console.WriteLine($"Subscribed to {topic}: {subscribeResult.Subscriptions[0].SubscribeReasonCode}");

Console.WriteLine("Waiting for 10 seconds to receive messages queued on the topic...");
await Task.Delay(10000).ConfigureAwait(false);

Console.WriteLine(string.Empty);
Console.WriteLine("Now publishing a QoS2 message every 15 seconds. Press Q to quit.");

// Publish Loop - press q to exit
var message_number = 0;
while (true)
{
    message_number++;
    var payload = JsonSerializer.Serialize(new
    {
        Content = "ConnectReceiveAndPublish",
        MessageNumber = message_number,
    });

    var message = new MQTT5PublishMessage
    {
        Topic = topic,
        Payload = Encoding.ASCII.GetBytes(payload),
        QoS = QualityOfService.ExactlyOnceDelivery,
    };

    var resultPublish = await client.PublishAsync(message).ConfigureAwait(false);
    Console.WriteLine($"Published QoS2 message {message_number} to topic {topic}: {resultPublish.QoS2ReasonCode}");

    for (var x = 0; x < 5; x++)
    {
        await Task.Delay(3750).ConfigureAwait(false);

        if (Console.KeyAvailable)
        {
            if (Console.ReadKey().Key == ConsoleKey.Q)
            {
                Console.WriteLine("Disconnecting gracefully...");
                await client.DisconnectAsync().ConfigureAwait(false);
                return;
            }
        }
    }
}

