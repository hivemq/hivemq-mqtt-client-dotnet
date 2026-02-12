# HiveMQClientOptions

The `HiveMQClientOptions` class provides options for configuring the HiveMQ MQTT client.

## Properties

### `ClientId`
----------------

* Type: `string?`
* Description: The client identifier to be used in the Client. Will be set automatically if not specified.

### `Host`
----------------

* Type: `string`
* Description: The IP address or DNS hostname of the MQTT broker to connect to.

### `Port`
----------------

* Type: `int`
* Description: The port to connect to on the MQTT broker.

### `PreferIPv6`
----------------

* Type: `bool`
* Description: When resolving a DNS hostname in the `Host` property, prefer IPv6 addresses over IPv4 addresses.

### `KeepAlive`
----------------

* Type: `int`
* Description: The maximum time interval that is permitted to elapse between the point at which the Client finishes transmitting one MQTT Control Packet and the point it starts sending the next.

### `CleanStart`
----------------

* Type: `bool`
* Description: Specifies whether the Connection starts a new Session or is a continuation of an existing Session.

### `UserName` and `Password`
-------------------------

* Type: `string?`
* Description: The MQTT CONNECT packet supports basic authentication of a Network Connection using the User Name and Password fields. While these fields are named for a simple password authentication, they can be used to carry other forms of authentication such as passing a token as the Password.

### `SessionExpiryInterval`
-------------------------

* Type: `long`
* Description: The session expiration interval in use by the MQTT broker.

### `ClientReceiveMaximum`
-------------------------

* Type: `int?`
* Description: The maximum number of QoS 1 and QoS 2 publications that this MQTT client is willing to process concurrently.

### `ManualAckEnabled`
-------------------------

* Type: `bool`
* Default: `false`
* Description: When true, the client does not send PubAck (QoS 1) or PubRec (QoS 2) until the application calls `AckAsync` on the client. Unacked messages consume slots in the Receive Maximum window until acknowledged or the connection is closed. See the [Manual Acknowledgement](/docs/how-to/manual-ack) how-to for details.
* Added in: v0.40.0

### `ClientMaximumPacketSize`
-------------------------

* Type: `long?`
* Description: The maximum packet size that the MQTT client is willing to accept.

### `ClientTopicAliasMaximum`
-------------------------

* Type: `int?`
* Description: The highest value that the Client will accept as a Topic Alias sent by the Server.

### `RequestResponseInformation` and `RequestProblemInformation`
-------------------------

* Type: `bool?`
* Description: The Request Response Information flag and the Request Problem Information flag.

### `UserProperties`
----------------

* Type: `Dictionary<string, string>`
* Description: A dictionary containing the User Properties returned by the MQTT broker.

### `AuthenticationMethod` and `AuthenticationData`
-------------------------

* Type: `string?` and `byte[]?`
* Description: The authentication method and data used for extended authentication.

### `UseTLS`
----------------

* Type: `bool`
* Description: Whether the MQTT client should use TLS.

### `ClientCertificates`
----------------

* Type: `X509CertificateCollection`
* Description: The collection of client X509 certificates.

### `AllowInvalidBrokerCertificates`
-------------------------

* Type: `bool`
* Description: Whether the MQTT client should allow invalid broker TLS certificates.

### `LastWillAndTestament`
-------------------------

* Type: `LastWillAndTestament?`
* Description: Whether a Last Will and Testament should be used in the Connect.

### `ConnectTimeoutInMs`
-------------------------

* Type: `int`
* Description: The time in milliseconds to wait for a connection to be established.

### `ResponseTimeoutInMs`
-------------------------

* Type: `int`
* Description: The time in milliseconds to wait for a response from transactional packets.

### `AutomaticReconnect`
-------------------------

* Type: `bool`
* Description: Indicates whether the client should automatically reconnect if the connection is lost or dropped.

### `WebSocketServer`
-------------------------

* Type: `string`
* Description: The WebSocket server address to connect to. Must include the protocol (ws:// or wss://). Example: `wss://broker.example.com:8884/mqtt`

### `WebSocketKeepAliveInterval`
-------------------------

* Type: `TimeSpan?`
* Description: The interval at which the WebSocket client sends keep-alive pings to the server. Only applicable when using WebSocket transport.

### `WebSocketRequestHeaders`
-------------------------

* Type: `Dictionary<string, string>?`
* Description: Custom HTTP headers to be sent during the WebSocket handshake. Useful for adding Authorization or custom headers required by your server.

### `WebSocketProxy`
-------------------------

* Type: `IWebProxy?`
* Description: The proxy configuration for WebSocket connections. This is the recommended way to configure proxy support. Only applicable when using WebSocket transport.

### `Proxy`
-------------------------

* Type: `IWebProxy?`
* Description: The proxy configuration for TCP connections. Uses the HTTP CONNECT method to tunnel MQTT traffic through the proxy. Only applicable when using TCP transport (not WebSocket). For WebSocket connections, use `WebSocketProxy` instead. See the [Configure a Proxy Server](/docs/how-to/configure-proxy) guide for details.
* Added in: v0.38.0

## Constructors
---------------

The `HiveMQClientOptions` class has a default constructor that initializes all properties to their default values.

## Methods
---------

### `Validate()`

* Return: `void`
* Throws: `HiveMQttClientException` on error
* Validate that the options specified in this class are all sane and valid.
