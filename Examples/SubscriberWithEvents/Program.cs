namespace SubscriberWithEvents;

using System.Text;
using System.Text.Json;
using HiveMQtt.Client;
using HiveMQtt.Client.Options;
using HiveMQtt.MQTT5.Types;

public class Program
{
    public static bool ExitRequested { get; set; }
    public static int MessageCount { get; set; }
    public static int PublishesReceivedCount { get; set; }

    public static async Task Main(string[] args)
    {
        MessageCount = 0;
        PublishesReceivedCount = 0;

        // Subscribe to the CancelKeyPress event
        Console.CancelKeyPress += (sender, e) =>
        {
            // Handle Ctrl+C (SIGINT) by setting exitRequested flag
            e.Cancel = true; // Prevent process termination
            ExitRequested = true;
            Console.WriteLine("Ctrl+C (SIGINT) received. Press Ctrl+C again to exit immediately.");
        };

        var options = new HiveMQClientOptions
        {
            Host = "127.0.0.1",
            Port = 1883,
            CleanStart = true,
            ClientId = "SubscriberWithEvents",
        };

        var client = new HiveMQClient(options);

        // Message Handler
        //
        // It's important that this is setup before we connect to the broker
        // otherwise queued messages that are sent down may be lost.
        //
        client.OnMessageReceived += (sender, args) =>
        {
            MessageCount++;
        };

        // client.OnPublishReceived += (sender, args) =>
        // {
        //     PublishesReceivedCount++;
        // };

        // Connect to the broker
        var connectResult = await client.ConnectAsync().ConfigureAwait(false);
        if (connectResult.ReasonCode != HiveMQtt.MQTT5.ReasonCodes.ConnAckReasonCode.Success)
        {
            throw new IOException($"Failed to connect: {connectResult.ReasonString}");
        }

        // Subscribe to a topic
        var topic = "load/test/1";
        var subscribeResult = await client.SubscribeAsync(topic, QualityOfService.ExactlyOnceDelivery).ConfigureAwait(false);
        Console.WriteLine($"Subscribed to {topic}: {subscribeResult.Subscriptions[0].SubscribeReasonCode}");

        var message_number = 0;
        while (!ExitRequested)
        {
            await Task.Delay(1000).ConfigureAwait(false);
            Console.WriteLine($"Received {MessageCount} msgs/sec");
            // Console.WriteLine($"Received {MessageCount} msgs/sec & {PublishesReceivedCount} publishes/sec");
            MessageCount = 0;
            PublishesReceivedCount = 0;
        }
    }
}
