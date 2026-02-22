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

using FluentAssertions;
using HiveMQtt.Sparkplug.HostApplication;
using HiveMQtt.Sparkplug.Topics;
using NUnit.Framework;

[TestFixture]
public class SparkplugHostApplicationOptionsTest
{
    [Test]
    public void Default_Values_Are_Sensible()
    {
        var options = new SparkplugHostApplicationOptions();

        options.SparkplugNamespace.Should().Be(SparkplugTopic.DefaultNamespace);
        options.SparkplugTopicFilter.Should().Be($"{SparkplugTopic.DefaultNamespace}/#");
        options.UseStateMessages.Should().BeTrue();
        options.UseStateLwt.Should().BeTrue();
        options.HostApplicationId.Should().BeNull();
    }

    [Test]
    public void Validate_With_Empty_TopicFilter_Throws()
    {
        var options = new SparkplugHostApplicationOptions
        {
            SparkplugTopicFilter = string.Empty,
        };

        var act = () => options.Validate();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*SparkplugTopicFilter*cannot*null*empty*");
    }

    [Test]
    public void Validate_With_Whitespace_TopicFilter_Throws()
    {
        var options = new SparkplugHostApplicationOptions
        {
            SparkplugTopicFilter = "   ",
        };

        var act = () => options.Validate();

        act.Should().Throw<InvalidOperationException>();
    }

    [Test]
    public void Validate_With_UseStateMessages_True_And_No_HostApplicationId_Throws()
    {
        var options = new SparkplugHostApplicationOptions
        {
            UseStateMessages = true,
            HostApplicationId = null,
        };

        var act = () => options.Validate();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*HostApplicationId*must be set*UseStateMessages*");
    }

    [Test]
    public void Validate_With_UseStateMessages_True_And_Empty_HostApplicationId_Throws()
    {
        var options = new SparkplugHostApplicationOptions
        {
            UseStateMessages = true,
            HostApplicationId = string.Empty,
        };

        var act = () => options.Validate();

        act.Should().Throw<InvalidOperationException>();
    }

    [Test]
    public void Validate_With_Valid_Options_Does_Not_Throw()
    {
        var options = new SparkplugHostApplicationOptions
        {
            SparkplugTopicFilter = "spBv1.0/#",
            HostApplicationId = "host1",
        };

        var act = () => options.Validate();

        act.Should().NotThrow();
    }

    [Test]
    public void Validate_With_UseStateMessages_False_Allows_Null_HostApplicationId()
    {
        var options = new SparkplugHostApplicationOptions
        {
            UseStateMessages = false,
            HostApplicationId = null,
            SparkplugTopicFilter = "spBv1.0/#",
        };

        var act = () => options.Validate();

        act.Should().NotThrow();
    }
}
