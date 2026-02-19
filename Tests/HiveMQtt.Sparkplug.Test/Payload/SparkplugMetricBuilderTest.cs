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

namespace HiveMQtt.Sparkplug.Test.Payload;

using FluentAssertions;
using HiveMQtt.Sparkplug.Payload;
using HiveMQtt.Sparkplug.Protobuf;
using NUnit.Framework;

[TestFixture]
public class SparkplugMetricBuilderTest
{
    [Test]
    public void Create_ReturnsNewBuilder()
    {
        var builder = SparkplugMetricBuilder.Create();
        builder.Should().NotBeNull();
    }

    [Test]
    public void Create_WithName_SetsName()
    {
        var builder = SparkplugMetricBuilder.Create("temperature");
        var metric = builder.Build();

        metric.Name.Should().Be("temperature");
    }

    [Test]
    public void WithName_SetsMetricName()
    {
        var metric = SparkplugMetricBuilder.Create()
            .WithName("pressure")
            .Build();

        metric.Name.Should().Be("pressure");
    }

    [Test]
    public void WithAlias_SetsMetricAlias()
    {
        var metric = SparkplugMetricBuilder.Create()
            .WithAlias(12345UL)
            .Build();

        metric.Alias.Should().Be(12345UL);
    }

    [Test]
    public void WithTimestamp_SetsMetricTimestamp()
    {
        var metric = SparkplugMetricBuilder.Create()
            .WithTimestamp(1234567890UL)
            .Build();

        metric.Timestamp.Should().Be(1234567890UL);
    }

    [Test]
    public void WithCurrentTimestamp_SetsReasonableTimestamp()
    {
        var beforeTime = (ulong)DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        var metric = SparkplugMetricBuilder.Create()
            .WithCurrentTimestamp()
            .Build();
        var afterTime = (ulong)DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        metric.Timestamp.Should().BeInRange(beforeTime, afterTime);
    }

    [Test]
    public void IsHistorical_SetsIsHistoricalFlag()
    {
        var metric = SparkplugMetricBuilder.Create()
            .IsHistorical()
            .Build();

        metric.IsHistorical.Should().BeTrue();
    }

    [Test]
    public void IsTransient_SetsIsTransientFlag()
    {
        var metric = SparkplugMetricBuilder.Create()
            .IsTransient()
            .Build();

        metric.IsTransient.Should().BeTrue();
    }

    [Test]
    public void AsNull_SetsIsNullFlagAndDataType()
    {
        var metric = SparkplugMetricBuilder.Create()
            .AsNull(DataType.Double)
            .Build();

        metric.IsNull.Should().BeTrue();
        metric.Datatype.Should().Be((uint)DataType.Double);
    }

    [Test]
    public void WithInt8Value_SetsCorrectDataTypeAndValue()
    {
        var metric = SparkplugMetricBuilder.Create()
            .WithInt8Value(-10)
            .Build();

        metric.Datatype.Should().Be((uint)DataType.Int8);
    }

    [Test]
    public void WithInt16Value_SetsCorrectDataTypeAndValue()
    {
        var metric = SparkplugMetricBuilder.Create()
            .WithInt16Value(-1000)
            .Build();

        metric.Datatype.Should().Be((uint)DataType.Int16);
    }

    [Test]
    public void WithInt32Value_SetsCorrectDataTypeAndValue()
    {
        var metric = SparkplugMetricBuilder.Create()
            .WithInt32Value(-100000)
            .Build();

        metric.Datatype.Should().Be((uint)DataType.Int32);
    }

    [Test]
    public void WithInt64Value_SetsCorrectDataTypeAndValue()
    {
        var metric = SparkplugMetricBuilder.Create()
            .WithInt64Value(-1234567890123L)
            .Build();

        metric.Datatype.Should().Be((uint)DataType.Int64);
    }

    [Test]
    public void WithUInt8Value_SetsCorrectDataTypeAndValue()
    {
        var metric = SparkplugMetricBuilder.Create()
            .WithUInt8Value(200)
            .Build();

        metric.Datatype.Should().Be((uint)DataType.Uint8);
        metric.IntValue.Should().Be(200u);
    }

