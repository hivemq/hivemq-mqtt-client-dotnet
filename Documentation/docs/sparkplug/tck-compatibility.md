---
sidebar_position: 6
---

# TCK Compatibility

The [Eclipse Sparkplug TCK](https://sparkplug.eclipse.org/specification/tck-process) validates Sparkplug compatibility for implementations in an interoperable ecosystem.

## What This Means for HiveMQtt.Sparkplug

- `HiveMQtt.Sparkplug` is a **client library** that implements Sparkplug Host Application and Edge Node roles.
- The official TCK process is commonly executed against Sparkplug-capable brokers, with clients participating in test scenarios.
- This library is designed to align with [Sparkplug B 3.0](https://sparkplug.eclipse.org/specification/version/3.0) and `sparkplug_b.proto`.

## Interoperability Guidance

- Use HiveMQtt.Sparkplug with Sparkplug-capable MQTT brokers.
- Validate broker compliance with the official Sparkplug TCK.
- Use this client as Host and Edge participants in your interoperability tests.

## References

- [Sparkplug B 3.0 Specification](https://sparkplug.eclipse.org/specification/version/3.0)
- [Sparkplug TCK Process](https://sparkplug.eclipse.org/specification/tck-process)
- [sparkplug_b.proto (Eclipse Tahu)](https://github.com/eclipse-tahu/tahu/blob/master/sparkplug_b/sparkplug_b.proto)
