# Sparkplug Host Application Example

This example runs a **Sparkplug B Host Application** using HiveMQtt.Sparkplug: it connects to an MQTT broker, subscribes to `spBv1.0/#`, and handles Node Birth/Death/Data and Device Birth/Data events. You can send a Rebirth command to an Edge Node by pressing Enter (target: group `example`, node `node1`).

## Prerequisites

- .NET 8 SDK
- An MQTT broker (e.g. [HiveMQ](https://www.hivemq.com/) or Mosquitto) running on `127.0.0.1:1883`

## Run

```bash
dotnet run
```

To see birth/data traffic, run an **Edge Node** (e.g. the `SparkplugEdgeNode` example) against the same broker so it publishes NBIRTH, NDATA, DBIRTH, DDATA.

## Options

Edit `Program.cs` to change:

- `WithHost` / `WithPort` — broker address
- `SparkplugTopicFilter` — e.g. `spBv1.0/myGroup/#` to scope to one group
- Rebirth target: `PublishRebirthCommandAsync("example", "node1")` — use your Edge Node's Group ID and Edge Node ID

## Links

- [Eclipse Sparkplug B 3.0](https://sparkplug.eclipse.org/specification/version/3.0)
- [HiveMQtt.Sparkplug README](../../Source/HiveMQtt.Sparkplug/README.md)
