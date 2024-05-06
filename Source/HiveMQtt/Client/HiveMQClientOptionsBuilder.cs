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
    /// Sets the port to connect to.
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
    /// Adds an X.509 certificate to be used for client authentication.
    /// </summary>
    /// <param name="clientCertificatePath">The path to the client X.509 certificate to be used for client authentication.</param>
    /// <param name="password">The optional password for the client X.509 certificate.</param>
    /// <returns>The HiveMQClientOptionsBuilder instance.</returns>
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
                    this.options.ClientCertificates.Add(new X509Certificate2(clientCertificatePath, password));
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
            throw new FileNotFoundException();
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
    public HiveMQClientOptionsBuilder WithAuthenticationMethod(string method)
    {
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
        this.options.UserName = username;
        return this;
    }

    /// <summary>
    /// Sets the password.
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
    /// <param name="password">The password value.</param>
    /// <returns>The HiveMQClientOptionsBuilder instance.</returns>
    public HiveMQClientOptionsBuilder WithPassword(string password)
    {
        this.options.Password = password;
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
    /// Builds the HiveMQClientOptions instance.
    /// </summary>
    /// <returns>The HiveMQClientOptions instance.</returns>
    public HiveMQClientOptions Build()
    {
        this.options.Validate();
        return this.options;
    }
}
