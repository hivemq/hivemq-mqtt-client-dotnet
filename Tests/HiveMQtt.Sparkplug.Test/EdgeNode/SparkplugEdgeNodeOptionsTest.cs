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

using FluentAssertions;
using HiveMQtt.Sparkplug.EdgeNode;
using HiveMQtt.Sparkplug.Topics;
using NUnit.Framework;

[TestFixture]
public class SparkplugEdgeNodeOptionsTest
{
    [Test]
    public void Default_Values_Are_Sensible()
    {
        var options = new SparkplugEdgeNodeOptions();

        options.SparkplugNamespace.Should().Be(SparkplugTopic.DefaultNamespace);
        options.GroupId.Should().BeNull();
        options.EdgeNodeId.Should().BeNull();
    }

    [Test]
    public void Validate_With_Null_GroupId_Throws()
    {
        var options = new SparkplugEdgeNodeOptions
        {
            GroupId = null,
            EdgeNodeId = "node1",
        };

        var act = () => options.Validate();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*GroupId*cannot*null*empty*");
    }

    [Test]
    public void Validate_With_Empty_GroupId_Throws()
    {
        var options = new SparkplugEdgeNodeOptions
        {
            GroupId = string.Empty,
            EdgeNodeId = "node1",
        };

        var act = () => options.Validate();

        act.Should().Throw<InvalidOperationException>();
    }

    [Test]
    public void Validate_With_Null_EdgeNodeId_Throws()
    {
        var options = new SparkplugEdgeNodeOptions
        {
            GroupId = "g1",
            EdgeNodeId = null,
        };

        var act = () => options.Validate();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*EdgeNodeId*cannot*null*empty*");
    }

    [Test]
    public void Validate_With_Empty_SparkplugNamespace_Throws()
    {
        var options = new SparkplugEdgeNodeOptions
        {
            GroupId = "g1",
            EdgeNodeId = "n1",
            SparkplugNamespace = string.Empty,
        };

        var act = () => options.Validate();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*SparkplugNamespace*");
    }

    [Test]
    public void Validate_With_Valid_Options_Does_Not_Throw()
    {
        var options = new SparkplugEdgeNodeOptions
        {
            GroupId = "g1",
            EdgeNodeId = "n1",
        };

        var act = () => options.Validate();

        act.Should().NotThrow();
    }
}
