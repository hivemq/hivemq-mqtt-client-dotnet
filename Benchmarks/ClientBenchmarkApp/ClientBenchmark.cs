namespace ClientBenchmarkApp;

using System;
using System.Text;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Engines;

using HiveMQtt.Client;
using HiveMQtt.Client.Options;
using HiveMQtt.MQTT5;
using HiveMQtt.MQTT5.ReasonCodes;
using HiveMQtt.MQTT5.Types;

[SimpleJob(RunStrategy.Monitoring, iterationCount: 100, id: "MonitoringJob")]
public class ClientBenchmarks : IDisposable
{
    private readonly string smallPayload = new string(/*lang=json,strict*/ "{\"interference\": \"1029384\"}");

    private readonly string payload256k;

    private readonly string payload256b;

    private HiveMQClient client;

    public ClientBenchmarks()
    {
        Console.WriteLine("Starting HiveMQ client benchmarks...");
        this.client = null!;

        // Generate a 256 byte payload
        var sb = new StringBuilder();
        for (var i = 0; i < 256; i++)
        {
            sb.Append('a');
        }

        this.payload256b = sb.ToString();

        // Generate a 256k payload
        sb = new StringBuilder();
        for (var i = 0; i < 256 * 1024; i++)
        {
            sb.Append('a');
        }

        this.payload256k = sb.ToString();
    }

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

    [Benchmark(Description = "Publish a QoS 0 message")]
    public async Task PublishQoS0MessageAsync() =>
        await this.client.PublishAsync(
            "benchmarks/PublishQoS0Messages",
            this.smallPayload).ConfigureAwait(false);

    [Benchmark(Description = "Publish a QoS 1 message")]
    public async Task PublishQoS1MessageAsync() =>
        await this.client.PublishAsync(
            "benchmarks/PublishQoS1Messages",
            this.smallPayload,
            QualityOfService.AtLeastOnceDelivery).ConfigureAwait(false);

    [Benchmark(Description = "Publish a QoS 2 message")]
    public async Task PublishQoS2MessageAsync() =>
        await this.client.PublishAsync(
            "benchmarks/PublishQoS2Messages",
            this.smallPayload,
            QualityOfService.ExactlyOnceDelivery).ConfigureAwait(false);

    [Benchmark(Description = "Publish 100 256b length payload QoS 0 messages")]
    public async Task Publish100256bQoS0MessageAsync()
    {
        for (var i = 0; i < 100; i++)
        {
            await this.client.PublishAsync(
                this.payload256b,
                this.smallPayload).ConfigureAwait(false);
        }
    }

    [Benchmark(Description = "Publish 100 256b length payload QoS 1 messages")]
    public async Task Publish100256bQoS1MessageAsync()
    {
        for (var i = 0; i < 100; i++)
        {
            await this.client.PublishAsync(
                "benchmarks/PublishQoS1Messages",
                this.payload256b,
                QualityOfService.AtLeastOnceDelivery).ConfigureAwait(false);
        }
    }

    [Benchmark(Description = "Publish 100 256b length payload QoS 2 messages")]
    public async Task Publish100256bQoS2MessageAsync()
    {
        for (var i = 0; i < 100; i++)
        {
            await this.client.PublishAsync(
                "benchmarks/PublishQoS2Messages",
                this.payload256b,
                QualityOfService.ExactlyOnceDelivery).ConfigureAwait(false);
        }
    }

    [Benchmark(Description = "Publish 100 256k length payload QoS 0 messages")]
    public async Task Publish100256kQoS0MessageAsync()
    {
        for (var i = 0; i < 100; i++)
        {
            await this.client.PublishAsync(
                this.payload256k,
                this.smallPayload).ConfigureAwait(false);
        }
    }

    [Benchmark(Description = "Publish 100 256k length payload QoS 1 messages")]
    public async Task Publish100256kQoS1MessageAsync()
    {
        for (var i = 0; i < 100; i++)
        {
            await this.client.PublishAsync(
                "benchmarks/PublishQoS1Messages",
                this.payload256k,
                QualityOfService.AtLeastOnceDelivery).ConfigureAwait(false);
        }
    }

    [Benchmark(Description = "Publish 100 256k length payload QoS 2 messages")]
    public async Task Publish100256kQoS2MessageAsync()
    {
        for (var i = 0; i < 100; i++)
        {
            await this.client.PublishAsync(
                "benchmarks/PublishQoS2Messages",
                this.payload256k,
                QualityOfService.ExactlyOnceDelivery).ConfigureAwait(false);
        }
    }

    public void Dispose() => GC.SuppressFinalize(this);
}
