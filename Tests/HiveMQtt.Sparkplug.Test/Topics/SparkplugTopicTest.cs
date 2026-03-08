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

namespace HiveMQtt.Sparkplug.Test.Topics;

using FluentAssertions;
using HiveMQtt.Sparkplug.Topics;
using NUnit.Framework;

[TestFixture]
public class SparkplugTopicTest
{
    [Test]
    public void Constructor_WithValidNodeTopic_CreatesInstance()
    {
        var topic = new SparkplugTopic("spBv1.0", "group1", SparkplugMessageType.NBIRTH, "node1");

        topic.Namespace.Should().Be("spBv1.0");
        topic.GroupId.Should().Be("group1");
        topic.MessageType.Should().Be(SparkplugMessageType.NBIRTH);
        topic.EdgeNodeId.Should().Be("node1");
        topic.DeviceId.Should().BeNull();
        topic.IsNodeMessage.Should().BeTrue();
        topic.IsDeviceMessage.Should().BeFalse();
    }

    [Test]
    public void Constructor_WithValidDeviceTopic_CreatesInstance()
    {
        var topic = new SparkplugTopic("spBv1.0", "group1", SparkplugMessageType.DBIRTH, "node1", "device1");

        topic.Namespace.Should().Be("spBv1.0");
        topic.GroupId.Should().Be("group1");
        topic.MessageType.Should().Be(SparkplugMessageType.DBIRTH);
        topic.EdgeNodeId.Should().Be("node1");
        topic.DeviceId.Should().Be("device1");
        topic.IsNodeMessage.Should().BeFalse();
        topic.IsDeviceMessage.Should().BeTrue();
    }

    [Test]
    public void Constructor_WithEmptyNamespace_ThrowsArgumentException()
    {
        var action = () => new SparkplugTopic(string.Empty, "group1", SparkplugMessageType.NBIRTH, "node1");
        action.Should().Throw<ArgumentException>().WithMessage("*Namespace*");
    }

    [Test]
    public void Constructor_WithEmptyGroupId_ThrowsArgumentException()
    {
        var action = () => new SparkplugTopic("spBv1.0", string.Empty, SparkplugMessageType.NBIRTH, "node1");
        action.Should().Throw<ArgumentException>().WithMessage("*Group ID*");
    }

    [Test]
    public void Constructor_WithEmptyEdgeNodeId_ThrowsArgumentException()
    {
        var action = () => new SparkplugTopic("spBv1.0", "group1", SparkplugMessageType.NBIRTH, string.Empty);
        action.Should().Throw<ArgumentException>().WithMessage("*Edge Node ID*");
    }

    [Test]
    public void Constructor_WithDeviceMessageMissingDeviceId_ThrowsArgumentException()
    {
        var action = () => new SparkplugTopic("spBv1.0", "group1", SparkplugMessageType.DBIRTH, "node1");
        action.Should().Throw<ArgumentException>().WithMessage("*Device ID*required*");
    }

    [Test]
    public void Constructor_WithNodeMessageHavingDeviceId_ThrowsArgumentException()
    {
        var action = () => new SparkplugTopic("spBv1.0", "group1", SparkplugMessageType.NBIRTH, "node1", "device1");
        action.Should().Throw<ArgumentException>().WithMessage("*Device ID*must not*");
    }

    [Test]
    public void Build_ForNodeTopic_ReturnsCorrectString()
    {
        var topic = new SparkplugTopic("spBv1.0", "group1", SparkplugMessageType.NDATA, "node1");
        topic.Build().Should().Be("spBv1.0/group1/NDATA/node1");
    }

    [Test]
    public void Build_ForDeviceTopic_ReturnsCorrectString()
    {
        var topic = new SparkplugTopic("spBv1.0", "group1", SparkplugMessageType.DDATA, "node1", "device1");
        topic.Build().Should().Be("spBv1.0/group1/DDATA/node1/device1");
    }

