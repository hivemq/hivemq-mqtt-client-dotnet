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

namespace HiveMQtt.Sparkplug.Payload;

using System;
using Google.Protobuf;
using HiveMQtt.Sparkplug.Protobuf;

/// <summary>
/// Builder for creating Sparkplug B metrics with a fluent API.
/// </summary>
public sealed class SparkplugMetricBuilder
{
    private readonly Protobuf.Payload.Types.Metric metric = new();

    /// <summary>
    /// Sets the metric name.
    /// </summary>
    /// <param name="name">The metric name.</param>
    /// <returns>This builder instance for chaining.</returns>
    public SparkplugMetricBuilder WithName(string name)
    {
        this.metric.Name = name ?? throw new ArgumentNullException(nameof(name));
        return this;
    }

    /// <summary>
    /// Sets the metric alias.
    /// </summary>
    /// <param name="alias">The metric alias.</param>
    /// <returns>This builder instance for chaining.</returns>
    public SparkplugMetricBuilder WithAlias(ulong alias)
    {
        this.metric.Alias = alias;
        return this;
    }

    /// <summary>
    /// Sets the metric timestamp.
    /// </summary>
    /// <param name="timestamp">The timestamp in milliseconds since epoch.</param>
    /// <returns>This builder instance for chaining.</returns>
    public SparkplugMetricBuilder WithTimestamp(ulong timestamp)
    {
        this.metric.Timestamp = timestamp;
        return this;
    }

    /// <summary>
    /// Sets the metric timestamp to the current time.
    /// </summary>
    /// <returns>This builder instance for chaining.</returns>
    public SparkplugMetricBuilder WithCurrentTimestamp()
    {
        this.metric.Timestamp = SparkplugPayloadEncoder.GetCurrentTimestamp();
        return this;
    }

    /// <summary>
    /// Sets the metric as historical.
    /// </summary>
    /// <param name="isHistorical">True if the metric is historical.</param>
    /// <returns>This builder instance for chaining.</returns>
    public SparkplugMetricBuilder IsHistorical(bool isHistorical = true)
    {
        this.metric.IsHistorical = isHistorical;
        return this;
    }

    /// <summary>
    /// Sets the metric as transient.
    /// </summary>
    /// <param name="isTransient">True if the metric is transient.</param>
    /// <returns>This builder instance for chaining.</returns>
    public SparkplugMetricBuilder IsTransient(bool isTransient = true)
    {
        this.metric.IsTransient = isTransient;
        return this;
    }

    /// <summary>
    /// Sets the metric as null.
    /// </summary>
    /// <param name="dataType">The data type of the null metric.</param>
    /// <returns>This builder instance for chaining.</returns>
    public SparkplugMetricBuilder AsNull(DataType dataType)
    {
        this.metric.IsNull = true;
        this.metric.Datatype = (uint)dataType;
        return this;
    }

    /// <summary>
    /// Sets the metric value to an Int8 (sbyte).
    /// </summary>
    /// <param name="value">The sbyte value.</param>
    /// <returns>This builder instance for chaining.</returns>
    public SparkplugMetricBuilder WithInt8Value(sbyte value)
    {
        this.metric.Datatype = (uint)DataType.Int8;
        this.metric.IntValue = unchecked((uint)(byte)value);
        return this;
    }

    /// <summary>
    /// Sets the metric value to an Int16 (short).
    /// </summary>
    /// <param name="value">The short value.</param>
    /// <returns>This builder instance for chaining.</returns>
    public SparkplugMetricBuilder WithInt16Value(short value)
    {
        this.metric.Datatype = (uint)DataType.Int16;
        this.metric.IntValue = unchecked((uint)(ushort)value);
        return this;
    }

    /// <summary>
    /// Sets the metric value to an Int32 (int).
    /// </summary>
    /// <param name="value">The int value.</param>
    /// <returns>This builder instance for chaining.</returns>
    public SparkplugMetricBuilder WithInt32Value(int value)
    {
        this.metric.Datatype = (uint)DataType.Int32;
        this.metric.IntValue = unchecked((uint)value);
        return this;
    }

    /// <summary>
    /// Sets the metric value to an Int64 (long).
    /// </summary>
    /// <param name="value">The long value.</param>
    /// <returns>This builder instance for chaining.</returns>
    public SparkplugMetricBuilder WithInt64Value(long value)
    {
        this.metric.Datatype = (uint)DataType.Int64;
        this.metric.LongValue = unchecked((ulong)value);
        return this;
    }

    /// <summary>
    /// Sets the metric value to a UInt8 (byte).
    /// </summary>
    /// <param name="value">The byte value.</param>
    /// <returns>This builder instance for chaining.</returns>
    public SparkplugMetricBuilder WithUInt8Value(byte value)
    {
        this.metric.Datatype = (uint)DataType.Uint8;
        this.metric.IntValue = value;
        return this;
    }

    /// <summary>
    /// Sets the metric value to a UInt16 (ushort).
    /// </summary>
    /// <param name="value">The ushort value.</param>
    /// <returns>This builder instance for chaining.</returns>
    public SparkplugMetricBuilder WithUInt16Value(ushort value)
    {
        this.metric.Datatype = (uint)DataType.Uint16;
        this.metric.IntValue = value;
        return this;
    }

    /// <summary>
    /// Sets the metric value to a UInt32 (uint).
    /// </summary>
    /// <param name="value">The uint value.</param>
    /// <returns>This builder instance for chaining.</returns>
    public SparkplugMetricBuilder WithUInt32Value(uint value)
    {
        this.metric.Datatype = (uint)DataType.Uint32;
        this.metric.IntValue = value;
        return this;
    }

