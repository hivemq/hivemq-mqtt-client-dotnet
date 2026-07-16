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

using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using HiveMQtt.Client;
using HiveMQtt.Client.Options;
using HiveMQtt.MQTT5.Types;
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
    public async Task PublishNodeDataAsync_With_Empty_Metrics_Throws()
    {
        var client = new FakeHiveMQClient();
        var options = new SparkplugEdgeNodeOptions { GroupId = "g1", EdgeNodeId = "n1" };
        var node = new SparkplugEdgeNode(client, options);

        Func<Task> act = () => node.PublishNodeDataAsync(Array.Empty<Payload.Types.Metric>());

        await act.Should().ThrowAsync<ArgumentException>().WithParameterName("metrics").ConfigureAwait(false);
    }

    [Test]
    public async Task PublishDeviceDataAsync_With_Empty_Metrics_Throws()
    {
        var client = new FakeHiveMQClient();
        var options = new SparkplugEdgeNodeOptions { GroupId = "g1", EdgeNodeId = "n1" };
        var node = new SparkplugEdgeNode(client, options);

        Func<Task> act = () => node.PublishDeviceDataAsync("d1", Array.Empty<Payload.Types.Metric>());

        await act.Should().ThrowAsync<ArgumentException>().WithParameterName("metrics").ConfigureAwait(false);
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
        payload.Metrics.Add(SparkplugMetricBuilder.Create(SparkplugPayloadEncoder.NodeControlRebirthMetricName).WithBooleanValue(true).Build());
        client.SimulateMessageReceived(topic, SparkplugPayloadEncoder.Encode(payload));

        await Task.Delay(100).ConfigureAwait(false);

        received.Should().NotBeNull();
        received!.Topic.MessageType.Should().Be(SparkplugMessageType.NCMD);
        received.Payload.Metrics.Should().ContainSingle(m => m.Name == SparkplugPayloadEncoder.NodeControlRebirthMetricName);
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

    [Test]
    public void Constructor_With_ClientOptions_And_UseDeathLwt_True_Does_Not_Set_LWT_In_Constructor()
    {
        var clientOptions = new HiveMQClientOptionsBuilder().WithClientId("edge-lwt").Build();
        var options = new SparkplugEdgeNodeOptions { GroupId = "myGroup", EdgeNodeId = "myNode", UseDeathLwt = true };

        _ = new SparkplugEdgeNode(clientOptions, options);

        clientOptions.LastWillAndTestament.Should().BeNull("LWT is set in StartAsync with session bdSeq, not in constructor.");
    }

    [Test]
    public void Constructor_With_ClientOptions_And_UseDeathLwt_False_Does_Not_Set_LastWillAndTestament()
    {
        var clientOptions = new HiveMQClientOptionsBuilder().WithClientId("edge-no-lwt").Build();
        var options = new SparkplugEdgeNodeOptions { GroupId = "g1", EdgeNodeId = "n1", UseDeathLwt = false };

        _ = new SparkplugEdgeNode(clientOptions, options);

        clientOptions.LastWillAndTestament.Should().BeNull();
    }

    [Test]
    public void Constructor_With_ClientOptions_And_Existing_LWT_Does_Not_Override()
    {
        var clientOptions = new HiveMQClientOptionsBuilder()
            .WithClientId("edge-existing-lwt")
            .WithLastWillAndTestament(new LastWillAndTestament("existing/lwt", "payload", QualityOfService.AtLeastOnceDelivery, false))
            .Build();
        var options = new SparkplugEdgeNodeOptions { GroupId = "g1", EdgeNodeId = "n1", UseDeathLwt = true };

        _ = new SparkplugEdgeNode(clientOptions, options);

        clientOptions.LastWillAndTestament!.Topic.Should().Be("existing/lwt");
    }

    [Test]
    public async Task StartAsync_Sets_CurrentSessionBdSeq_And_NBIRTH_Contains_BdSeq_Metric()
    {
        var client = new FakeHiveMQClient();
        var options = new SparkplugEdgeNodeOptions { GroupId = "g1", EdgeNodeId = "n1" };
        var node = new SparkplugEdgeNode(client, options);

        await node.StartAsync().ConfigureAwait(false);

        node.CurrentSessionBdSeq.Should().Be(0UL);
        var nbirth = client.PublishedMessages.Find(m => m.Topic == "spBv1.0/g1/NBIRTH/n1");
        nbirth.Should().NotBeNull();
        var payload = SparkplugPayloadEncoder.Decode(nbirth!.Payload!);
        var bdSeqMetric = payload.Metrics.FirstOrDefault(m => m.Name == SparkplugPayloadEncoder.BdSeqMetricName);
        bdSeqMetric.Should().NotBeNull();
        bdSeqMetric!.LongValue.Should().Be(0UL);
    }

    [Test]
    public async Task StopAsync_NDEATH_Payload_Contains_BdSeq_Metric()
    {
        var client = new FakeHiveMQClient();
        var options = new SparkplugEdgeNodeOptions { GroupId = "g1", EdgeNodeId = "n1" };
        var node = new SparkplugEdgeNode(client, options);
        await node.StartAsync().ConfigureAwait(false);

        await node.StopAsync().ConfigureAwait(false);

        var ndeath = client.PublishedMessages.Find(m => m.Topic == "spBv1.0/g1/NDEATH/n1");
        ndeath.Should().NotBeNull();
        var payload = SparkplugPayloadEncoder.Decode(ndeath!.Payload!);
        var bdSeqMetric = payload.Metrics.FirstOrDefault(m => m.Name == SparkplugPayloadEncoder.BdSeqMetricName);
        bdSeqMetric.Should().NotBeNull();
        bdSeqMetric!.LongValue.Should().Be(0UL);
    }

    [Test]
    public async Task Second_StartAsync_After_Stop_Uses_Incremented_BdSeq()
    {
        var client = new FakeHiveMQClient();
        var options = new SparkplugEdgeNodeOptions { GroupId = "g1", EdgeNodeId = "n1" };
        var node = new SparkplugEdgeNode(client, options);
        await node.StartAsync().ConfigureAwait(false);
        node.CurrentSessionBdSeq.Should().Be(0UL);
        await node.StopAsync().ConfigureAwait(false);
        client.PublishedMessages.Clear();

        await node.StartAsync().ConfigureAwait(false);

        node.CurrentSessionBdSeq.Should().Be(1UL);
        var nbirth = client.PublishedMessages.Find(m => m.Topic == "spBv1.0/g1/NBIRTH/n1");
        nbirth.Should().NotBeNull();
        var payload = SparkplugPayloadEncoder.Decode(nbirth!.Payload!);
        var bdSeqMetric = payload.Metrics.FirstOrDefault(m => m.Name == SparkplugPayloadEncoder.BdSeqMetricName);
        bdSeqMetric.Should().NotBeNull();
        bdSeqMetric!.LongValue.Should().Be(1UL);
    }

    [Test]
    public async Task StartAsync_WhenNBirthPublishFails_DoesNotDuplicateMessageHandlerOnRetry()
    {
        var client = new FakeHiveMQClient();
        var options = new SparkplugEdgeNodeOptions { GroupId = "g1", EdgeNodeId = "n1" };
        var node = new SparkplugEdgeNode(client, options);

        var failFirstBirth = true;
        client.PublishFailureFactory = message =>
        {
            if (failFirstBirth && message.Topic == "spBv1.0/g1/NBIRTH/n1")
            {
                failFirstBirth = false;
                return new InvalidOperationException("NBIRTH publish failed.");
            }

            return null;
        };

        Func<Task> firstStart = () => node.StartAsync();
        await firstStart.Should().ThrowAsync<InvalidOperationException>().ConfigureAwait(false);
        node.IsConnected.Should().BeFalse();

        var commandEvents = 0;
        node.NodeCommandReceived += (_, _) => commandEvents++;

        await node.StartAsync().ConfigureAwait(false);

        var topic = "spBv1.0/g1/NCMD/n1";
        var payload = SparkplugPayloadEncoder.CreatePayload(SparkplugPayloadEncoder.GetCurrentTimestamp(), 0);
        payload.Metrics.Add(SparkplugMetricBuilder.Create(SparkplugPayloadEncoder.NodeControlRebirthMetricName).WithBooleanValue(true).Build());
        client.SimulateMessageReceived(topic, SparkplugPayloadEncoder.Encode(payload));

        await Task.Delay(100).ConfigureAwait(false);
        commandEvents.Should().Be(1, "message handler should be wired exactly once after retrying StartAsync.");
    }

    [Test]
    public async Task StartAsync_With_PrimaryHost_Subscribes_To_Exact_State_Topic_And_Waits_For_Online()
    {
        var client = new FakeHiveMQClient();
        var onlineTs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        client.AfterSubscribeCallback = (c, _) =>
        {
            c.SimulateMessageReceived(
                "spBv1.0/STATE/host1",
                SparkplugStatePayload.CreateOnline(onlineTs).ToUtf8Bytes());
        };

        SparkplugMessageReceivedEventArgs? stateArgs = null;
        var options = new SparkplugEdgeNodeOptions
        {
            GroupId = "g1",
            EdgeNodeId = "n1",
            PrimaryHostApplicationId = "host1",
        };
        var node = new SparkplugEdgeNode(client, options);
        node.StateMessageReceived += (_, e) => stateArgs = e;

        await node.StartAsync().ConfigureAwait(false);

        node.IsConnected.Should().BeTrue();
        node.IsPrimaryHostOnline.Should().BeTrue();
        client.Subscriptions.Should().HaveCount(3);
        client.Subscriptions.Select(s => s.TopicFilter.Topic).Should().Contain("spBv1.0/STATE/host1");
        client.PublishedMessages.Should().ContainSingle(m => m.Topic == "spBv1.0/g1/NBIRTH/n1");
        stateArgs.Should().NotBeNull();
        stateArgs!.PrimaryHostId.Should().Be("host1");
        stateArgs.StatePayload!.Online.Should().BeTrue();
        stateArgs.StatePayload.Timestamp.Should().Be(onlineTs);
    }

    [Test]
    public async Task StartAsync_With_PrimaryHost_Does_Not_Publish_NBirth_While_Host_Offline()
    {
        var client = new FakeHiveMQClient();
        client.AfterSubscribeCallback = (c, _) =>
        {
            c.SimulateMessageReceived(
                "spBv1.0/STATE/host1",
                SparkplugStatePayload.CreateOffline(timestampMs: 1).ToUtf8Bytes());
        };

        var options = new SparkplugEdgeNodeOptions
        {
            GroupId = "g1",
            EdgeNodeId = "n1",
            PrimaryHostApplicationId = "host1",
        };
        var node = new SparkplugEdgeNode(client, options);

        using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(200));
        Func<Task> act = () => node.StartAsync(cancellationToken: cts.Token);

        await act.Should().ThrowAsync<OperationCanceledException>().ConfigureAwait(false);
        client.PublishedMessages.Should().NotContain(m => m.Topic == "spBv1.0/g1/NBIRTH/n1");
        node.IsConnected.Should().BeFalse();
    }

    [Test]
    public async Task PrimaryHost_Valid_Offline_Publishes_NDeath_And_Disconnects()
    {
        var client = new FakeHiveMQClient();
        var onlineTs = 1000L;
        client.AfterSubscribeCallback = (c, _) =>
        {
            c.SimulateMessageReceived(
                "spBv1.0/STATE/host1",
                SparkplugStatePayload.CreateOnline(onlineTs).ToUtf8Bytes());
        };

        var options = new SparkplugEdgeNodeOptions
        {
            GroupId = "g1",
            EdgeNodeId = "n1",
            PrimaryHostApplicationId = "host1",
        };
        var node = new SparkplugEdgeNode(client, options);
        await node.StartAsync().ConfigureAwait(false);
        client.PublishedMessages.Clear();

        client.SimulateMessageReceived(
            "spBv1.0/STATE/host1",
            SparkplugStatePayload.CreateOffline(timestampMs: onlineTs + 1).ToUtf8Bytes());

        await WaitUntilAsync(() => !node.IsConnected && client.PublishedMessages.Any(m => m.Topic == "spBv1.0/g1/NDEATH/n1")).ConfigureAwait(false);

        node.IsConnected.Should().BeFalse();
        node.IsPrimaryHostOnline.Should().BeFalse();
        client.PublishedMessages.Should().Contain(m => m.Topic == "spBv1.0/g1/NDEATH/n1");
        client.IsConnected().Should().BeFalse();
    }

    [Test]
    public async Task PrimaryHost_Stale_Offline_Does_Not_Terminate_Session()
    {
        var client = new FakeHiveMQClient();
        var onlineTs = 2000L;
        client.AfterSubscribeCallback = (c, _) =>
        {
            c.SimulateMessageReceived(
                "spBv1.0/STATE/host1",
                SparkplugStatePayload.CreateOnline(onlineTs).ToUtf8Bytes());
        };

        var options = new SparkplugEdgeNodeOptions
        {
            GroupId = "g1",
            EdgeNodeId = "n1",
            PrimaryHostApplicationId = "host1",
        };
        var node = new SparkplugEdgeNode(client, options);
        await node.StartAsync().ConfigureAwait(false);
        client.PublishedMessages.Clear();

        client.SimulateMessageReceived(
            "spBv1.0/STATE/host1",
            SparkplugStatePayload.CreateOffline(timestampMs: onlineTs - 1).ToUtf8Bytes());

        await Task.Delay(150).ConfigureAwait(false);

        node.IsConnected.Should().BeTrue();
        client.PublishedMessages.Should().NotContain(m => m.Topic == "spBv1.0/g1/NDEATH/n1");
        client.IsConnected().Should().BeTrue();
    }

    [Test]
    public async Task PrimaryHost_Ignores_State_For_Different_Host_Id()
    {
        var client = new FakeHiveMQClient();
        client.AfterSubscribeCallback = (c, _) =>
        {
            c.SimulateMessageReceived(
                "spBv1.0/STATE/host1",
                SparkplugStatePayload.CreateOnline(1000).ToUtf8Bytes());
        };

        var stateEvents = 0;
        var options = new SparkplugEdgeNodeOptions
        {
            GroupId = "g1",
            EdgeNodeId = "n1",
            PrimaryHostApplicationId = "host1",
        };
        var node = new SparkplugEdgeNode(client, options);
        node.StateMessageReceived += (_, _) => stateEvents++;
        await node.StartAsync().ConfigureAwait(false);
        stateEvents.Should().Be(1);

        client.SimulateMessageReceived(
            "spBv1.0/STATE/otherHost",
            SparkplugStatePayload.CreateOffline(timestampMs: 9999).ToUtf8Bytes());

        await Task.Delay(100).ConfigureAwait(false);

        stateEvents.Should().Be(1);
        node.IsConnected.Should().BeTrue();
    }

    [Test]
    public async Task PrimaryHost_Invalid_State_Json_Raises_MessageParseError()
    {
        var client = new FakeHiveMQClient();
        client.AfterSubscribeCallback = (c, _) =>
        {
            c.SimulateMessageReceived(
                "spBv1.0/STATE/host1",
                SparkplugStatePayload.CreateOnline(1000).ToUtf8Bytes());
        };

        SparkplugMessageParseErrorEventArgs? error = null;
        var options = new SparkplugEdgeNodeOptions
        {
            GroupId = "g1",
            EdgeNodeId = "n1",
            PrimaryHostApplicationId = "host1",
        };
        var node = new SparkplugEdgeNode(client, options);
        node.MessageParseError += (_, e) => error = e;
        await node.StartAsync().ConfigureAwait(false);

        client.SimulateMessageReceived("spBv1.0/STATE/host1", Encoding.UTF8.GetBytes("not-json"));

        await Task.Delay(50).ConfigureAwait(false);

        error.Should().NotBeNull();
        error!.Reason.Should().Contain("STATE payload is not valid Sparkplug 3.0 JSON");
        node.IsConnected.Should().BeTrue();
    }

    [Test]
    public async Task PrimaryHost_Can_Restart_After_Offline_When_Host_Returns_Online()
    {
        var client = new FakeHiveMQClient();
        var deliverOnline = true;
        client.AfterSubscribeCallback = (c, _) =>
        {
            if (deliverOnline)
            {
                c.SimulateMessageReceived(
                    "spBv1.0/STATE/host1",
                    SparkplugStatePayload.CreateOnline(3000).ToUtf8Bytes());
            }
        };

        var options = new SparkplugEdgeNodeOptions
        {
            GroupId = "g1",
            EdgeNodeId = "n1",
            PrimaryHostApplicationId = "host1",
        };
        var node = new SparkplugEdgeNode(client, options);
        await node.StartAsync().ConfigureAwait(false);

        client.SimulateMessageReceived(
            "spBv1.0/STATE/host1",
            SparkplugStatePayload.CreateOffline(timestampMs: 3001).ToUtf8Bytes());
        await WaitUntilAsync(() => !node.IsConnected).ConfigureAwait(false);

        client.PublishedMessages.Clear();
        deliverOnline = true;
        await node.StartAsync().ConfigureAwait(false);

        node.IsConnected.Should().BeTrue();
        client.PublishedMessages.Should().Contain(m => m.Topic == "spBv1.0/g1/NBIRTH/n1");
    }

    private static async Task WaitUntilAsync(Func<bool> condition, int timeoutMs = 2000)
    {
        var deadline = DateTime.UtcNow.AddMilliseconds(timeoutMs);
        while (DateTime.UtcNow < deadline)
        {
            if (condition())
            {
                return;
            }

            await Task.Delay(20).ConfigureAwait(false);
        }

        condition().Should().BeTrue("condition should become true within timeout");
    }
}
