# Sparkplug Edge Node Example

This example runs a **Sparkplug B Edge Node** using HiveMQtt.Sparkplug: it connects to an MQTT broker, publishes Node Birth (NBIRTH), subscribes to NCMD and DCMD, and publishes Node Data (NDATA) every 5 seconds with a counter and temperature metric. It handles the **Rebirth** command (NCMD with `Rebirth` metric): when received, it publishes a fresh NBIRTH. Press Q to stop (publishes NDEATH and disconnects).

## Prerequisites

- .NET 8 SDK
- An MQTT broker (e.g. [HiveMQ](https://www.hivemq.com/) or Mosquitto) running on `127.0.0.1:1883`

## Run

```bash
dotnet run
```

To send commands to this Edge Node, run a **Host Application** (e.g. the `SparkplugHostApplication` example) and use Rebirth or NCMD/DCMD targeting group `example` and node `node1`.

## Options

Edit `Program.cs` to change:

- `WithHost` / `WithPort` — broker address
- `GroupId` / `EdgeNodeId` — Sparkplug group and node identity
- Add `PublishDeviceBirthAsync` / `PublishDeviceDataAsync` for devices under this node

## Links

- [Eclipse Sparkplug B 3.0](https://sparkplug.eclipse.org/specification/version/3.0)
- [HiveMQtt.Sparkplug README](../../Source/HiveMQtt.Sparkplug/README.md)
