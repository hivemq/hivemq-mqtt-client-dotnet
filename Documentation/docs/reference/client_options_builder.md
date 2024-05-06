# HiveMQClientOptionsBuilder

The `HiveMQClientOptionsBuilder` class is a builder pattern implementation that provides a convenient way to construct `HiveMQClientOptions` objects for configuring HiveMQClient connections to an MQTT broker.

## Methods

### `WithBroker(string broker)`

Sets the address of the broker to connect to.

**Description:** The broker address can be an IPv4 or IPv6 address, a hostname, or a URL. This value is used to establish the connection to the MQTT broker.

**Example:** `WithBroker("mqtt.example.com")`

### `WithPort(int port)`

Sets the port to connect to.

**Description:** The port number is used to establish the connection to the MQTT broker. The default port number for MQTT is 1883.

**Example:** `WithPort(1883)`

### `WithClientId(string clientId)`

Sets the client identifier.

**Description:** The client identifier is a unique identifier for the client connecting to the MQTT broker. It is used to identify the client and maintain session state.

**Example:** `WithClientId("myClientId")`

### `WithAllowInvalidBrokerCertificates(bool allowInvalidCertificates)`

Sets whether to allow invalid broker certificates.

**Description:** This flag allows the client to ignore invalid broker certificates. This is useful for testing purposes, but should not be used in production.

**Example:** `WithAllowInvalidBrokerCertificates(true)`

### `WithUseTls(bool useTls)`

Sets whether to use TLS when connecting to the broker.

**Description:** This flag enables or disables the use of Transport Layer Security (TLS) when connecting to the MQTT broker.

**Example:** `WithUseTls(true)`

### `WithClientCertificate(X509Certificate2 clientCertificate)`

Adds a client X.509 certificate to be used for client authentication.

**Description:** This method adds a client X.509 certificate to be used for client authentication. The certificate is used to verify the client's identity.

**Example:** `WithClientCertificate(new X509Certificate2("path/to/certificate.pfx", "password"))`

### `WithClientCertificates(List<X509Certificate2> clientCertificates)`

Adds a list of client X.509 certificates to be used for client authentication.

**Description:** This method adds a list of client X.509 certificates to be used for client authentication. The certificates are used to verify the client's identity.

**Example:** `WithClientCertificates(new List<X509Certificate2> { new X509Certificate2("path/to/certificate1.pfx", "password"), new X509Certificate2("path/to/certificate2.pfx", "password") })`

### `WithClientCertificate(string clientCertificatePath, string? password = null)`

Adds a client X.509 certificate to be used for client authentication from a file.

**Description:** This method adds a client X.509 certificate to be used for client authentication from a file. The certificate is used to verify the client's identity.

**Example:** `WithClientCertificate("path/to/certificate.pfx", "password")`

### `WithCleanStart(bool cleanStart)`

Sets whether to use a clean start.

**Description:** This flag indicates whether to use a clean start when connecting to the MQTT broker. A clean start means that the broker will discard any previous session state and start a new session.

**Example:** `WithCleanStart(true)`

### `WithKeepAlive(int keepAlive)`

Sets the keep alive period.

**Description:** The keep alive period is the time interval between two control packets (PINGREQ and PINGRESP) sent by the client to the broker. This value is used to detect network failures and maintain the connection.

**Example:** `WithKeepAlive(60)`

### `WithAuthenticationMethod(string method)`

Sets the authentication method.

**Description:** The authentication method is used to authenticate the client with the MQTT broker. The available authentication methods are "username_password" and "none".

**Example:** `WithAuthenticationMethod("username_password")`

### `WithAuthenticationData(byte[] data)`

Sets the authentication data.

**Description:** The authentication data is used to authenticate the client with the MQTT broker. The format and interpretation of the authentication data depend on the chosen authentication method.

**Example:** `WithAuthenticationData(Encoding.UTF8.GetBytes("username_password"))`

### `WithUserProperty(string key, string value)`

Adds a user property to be sent in the connect call.

