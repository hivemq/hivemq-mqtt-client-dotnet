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
public class ClientBenchmarks : IDisposable
{
    private readonly string smallPayload = new string(/*lang=json,strict*/ "{\"interference\": \"1029384\"}");

    private HiveMQClient client;

    [GlobalSetup]
    public async Task SetupAsync()
    {
        var options = new HiveMQClientOptions
        {
            Host = "127.0.0.1",
            Port = 1883,
        };

        this.client = new HiveMQClient(options);
        Console.WriteLine($"Connecting to {options.Host} on port {options.Port}...");
        await this.client.ConnectAsync().ConfigureAwait(false);

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
    public async Task CleanUpAsync()
    {
        Console.WriteLine("Disconnecting from HiveMQ...");
        await this.client.DisconnectAsync().ConfigureAwait(false);
    }

    [Benchmark(Description = "Publish a QoS 0 messages to the broker.")]
    public async Task PublishQoS0MessageAsync()
    {
        await this.client.PublishAsync("benchmarks/PublishQoS0Messages", this.smallPayload).ConfigureAwait(false);
    }

    [Benchmark(Description = "Publish a QoS 1 messages to the broker.")]
    public async Task PublishQoS1MessageAsync()
    {
        await this.client.PublishAsync("benchmarks/PublishQoS1Messages", this.smallPayload, QualityOfService.AtLeastOnceDelivery).ConfigureAwait(false);
    }

    [Benchmark(Description = "Publish a QoS 2 messages to the broker.")]
    public async Task PublishQoS2MessageAsync()
    {
        await this.client.PublishAsync("benchmarks/PublishQoS1Messages", this.smallPayload, QualityOfService.ExactlyOnceDelivery).ConfigureAwait(false);
    }

    public void Dispose() => GC.SuppressFinalize(this);
}
