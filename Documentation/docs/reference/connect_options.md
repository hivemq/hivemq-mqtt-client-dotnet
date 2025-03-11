# ConnectOptions

The `ConnectOptions` class provides options for a connect call made with `ConnectAsync`. These options can override settings that were originally set in `HiveMQClientOptions`.

## Constructors

* `ConnectOptions()`: Initializes a new instance of the `ConnectOptions` class with defaults.

## Properties

* `SessionExpiryInterval`: Gets or sets the session expiry interval in seconds. This overrides any value set in HiveMQClientOptions.
    + Example: `SessionExpiryInterval = 3600` sets the session to expire in 1 hour.

* `KeepAlive`: Gets or sets the keep alive period in seconds. This overrides any value set in HiveMQClientOptions.
    + Example: `KeepAlive = 60` sets the keep alive to 60 seconds.

* `CleanStart`: Gets or sets whether to use a clean start. This overrides any value set in HiveMQClientOptions.
    + Example: `CleanStart = true` starts a new session, discarding any existing session.

## Examples

```csharp
ConnectOptions connectOptions = new ConnectOptions();
connectOptions.SessionExpiryInterval = 3600;  // 1 hour session expiry
connectOptions.KeepAlive = 60;                // 60 second keep alive
connectOptions.CleanStart = true;             // Start with a clean session

await client.ConnectAsync(connectOptions);
```

## See Also

* [HiveMQClientOptions Reference](/docs/reference/client_options)
* [Connecting to an MQTT Broker](/docs/connecting)
* [Session Handling](/docs/how-to/session-handling) 
