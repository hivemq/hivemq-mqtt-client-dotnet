// Copyright 2026-present HiveMQ and the HiveMQ Community
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using HiveMQtt.Client;
using HiveMQtt.Client.Options;
using HiveMQtt.Sparkplug.HostApplication;
using HiveMQtt.Sparkplug.Topics;

// Connect to broker and run a Sparkplug B Host Application: subscribe to spBv1.0/#,
// handle Node/Device birth and data, and optionally send a Rebirth command.
// Requires a running MQTT broker (e.g. HiveMQ on localhost:1883).
var clientOptions = new HiveMQClientOptionsBuilder()
    .WithBroker("127.0.0.1")
    .WithPort(1883)
    .WithClientId("SparkplugHostExample")
    .Build();

var sparkplugOptions = new SparkplugHostApplicationOptions
{
    SparkplugTopicFilter = $"{SparkplugTopic.DefaultNamespace}/#",
    UseStateMessages = false,
};

var host = new SparkplugHostApplication(clientOptions, sparkplugOptions);

host.NodeBirthReceived += (_, e) =>
{
    Console.WriteLine($"[NBIRTH] {e.Topic.GroupId}/{e.Topic.EdgeNodeId}");
};

host.NodeDeathReceived += (_, e) =>
{
    Console.WriteLine($"[NDEATH] {e.Topic.GroupId}/{e.Topic.EdgeNodeId}");
};

host.NodeDataReceived += (_, e) =>
{
    Console.WriteLine($"[NDATA]  {e.Topic.GroupId}/{e.Topic.EdgeNodeId} ({e.Payload.Metrics.Count} metrics)");
};

host.DeviceBirthReceived += (_, e) =>
{
    Console.WriteLine($"[DBIRTH] {e.Topic.GroupId}/{e.Topic.EdgeNodeId}/{e.Topic.DeviceId}");
};

host.DeviceDataReceived += (_, e) =>
{
    Console.WriteLine($"[DDATA]  {e.Topic.GroupId}/{e.Topic.EdgeNodeId}/{e.Topic.DeviceId} ({e.Payload.Metrics.Count} metrics)");
};

host.MessageParseError += (_, e) =>
{
    Console.WriteLine($"[PARSE ERROR] {e.RawTopic}: {e.Reason}");
};

Console.WriteLine("Starting Sparkplug Host Application (subscribe to spBv1.0/#)...");
await host.StartAsync().ConfigureAwait(false);
Console.WriteLine("Host started. Waiting for Edge Node / Device birth and data. Press Enter to send Rebirth to group 'example' node 'node1', or Q to quit.");

while (true)
{
    var key = Console.ReadKey(intercept: true);
    if (key.Key == ConsoleKey.Q)
    {
        break;
    }

    if (key.Key == ConsoleKey.Enter)
    {
        try
        {
            await host.PublishRebirthCommandAsync("example", "node1").ConfigureAwait(false);
            Console.WriteLine("Rebirth command sent to example/node1.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Rebirth failed: {ex.Message}");
        }
    }
}

Console.WriteLine("Stopping Host...");
await host.StopAsync().ConfigureAwait(false);
host.Dispose();
Console.WriteLine("Done.");
