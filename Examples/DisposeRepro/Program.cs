using HiveMQtt.Client;
using HiveMQtt.Client.Options;

Console.CancelKeyPress += (_, e) =>
{
    Console.WriteLine("CTRL+C pressed.");
    e.Cancel = false;
};

var options = new HiveMQClientOptions
{
    Host = "broker.hivemq.com",
    Port = 1883,
    ClientId = $"dispose-repro-{Guid.NewGuid():N}"
};

using var client = new HiveMQClient(options);

Console.WriteLine("Connecting...");
await client.ConnectAsync();

Console.WriteLine("Disconnecting...");
await client.DisconnectAsync();

Console.WriteLine("Disposing...");
client.Dispose();

Console.WriteLine("Disposed.");

await Task.Delay(1000);