    [Test]
    public void WithUInt16Value_SetsCorrectDataTypeAndValue()
    {
        var metric = SparkplugMetricBuilder.Create()
            .WithUInt16Value(50000)
            .Build();

        metric.Datatype.Should().Be((uint)DataType.Uint16);
        metric.IntValue.Should().Be(50000u);
    }

    [Test]
    public void WithUInt32Value_SetsCorrectDataTypeAndValue()
    {
        var metric = SparkplugMetricBuilder.Create()
            .WithUInt32Value(3000000000u)
            .Build();

        metric.Datatype.Should().Be((uint)DataType.Uint32);
        metric.IntValue.Should().Be(3000000000u);
    }

    [Test]
    public void WithUInt64Value_SetsCorrectDataTypeAndValue()
    {
        var metric = SparkplugMetricBuilder.Create()
            .WithUInt64Value(12345678901234567890UL)
            .Build();

        metric.Datatype.Should().Be((uint)DataType.Uint64);
        metric.LongValue.Should().Be(12345678901234567890UL);
    }

    [Test]
    public void WithFloatValue_SetsCorrectDataTypeAndValue()
    {
        var metric = SparkplugMetricBuilder.Create()
            .WithFloatValue(3.14f)
            .Build();

        metric.Datatype.Should().Be((uint)DataType.Float);
        metric.FloatValue.Should().BeApproximately(3.14f, 0.001f);
    }

    [Test]
    public void WithDoubleValue_SetsCorrectDataTypeAndValue()
    {
        var metric = SparkplugMetricBuilder.Create()
            .WithDoubleValue(3.14159265359)
            .Build();

        metric.Datatype.Should().Be((uint)DataType.Double);
        metric.DoubleValue.Should().BeApproximately(3.14159265359, 0.0000001);
    }

    [Test]
    public void WithBooleanValue_True_SetsCorrectDataTypeAndValue()
    {
        var metric = SparkplugMetricBuilder.Create()
            .WithBooleanValue(true)
            .Build();

        metric.Datatype.Should().Be((uint)DataType.Boolean);
        metric.BooleanValue.Should().BeTrue();
    }

    [Test]
    public void WithBooleanValue_False_SetsCorrectDataTypeAndValue()
    {
        var metric = SparkplugMetricBuilder.Create()
            .WithBooleanValue(false)
            .Build();

        metric.Datatype.Should().Be((uint)DataType.Boolean);
        metric.BooleanValue.Should().BeFalse();
    }

    [Test]
    public void WithStringValue_SetsCorrectDataTypeAndValue()
    {
        var metric = SparkplugMetricBuilder.Create()
            .WithStringValue("hello world")
            .Build();

        metric.Datatype.Should().Be((uint)DataType.String);
        metric.StringValue.Should().Be("hello world");
    }

    [Test]
    public void WithDateTimeValue_DateTimeOffset_SetsCorrectDataTypeAndValue()
    {
        var dateTime = new DateTimeOffset(2024, 6, 15, 12, 30, 0, TimeSpan.Zero);
        var metric = SparkplugMetricBuilder.Create()
            .WithDateTimeValue(dateTime)
            .Build();

        metric.Datatype.Should().Be((uint)DataType.DateTime);
        metric.LongValue.Should().Be((ulong)dateTime.ToUnixTimeMilliseconds());
    }

    [Test]
    public void WithDateTimeValue_Ulong_SetsCorrectDataTypeAndValue()
    {
        var metric = SparkplugMetricBuilder.Create()
            .WithDateTimeValue(1718451000000UL)
            .Build();

        metric.Datatype.Should().Be((uint)DataType.DateTime);
        metric.LongValue.Should().Be(1718451000000UL);
    }

