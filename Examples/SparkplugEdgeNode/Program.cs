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
using HiveMQtt.Sparkplug.EdgeNode;
using HiveMQtt.Sparkplug.Payload;

// Connect to broker and run a Sparkplug B Edge Node: publish NBIRTH, NDATA, DBIRTH, DDATA,
// and handle NCMD/DCMD. Requires a running MQTT broker (e.g. HiveMQ on localhost:1883).
var clientOptions = new HiveMQClientOptionsBuilder()
    .WithBroker("127.0.0.1")
    .WithPort(1883)
    .WithClientId("SparkplugEdgeNodeExample")
    .Build();

var sparkplugOptions = new SparkplugEdgeNodeOptions
{
    GroupId = "example",
    EdgeNodeId = "node1",
};

var edgeNode = new SparkplugEdgeNode(clientOptions, sparkplugOptions);

edgeNode.NodeCommandReceived += (_, e) =>
{
    Console.WriteLine($"[NCMD] {e.Payload.Metrics.Count} metrics");
    foreach (var m in e.Payload.Metrics)
    {
        Console.WriteLine($"  - {m.Name}");
    }
};

edgeNode.DeviceCommandReceived += (_, e) =>
{
    Console.WriteLine($"[DCMD] device={e.Topic.DeviceId} ({e.Payload.Metrics.Count} metrics)");
};

edgeNode.MessageParseError += (_, e) =>
{
    Console.WriteLine($"[PARSE ERROR] {e.RawTopic}: {e.Reason}");
};

Console.WriteLine("Starting Sparkplug Edge Node (group=example, node=node1)...");
await edgeNode.StartAsync().ConfigureAwait(false);
Console.WriteLine("Edge Node started. Publishing NDATA every 5 seconds. Press Q to quit.");

var dataCount = 0;
while (true)
{
    await Task.Delay(5000).ConfigureAwait(false);

    if (Console.KeyAvailable && Console.ReadKey(intercept: true).Key == ConsoleKey.Q)
    {
        break;
    }

    dataCount++;
    var metrics = new[]
    {
        SparkplugMetricBuilder.Create("counter").WithInt64Value(dataCount).Build(),
        SparkplugMetricBuilder.Create("temperature").WithFloatValue(20.0f + (dataCount % 10)).Build(),
    };

    await edgeNode.PublishNodeDataAsync(metrics).ConfigureAwait(false);
    Console.WriteLine($"Published NDATA (seq={edgeNode.SequenceNumber}, counter={dataCount})");
}

Console.WriteLine("Stopping Edge Node (publishing NDEATH)...");
await edgeNode.StopAsync().ConfigureAwait(false);
edgeNode.Dispose();
Console.WriteLine("Done.");
