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

namespace HiveMQtt.Sparkplug.Test.EdgeNode;

using System.Threading.Tasks;
using FluentAssertions;
using HiveMQtt.Client;
using HiveMQtt.Sparkplug.EdgeNode;
using HiveMQtt.Sparkplug.HostApplication;
using HiveMQtt.Sparkplug.Payload;
using HiveMQtt.Sparkplug.Protobuf;
using HiveMQtt.Sparkplug.Test.HostApplication;
using HiveMQtt.Sparkplug.Topics;
using NUnit.Framework;

[TestFixture]
public class SparkplugEdgeNodeTest
{
    [Test]
    public void Constructor_With_Null_Client_Throws()
    {
        var options = new SparkplugEdgeNodeOptions { GroupId = "g1", EdgeNodeId = "n1" };

        var act = () => new SparkplugEdgeNode((IHiveMQClient)null!, options);

        act.Should().Throw<ArgumentNullException>().WithParameterName("client");
    }

    [Test]
    public void Constructor_With_Null_Options_Throws()
    {
        var client = new FakeHiveMQClient();

        var act = () => new SparkplugEdgeNode(client, null!);

        act.Should().Throw<ArgumentNullException>().WithParameterName("options");
    }

    [Test]
    public void Constructor_With_Invalid_Options_Throws()
    {
        var client = new FakeHiveMQClient();
        var options = new SparkplugEdgeNodeOptions { GroupId = "g1" }; // EdgeNodeId null

        var act = () => new SparkplugEdgeNode(client, options);

        act.Should().Throw<InvalidOperationException>();
    }

    [Test]
    public void Client_And_Options_Exposed()
    {
        var client = new FakeHiveMQClient();
        var options = new SparkplugEdgeNodeOptions { GroupId = "g1", EdgeNodeId = "n1" };
        var node = new SparkplugEdgeNode(client, options);

        node.Client.Should().BeSameAs(client);
        node.Options.Should().BeSameAs(options);
    }

    [Test]
    public async Task StartAsync_Connects_Subscribes_And_Publishes_NBIRTH()
    {
        var client = new FakeHiveMQClient();
        var options = new SparkplugEdgeNodeOptions { GroupId = "g1", EdgeNodeId = "n1" };
        var node = new SparkplugEdgeNode(client, options);

        await node.StartAsync().ConfigureAwait(false);

        node.IsConnected.Should().BeTrue();
        node.SequenceNumber.Should().Be(1);
        client.Subscriptions.Should().HaveCount(2);
        client.PublishedMessages.Should().ContainSingle(m => m.Topic == "spBv1.0/g1/NBIRTH/n1");
    }

    [Test]
    public async Task StopAsync_Publishes_NDEATH_And_Unwires()
    {
        var client = new FakeHiveMQClient();
        var options = new SparkplugEdgeNodeOptions { GroupId = "g1", EdgeNodeId = "n1" };
        var node = new SparkplugEdgeNode(client, options);
        await node.StartAsync().ConfigureAwait(false);

        await node.StopAsync().ConfigureAwait(false);

        node.IsConnected.Should().BeFalse();
        client.PublishedMessages.Should().Contain(m => m.Topic == "spBv1.0/g1/NDEATH/n1");
    }

    [Test]
    public async Task PublishNodeDataAsync_Publishes_To_NDATA_And_Advances_Sequence()
    {
        var client = new FakeHiveMQClient();
        var options = new SparkplugEdgeNodeOptions { GroupId = "g1", EdgeNodeId = "n1" };
        var node = new SparkplugEdgeNode(client, options);
        await node.StartAsync().ConfigureAwait(false);
        var metrics = new[] { SparkplugMetricBuilder.Create("x").WithInt32Value(42).Build() };

        var result = await node.PublishNodeDataAsync(metrics).ConfigureAwait(false);

        result.Should().NotBeNull();
        node.SequenceNumber.Should().Be(2);
        client.PublishedMessages.Should().Contain(m => m.Topic == "spBv1.0/g1/NDATA/n1");
    }

    [Test]
    public async Task PublishDeviceBirthAsync_Publishes_To_DBIRTH_Topic()
    {
        var client = new FakeHiveMQClient();
        var options = new SparkplugEdgeNodeOptions { GroupId = "g1", EdgeNodeId = "n1" };
        var node = new SparkplugEdgeNode(client, options);
        await node.StartAsync().ConfigureAwait(false);

        var result = await node.PublishDeviceBirthAsync("d1", null).ConfigureAwait(false);

        result.Should().NotBeNull();
        client.PublishedMessages.Should().Contain(m => m.Topic == "spBv1.0/g1/DBIRTH/n1/d1");
    }

    [Test]
    public async Task PublishDeviceBirthAsync_With_Null_DeviceId_Throws()
    {
        var client = new FakeHiveMQClient();
        var options = new SparkplugEdgeNodeOptions { GroupId = "g1", EdgeNodeId = "n1" };
        var node = new SparkplugEdgeNode(client, options);

        Func<Task> act = () => node.PublishDeviceBirthAsync(null!, null);

        await act.Should().ThrowAsync<ArgumentException>().WithParameterName("deviceId").ConfigureAwait(false);
    }