    [Test]
    public void WithTextValue_SetsCorrectDataTypeAndValue()
    {
        var metric = SparkplugMetricBuilder.Create()
            .WithTextValue("This is a longer text value.")
            .Build();

        metric.Datatype.Should().Be((uint)DataType.Text);
        metric.StringValue.Should().Be("This is a longer text value.");
    }

    [Test]
    public void WithUuidValue_String_SetsCorrectDataTypeAndValue()
    {
        var uuid = "550e8400-e29b-41d4-a716-446655440000";
        var metric = SparkplugMetricBuilder.Create()
            .WithUuidValue(uuid)
            .Build();

        metric.Datatype.Should().Be((uint)DataType.Uuid);
        metric.StringValue.Should().Be(uuid);
    }

    [Test]
    public void WithUuidValue_Guid_SetsCorrectDataTypeAndValue()
    {
        var guid = Guid.NewGuid();
        var metric = SparkplugMetricBuilder.Create()
            .WithUuidValue(guid)
            .Build();

        metric.Datatype.Should().Be((uint)DataType.Uuid);
        metric.StringValue.Should().Be(guid.ToString());
    }

    [Test]
    public void WithBytesValue_SetsCorrectDataTypeAndValue()
    {
        var bytes = new byte[] { 0x01, 0x02, 0x03, 0x04 };
        var metric = SparkplugMetricBuilder.Create()
            .WithBytesValue(bytes)
            .Build();

        metric.Datatype.Should().Be((uint)DataType.Bytes);
        metric.BytesValue.ToByteArray().Should().BeEquivalentTo(bytes);
    }

    [Test]
    public void Build_ReturnsClonedMetric()
    {
        var builder = SparkplugMetricBuilder.Create("test")
            .WithDoubleValue(42.0);

        var metric1 = builder.Build();
        var metric2 = builder.Build();

        metric1.Should().NotBeSameAs(metric2);
        metric1.Name.Should().Be(metric2.Name);
        metric1.DoubleValue.Should().Be(metric2.DoubleValue);
    }

    [Test]
    public void FluentChaining_AllMethodsReturnBuilder()
    {
        var metric = SparkplugMetricBuilder.Create()
            .WithName("sensor")
            .WithAlias(100)
            .WithCurrentTimestamp()
            .IsHistorical(false)
            .IsTransient(false)
            .WithDoubleValue(25.5)
            .Build();

        metric.Name.Should().Be("sensor");
        metric.Alias.Should().Be(100UL);
        metric.DoubleValue.Should().Be(25.5);
    }

    [Test]
    public void WithName_NullValue_ThrowsArgumentNullException()
    {
        var action = () => SparkplugMetricBuilder.Create().WithName(null!);
        action.Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void WithStringValue_NullValue_ThrowsArgumentNullException()
    {
        var action = () => SparkplugMetricBuilder.Create().WithStringValue(null!);
        action.Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void WithTextValue_NullValue_ThrowsArgumentNullException()
    {
        var action = () => SparkplugMetricBuilder.Create().WithTextValue(null!);
        action.Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void WithUuidValue_NullString_ThrowsArgumentNullException()
    {
        var action = () => SparkplugMetricBuilder.Create().WithUuidValue(null!);
        action.Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void WithBytesValue_NullValue_ThrowsArgumentNullException()
    {
        var action = () => SparkplugMetricBuilder.Create().WithBytesValue(null!);
        action.Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void CompleteMetric_CanBeEncodedAndDecoded()
    {
        var metric = SparkplugMetricBuilder.Create("temperature")
            .WithAlias(1)
            .WithTimestamp(1234567890UL)
            .WithDoubleValue(25.5)
            .Build();

        var payload = new Protobuf.Payload();
        payload.Metrics.Add(metric);

        var bytes = SparkplugPayloadEncoder.Encode(payload);
        var decoded = SparkplugPayloadEncoder.Decode(bytes);

        decoded.Metrics.Should().HaveCount(1);
        decoded.Metrics[0].Name.Should().Be("temperature");
        decoded.Metrics[0].Alias.Should().Be(1UL);
        decoded.Metrics[0].DoubleValue.Should().Be(25.5);
    }
}
