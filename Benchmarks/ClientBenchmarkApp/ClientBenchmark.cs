namespace ClientBenchmarkApp;

using System;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

using HiveMQtt.Client;
using HiveMQtt.Client.Options;
using HiveMQtt.MQTT5.ReasonCodes;
using HiveMQtt.MQTT5.Types;

public class ClientBenchmark
{
    private HiveMQClient client;

    public ClientBenchmark()
    {
        var options = new HiveMQClientOptions
        {
            Host = "127.0.0.1",
            Port = 1883,
        };

        this.client = new HiveMQClient(options);
        Console.WriteLine($"Connecting to {options.Host} on port {options.Port}...");
        this.client.ConnectAsync().Wait();
    }

    [Benchmark]
    public void Publish100QoS0Messages()
    {
        // Code to benchmark
        for (var i = 0; i < 100; i++)
        {
            // Add your code to benchmark here
            var result = this.SomeMethodToBenchmark();
        }
    }

    private int SomeMethodToBenchmark()
    {
        // Example method to benchmark
        return 42;
    }
}
