# ConnectOptionsBuilder

The `ConnectOptionsBuilder` class is a builder pattern implementation that provides a convenient way to construct `ConnectOptions` objects for configuring connect behavior in HiveMQtt client applications.

## Methods

### `WithSessionExpiryInterval(long sessionExpiryInterval)`

Sets the session expiry interval for the connection.

- **Description:** Specifies the duration, in seconds, for which the session state will be maintained by the broker. This overrides any value set in HiveMQClientOptions.
- **Example:** 
  ```csharp
  .WithSessionExpiryInterval(3600) // 1 hour session expiry
  ```

### `WithKeepAlive(int keepAlive)`

Sets the keep alive period for the connection.

- **Description:** Specifies the maximum time interval that is permitted to elapse between the point at which the Client finishes transmitting one Control Packet and the point it starts sending the next. This overrides any value set in HiveMQClientOptions.
- **Example:** 
  ```csharp
  .WithKeepAlive(60) // 60 second keep alive
  ```

### `WithCleanStart(bool cleanStart)`

Sets whether to use a clean start for the connection.

- **Description:** Specifies whether the Connection starts a new Session or is a continuation of an existing Session. This overrides any value set in HiveMQClientOptions.
- **Example:** 
  ```csharp
  .WithCleanStart(true) // Start with a clean session
  ```

### `Build()`

Builds the ConnectOptions instance.

- **Description:** Creates and returns a new ConnectOptions object with all the configured settings.
- **Example:**
  ```csharp
  ConnectOptions options = new ConnectOptionsBuilder()
      .WithSessionExpiryInterval(3600)
      .WithKeepAlive(60)
      .WithCleanStart(true)
      .Build();
  ```

## Complete Example

```csharp
var connectOptions = new ConnectOptionsBuilder()
    .WithSessionExpiryInterval(3600)  // 1 hour session expiry
    .WithKeepAlive(60)               // 60 second keep alive
    .WithCleanStart(true)            // Start with a clean session
    .Build();

await client.ConnectAsync(connectOptions);
```

## See Also

* [ConnectOptions Reference](/docs/reference/connect_options)
* [HiveMQClientOptions Reference](/docs/reference/client_options)
* [Connecting to an MQTT Broker](/docs/connecting)
