---
sidebar_position: 5
---

# Payloads and Metrics

`HiveMQtt.Sparkplug` supports Sparkplug B protobuf payloads and metric composition for both Host and Edge workflows.

## Metric Builder

For scalar and simple metric types, use `SparkplugMetricBuilder`:

```csharp
var temperatureMetric = SparkplugMetricBuilder
    .Create("temperature")
    .WithFloatValue(22.5)
    .Build();
```

## DataSet, Template, and File Metrics

For complex metric types, construct payload metrics using protobuf types:

1. Build a base metric and then set `Datatype` and the complex payload fields.
2. Or create `Payload.Types.Metric` directly and populate all required members.

Example pattern:

```csharp
var metric = new Payload.Types.Metric
{
    Name = "datasetExample",
    Datatype = (uint)DataType.DataSet,
    DatasetValue = new Payload.Types.DataSet
    {
        NumOfColumns = 2,
        Columns = { "col1", "col2" },
        Types = { (uint)DataType.String, (uint)DataType.Int32 }
    }
};
```

See Sparkplug protobuf schema details:

- [sparkplug_b.proto (Eclipse Tahu)](https://github.com/eclipse-tahu/tahu/blob/master/sparkplug_b/sparkplug_b.proto)

## Topic Model

Sparkplug topics follow:

`spBv1.0/{group_id}/{message_type}/{edge_node_id}[/{device_id}]`

Use topic build/parse helpers in the Sparkplug package where available to reduce formatting errors.

## See Also

- [Sparkplug Quickstart](/docs/sparkplug/quickstart)
- [Edge Node Guide](/docs/sparkplug/edge-node)
- [TCK Compatibility](/docs/sparkplug/tck-compatibility)
