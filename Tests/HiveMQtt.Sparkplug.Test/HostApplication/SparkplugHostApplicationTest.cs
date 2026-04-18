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

namespace HiveMQtt.Sparkplug.Test.HostApplication;

using System.Threading.Tasks;
using FluentAssertions;
using HiveMQtt.Client;
using HiveMQtt.Client.Options;
using HiveMQtt.Sparkplug.HostApplication;
using HiveMQtt.Sparkplug.Payload;
using HiveMQtt.Sparkplug.Protobuf;
using HiveMQtt.Sparkplug.Topics;
using NUnit.Framework;

[TestFixture]
public class SparkplugHostApplicationTest
{
    [Test]
    public void Constructor_With_Null_Client_Throws()
    {
        var options = new SparkplugHostApplicationOptions { SparkplugTopicFilter = "spBv1.0/#" };

        var act = () => new SparkplugHostApplication((IHiveMQClient)null!, options);

        act.Should().Throw<ArgumentNullException>().WithParameterName("client");
    }

    [Test]
    public void Constructor_With_Null_Options_Throws()
    {
        var client = new FakeHiveMQClient();

        var act = () => new SparkplugHostApplication(client, null!);

        act.Should().Throw<ArgumentNullException>().WithParameterName("options");
    }

    [Test]
    public void Constructor_With_Invalid_Options_Throws()
    {
        var client = new FakeHiveMQClient();
        var options = new SparkplugHostApplicationOptions
        {
            UseStateMessages = true,
            HostApplicationId = null,
        };

        var act = () => new SparkplugHostApplication(client, options);

        act.Should().Throw<InvalidOperationException>();
    }

    [Test]
    public void Client_And_Options_Exposed()
    {
        var client = new FakeHiveMQClient();
        var options = new SparkplugHostApplicationOptions { SparkplugTopicFilter = "spBv1.0/#", UseStateMessages = false };
        var host = new SparkplugHostApplication(client, options);

        host.Client.Should().BeSameAs(client);
        host.Options.Should().BeSameAs(options);
    }

    [Test]
    public void GetNodeState_When_Empty_Returns_Null()
    {
        var client = new FakeHiveMQClient();
        var options = new SparkplugHostApplicationOptions { SparkplugTopicFilter = "spBv1.0/#", UseStateMessages = false };
        var host = new SparkplugHostApplication(client, options);

        host.GetNodeState("g", "n").Should().BeNull();
    }

    [Test]
    public void GetDeviceState_When_Empty_Returns_Null()
    {
        var client = new FakeHiveMQClient();
        var options = new SparkplugHostApplicationOptions { SparkplugTopicFilter = "spBv1.0/#", UseStateMessages = false };
        var host = new SparkplugHostApplication(client, options);

        host.GetDeviceState("g", "n", "d").Should().BeNull();
    }

    [Test]
    public void GetNodeState_With_Null_GroupId_Throws()
    {
        var client = new FakeHiveMQClient();
        var options = new SparkplugHostApplicationOptions { SparkplugTopicFilter = "spBv1.0/#", UseStateMessages = false };
        var host = new SparkplugHostApplication(client, options);

        var act = () => host.GetNodeState(null!, "n1");

        act.Should().Throw<ArgumentException>().WithParameterName("groupId");
    }

    [Test]
    public void GetNodeState_With_Empty_EdgeNodeId_Throws()
    {
        var client = new FakeHiveMQClient();
        var options = new SparkplugHostApplicationOptions { SparkplugTopicFilter = "spBv1.0/#", UseStateMessages = false };
        var host = new SparkplugHostApplication(client, options);

        var act = () => host.GetNodeState("g1", string.Empty);

        act.Should().Throw<ArgumentException>().WithParameterName("edgeNodeId");
    }

    [Test]
    public void GetDeviceState_With_Null_DeviceId_Throws()
    {
        var client = new FakeHiveMQClient();
        var options = new SparkplugHostApplicationOptions { SparkplugTopicFilter = "spBv1.0/#", UseStateMessages = false };
        var host = new SparkplugHostApplication(client, options);

        var act = () => host.GetDeviceState("g1", "n1", null!);

        act.Should().Throw<ArgumentException>().WithParameterName("deviceId");
    }

    [Test]
    public void NodeStates_And_DeviceStates_Initially_Empty()
    {
        var client = new FakeHiveMQClient();
        var options = new SparkplugHostApplicationOptions { SparkplugTopicFilter = "spBv1.0/#", UseStateMessages = false };
        var host = new SparkplugHostApplication(client, options);

        host.NodeStates.Should().BeEmpty();
        host.DeviceStates.Should().BeEmpty();
    }

