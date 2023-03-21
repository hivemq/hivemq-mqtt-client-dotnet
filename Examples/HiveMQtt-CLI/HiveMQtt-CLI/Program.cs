using HiveMQtt.Client;
using HiveMQtt.Client.Options;
using HiveMQtt.MQTT5.ReasonCodes;
using HiveMQtt.MQTT5.Types;

// Built from the Microsoft CLI Template.
// See https://aka.ms/new-console-template for more information

// Example Options to connect to HiveMQ cloud
// These values could instead be set with:
//    Environment.GetEnvironmentVariable("HIVEMQTTCLI_HOST")
// if you set your environment variables externally.
var options = new HiveMQClientOptions
{
    Host = "b8212ae75b11f4y2abs254bdea608173b.s1.eu.hivemq.cloud",
    Port = 8883,
    UseTLS = true,
    UserName = 'myusername',
    Password = "mypassword',

};

// Example HiveMQClientOptions to connect to a local MQTT broker without authentication
/*
var options = new HiveMQClientOptions
{
    Host = "127.0.0.1",
    Port = 8883,
    UseTLS = true,
};
*/

var client = new HiveMQClient(options);

if (options.UseTLS)
{
    Console.WriteLine($"Connecting to {options.Host} on port {options.Port} using TLS...");
}
else
{
    Console.WriteLine($"Connecting to {options.Host} on port {options.Port} without TLS...");
}

var connectResult = await client.ConnectAsync().ConfigureAwait(false);

if (connectResult.ReasonCode == ConnAckReasonCode.Success)
{
    Console.WriteLine("Connect successful!");
}
else
{
    // FIXME: Add ToString
    Console.WriteLine($"Connect failed: {connectResult}");
    Environment.Exit(-1);
}

Console.WriteLine("Publishing a QoS level 2 message...");

var msg = new string(/*lang=json,strict*/
"{\"interference\": \"1029384\"}");
var result = await client.PublishAsync("tests/MostBasicPublishWithQoS2Async", msg, QualityOfService.ExactlyOnceDelivery).ConfigureAwait(false);

Console.WriteLine(result);

Console.WriteLine("Disconnecting from broker...");
var disconnectResult = await client.DisconnectAsync().ConfigureAwait(false);
