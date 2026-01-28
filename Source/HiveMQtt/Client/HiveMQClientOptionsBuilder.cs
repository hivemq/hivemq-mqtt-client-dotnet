/*
 * Copyright 2023-present HiveMQ and the HiveMQ Community
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
namespace HiveMQtt.Client;

using System;
using System.Collections.Generic;
using System.Net;
using System.Security;
using System.Security.Cryptography.X509Certificates;
using HiveMQtt.Client.Options;

/// <summary>
/// A builder for HiveMQClientOptions.
/// <para>
/// The HiveMQClientOptionsBuilder class is a builder pattern implementation that provides a convenient
/// way to construct HiveMQClientOptions objects for configuring HiveMQClient connection to an MQTT
/// broker. It offers methods to set various connection parameters such as the broker host, port,
/// client ID, authentication credentials, TLS settings, clean start flag, keep-alive period, and more.
/// </para>
/// <para>
/// The builder allows for a flexible and customizable configuration of the HiveMQClient, enabling
/// developers to create and customize the options based on their specific requirements.
/// </para>
/// Usage example:
/// <code>
/// var options = new HiveMQClientOptionsBuilder()
///     .WithBroker("mqtt.example.com")
///     .WithPort(1883)
///     .WithClientId("myClientId")
///     .WithAllowInvalidBrokerCertificates(true)
///     .WithUseTls(true)
///     .WithCleanStart(true)
///     .WithKeepAlive(60)
///     .WithAuthenticationMethod(AuthenticationMethod.UsernamePassword)
///     .WithAuthenticationData(Encoding.UTF8.GetBytes("authenticationData"))
///     .WithUserProperties(new Dictionary(string, string) { { "property1", "value1" }, { "property2", "value2" } })
///     .WithLastWillAndTestament(new LastWill { Topic = "lwt/topic", Message = "LWT message", QualityOfServiceLevel = MqttQualityOfServiceLevel.AtLeastOnce, Retain = true })
///     .WithMaximumPacketSize(1024)
///     .WithReceiveMaximum(100)
///     .WithSessionExpiryInterval(3600)
///     .WithUserName("myUserName")
///     .WithPassword("myPassword")
///     .WithPreferIPv6(true)
///     .WithTopicAliasMaximum(10)
///     .WithRequestProblemInformation(true)
///     .WithRequestResponseInformation(true)
///     .Build();
/// </code>
/// </summary>
public class HiveMQClientOptionsBuilder
{
    private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
    private readonly HiveMQClientOptions options = new();

    /// <summary>
    /// Sets the address of the broker to connect to.
    /// <para>
    /// This can be either an IPv4 address, IPv6 address or a hostname.
    /// </para>
    /// </summary>
    /// <param name="broker">The broker to connect to.</param>
    /// <returns>The HiveMQClientOptionsBuilder instance.</returns>
    public HiveMQClientOptionsBuilder WithBroker(string broker)
    {
        this.options.Host = broker;
        return this;
    }

    /// <summary>
    /// Sets the WebSocket server to connect to.
    /// <para>
    /// This must be a fully qualified URI, e.g. "ws://localhost:8884/mqtt" or
    /// "wss://localhost:8884/mqtt".
    /// </para>
    /// </summary>
    /// <param name="webSocketServer">The WebSocket server to connect to.</param>
    /// <returns>The HiveMQClientOptionsBuilder instance.</returns>
    public HiveMQClientOptionsBuilder WithWebSocketServer(string webSocketServer)
    {
        this.options.WebSocketServer = webSocketServer;
        return this;
    }

    /// <summary>
    /// Sets the WebSocket keep-alive interval.
    /// <para>
    /// This specifies the interval at which the WebSocket client will send keep-alive pings to the server
    /// to maintain the connection. If not set, the default WebSocket keep-alive behavior is used.
    /// </para>
    /// <para>
    /// This option is only applicable when using WebSocket transport (ws:// or wss://).
    /// </para>
    /// </summary>
    /// <param name="keepAliveInterval">The keep-alive interval.</param>
    /// <returns>The HiveMQClientOptionsBuilder instance.</returns>
    public HiveMQClientOptionsBuilder WithWebSocketKeepAliveInterval(TimeSpan keepAliveInterval)
    {
        this.options.WebSocketKeepAliveInterval = keepAliveInterval;
        return this;
    }

    /// <summary>
    /// Sets custom HTTP headers to be sent during the WebSocket handshake.
    /// <para>
    /// This allows you to add custom headers such as Authorization, X-API-Key, or any other
    /// custom headers required by your WebSocket server. Headers are sent during the initial WebSocket
    /// connection handshake.
    /// </para>
    /// <para>
    /// This option is only applicable when using WebSocket transport (ws:// or wss://).
    /// </para>
    /// </summary>
    /// <param name="headers">A dictionary of header names and values.</param>
    /// <returns>The HiveMQClientOptionsBuilder instance.</returns>
    public HiveMQClientOptionsBuilder WithWebSocketRequestHeaders(Dictionary<string, string> headers)
    {
        this.options.WebSocketRequestHeaders = headers;
        return this;
    }

    /// <summary>
    /// Adds a custom HTTP header to be sent during the WebSocket handshake.
    /// <para>
    /// This is a convenience method for adding a single header. For multiple headers, use
    /// <see cref="WithWebSocketRequestHeaders(Dictionary{string, string})"/>.
    /// </para>
    /// <para>
    /// This option is only applicable when using WebSocket transport (ws:// or wss://).
    /// </para>
    /// </summary>
    /// <param name="name">The header name.</param>
    /// <param name="value">The header value.</param>
    /// <returns>The HiveMQClientOptionsBuilder instance.</returns>
    public HiveMQClientOptionsBuilder WithWebSocketRequestHeader(string name, string value)
    {
        this.options.WebSocketRequestHeaders ??= new Dictionary<string, string>();
        this.options.WebSocketRequestHeaders[name] = value;
        return this;
    }

    /// <summary>
    /// Sets the proxy configuration for WebSocket connections.
    /// <para>
    /// This allows you to configure a proxy server for WebSocket connections. If not set, the system's
    /// default proxy settings are used.
    /// </para>
    /// <para>
    /// This option is only applicable when using WebSocket transport (ws:// or wss://).
    /// </para>
    /// </summary>
    /// <param name="proxy">The proxy configuration.</param>
    /// <returns>The HiveMQClientOptionsBuilder instance.</returns>
    public HiveMQClientOptionsBuilder WithWebSocketProxy(IWebProxy proxy)
    {
        this.options.WebSocketProxy = proxy;
        return this;
    }

    /// <summary>
    /// Sets the proxy configuration for TCP connections.
    /// <para>
    /// This allows you to configure an HTTP proxy server for TCP connections. The client will use
    /// the HTTP CONNECT method to tunnel the MQTT connection through the proxy.
    /// </para>
    /// <para>
    /// This option is only applicable when using TCP transport (not WebSocket). For WebSocket
    /// connections, use <see cref="WithWebSocketProxy(IWebProxy)"/> instead.
    /// </para>
    /// <para>
    /// The proxy must support the HTTP CONNECT method for TCP tunneling.
    /// </para>
    /// <para>
    /// Example:
    /// <code>
    /// // Basic proxy without authentication
    /// var options = new HiveMQClientOptionsBuilder()
    ///     .WithBroker("mqtt.example.com")
    ///     .WithProxy(new WebProxy("http://proxy.example.com:8080"))
    ///     .Build();
    ///
    /// // Proxy with authentication
    /// var proxy = new WebProxy("http://proxy.example.com:8080");
    /// proxy.Credentials = new NetworkCredential("username", "password");
    /// var options = new HiveMQClientOptionsBuilder()
    ///     .WithBroker("mqtt.example.com")
    ///     .WithProxy(proxy)
    ///     .Build();
    /// </code>
    /// </para>
    /// </summary>
    /// <param name="proxy">The proxy configuration.</param>
    /// <returns>The HiveMQClientOptionsBuilder instance.</returns>
    public HiveMQClientOptionsBuilder WithProxy(IWebProxy proxy)
    {
        this.options.Proxy = proxy;
        return this;
    }

    /// <summary>
    /// Sets the port to connect to.
    /// <para>
    /// Default ports are:
    ///  - 1883 for non-TLS connections
    ///  - 8883 for TLS connections
    ///  - 1884 for non-TLS websocket connections (ws://)
    ///  - 8884 for TLS websocket connections (wss://)
    /// .
    /// </para>
    /// </summary>
    /// <param name="port">The port to connect to.</param>
    /// <returns>The HiveMQClientOptionsBuilder instance.</returns>
    public HiveMQClientOptionsBuilder WithPort(int port)
    {
        this.options.Port = port;
        return this;
    }

    /// <summary>
    /// Sets the client identifier.
    /// <para>
    /// The Client Identifier is a mandatory parameter that must be included in the CONNECT packet
    /// during the connection establishment process. It is a unique identifier that identifies the
    /// MQTT client connecting to the broker. The Client Identifier allows the broker to distinguish
    /// and track individual clients and maintain their session state.
    /// </para>
    /// <para>
    /// If a client identifier isn't specified, one will be generated automatically.
    /// </para>
    /// </summary>
    /// <param name="clientId">The client identifier.</param>
    /// <returns>The HiveMQClientOptionsBuilder instance.</returns>
    public HiveMQClientOptionsBuilder WithClientId(string clientId)
    {
        if (clientId.Length is < 0 or > 65535)
        {
            Logger.Error("Client Id must be between 0 and 65535 characters.");
            throw new ArgumentException("Client Id must be between 0 and 65535 characters.");
        }

        this.options.ClientId = clientId;
        return this;
    }

    /// <summary>
    /// Sets whether to allow invalid broker certificates.
    /// <para>
    /// In some cases, you may want to ignore and accept invalid broker certificates.  If so, set this
    /// to true.  This is useful for testing purposes, but should not be used in production.
    /// </para>
    /// </summary>
    /// <param name="allowInvalidCertificates">Whether to allow invalid broker certificates.</param>
    /// <returns>The HiveMQClientOptionsBuilder instance.</returns>
    public HiveMQClientOptionsBuilder WithAllowInvalidBrokerCertificates(bool allowInvalidCertificates)
    {
        this.options.AllowInvalidBrokerCertificates = allowInvalidCertificates;
        return this;
    }

    /// <summary>
    /// Sets whether to use TLS when connecting to the broker.
    /// </summary>
    /// <param name="useTls">A boolean indicating whether to use TLS when connecting to the broker.</param>
    /// <returns>The HiveMQClientOptionsBuilder instance.</returns>
    public HiveMQClientOptionsBuilder WithUseTls(bool useTls)
    {
        this.options.UseTLS = useTls;
        return this;
    }

    /// <summary>
    /// Adds an X.509 certificate to be used for client authentication.  This can be called
    /// multiple times to add multiple certificates.
    /// </summary>
    /// <param name="clientCertificate">The client X.509 certificate to be used for client authentication.</param>
    /// <returns>The HiveMQClientOptionsBuilder instance.</returns>
    public HiveMQClientOptionsBuilder WithClientCertificate(X509Certificate2 clientCertificate)
    {
        this.options.ClientCertificates.Add(clientCertificate);
        return this;
    }

    /// <summary>
    /// Adds a list of X.509 certificates to be used for client authentication.
    /// </summary>
    /// <param name="clientCertificates">The list of client X.509 certificates to be used for client authentication.</param>
    /// <returns>The HiveMQClientOptionsBuilder instance.</returns>
    public HiveMQClientOptionsBuilder WithClientCertificates(List<X509Certificate2> clientCertificates)
    {
        foreach (var certificate in clientCertificates)
        {
            this.options.ClientCertificates.Add(certificate);
        }

        return this;
    }

    /// <summary>
    /// Adds an X.509 certificate to be used for client authentication with secure password handling.
    /// </summary>
    /// <param name="clientCertificatePath">The path to the client X.509 certificate to be used for client authentication.</param>
    /// <param name="password">The optional password for the client X.509 certificate as a SecureString for enhanced security.</param>
    /// <returns>The HiveMQClientOptionsBuilder instance.</returns>
    public HiveMQClientOptionsBuilder WithClientCertificate(string clientCertificatePath, SecureString? password = null)
    {
        if (File.Exists(clientCertificatePath))
        {
            // Check if the file is readable
            try
            {
                using (var fileStream = File.OpenRead(clientCertificatePath))
                {
                    // File exists and is readable.
#if NET9_0_OR_GREATER
#pragma warning disable SYSLIB0057 // X509Certificate2 constructor obsolete in .NET 9 - using X509Certificate2.CreateFromPemFile would require different API for PFX files
                    // Use X509Certificate2 constructor for .NET 9 compatibility with .NET 6-8
                    // X509CertificateLoader requires .NET 9+, and CreateFromPemFile doesn't support password-protected PFX files
                    this.options.ClientCertificates.Add(new X509Certificate2(clientCertificatePath, password));
#pragma warning restore SYSLIB0057
#else
                    this.options.ClientCertificates.Add(new X509Certificate2(clientCertificatePath, password));
#endif
                    return this;
                }
            }
            catch (UnauthorizedAccessException)
            {
                Logger.Error("WithClientCertificate: File exists but is not readable due to access permissions.");
                throw;
            }
            catch (IOException)
            {
                Logger.Error("WithClientCertificate: An I/O error occurred while trying to read the file.");
                throw;
            }
        }
        else
        {
            Logger.Error("WithClientCertificate: The specified client certificate file does not exist.");
            throw new FileNotFoundException($"The specified client certificate file does not exist: {clientCertificatePath}");
        }
    }

    /// <summary>
    /// Adds an X.509 certificate to be used for client authentication (for backward compatibility).
    /// </summary>
    /// <param name="clientCertificatePath">The path to the client X.509 certificate to be used for client authentication.</param>
    /// <param name="password">The optional password for the client X.509 certificate as a plain string.</param>
    /// <returns>The HiveMQClientOptionsBuilder instance.</returns>
    [Obsolete("Use WithClientCertificate(string, SecureString) for enhanced security. This method stores passwords as plain text in memory.")]
    public HiveMQClientOptionsBuilder WithClientCertificate(string clientCertificatePath, string? password = null)
    {
        if (File.Exists(clientCertificatePath))
        {
            // Check if the file is readable
            try
            {
                using (var fileStream = File.OpenRead(clientCertificatePath))
                {
                    // File exists and is readable.
                    // Convert string password to SecureString for secure handling
                    SecureString? securePassword = null;
                    if (!string.IsNullOrEmpty(password))
                    {
                        securePassword = new SecureString();
                        foreach (var c in password)
                        {
                            securePassword.AppendChar(c);
                        }

                        securePassword.MakeReadOnly();
                    }

                    this.options.ClientCertificates.Add(new X509Certificate2(clientCertificatePath, securePassword));
                    return this;
                }
            }
            catch (UnauthorizedAccessException)
            {
                Logger.Error("WithClientCertificate: File exists but is not readable due to access permissions.");
                throw;
            }
            catch (IOException)
            {
                Logger.Error("WithClientCertificate: An I/O error occurred while trying to read the file.");
                throw;
            }
        }
        else
        {
            Logger.Error("WithClientCertificate: The specified client certificate file does not exist.");
            throw new FileNotFoundException($"The specified client certificate file does not exist: {clientCertificatePath}");
        }
    }

    /// <summary>
    /// Sets whether to use a clean start.
    /// <para>
    /// This flag indicates whether the client wants to start a new session or resume a previous session
    /// with the broker.
    /// </para>
    /// <para>
    /// When the Clean Start flag is set to true, the broker discards any previous session state associated
    /// with the client and starts a new session. This means that the client will not receive any missed
    /// messages or subscriptions from previous sessions.
    /// </para>
    /// <para>
    /// When false, the broker attempts to resume the previous session for the client. The broker will
    /// restore the client's subscriptions and deliver any missed messages that were published during
    /// the client's offline period.
    /// </para>
    /// </summary>
    /// <param name="cleanStart">A boolean indicating whether to use the clean start flag when connecting.</param>
    /// <returns>The HiveMQClientOptionsBuilder instance.</returns>
    public HiveMQClientOptionsBuilder WithCleanStart(bool cleanStart)
    {
        this.options.CleanStart = cleanStart;
        return this;
    }

    /// <summary>
    /// Sets the keep alive period.
    /// <para>
    /// This value specifies the maximum time interval, in seconds, between two control packets
    /// (PINGREQ and PINGRESP) sent by the client to the broker.  The Keep Alive Period serves as
    /// a mechanism to ensure that the connection between the client and the broker remains active
    /// and detect any potential network or client failures. If the client does not send a control
    /// packet within the Keep Alive Period, the broker assumes that the client is no longer active
    /// and may terminate the connection.
    /// </para>
    /// <para>
    /// By setting an appropriate Keep Alive Period, clients can maintain an active connection with
    /// the broker and prevent it from being closed due to inactivity. The Keep Alive Period should
    /// be set to a value that is less than the session expiry interval specified in the CONNECT
    /// packet to ensure the session remains active.
    /// </para>
    /// </summary>
    /// <param name="keepAlive">The keep alive period.</param>
    /// <returns>The HiveMQClientOptionsBuilder instance.</returns>
    public HiveMQClientOptionsBuilder WithKeepAlive(int keepAlive)
    {
        this.options.KeepAlive = (ushort)keepAlive;
        return this;
    }

    /// <summary>
    /// Sets the authentication method.
    /// </summary>
    /// <param name="method">The authentication method.</param>
    /// <returns>The HiveMQClientOptionsBuilder instance.</returns>
    /// <exception cref="ArgumentException">Thrown when the authentication method is not between 1 and 65535 characters.</exception>
    public HiveMQClientOptionsBuilder WithAuthenticationMethod(string method)
    {
        if (method.Length is < 1 or > 65535)
        {
            Logger.Error("Authentication method must be between 1 and 65535 characters.");
            throw new ArgumentException("Authentication method must be between 1 and 65535 characters.");
        }

        this.options.AuthenticationMethod = method;
        return this;
    }

    /// <summary>
    /// Sets the authentication data.
    /// </summary>
    /// <param name="data">The authentication data.</param>
    /// <returns>The HiveMQClientOptionsBuilder instance.</returns>
    public HiveMQClientOptionsBuilder WithAuthenticationData(byte[] data)
    {
        this.options.AuthenticationData = data;
        return this;
    }

    /// <summary>
    /// Adds a user property to be sent in the connect call.
    /// <para>
    /// In MQTT 5, User Properties provide a flexible way to include custom key-value pairs in MQTT messages.
    /// User Properties allow clients to attach additional metadata or application-specific information to
    /// messages beyond the standard MQTT headers and payload. These properties can be used for various purposes
    /// such as message routing, filtering, or conveying application-specific data. User Properties are optional
    /// and can be included in MQTT packets like CONNECT, PUBLISH, SUBSCRIBE, UNSUBSCRIBE, and others. They enable
    /// extensibility and interoperability by allowing clients and brokers to exchange custom information in a
    /// standardized manner within the MQTT protocol.
    /// </para>
    /// </summary>
    /// <param name="key">The key of the user property.</param>
    /// <param name="value">The value of the user property.</param>
    /// <returns>The HiveMQClientOptionsBuilder instance.</returns>
    public HiveMQClientOptionsBuilder WithUserProperty(string key, string value)
    {
        if (key.Length is < 1 or > 65535)
        {
            Logger.Error("User property key must be between 1 and 65535 characters.");
            throw new ArgumentException("User property key must be between 1 and 65535 characters.");
        }

        if (value.Length is < 1 or > 65535)
        {
            Logger.Error("User property value must be between 1 and 65535 characters.");
            throw new ArgumentException("User property value must be between 1 and 65535 characters.");
        }

        this.options.UserProperties.Add(key, value);
        return this;
    }

    /// <summary>
    /// Sets the user properties to be sent in the connect call.
    /// <para>
    /// In MQTT 5, User Properties provide a flexible way to include custom key-value pairs in MQTT messages.
    /// User Properties allow clients to attach additional metadata or application-specific information to
    /// messages beyond the standard MQTT headers and payload. These properties can be used for various purposes
    /// such as message routing, filtering, or conveying application-specific data. User Properties are optional
    /// and can be included in MQTT packets like CONNECT, PUBLISH, SUBSCRIBE, UNSUBSCRIBE, and others. They enable
    /// extensibility and interoperability by allowing clients and brokers to exchange custom information in a
    /// standardized manner within the MQTT protocol.
    /// </para>
    /// </summary>
    /// <param name="properties">The user properties to be sent in the connect call.</param>
    /// <returns>The HiveMQClientOptionsBuilder instance.</returns>
    public HiveMQClientOptionsBuilder WithUserProperties(Dictionary<string, string> properties)
    {
        foreach (var property in properties)
        {
            this.options.UserProperties.Add(property.Key, property.Value);
        }

        return this;
    }

    /// <summary>
    /// Sets the last will and testament.
    /// <para>
    /// In MQTT 5, a Will Message, also known as the Last Will and Testament (LWT), allows a client to specify a
    /// message that will be published by the broker on behalf of the client when the client unexpectedly disconnects.
    /// The Will Message includes a topic, payload, QoS level, and retain flag. This feature is useful for clients
    /// to communicate their status or leave a message for others in case of an unexpected disconnection. It is an
    /// optional feature that clients can choose to use when establishing an MQTT connection.
    /// </para>
    /// </summary>
    /// <param name="lwt">The last will and testament.</param>
    /// <returns>The HiveMQClientOptionsBuilder instance.</returns>
    public HiveMQClientOptionsBuilder WithLastWillAndTestament(LastWillAndTestament lwt)
    {
        this.options.LastWillAndTestament = lwt;
        return this;
    }

    /// <summary>
    /// Sets the maximum packet size in bytes.
    /// <para>
    /// the Maximum Packet Size is an optional parameter that can be included in the CONNECT packet during
    /// the connection establishment process. It specifies the maximum size, in bytes, that the client is willing
    /// to accept for an MQTT packet from the broker. This value allows the client to limit the size of incoming
    /// packets to prevent excessive memory usage or denial-of-service attacks. The Maximum Packet Size is negotiated
    /// between the client and the broker, and the broker will ensure that packets sent to the client do not exceed
    /// this size. If the Maximum Packet Size is not specified by the client, the broker may impose its own limit or
    /// use a default value.
    /// </para>
    /// </summary>
    /// <param name="maximumPacketSize">The maximum packet size in bytes.</param>
    /// <returns>The HiveMQClientOptionsBuilder instance.</returns>
    public HiveMQClientOptionsBuilder WithMaximumPacketSize(int maximumPacketSize)
    {
        this.options.ClientMaximumPacketSize = maximumPacketSize;
        return this;
    }

    /// <summary>
    /// Sets the receive maximum.
    /// <para>
    /// The Receive Maximum is an optional parameter that can be included in the CONNECT packet during the
    /// connection establishment process. It specifies the maximum number of QoS 1 and QoS 2 messages that
    /// the client is willing to receive simultaneously from the broker. The Receive Maximum value allows
    /// the client to control the flow of incoming messages and prevent overwhelming its resources. By setting
    /// a specific Receive Maximum value, the client can limit the number of messages that the broker can send
    /// to it at any given time. If the Receive Maximum is not specified by the client, the broker may impose
    /// its own limit or use a default value.
    /// </para>
    /// </summary>
    /// <param name="receiveMaximum">The receive maximum.</param>
    /// <returns>The HiveMQClientOptionsBuilder instance.</returns>
    public HiveMQClientOptionsBuilder WithReceiveMaximum(int receiveMaximum)
    {
        this.options.ClientReceiveMaximum = receiveMaximum;
        return this;
    }

    /// <summary>
    /// Sets the session expiry interval.
    /// <para>
    /// The Session Expiry Interval is an optional parameter that can be included in the CONNECT packet
    /// during the connection establishment process. It specifies the maximum duration, in seconds, for
    /// which the broker should maintain the client's session state after the client disconnects. The Session
    /// Expiry Interval allows clients to control the lifespan of their session on the broker. If the client
    /// reconnects within the session expiry interval, the broker will resume the session and restore any
    /// relevant state information. However, if the client does not reconnect within the specified interval,
    /// the broker will consider the session expired and discard any associated state. If the Session Expiry
    /// Interval is not specified by the client, the broker may impose its own limit or use a default value.
    /// </para>
    /// </summary>
    /// <param name="sessionExpiryInterval">The session expiry interval.</param>
    /// <returns>The HiveMQClientOptionsBuilder instance.</returns>
    public HiveMQClientOptionsBuilder WithSessionExpiryInterval(int sessionExpiryInterval)
    {
        this.options.SessionExpiryInterval = sessionExpiryInterval;
        return this;
    }

    /// <summary>
    /// Sets the username.
    /// <para>
    /// The Username is an optional parameter that can be included in the CONNECT packet during the connection
    /// establishment process. It is used for authentication and represents the username or identifier
    /// associated with the client. The Username is typically used in conjunction with a password or other
    /// authentication mechanisms to verify the client's identity and grant access to the MQTT broker. The
    /// format and interpretation of the Username are specific to the authentication method being used, and
    /// it allows the broker to authenticate and authorize the client based on the provided credentials. If
    /// the client does not require authentication, the Username field may be omitted from the CONNECT packet.
    /// </para>
    /// </summary>
    /// <param name="username">The username value.</param>
    /// <returns>The HiveMQClientOptionsBuilder instance.</returns>
    public HiveMQClientOptionsBuilder WithUserName(string username)
    {
        if (username.Length is < 0 or > 65535)
        {
            Logger.Error("Username must be between 0 and 65535 characters.");
            throw new ArgumentException("Username must be between 0 and 65535 characters.");
        }

        this.options.UserName = username;
        return this;
    }

    /// <summary>
    /// Sets the password using a SecureString for enhanced security.
    /// <para>
    /// The Password is an optional parameter that can be included in the CONNECT packet during the
    /// connection establishment process. It is used for client authentication in conjunction with the
    /// Username or other authentication mechanisms.
    /// </para>
    /// <para>
    /// The Password field allows clients to provide a secret credential or authentication token
    /// associated with the provided Username. The format and interpretation of the Password are
    /// specific to the chosen authentication method and agreed upon by the client and the broker.
    /// </para>
    /// <para>
    /// The broker will use the provided Password, along with the Username or other authentication
    /// information, to verify the client's identity and grant access to the MQTT communication based on
    /// the configured authentication rules.
    /// </para>
    /// <para>
    /// If the client does not require authentication or the chosen authentication method does not involve
    /// a password, the Password field may be omitted from the CONNECT packet.
    /// </para>
    /// <para>
    /// This method accepts a SecureString for enhanced security, preventing password exposure in memory.
    /// </para>
    /// </summary>
    /// <param name="password">The password as a SecureString for enhanced security.</param>
    /// <returns>The HiveMQClientOptionsBuilder instance.</returns>
    public HiveMQClientOptionsBuilder WithPassword(SecureString password)
    {
        ArgumentNullException.ThrowIfNull(password);

        if (password.Length > 65535)
        {
            Logger.Error("Password must be between 0 and 65535 characters.");
            throw new ArgumentException("Password must be between 0 and 65535 characters.");
        }

        this.options.Password = password;
        return this;
    }

    /// <summary>
    /// Sets the password using a plain string (for backward compatibility).
    /// <para>
    /// WARNING: This method stores the password as a plain string in memory, which is less secure.
    /// Consider using WithPassword(SecureString) for enhanced security.
    /// </para>
    /// <para>
    /// The Password is an optional parameter that can be included in the CONNECT packet during the
    /// connection establishment process. It is used for client authentication in conjunction with the
    /// Username or other authentication mechanisms.
    /// </para>
    /// <para>
    /// The Password field allows clients to provide a secret credential or authentication token
    /// associated with the provided Username. The format and interpretation of the Password are
    /// specific to the chosen authentication method and agreed upon by the client and the broker.
    /// </para>
    /// <para>
    /// The broker will use the provided Password, along with the Username or other authentication
    /// information, to verify the client's identity and grant access to the MQTT communication based on
    /// the configured authentication rules.
    /// </para>
    /// <para>
    /// If the client does not require authentication or the chosen authentication method does not involve
    /// a password, the Password field may be omitted from the CONNECT packet.
    /// </para>
    /// </summary>
    /// <param name="password">The password value as a plain string.</param>
    /// <returns>The HiveMQClientOptionsBuilder instance.</returns>
    [Obsolete("Use WithPassword(SecureString) for enhanced security. This method stores passwords as plain text in memory.")]
    public HiveMQClientOptionsBuilder WithPassword(string password)
    {
        ArgumentNullException.ThrowIfNull(password);

        if (password.Length is < 0 or > 65535)
        {
            Logger.Error("Password must be between 0 and 65535 characters.");
            throw new ArgumentException("Password must be between 0 and 65535 characters.");
        }

        // Convert string to SecureString
        var securePassword = new SecureString();
        foreach (var c in password)
        {
            securePassword.AppendChar(c);
        }

        securePassword.MakeReadOnly();

        this.options.Password = securePassword;
        return this;
    }

    /// <summary>
    /// Sets whether to prefer IPv6.
    /// </summary>
    /// <param name="preferIPv6">A boolean indicating whether to prefer IPv6.</param>
    /// <returns>The HiveMQClientOptionsBuilder instance.</returns>
    public HiveMQClientOptionsBuilder WithPreferIPv6(bool preferIPv6)
    {
        this.options.PreferIPv6 = preferIPv6;
        return this;
    }

    /// <summary>
    /// Sets the topic alias maximum.
    /// <para>
    /// This value represents the maximum number of topic aliases that the client is willing
    /// to use in subsequent MQTT messages.
    /// </para>
    /// <para>
    /// Topic aliases are a feature introduced in MQTT 5 to optimize network bandwidth by reducing the
    /// size of MQTT messages. Instead of repeating the full topic string in every message, the client
    /// and broker can use a shorter topic alias value to represent the topic. This reduces the payload
    /// size and improves overall message transmission efficiency.
    /// </para>
    /// <para>
    /// The Topic Alias Maximum value indicates the highest topic alias number that the client can use.
    /// The actual value negotiated between the client and broker will be the minimum of the Topic Alias
    /// Maximum values specified by both parties.
    /// </para>
    /// </summary>
    /// <param name="topicAliasMaximum">The topic alias maximum.</param>
    /// <returns>The HiveMQClientOptionsBuilder instance.</returns>
    public HiveMQClientOptionsBuilder WithTopicAliasMaximum(int topicAliasMaximum)
    {
        this.options.ClientTopicAliasMaximum = topicAliasMaximum;
        return this;
    }

    /// <summary>
    /// Sets whether to request response information.
    /// <para>
    /// This value indicates whether the client requests the broker to provide response information in
    /// certain MQTT control packets.
    /// </para>
    /// <para>
    /// When the Request Response Information flag is set to true, the client is indicating its interest
    /// in receiving additional information from the broker in response to specific MQTT control packets.
    /// This information can include response codes, reason strings, and user properties.
    /// </para>
    /// </summary>
    /// <param name="requestResponseInformation">A boolean indicating whether to request response
    /// information.</param>
    /// <returns>The HiveMQClientOptionsBuilder instance.</returns>
    public HiveMQClientOptionsBuilder WithRequestResponseInformation(bool requestResponseInformation)
    {
        this.options.RequestResponseInformation = requestResponseInformation;
        return this;
    }

    /// <summary>
    /// Sets whether to request problem information.
    /// <para>
    /// This value indicates whether the client requests the broker to provide additional information in
    /// case of errors or problems encountered during the MQTT communication.
    /// </para>
    /// <para>
    /// When the Request Problem Information flag is set to true, the client is indicating its interest
    /// in receiving detailed information from the broker when errors or problems occur. This information
    /// can include error codes, reason strings, and user properties associated with the encountered issues.
    /// </para>
    /// <para>
    /// By setting the Request Problem Information flag, the client can receive more specific and helpful
    /// information from the broker, aiding in troubleshooting and understanding the nature of any encountered
    /// problems during the MQTT communication.
    /// </para>
    /// </summary>
    /// <param name="requestProblemInformation">A boolean indicating whether to request problem information.</param>
    /// <returns>The HiveMQClientOptionsBuilder instance.</returns>
    public HiveMQClientOptionsBuilder WithRequestProblemInformation(bool requestProblemInformation)
    {
        this.options.RequestProblemInformation = requestProblemInformation;
        return this;
    }

    /// <summary>
    /// Sets whether to automatically reconnect.
    /// <para>
    /// This value indicates whether the client should automatically reconnect to the broker in case of
    /// a connection failure or disconnection.
    /// </para>
    /// <para>
    /// When the Automatic Reconnect flag is set to true, the client will attempt to automatically reconnect
    /// to the broker if the connection is lost or disconnected unexpectedly. The client will continue to
    /// retry the connection at increasing intervals until the connection is re-established successfully.
    /// </para>
    /// <para>
    /// By enabling the Automatic Reconnect feature, clients can ensure that their MQTT communication remains
    /// robust and resilient to network disruptions or temporary outages. The client will automatically recover
    /// from connection failures and resume communication with the broker without manual intervention.
    /// </para>
    /// </summary>
    /// <param name="automaticReconnect">A boolean indicating whether to automatically reconnect.</param>
    /// <returns>The HiveMQClientOptionsBuilder instance.</returns>
    public HiveMQClientOptionsBuilder WithAutomaticReconnect(bool automaticReconnect)
    {
        this.options.AutomaticReconnect = automaticReconnect;
        return this;
    }

    /// <summary>
    /// Builds the HiveMQClientOptions instance.
    /// </summary>
    /// <returns>The HiveMQClientOptions instance.</returns>
    public HiveMQClientOptions Build()
    {
        this.options.Validate();
        return this.options;
    }
}