    [Test]
    public void Parse_WithValidNodeTopic_ReturnsCorrectInstance()
    {
        var topic = SparkplugTopic.Parse("spBv1.0/group1/NBIRTH/node1");

        topic.Namespace.Should().Be("spBv1.0");
        topic.GroupId.Should().Be("group1");
        topic.MessageType.Should().Be(SparkplugMessageType.NBIRTH);
        topic.EdgeNodeId.Should().Be("node1");
        topic.DeviceId.Should().BeNull();
    }

    [Test]
    public void Parse_WithValidDeviceTopic_ReturnsCorrectInstance()
    {
        var topic = SparkplugTopic.Parse("spBv1.0/group1/DBIRTH/node1/device1");

        topic.Namespace.Should().Be("spBv1.0");
        topic.GroupId.Should().Be("group1");
        topic.MessageType.Should().Be(SparkplugMessageType.DBIRTH);
        topic.EdgeNodeId.Should().Be("node1");
        topic.DeviceId.Should().Be("device1");
    }

    [Test]
    public void Parse_WithEmptyString_ThrowsArgumentException()
    {
        var action = () => SparkplugTopic.Parse(string.Empty);
        action.Should().Throw<ArgumentException>();
    }

    [Test]
    public void Parse_WithTooFewParts_ThrowsFormatException()
    {
        var action = () => SparkplugTopic.Parse("spBv1.0/group1/NBIRTH");
        action.Should().Throw<FormatException>().WithMessage("*at least 4 parts*");
    }

    [Test]
    public void Parse_WithTooManyParts_ThrowsFormatException()
    {
        var action = () => SparkplugTopic.Parse("spBv1.0/group1/NBIRTH/node1/device1/extra");
        action.Should().Throw<FormatException>().WithMessage("*at most 5 parts*");
    }

    [Test]
    public void Parse_WithInvalidMessageType_ThrowsFormatException()
    {
        var action = () => SparkplugTopic.Parse("spBv1.0/group1/INVALID/node1");
        action.Should().Throw<FormatException>().WithMessage("*Invalid Sparkplug message type*");
    }

    [Test]
    public void TryParse_WithValidTopic_ReturnsTrueAndParsedTopic()
    {
        var success = SparkplugTopic.TryParse("spBv1.0/group1/NBIRTH/node1", out var topic);

        success.Should().BeTrue();
        topic.Should().NotBeNull();
        topic!.MessageType.Should().Be(SparkplugMessageType.NBIRTH);
    }

    [Test]
    public void TryParse_WithInvalidTopic_ReturnsFalse()
    {
        var success = SparkplugTopic.TryParse("invalid", out var topic);

        success.Should().BeFalse();
        topic.Should().BeNull();
    }

    [Test]
    public void TryParse_WithEmptyString_ReturnsFalse()
    {
        var success = SparkplugTopic.TryParse(string.Empty, out var topic);

        success.Should().BeFalse();
        topic.Should().BeNull();
    }

    [Test]
    [TestCase(SparkplugMessageType.NBIRTH)]
    [TestCase(SparkplugMessageType.NDEATH)]
    [TestCase(SparkplugMessageType.NDATA)]
    [TestCase(SparkplugMessageType.NCMD)]
    public void AllNodeMessageTypes_AreValidWithoutDeviceId(SparkplugMessageType messageType)
    {
        var topic = new SparkplugTopic("spBv1.0", "group1", messageType, "node1");
        topic.IsNodeMessage.Should().BeTrue();
        topic.IsDeviceMessage.Should().BeFalse();
    }

    [Test]
    [TestCase(SparkplugMessageType.DBIRTH)]
    [TestCase(SparkplugMessageType.DDEATH)]
    [TestCase(SparkplugMessageType.DDATA)]
    [TestCase(SparkplugMessageType.DCMD)]
    public void AllDeviceMessageTypes_RequireDeviceId(SparkplugMessageType messageType)
    {
        var topic = new SparkplugTopic("spBv1.0", "group1", messageType, "node1", "device1");
        topic.IsDeviceMessage.Should().BeTrue();
        topic.IsNodeMessage.Should().BeFalse();
    }

