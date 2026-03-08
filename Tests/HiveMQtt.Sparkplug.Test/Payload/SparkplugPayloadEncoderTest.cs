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
using Google.Protobuf;
using HiveMQtt.Sparkplug.Payload;
using HiveMQtt.Sparkplug.Protobuf;
using NUnit.Framework;

[TestFixture]
public class SparkplugPayloadEncoderTest
{
    [Test]
    public void Encode_WithValidPayload_ReturnsBytes()
    {
        var payload = new Protobuf.Payload
        {
            Timestamp = 1234567890UL,
            Seq = 42UL,
        };

        var bytes = SparkplugPayloadEncoder.Encode(payload);

        bytes.Should().NotBeNull();
        bytes.Length.Should().BeGreaterThan(0);
    }

    [Test]
    public void Encode_WithNullPayload_ThrowsArgumentNullException()
    {
        var action = () => SparkplugPayloadEncoder.Encode(null!);
        action.Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void Decode_WithValidBytes_ReturnsPayload()
    {
        var original = new Protobuf.Payload
        {
            Timestamp = 1234567890UL,
            Seq = 42UL,
        };
        var bytes = original.ToByteArray();

        var decoded = SparkplugPayloadEncoder.Decode(bytes);

        decoded.Timestamp.Should().Be(1234567890UL);
        decoded.Seq.Should().Be(42UL);
    }

    [Test]
    public void Decode_WithNullBytes_ThrowsArgumentNullException()
    {
        var action = () => SparkplugPayloadEncoder.Decode(null!);
        action.Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void Decode_ReadOnlySpan_WithValidBytes_ReturnsPayload()
    {
        var original = new Protobuf.Payload
        {
            Timestamp = 9876543210UL,
            Seq = 100UL,
        };
        var bytes = original.ToByteArray();
        ReadOnlySpan<byte> span = bytes;

        var decoded = SparkplugPayloadEncoder.Decode(span);

        decoded.Timestamp.Should().Be(9876543210UL);
        decoded.Seq.Should().Be(100UL);
    }

    [Test]
    public void TryDecode_WithValidBytes_ReturnsTrueAndPayload()
    {
        var original = new Protobuf.Payload
        {
            Timestamp = 1234567890UL,
            Seq = 42UL,
        };
        var bytes = original.ToByteArray();

        var success = SparkplugPayloadEncoder.TryDecode(bytes, out var payload);

        success.Should().BeTrue();
        payload.Should().NotBeNull();
        payload!.Timestamp.Should().Be(1234567890UL);
    }

    [Test]
    public void TryDecode_WithNullBytes_ReturnsFalse()
    {
        var success = SparkplugPayloadEncoder.TryDecode(null!, out var payload);

        success.Should().BeFalse();
        payload.Should().BeNull();
    }

    [Test]
    public void TryDecode_WithEmptyBytes_ReturnsFalse()
    {
        var success = SparkplugPayloadEncoder.TryDecode(Array.Empty<byte>(), out var payload);

        success.Should().BeFalse();
        payload.Should().BeNull();
    }

    [Test]
    public void EncodeAndDecode_RoundTrip_PreservesData()
    {
        var original = new Protobuf.Payload
        {
            Timestamp = 1234567890UL,
            Seq = 42UL,
            Uuid = "test-uuid-1234",
        };

        var metric = new Protobuf.Payload.Types.Metric
        {
            Name = "temperature",
            Datatype = (uint)DataType.Double,
            DoubleValue = 25.5,
        };
        original.Metrics.Add(metric);

        var bytes = SparkplugPayloadEncoder.Encode(original);
        var decoded = SparkplugPayloadEncoder.Decode(bytes);

        decoded.Timestamp.Should().Be(original.Timestamp);
        decoded.Seq.Should().Be(original.Seq);
        decoded.Uuid.Should().Be(original.Uuid);
        decoded.Metrics.Should().HaveCount(1);
        decoded.Metrics[0].Name.Should().Be("temperature");
        decoded.Metrics[0].DoubleValue.Should().Be(25.5);
    }

    [Test]
    public void CreatePayload_WithTimestampAndSequence_CreatesPayload()
    {
        var payload = SparkplugPayloadEncoder.CreatePayload(1234567890UL, 42);

        payload.Timestamp.Should().Be(1234567890UL);
        payload.Seq.Should().Be(42UL);
    }

    [Test]
    public void CreatePayload_WithOnlySequence_SetsCurrentTimestamp()
    {
        var beforeTime = (ulong)DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        var payload = SparkplugPayloadEncoder.CreatePayload(42);
        var afterTime = (ulong)DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        payload.Timestamp.Should().BeInRange(beforeTime, afterTime);
        payload.Seq.Should().Be(42UL);
    }

    [Test]
    public void CreatePayload_WithSequenceZero_CreatesValidPayload()
    {
        var payload = SparkplugPayloadEncoder.CreatePayload(1000UL, 0);
        payload.Seq.Should().Be(0UL);
    }

    [Test]
    public void CreatePayload_WithSequence255_CreatesValidPayload()
    {
        var payload = SparkplugPayloadEncoder.CreatePayload(1000UL, 255);
        payload.Seq.Should().Be(255UL);
    }

    [Test]
    public void CreatePayload_WithNegativeSequence_ThrowsArgumentOutOfRangeException()
    {
        var action = () => SparkplugPayloadEncoder.CreatePayload(1000UL, -1);
        action.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Test]
    public void CreatePayload_WithSequenceAbove255_ThrowsArgumentOutOfRangeException()
    {
        var action = () => SparkplugPayloadEncoder.CreatePayload(1000UL, 256);
        action.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Test]
    public void NextSequenceNumber_IncrementsByOne()
    {
        SparkplugPayloadEncoder.NextSequenceNumber(0).Should().Be(1);
        SparkplugPayloadEncoder.NextSequenceNumber(100).Should().Be(101);
        SparkplugPayloadEncoder.NextSequenceNumber(254).Should().Be(255);
    }

    [Test]
    public void NextSequenceNumber_WrapsFrom255ToZero() =>
        SparkplugPayloadEncoder.NextSequenceNumber(255).Should().Be(0);

    [Test]
    public void GetCurrentTimestamp_ReturnsReasonableValue()
    {
        var beforeTime = (ulong)DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        var timestamp = SparkplugPayloadEncoder.GetCurrentTimestamp();
        var afterTime = (ulong)DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        timestamp.Should().BeInRange(beforeTime, afterTime);
    }

    [Test]
    public void MaxSequenceNumber_Is255() =>
        SparkplugPayloadEncoder.MaxSequenceNumber.Should().Be(255);
}