**Description:** User properties are custom key-value pairs that can be sent in the CONNECT packet. They are used to provide additional metadata or application-specific information.

**Example:** `WithUserProperty("property1", "value1")`

### `WithUserProperties(Dictionary<string, string> properties)`

Adds a dictionary of user properties to be sent in the connect call.

**Description:** This method adds a dictionary of user properties to be sent in the connect call. The dictionary is used to provide additional metadata or application-specific information.

**Example:** `WithUserProperties(new Dictionary<string, string> { { "property1", "value1" }, { "property2", "value2" } })`

### `WithLastWillAndTestament(LastWillAndTestament lwt)`

Sets the last will and testament.

**Description:** The last will and testament is a message that is published by the broker when the client disconnects unexpectedly. It is used to notify other clients of the client's disconnection.

**Example:** `WithLastWillAndTestament(new LastWillAndTestament("topic", "message", MqttQualityOfServiceLevel.AtLeastOnce, true))`

### `WithMaximumPacketSize(int maximumPacketSize)`

Sets the maximum packet size.

**Description:** The maximum packet size is the maximum size of an MQTT packet that the client is willing to receive from the broker. This value is used to prevent the client from receiving oversized packets.

**Example:** `WithMaximumPacketSize(1024)`

### `WithReceiveMaximum(int receiveMaximum)`

Sets the receive maximum.

**Description:** The receive maximum is the maximum number of QoS 1 and QoS 2 messages that the client is willing to receive simultaneously from the broker. This value is used to prevent the client from receiving too many messages at once.

**Example:** `WithReceiveMaximum(100)`

### `WithSessionExpiryInterval(int sessionExpiryInterval)`

Sets the session expiry interval.

**Description:** The session expiry interval is the maximum duration for which the broker maintains the client's session state. This value is used to control the lifespan of the client's session on the broker.

**Example:** `WithSessionExpiryInterval(3600)`

### `WithUserName(string username)`

Sets the username.

**Description:** The username is used to authenticate the client with the MQTT broker. It is used in conjunction with the password to verify the client's identity.

**Example:** `WithUserName("myUsername")`

### `WithPassword(string password)`

Sets the password.

**Description:** The password is used to authenticate the client with the MQTT broker. It is used in conjunction with the username to verify the client's identity.

**Example:** `WithPassword("myPassword")`

### `WithPreferIPv6(bool preferIPv6)`

Sets whether to prefer IPv6.

**Description:** This flag indicates whether to prefer IPv6 over IPv4 when connecting to the MQTT broker.

**Example:** `WithPreferIPv6(true)`

### `WithTopicAliasMaximum(int topicAliasMaximum)`

Sets the topic alias maximum.

**Description:** The topic alias maximum is the maximum number of topic aliases that the client is willing to use in subsequent MQTT messages.

**Example:** `WithTopicAliasMaximum(10)`

### `WithRequestResponseInformation(bool requestResponseInformation)`

Sets whether to request response information.

**Description:** This flag indicates whether to request response information from the broker. The response information includes the response code, reason string, and user properties.

**Example:** `WithRequestResponseInformation(true)`

### `WithRequestProblemInformation(bool requestProblemInformation)`

Sets whether to request problem information.

**Description:** This flag indicates whether to request problem information from the broker. The problem information includes the error code, reason string, and user properties.

**Example:** `WithRequestProblemInformation(true)`

### `Build()`

Builds the `HiveMQClientOptions` instance.

**Description:** This method builds the `HiveMQClientOptions` instance based on the configured options.

**Example:** `HiveMQClientOptions options = new HiveMQClientOptionsBuilder().WithBroker("mqtt.example.com").WithPort(1883).Build();`

## Properties

### `HiveMQClientOptions options`

The `HiveMQClientOptions` instance being built.

## Notes

* The `HiveMQClientOptionsBuilder` class is a fluent API that allows you to construct `HiveMQClientOptions` objects in a fluent manner.
* The `HiveMQClientOptions` instance is built by calling the `Build()` method.
* The `HiveMQClientOptions` instance can be used to configure the HiveMQClient connection to an MQTT broker.