    [Test]
    public void NodeBirth_CreatesCorrectTopic()
    {
        var topic = SparkplugTopic.NodeBirth("group1", "node1");
        topic.Build().Should().Be("spBv1.0/group1/NBIRTH/node1");
    }

    [Test]
    public void NodeDeath_CreatesCorrectTopic()
    {
        var topic = SparkplugTopic.NodeDeath("group1", "node1");
        topic.Build().Should().Be("spBv1.0/group1/NDEATH/node1");
    }

    [Test]
    public void NodeData_CreatesCorrectTopic()
    {
        var topic = SparkplugTopic.NodeData("group1", "node1");
        topic.Build().Should().Be("spBv1.0/group1/NDATA/node1");
    }

    [Test]
    public void NodeCommand_CreatesCorrectTopic()
    {
        var topic = SparkplugTopic.NodeCommand("group1", "node1");
        topic.Build().Should().Be("spBv1.0/group1/NCMD/node1");
    }

    [Test]
    public void DeviceBirth_CreatesCorrectTopic()
    {
        var topic = SparkplugTopic.DeviceBirth("group1", "node1", "device1");
        topic.Build().Should().Be("spBv1.0/group1/DBIRTH/node1/device1");
    }

    [Test]
    public void DeviceDeath_CreatesCorrectTopic()
    {
        var topic = SparkplugTopic.DeviceDeath("group1", "node1", "device1");
        topic.Build().Should().Be("spBv1.0/group1/DDEATH/node1/device1");
    }

    [Test]
    public void DeviceData_CreatesCorrectTopic()
    {
        var topic = SparkplugTopic.DeviceData("group1", "node1", "device1");
        topic.Build().Should().Be("spBv1.0/group1/DDATA/node1/device1");
    }

    [Test]
    public void DeviceCommand_CreatesCorrectTopic()
    {
        var topic = SparkplugTopic.DeviceCommand("group1", "node1", "device1");
        topic.Build().Should().Be("spBv1.0/group1/DCMD/node1/device1");
    }

    [Test]
    public void FactoryMethods_WithCustomNamespace_UseCustomNamespace()
    {
        var topic = SparkplugTopic.NodeBirth("group1", "node1", "customNS");
        topic.Build().Should().Be("customNS/group1/NBIRTH/node1");
    }

    [Test]
    public void ToString_ReturnsBuildResult()
    {
        var topic = SparkplugTopic.NodeBirth("group1", "node1");
        topic.ToString().Should().Be(topic.Build());
    }

    [Test]
    public void Equals_WithSameTopic_ReturnsTrue()
    {
        var topic1 = SparkplugTopic.NodeBirth("group1", "node1");
        var topic2 = SparkplugTopic.NodeBirth("group1", "node1");

        topic1.Equals(topic2).Should().BeTrue();
        topic1.GetHashCode().Should().Be(topic2.GetHashCode());
    }

    [Test]
    public void Equals_WithDifferentTopic_ReturnsFalse()
    {
        var topic1 = SparkplugTopic.NodeBirth("group1", "node1");
        var topic2 = SparkplugTopic.NodeBirth("group1", "node2");

        topic1.Equals(topic2).Should().BeFalse();
    }

    [Test]
    public void Equals_WithNull_ReturnsFalse()
    {
        var topic = SparkplugTopic.NodeBirth("group1", "node1");
        topic.Equals(null).Should().BeFalse();
    }

    [Test]
    public void DefaultNamespace_IsSpBv10() =>
        SparkplugTopic.DefaultNamespace.Should().Be("spBv1.0");

    [Test]
    public void Parse_RoundTrip_PreservesAllProperties()
    {
        var original = new SparkplugTopic("spBv1.0", "myGroup", SparkplugMessageType.DDATA, "edgeNode", "sensor1");
        var parsed = SparkplugTopic.Parse(original.Build());

        parsed.Namespace.Should().Be(original.Namespace);
        parsed.GroupId.Should().Be(original.GroupId);
        parsed.MessageType.Should().Be(original.MessageType);
        parsed.EdgeNodeId.Should().Be(original.EdgeNodeId);
        parsed.DeviceId.Should().Be(original.DeviceId);
    }
}
