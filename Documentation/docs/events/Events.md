---
sidebar_position: 0
---

# Lifecycle Events

The HiveMQ client offers a comprehensive set of built-in lifecycle events that empower users to seamlessly integrate with every aspect of the client's operation. These events serve as hooks, enabling developers to customize behavior, closely monitor activity, and extend the functionality of the client to suit their specific requirements.

## Overview

By leveraging these events, developers can dynamically modify the client's behavior at various stages of its lifecycle. Whether it's intercepting incoming or outgoing messages, performing custom authentication or authorization logic, or implementing advanced monitoring and logging capabilities, the event system provides a powerful mechanism for fine-grained control and extensibility.

## Event Categories

The client provides two categories of events:

| Category | Purpose | Use Case |
|----------|---------|----------|
| **General Events** | High-level lifecycle operations | Application logic, user notifications |
| **Packet-Level Events** | Low-level MQTT protocol activity | Debugging, protocol analysis, advanced customization |

## Common Use Cases

- **Connection monitoring**: Track when the client connects or disconnects
- **Message logging**: Log all incoming and outgoing messages
- **Custom authentication**: Implement additional auth logic before connecting
- **Debugging**: Monitor packet-level activity for troubleshooting
- **Metrics collection**: Gather statistics about client operations

These events cover a wide range of scenarios, including connection establishment and termination, message publishing and reception, subscription management, error handling, and more. By tapping into these events, developers can seamlessly integrate their own code and business logic into the client's workflow, enabling them to build robust and tailored MQTT applications that align perfectly with their unique use cases.

## Next Steps

- [Event Reference](./Reference) - Complete list of all available events
- [Usage Examples](./Examples) - Practical examples of using events
