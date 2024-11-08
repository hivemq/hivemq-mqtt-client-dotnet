
# DisconnectOptionsBuilder

The `DisconnectOptionsBuilder` class is a builder pattern implementation that provides a convenient way to construct `DisconnectOptions` objects for configuring disconnect behavior in HiveMQtt client applications.

## Methods

### `WithSessionExpiryInterval(int sessionExpiryInterval)`

Sets the session expiry interval for the disconnect.

- **Description:** Specifies the duration, in seconds, for which the session state will be maintained by the broker after the client disconnects. A value of 0 means the session will not be saved.
- **Example:** 
  ```csharp
  .WithSessionExpiryInterval(60)
  ```

### `WithReasonCode(DisconnectReasonCode reasonCode)`

Sets the reason code for the disconnect.

- **Description:** Specifies the reason for disconnecting, represented by a `DisconnectReasonCode` enum.
- **Example:** 
  ```csharp
  .WithReasonCode(DisconnectReasonCode.NormalDisconnection)
  ```

### `WithReasonString(string reasonString)`

Sets the reason string for the disconnect.

- **Description:** Provides a textual reason for disconnecting. This string must be between 1 and 65535 characters.
- **Example:** 
  ```csharp
  .WithReasonString("Normal shutdown")
  ```
- **Exceptions:**
  - Throws `ArgumentNullException` if `reasonString` is null.
  - Throws `ArgumentException` if `reasonString` length is not between 1 and 65535 characters.

### `WithUserProperty(string key, string value)`

Adds a single user property to the disconnect.

- **Description:** Sets a key-value pair as a user-defined property for the disconnect message. Both the `key` and `value` must be between 1 and 65535 characters.
- **Example:** 
  ```csharp
  .WithUserProperty("disconnect_reason", "scheduled_maintenance")
  ```
- **Exceptions:**
  - Throws `ArgumentNullException` if `key` or `value` is null.
  - Throws `ArgumentException` if `key` or `value` length is not between 1 and 65535 characters.

### `WithUserProperties(Dictionary<string, string> properties)`

Adds multiple user properties to the disconnect.

- **Description:** Sets multiple key-value pairs as user-defined properties for the disconnect message. Each `key` and `value` must be between 1 and 65535 characters.

- **Exceptions:**
	- Throws ArgumentNullException if key or value is null.
	- Throws ArgumentException if key or value length is not between 1 and 65535 characters.

### WithUserProperties(Dictionary<string, string> properties)

Adds multiple user properties to the disconnect.

- **Description:** Sets multiple key-value pairs as user-defined properties for the disconnect message. Each key and value must be between 1 and 65535 characters.

- **Example:**
    ```csharp
    WithUserProperties(new Dictionary<string, string> {
        { "disconnect_reason", "scheduled_maintenance" },
        { "session_end", "true" }
    })
    ```
- **Exceptions:**
	- Throws ArgumentNullException if any key or value is null.
	- Throws ArgumentException if any key or value length is not between 1 and 65535 characters.

### Build()

Builds the DisconnectOptions instance.

- **Description:** Validates and constructs a DisconnectOptions object based on the options provided through previous method calls.

- **Example:**
    ```csharp
    DisconnectOptions disconnectOptions = new DisconnectOptionsBuilder()
        .WithReasonCode(DisconnectReasonCode.NormalDisconnection)
        .WithReasonString("Client shutdown")
        .WithUserProperty("disconnect_reason", "user_initiated")
        .Build();
    ```
### Properties

**DisconnectOptions options**

The constructed `DisconnectOptions` instance that contains all configured settings for the disconnect.

### Notes

- The `DisconnectOptionsBuilder` class follows a fluent API design, allowing method chaining.
- Call `Build()` to finalize and retrieve the DisconnectOptions instance.
- `DisconnectOptions` instances are used by the HiveMQtt client to handle custom disconnect behavior with specified reasons, user properties, and session handling.
