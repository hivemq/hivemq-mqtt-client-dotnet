namespace ClientBenchmarkApp;

using System;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Engines;

using HiveMQtt.Client;
using HiveMQtt.Client.Options;
using HiveMQtt.MQTT5;
using HiveMQtt.MQTT5.ReasonCodes;
using HiveMQtt.MQTT5.Types;

[SimpleJob(RunStrategy.Monitoring, iterationCount: 10, id: "MonitoringJob")]
public class ClientBenchmarks
{
    private HiveMQClient client;

    private string smallPayload = new string(/*lang=json,strict*/ "{\"interference\": \"1029384\"}");

    [GlobalSetup]
    public void Setup()
    {
        var options = new HiveMQClientOptions
        {
            Host = "127.0.0.1",
            Port = 1883,
        };

        this.client = new HiveMQClient(options);
        Console.WriteLine($"Connecting to {options.Host} on port {options.Port}...");
        this.client.ConnectAsync().Wait();

        if (this.client.IsConnected())
        {
            Console.WriteLine("HiveMQ client connected.");
        }
        else
        {
            Console.WriteLine("Client failed to connect!");
        }
    }

    [GlobalCleanup]
    public void CleanUp()
    {
        Console.WriteLine("Disconnecting from HiveMQ...");
        this.client.DisconnectAsync().Wait();
    }

    // [Benchmark(Description = "Publish 100 QoS 0 messages to the broker.")]
    // public void Publish100QoS0Messages()
    // {
    //     var msg = new string(/*lang=json,strict*/ "{\"interference\": \"1029384\"}");
    //     for (var i = 0; i < 100; i++)
    //     {
    //         this.client.PublishAsync("benchmarks/Publish100QoS0Messages", msg).Wait();
    //     }
    // }

    [Benchmark(Description = "Publish a QoS 0 messages to the broker.")]
    public void PublishQoS0Message()
    {
        var task = this.client.PublishAsync("benchmarks/PublishQoS0Messages", this.smallPayload);
        task.Wait();
    }


    [Benchmark(Description = "Publish a QoS 1 messages to the broker.")]
    public void PublishQoS1Message()
    {
        var task = this.client.PublishAsync("benchmarks/PublishQoS1Messages", this.smallPayload, QualityOfService.AtLeastOnceDelivery);
        task.Wait();
    }

    [Benchmark(Description = "Publish a QoS 2 messages to the broker.")]
    public void PublishQoS2Message()
    {
        var task = this.client.PublishAsync("benchmarks/PublishQoS1Messages", this.smallPayload, QualityOfService.ExactlyOnceDelivery);
        task.Wait();
    }
}
