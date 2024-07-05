# Automatic Reconnect

The HiveMQtt MQTT library provides an automatic reconnect functionality that allows the client to automatically reconnect to the MQTT broker in case of a disconnection. This feature is disabled by default.

# Example

```csharp
var options = new HiveMQClientOptionsBuilder()
                    .WithAutomaticReconnect(true)
                    .Build();

// Create a new client with the configured options
var client = new HiveMQttClient(options);
```

# Backoff Strategy

The automatic reconnect functionality uses a backoff strategy to attempt to reconnect to the MQTT broker periodically until success. The backoff strategy starts with a delay of 5 seconds and doubles the delay with each failed attempt, up to a maximum of 1 minute.

# Maximum Attempts

The backoff strategy will attempt to reconnect a maximum of once per minute.  The client will attempt to reconnect indefinitely until successful.

# Summary

The automatic reconnect functionality a convenient way to handle disconnections from the MQTT broker. Users can also use the `OnConnect` event handler to add custom logic when the client successfully reconnects to the MQTT broker.