    [Test]
    public async Task PublishDeviceDataAsync_With_Empty_DeviceId_Throws()
    {
        var client = new FakeHiveMQClient();
        var options = new SparkplugEdgeNodeOptions { GroupId = "g1", EdgeNodeId = "n1" };
        var node = new SparkplugEdgeNode(client, options);
        var metrics = Array.Empty<Payload.Types.Metric>();

        Func<Task> act = () => node.PublishDeviceDataAsync(string.Empty, metrics);

        await act.Should().ThrowAsync<ArgumentException>().WithParameterName("deviceId").ConfigureAwait(false);
    }

    [Test]
    public async Task PublishDeviceDeathAsync_With_Whitespace_DeviceId_Throws()
    {
        var client = new FakeHiveMQClient();
        var options = new SparkplugEdgeNodeOptions { GroupId = "g1", EdgeNodeId = "n1" };
        var node = new SparkplugEdgeNode(client, options);

        Func<Task> act = () => node.PublishDeviceDeathAsync("   ");

        await act.Should().ThrowAsync<ArgumentException>().WithParameterName("deviceId").WithMessage("*whitespace*").ConfigureAwait(false);
    }

    [Test]
    public async Task PublishDeviceDeathAsync_Throws_When_DeviceId_Empty()
    {
        var client = new FakeHiveMQClient();
        var options = new SparkplugEdgeNodeOptions { GroupId = "g1", EdgeNodeId = "n1" };
        var node = new SparkplugEdgeNode(client, options);
        await node.StartAsync().ConfigureAwait(false);

        var act = () => node.PublishDeviceDeathAsync(string.Empty!);

        await act.Should().ThrowAsync<ArgumentException>().WithParameterName("deviceId").ConfigureAwait(false);
    }

    [Test]
    public async Task When_NCMD_Received_NodeCommandReceived_Fires()
    {
        var client = new FakeHiveMQClient();
        var options = new SparkplugEdgeNodeOptions { GroupId = "g1", EdgeNodeId = "n1" };
        var node = new SparkplugEdgeNode(client, options);
        SparkplugMessageReceivedEventArgs? received = null;
        node.NodeCommandReceived += (_, e) => received = e;
        await node.StartAsync().ConfigureAwait(false);

        var topic = "spBv1.0/g1/NCMD/n1";
        var payload = SparkplugPayloadEncoder.CreatePayload(SparkplugPayloadEncoder.GetCurrentTimestamp(), 0);
        payload.Metrics.Add(SparkplugMetricBuilder.Create("Rebirth").WithBooleanValue(true).Build());
        client.SimulateMessageReceived(topic, SparkplugPayloadEncoder.Encode(payload));

        await Task.Delay(100).ConfigureAwait(false);

        received.Should().NotBeNull();
        received!.Topic.MessageType.Should().Be(SparkplugMessageType.NCMD);
        received.Payload.Metrics.Should().ContainSingle(m => m.Name == "Rebirth");
    }

    [Test]
    public async Task When_DCMD_Received_DeviceCommandReceived_Fires()
    {
        var client = new FakeHiveMQClient();
        var options = new SparkplugEdgeNodeOptions { GroupId = "g1", EdgeNodeId = "n1" };
        var node = new SparkplugEdgeNode(client, options);
        SparkplugMessageReceivedEventArgs? received = null;
        node.DeviceCommandReceived += (_, e) => received = e;
        await node.StartAsync().ConfigureAwait(false);

        var topic = "spBv1.0/g1/DCMD/n1/d1";
        var payload = SparkplugPayloadEncoder.CreatePayload(SparkplugPayloadEncoder.GetCurrentTimestamp(), 0);
        client.SimulateMessageReceived(topic, SparkplugPayloadEncoder.Encode(payload));

        await Task.Delay(100).ConfigureAwait(false);

        received.Should().NotBeNull();
        received!.Topic.MessageType.Should().Be(SparkplugMessageType.DCMD);
        received.Topic.DeviceId.Should().Be("d1");
    }

    [Test]
    public async Task PublishNodeDataAsync_With_Null_Metrics_Throws()
    {
        var client = new FakeHiveMQClient();
        var options = new SparkplugEdgeNodeOptions { GroupId = "g1", EdgeNodeId = "n1" };
        var node = new SparkplugEdgeNode(client, options);

        Func<Task> act = () => node.PublishNodeDataAsync(null!);

        await act.Should().ThrowAsync<ArgumentNullException>().ConfigureAwait(false);
    }

    [Test]
    public void Dispose_Can_Be_Called_Without_Throw()
    {
        var clientOptions = new HiveMQClientOptionsBuilder().WithClientId("edge-dispose").Build();
        var options = new SparkplugEdgeNodeOptions { GroupId = "g1", EdgeNodeId = "n1" };
        var node = new SparkplugEdgeNode(clientOptions, options);

        node.Invoking(n => n.Dispose()).Should().NotThrow();
    }
}
