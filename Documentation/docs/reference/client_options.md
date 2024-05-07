---
sidebar_position: 45
---
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

## Constructors
---------------

The `HiveMQClientOptions` class has a default constructor that initializes all properties to their default values.

## Methods
---------

### `Validate()`

* Return: `void`
* Throws: `HiveMQttClientException` on error
* Validate that the options specified in this class are all sane and valid.