    [Test]
    public async Task StartAsync_Connects_And_Subscribes()
    {
        var client = new FakeHiveMQClient();
        var options = new SparkplugHostApplicationOptions
        {
            SparkplugTopicFilter = "spBv1.0/#",
            UseStateMessages = false,
        };
        var host = new SparkplugHostApplication(client, options);

        await host.StartAsync().ConfigureAwait(false);

        host.IsConnected.Should().BeTrue();
        client.Subscriptions.Should().HaveCount(1);
        client.Subscriptions[0].TopicFilter.Topic.Should().Be("spBv1.0/#");
    }

    [Test]
    public async Task StopAsync_Unwires_And_Reports_Not_Connected()
    {
        var client = new FakeHiveMQClient();
        var options = new SparkplugHostApplicationOptions
        {
            SparkplugTopicFilter = "spBv1.0/#",
            UseStateMessages = false,
        };
        var host = new SparkplugHostApplication(client, options);

        await host.StartAsync().ConfigureAwait(false);
        host.IsConnected.Should().BeTrue();

        await host.StopAsync().ConfigureAwait(false);

        host.IsConnected.Should().BeFalse();
    }

    [Test]
    public async Task PublishNodeCommandAsync_Publishes_To_NCMD_Topic()
    {
        var client = new FakeHiveMQClient();
        var options = new SparkplugHostApplicationOptions { SparkplugTopicFilter = "spBv1.0/#", UseStateMessages = false };
        var host = new SparkplugHostApplication(client, options);
        await host.StartAsync().ConfigureAwait(false);

        var payload = SparkplugPayloadEncoder.CreatePayload(SparkplugPayloadEncoder.GetCurrentTimestamp(), 0);
        payload.Metrics.Add(SparkplugMetricBuilder.Create("test").WithInt32Value(1).Build());

        var result = await host.PublishNodeCommandAsync("g1", "n1", payload).ConfigureAwait(false);

        result.Should().NotBeNull();
        client.PublishedMessages.Should().HaveCount(1);
        client.PublishedMessages[0].Topic.Should().Be("spBv1.0/g1/NCMD/n1");
    }

    [Test]
    public async Task PublishNodeCommandAsync_With_Null_GroupId_Throws()
    {
        var client = new FakeHiveMQClient();
        var options = new SparkplugHostApplicationOptions { SparkplugTopicFilter = "spBv1.0/#", UseStateMessages = false };
        var host = new SparkplugHostApplication(client, options);
        await host.StartAsync().ConfigureAwait(false);
        var payload = SparkplugPayloadEncoder.CreatePayload(SparkplugPayloadEncoder.GetCurrentTimestamp(), 0);

        var act = async () => await host.PublishNodeCommandAsync(null!, "n1", payload).ConfigureAwait(false);

        await act.Should().ThrowAsync<ArgumentException>().WithParameterName("groupId").ConfigureAwait(false);
    }

    [Test]
    public async Task PublishNodeCommandAsync_With_GroupId_Containing_Hash_When_Strict_Throws()
    {
        var client = new FakeHiveMQClient();
        var options = new SparkplugHostApplicationOptions
        {
            SparkplugTopicFilter = "spBv1.0/#",
            UseStateMessages = false,
            StrictIdentifierValidation = true,
        };
        var host = new SparkplugHostApplication(client, options);
        await host.StartAsync().ConfigureAwait(false);
        var payload = SparkplugPayloadEncoder.CreatePayload(SparkplugPayloadEncoder.GetCurrentTimestamp(), 0);

        var act = async () => await host.PublishNodeCommandAsync("g#1", "n1", payload).ConfigureAwait(false);

        await act.Should().ThrowAsync<ArgumentException>().WithParameterName("groupId").WithMessage("*'#'*").ConfigureAwait(false);
    }

    [Test]
    public async Task PublishDeviceCommandAsync_With_Null_DeviceId_Throws()
    {
        var client = new FakeHiveMQClient();
        var options = new SparkplugHostApplicationOptions { SparkplugTopicFilter = "spBv1.0/#", UseStateMessages = false };
        var host = new SparkplugHostApplication(client, options);
        await host.StartAsync().ConfigureAwait(false);
        var payload = SparkplugPayloadEncoder.CreatePayload(SparkplugPayloadEncoder.GetCurrentTimestamp(), 0);

        var act = async () => await host.PublishDeviceCommandAsync("g1", "n1", null!, payload).ConfigureAwait(false);

        await act.Should().ThrowAsync<ArgumentException>().WithParameterName("deviceId").ConfigureAwait(false);
    }