    /// <summary>
    /// Sets the metric value to a UInt64 (ulong).
    /// </summary>
    /// <param name="value">The ulong value.</param>
    /// <returns>This builder instance for chaining.</returns>
    public SparkplugMetricBuilder WithUInt64Value(ulong value)
    {
        this.metric.Datatype = (uint)DataType.Uint64;
        this.metric.LongValue = value;
        return this;
    }

    /// <summary>
    /// Sets the metric value to a float.
    /// </summary>
    /// <param name="value">The float value.</param>
    /// <returns>This builder instance for chaining.</returns>
    public SparkplugMetricBuilder WithFloatValue(float value)
    {
        this.metric.Datatype = (uint)DataType.Float;
        this.metric.FloatValue = value;
        return this;
    }

    /// <summary>
    /// Sets the metric value to a double.
    /// </summary>
    /// <param name="value">The double value.</param>
    /// <returns>This builder instance for chaining.</returns>
    public SparkplugMetricBuilder WithDoubleValue(double value)
    {
        this.metric.Datatype = (uint)DataType.Double;
        this.metric.DoubleValue = value;
        return this;
    }

    /// <summary>
    /// Sets the metric value to a boolean.
    /// </summary>
    /// <param name="value">The boolean value.</param>
    /// <returns>This builder instance for chaining.</returns>
    public SparkplugMetricBuilder WithBooleanValue(bool value)
    {
        this.metric.Datatype = (uint)DataType.Boolean;
        this.metric.BooleanValue = value;
        return this;
    }

    /// <summary>
    /// Sets the metric value to a string.
    /// </summary>
    /// <param name="value">The string value.</param>
    /// <returns>This builder instance for chaining.</returns>
    public SparkplugMetricBuilder WithStringValue(string value)
    {
        this.metric.Datatype = (uint)DataType.String;
        this.metric.StringValue = value ?? throw new ArgumentNullException(nameof(value));
        return this;
    }

    /// <summary>
    /// Sets the metric value to a DateTime (as milliseconds since epoch).
    /// </summary>
    /// <param name="value">The DateTimeOffset value.</param>
    /// <returns>This builder instance for chaining.</returns>
    public SparkplugMetricBuilder WithDateTimeValue(DateTimeOffset value)
    {
        this.metric.Datatype = (uint)DataType.DateTime;
        this.metric.LongValue = (ulong)value.ToUnixTimeMilliseconds();
        return this;
    }

    /// <summary>
    /// Sets the metric value to a DateTime (as milliseconds since epoch).
    /// </summary>
    /// <param name="millisecondsSinceEpoch">The timestamp as milliseconds since Unix epoch.</param>
    /// <returns>This builder instance for chaining.</returns>
    public SparkplugMetricBuilder WithDateTimeValue(ulong millisecondsSinceEpoch)
    {
        this.metric.Datatype = (uint)DataType.DateTime;
        this.metric.LongValue = millisecondsSinceEpoch;
        return this;
    }

    /// <summary>
    /// Sets the metric value to text.
    /// </summary>
    /// <param name="value">The text value.</param>
    /// <returns>This builder instance for chaining.</returns>
    public SparkplugMetricBuilder WithTextValue(string value)
    {
        this.metric.Datatype = (uint)DataType.Text;
        this.metric.StringValue = value ?? throw new ArgumentNullException(nameof(value));
        return this;
    }

    /// <summary>
    /// Sets the metric value to a UUID string.
    /// </summary>
    /// <param name="uuid">The UUID string value.</param>
    /// <returns>This builder instance for chaining.</returns>
    public SparkplugMetricBuilder WithUuidValue(string uuid)
    {
        this.metric.Datatype = (uint)DataType.Uuid;
        this.metric.StringValue = uuid ?? throw new ArgumentNullException(nameof(uuid));
        return this;
    }

    /// <summary>
    /// Sets the metric value to a UUID.
    /// </summary>
    /// <param name="uuid">The GUID value.</param>
    /// <returns>This builder instance for chaining.</returns>
    public SparkplugMetricBuilder WithUuidValue(Guid uuid)
    {
        return this.WithUuidValue(uuid.ToString());
    }

    /// <summary>
    /// Sets the metric value to bytes.
    /// </summary>
    /// <param name="value">The byte array value.</param>
    /// <returns>This builder instance for chaining.</returns>
    public SparkplugMetricBuilder WithBytesValue(byte[] value)
    {
        this.metric.Datatype = (uint)DataType.Bytes;
        this.metric.BytesValue = ByteString.CopyFrom(value ?? throw new ArgumentNullException(nameof(value)));
        return this;
    }

    /// <summary>
    /// Builds the metric.
    /// </summary>
    /// <returns>The constructed metric.</returns>
    public Protobuf.Payload.Types.Metric Build()
    {
        return this.metric.Clone();
    }

    /// <summary>
    /// Creates a new metric builder.
    /// </summary>
    /// <returns>A new builder instance.</returns>
    public static SparkplugMetricBuilder Create() => new();

    /// <summary>
    /// Creates a new metric builder with the specified name.
    /// </summary>
    /// <param name="name">The metric name.</param>
    /// <returns>A new builder instance.</returns>
    public static SparkplugMetricBuilder Create(string name) => new SparkplugMetricBuilder().WithName(name);
}
