---
sidebar_position: 100
---

# Getting Help

Need assistance with the HiveMQ MQTT Client for .NET? Here are your options, from self-service to professional support.

## Quick Links

| Resource | Best For |
|----------|----------|
| [GitHub Issues](https://github.com/hivemq/hivemq-mqtt-client-dotnet/issues) | Bug reports, feature requests |
| [Community Forum](https://community.hivemq.com/) | Questions, discussions, best practices |
| [Professional Support](https://www.hivemq.com/support/) | Enterprise assistance, SLA-backed support |

## Troubleshooting Tips

Before reaching out, try these common debugging steps:

### Enable Detailed Logging

```csharp
// Add NLog.config to your project with TRACE level
// See: /docs/how-to/configure-logging
```

### Check Connection Issues

```csharp
client.AfterConnect += (sender, args) =>
{
    Console.WriteLine($"Connection result: {args.ConnectResult.ReasonCode}");
};

client.AfterDisconnect += (sender, args) =>
{
    Console.WriteLine($"Disconnected. Clean: {args.CleanDisconnect}");
};
```

### Verify Broker Connectivity

```bash
# Test with mosquitto client (if available)
mosquitto_sub -h your-broker.com -p 1883 -t test/topic
```

## Community Forum

The [HiveMQ Community Forum](https://community.hivemq.com/) is an excellent place to:

- Ask questions and get answers from the community
- Share your experiences and solutions
- Discuss best practices and architecture patterns
- Connect with other MQTT developers

## GitHub Repository

For technical issues specific to this client library:

- [Open an Issue](https://github.com/hivemq/hivemq-mqtt-client-dotnet/issues/new) - Bug reports and feature requests
- [View Source Code](https://github.com/hivemq/hivemq-mqtt-client-dotnet) - Review implementation details
- [Submit a Pull Request](https://github.com/hivemq/hivemq-mqtt-client-dotnet/pulls) - Contribute improvements

## Professional Support

For enterprise customers or those requiring guaranteed response times:

- [HiveMQ Support Plans](https://www.hivemq.com/support/) - SLA-backed professional support
- [Contact Sales](https://www.hivemq.com/contact/) - Enterprise licensing and custom solutions

## Additional Resources

### Learning
- [MQTT Essentials](https://www.hivemq.com/mqtt-essentials/) - Comprehensive MQTT guide
- [HiveMQ Blog](https://www.hivemq.com/blog/) - Latest updates and best practices

### Tools
- [MQTT Toolbox](https://www.hivemq.com/mqtt-toolbox/) - Testing and debugging tools
- [HiveMQ Public Broker](http://www.mqtt-dashboard.com) - Free broker for testing

### Documentation
- [HiveMQ Documentation](https://www.hivemq.com/docs/) - Broker documentation
- [MQTT 5.0 Specification](https://docs.oasis-open.org/mqtt/mqtt/v5.0/mqtt-v5.0.html) - Protocol specification
