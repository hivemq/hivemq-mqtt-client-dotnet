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

namespace HiveMQtt.Sparkplug.Test.Validation;

using FluentAssertions;
using HiveMQtt.Sparkplug.Validation;
using NUnit.Framework;

[TestFixture]
public class SparkplugIdValidatorTest
{
    [Test]
    public void ValidateGroupId_With_Valid_Value_Does_Not_Throw()
    {
        var act = () => SparkplugIdValidator.ValidateGroupId("myGroup");
        act.Should().NotThrow();
    }

    [Test]
    public void ValidateGroupId_With_Null_Throws()
    {
        var act = () => SparkplugIdValidator.ValidateGroupId(null);
        act.Should().Throw<ArgumentException>().WithParameterName("groupId").WithMessage("*cannot be null*");
    }

    [Test]
    public void ValidateGroupId_With_Empty_Throws()
    {
        var act = () => SparkplugIdValidator.ValidateGroupId(string.Empty);
        act.Should().Throw<ArgumentException>().WithParameterName("groupId").WithMessage("*cannot be empty*");
    }

    [Test]
    public void ValidateGroupId_With_Whitespace_Throws()
    {
        var act = () => SparkplugIdValidator.ValidateGroupId("   ");
        act.Should().Throw<ArgumentException>().WithMessage("*whitespace*");
    }

    [Test]
    public void ValidateGroupId_With_Hash_When_Strict_Throws()
    {
        var act = () => SparkplugIdValidator.ValidateGroupId("group#1", "groupId", strict: true);
        act.Should().Throw<ArgumentException>().WithParameterName("groupId").WithMessage("*'#'*strict*");
    }

    [Test]
    public void ValidateGroupId_With_Plus_When_Strict_Throws()
    {
        var act = () => SparkplugIdValidator.ValidateGroupId("group+1", "groupId", strict: true);
        act.Should().Throw<ArgumentException>().WithParameterName("groupId").WithMessage("*'+'*strict*");
    }

    [Test]
    public void ValidateGroupId_With_Hash_When_Not_Strict_Does_Not_Throw()
    {
        var act = () => SparkplugIdValidator.ValidateGroupId("group#1", strict: false);
        act.Should().NotThrow();
    }

    [Test]
    public void ValidateGroupId_With_Null_Char_Throws()
    {
        var act = () => SparkplugIdValidator.ValidateIdentifier("a\0b", "id", "Test ID", strict: false);
        act.Should().Throw<ArgumentException>().WithMessage("*null*U+0000*");
    }

    [Test]
    public void ValidateEdgeNodeId_With_Valid_Value_Does_Not_Throw()
    {
        var act = () => SparkplugIdValidator.ValidateEdgeNodeId("node1");
        act.Should().NotThrow();
    }

    [Test]
    public void ValidateEdgeNodeId_With_Null_Throws()
    {
        var act = () => SparkplugIdValidator.ValidateEdgeNodeId(null);
        act.Should().Throw<ArgumentException>().WithParameterName("edgeNodeId");
    }

    [Test]
    public void ValidateDeviceId_With_Valid_Value_Does_Not_Throw()
    {
        var act = () => SparkplugIdValidator.ValidateDeviceId("device1");
        act.Should().NotThrow();
    }

    [Test]
    public void ValidateDeviceId_With_Empty_Throws()
    {
        var act = () => SparkplugIdValidator.ValidateDeviceId(string.Empty);
        act.Should().Throw<ArgumentException>().WithParameterName("deviceId");
    }

    [Test]
    public void ValidateHostApplicationId_With_Valid_Value_Does_Not_Throw()
    {
        var act = () => SparkplugIdValidator.ValidateHostApplicationId("host1");
        act.Should().NotThrow();
    }

    [Test]
    public void ValidateHostApplicationId_With_Plus_When_Strict_Throws()
    {
        var act = () => SparkplugIdValidator.ValidateHostApplicationId("host+1", strict: true);
        act.Should().Throw<ArgumentException>().WithMessage("*'+'*");
    }

    [Test]
    public void ValidateIdentifier_Exceeding_MaxLength_Throws()
    {
        var longId = new string('a', SparkplugIdValidator.MaxIdLength + 1);
        var act = () => SparkplugIdValidator.ValidateIdentifier(longId, "id", "Test", strict: false);
        act.Should().Throw<ArgumentException>().WithMessage($"*cannot exceed {SparkplugIdValidator.MaxIdLength}*");
    }

    [Test]
    public void ValidateIdentifier_At_MaxLength_Does_Not_Throw()
    {
        var maxId = new string('a', SparkplugIdValidator.MaxIdLength);
        var act = () => SparkplugIdValidator.ValidateIdentifier(maxId, "id", "Test", strict: false);
        act.Should().NotThrow();
    }
}
