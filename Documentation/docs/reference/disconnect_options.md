# DisconnectOptions

The `DisconnectOptions` class provides options for a disconnect call made with `DisconnectAsync`.

## Constructors

* `DisconnectOptions()`: Initializes a new instance of the `DisconnectOptions` class with defaults.

## Properties

* `ReasonCode`: Gets or sets the reason code for the disconnection. The default value is `NormalDisconnection`.
	+ Possible values are details in [DisconnectReasonCode enum](https://github.com/hivemq/hivemq-mqtt-client-dotnet/blob/main/Source/HiveMQtt/MQTT5/ReasonCodes/DisconnectReasonCode.cs).
* `SessionExpiry`: Gets or sets the session expiry in seconds. This sets the expiration for the session to the indicated value.
	+ Example: `SessionExpiry = 3600` sets the session to expire in 1 hour.
* `ReasonString`: Gets or sets the reason string for the disconnection. This is a human-readable string used for diagnostics only.
	+ Example: `ReasonString = "Device power save mode."`
* `UserProperties`: Gets or sets the user properties for the disconnection. This is a dictionary of key-value pairs.
	+ Example: `UserProperties = new Dictionary<string, string> { { "device-id", "xrw02k-224a" }, { "TZ", "CEST" } }` sets the user properties to include in the disconnect call.

## Examples

```csharp
DisconnectOptions disconnectOptions = new DisconnectOptions();
disconnectOptions.ReasonCode = DisconnectReasonCode.NormalDisconnection;
disconnectOptions.SessionExpiry = 3600;
disconnectOptions.ReasonString = "Device power save mode";
disconnectOptions.UserProperties = new Dictionary<string, string> { { "device-id", "xrw02k-224a" }, { "TZ", "CEST" } };

await client.DisconnectAsync(disconnectOptions);
```