    [Test]
    public async Task PublishDeviceCommandAsync_Publishes_To_DCMD_Topic()
    {
        var client = new FakeHiveMQClient();
        var options = new SparkplugHostApplicationOptions { SparkplugTopicFilter = "spBv1.0/#", UseStateMessages = false };
        var host = new SparkplugHostApplication(client, options);
        await host.StartAsync().ConfigureAwait(false);

        var payload = SparkplugPayloadEncoder.CreatePayload(SparkplugPayloadEncoder.GetCurrentTimestamp(), 0);

        var result = await host.PublishDeviceCommandAsync("g1", "n1", "d1", payload).ConfigureAwait(false);

        result.Should().NotBeNull();
        client.PublishedMessages.Should().HaveCount(1);
        client.PublishedMessages[0].Topic.Should().Be("spBv1.0/g1/DCMD/n1/d1");
    }

    [Test]
    public async Task PublishRebirthCommandAsync_Publishes_NCMD_With_Rebirth_Metric()
    {
        var client = new FakeHiveMQClient();
        var options = new SparkplugHostApplicationOptions { SparkplugTopicFilter = "spBv1.0/#", UseStateMessages = false };
        var host = new SparkplugHostApplication(client, options);
        await host.StartAsync().ConfigureAwait(false);

        var result = await host.PublishRebirthCommandAsync("g1", "n1").ConfigureAwait(false);

        result.Should().NotBeNull();
        client.PublishedMessages.Should().HaveCount(1);
        client.PublishedMessages[0].Topic.Should().Be("spBv1.0/g1/NCMD/n1");
        client.PublishedMessages[0].Payload.Should().NotBeNull();
        var decoded = SparkplugPayloadEncoder.Decode(client.PublishedMessages[0].Payload!);
        decoded.Metrics.Should().ContainSingle(m => m.Name == "Rebirth" && m.Datatype == (uint)DataType.Boolean && m.BooleanValue);
    }

    [Test]
    public async Task When_NBIRTH_Received_NodeBirthReceived_Fires_And_State_Updated()
    {
        var client = new FakeHiveMQClient();
        var options = new SparkplugHostApplicationOptions { SparkplugTopicFilter = "spBv1.0/#", UseStateMessages = false };
        var host = new SparkplugHostApplication(client, options);
        SparkplugMessageReceivedEventArgs? received = null;
        host.NodeBirthReceived += (_, e) => received = e;
        await host.StartAsync().ConfigureAwait(false);

        var topic = "spBv1.0/grp1/NBIRTH/node1";
        var payload = SparkplugPayloadEncoder.CreatePayload(SparkplugPayloadEncoder.GetCurrentTimestamp(), 0);
        var payloadBytes = SparkplugPayloadEncoder.Encode(payload);
        client.SimulateMessageReceived(topic, payloadBytes);

        await Task.Delay(100).ConfigureAwait(false);

        received.Should().NotBeNull();
        received!.Topic.MessageType.Should().Be(SparkplugMessageType.NBIRTH);
        received.Topic.GroupId.Should().Be("grp1");
        received.Topic.EdgeNodeId.Should().Be("node1");
        host.GetNodeState("grp1", "node1").Should().NotBeNull();
        host.GetNodeState("grp1", "node1")!.IsOnline.Should().BeTrue();
    }

    [Test]
    public async Task When_NDEATH_Received_NodeDeathReceived_Fires_And_State_Updated()
    {
        var client = new FakeHiveMQClient();
        var options = new SparkplugHostApplicationOptions { SparkplugTopicFilter = "spBv1.0/#", UseStateMessages = false };
        var host = new SparkplugHostApplication(client, options);
        SparkplugMessageReceivedEventArgs? received = null;
        host.NodeDeathReceived += (_, e) => received = e;
        await host.StartAsync().ConfigureAwait(false);

        var topic = "spBv1.0/grp1/NDEATH/node1";
        var payload = SparkplugPayloadEncoder.CreatePayload(SparkplugPayloadEncoder.GetCurrentTimestamp(), 0);
        var payloadBytes = SparkplugPayloadEncoder.Encode(payload);
        client.SimulateMessageReceived(topic, payloadBytes);

        await Task.Delay(100).ConfigureAwait(false);

        received.Should().NotBeNull();
        received!.Topic.MessageType.Should().Be(SparkplugMessageType.NDEATH);
        host.GetNodeState("grp1", "node1").Should().NotBeNull();
        host.GetNodeState("grp1", "node1")!.IsOnline.Should().BeFalse();
    }

    [Test]
    public void Dispose_Can_Be_Called_Without_Throw()
    {
        var clientOptions = new HiveMQClientOptionsBuilder().WithClientId("dispose-test").Build();
        var sparkplugOptions = new SparkplugHostApplicationOptions
        {
            SparkplugTopicFilter = "spBv1.0/#",
            UseStateMessages = false,
        };
        var host = new SparkplugHostApplication(clientOptions, sparkplugOptions);

        host.Invoking(h => h.Dispose()).Should().NotThrow();
    }
}
