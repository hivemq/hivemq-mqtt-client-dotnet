/*
 * Copyright 2022-present HiveMQ and the HiveMQ Community
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
namespace HiveMQtt.Client.Options;

using System;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using HiveMQtt.Client;
using HiveMQtt.Client.Exceptions;

/// <summary>
/// A class to manage the MQTT options available in the Client.
/// </summary>
public class HiveMQClientOptions
{
    private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

    // The set of valid characters that a client identifier can consist of
    private readonly string clientIdCharset = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";

    public HiveMQClientOptions()
    {
        this.GenerateClientID();
        this.Host = "127.0.0.1";
        this.Port = 1883;
        this.PreferIPv6 = false;
        this.KeepAlive = 60;
        this.SessionExpiryInterval = 300;
        this.CleanStart = true;
        this.UserProperties = new Dictionary<string, string>();
        this.UseTLS = false;
        this.AllowInvalidBrokerCertificates = false;
        this.ClientCertificates = new X509CertificateCollection();
        this.ClientReceiveMaximum = 10;
        this.ConnectTimeoutInMs = 5000;
        this.ResponseTimeoutInMs = 5000;
    }

    // Client Identifier to be used in the Client.  Will be set automatically if not specified.
    public string? ClientId { get; set; }

    // IP Address or DNS Hostname of the MQTT Broker to connect to
    public string Host { get; set; }

    // The port to connect to on the MQTT Broker
    public int Port { get; set; }

    // When resolving a DNS hostname in the Host property, prefer IPv6 addresses over IPv4 addresses.
    public bool PreferIPv6 { get; set; }

    // The the maximum time interval in seconds that is permitted to elapse between the point at which the Client
    // finishes transmitting one MQTT Control Packet and the point it starts sending the next.
    // Valid values: 0..65535
    public int KeepAlive { get; set; }

    // Specifies whether the Connection starts a new Session or is a continuation of an existing Session.
    public bool CleanStart { get; set; }

    // The MQTT CONNECT packet supports basic authentication of a Network Connection using the User Name
    // and Password fields. While these fields are named for a simple password authentication, they can
    // be used to carry other forms of authentication such as passing a token as the Password.
    public string? UserName { get; set; }

    // The MQTT CONNECT packet supports basic authentication of a Network Connection using the User Name
    // and Password fields. While these fields are named for a simple password authentication, they can
    // be used to carry other forms of authentication such as passing a token as the Password.
    public string? Password { get; set; }

    /// <summary>
    /// Gets or sets a value that represents the session expiration interval in use by the MQTT broker.
    /// <para>
    /// In order to implement QoS 1 and QoS 2 protocol flows the Client and Server need to associate state
    /// with the Client Identifier, this is referred to as the Session State. The Server also stores the
    /// subscriptions as part of the Session State.  The session can continue across a sequence of
    /// Network Connections.
    /// </para>
    /// <para>
    /// The Session State lasts as long as the latest Network Connection plus the Session Expiry Interval.
    /// </para>
    /// </summary>
    /// We use long here because uint in C# is non CLS compliant.  The value range is unsigned 4 byte integer (uint).
    public long SessionExpiryInterval { get; set; }

    /// <summary>
    /// Gets or sets a value that indicates the maximum number of QoS 1 and QoS 2 publications that this
    /// MQTT client is willing to process concurrently.
    /// <para>
    /// The Client uses this value to limit the number of QoS 1 and QoS 2 publications that it is willing
    /// to process concurrently. There is no mechanism to limit the QoS 0 publications that the Server might
    /// try to send.
    /// </para>
    /// <para>
    /// The value of Receive Maximum applies only to the current Network Connection. If the Receive Maximum
    /// value is absent then its value defaults to 65,535.
    /// </para>
    /// </summary>
    public int ClientReceiveMaximum { get; set; }

    /// <summary>
    /// Gets or sets a value that indicates the maximum packet size that the MQTT client is willing
    /// accept.  If not set, no limit is imposed beyond the protocol maximum (4.294.967.295).
    /// <para>
    /// The valid range of values are 1..4_294_967_295 equivalent to a 32-bit unsigned integer.
    /// </para>
    /// </summary>
    public long? ClientMaximumPacketSize { get; set; }

    /// <summary>
    /// Gets or sets a value that indicates the highest value that the Client will accept as a Topic Alias sent by the Server.
    /// <para>
    /// The Client uses this value to limit the number of Topic Aliases that it is willing to hold on this Connection.
    /// A value of 0 indicates that the Client does not accept any Topic Aliases on this connection.
    /// </para>
    /// </summary>
    public int? ClientTopicAliasMaximum { get; set; }

    /// <summary>
    /// Gets or sets the Request Response Information flag.  The Client uses this value to request the Server to
    /// return Response Information in the CONNACK.
    /// </summary>
    public bool? RequestResponseInformation { get; set; }

    /// <summary>
    /// Gets or sets the Request Problem Information flag. The Client uses this value to indicate whether the
    /// Reason String or User Properties are sent in the case of failures.
    /// </summary>
    public bool? RequestProblemInformation { get; set; }

    /// <summary>
    /// Gets or sets a Dictionary containing the User Properties returned by the MQTT broker.
    /// </summary>
    public Dictionary<string, string> UserProperties { get; set; }

    /// <summary>
    /// Gets or sets a UTF-8 Encoded String containing the name of the authentication method used for extended authentication.
    /// If Authentication Method is absent, extended authentication is not performed.
    /// </summary>
    public string? AuthenticationMethod { get; set; }

    /// <summary>
    /// Gets or sets an array of bytes containing the authentication data used for extended authentication.
    /// </summary>
    public byte[]? AuthenticationData { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the MQTT client should use TLS.
    /// </summary>
    public bool UseTLS { get; set; }

    /// <summary>
    /// Gets or sets the collection of client X509 certificates.
    /// </summary>
    public X509CertificateCollection ClientCertificates { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the MQTT client should allow invalid broker TLS certificates.
    /// </summary>
    public bool AllowInvalidBrokerCertificates { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether a Last Will and Testament should be used in the Connect.
    /// </summary>
    public LastWillAndTestament? LastWillAndTestament { get; set; }

    /// <summary>
    /// Gets or sets the time in milliseconds to wait for a connection to be established.
    /// </summary>
    public int ConnectTimeoutInMs { get; set; }

    /// <summary>
    /// Gets or sets the time in milliseconds to wait for a response in a transactional operation.
    /// This could be a Publish, Subscribe, Unsubscribe, or Disconnect operation.
    /// </summary>
    public int ResponseTimeoutInMs { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the client should automatically reconnect if the connection is lost.
    /// Reconnection attempts will be made with a retry back off strategy.
    /// </summary>
    public bool AutomaticReconnect { get; set; }

    /// <summary>
    /// Generate a semi-random client identifier to be used in <c>Client</c> connections.
    /// hmqc#-pid-randomstring.
    /// </summary>
    public void GenerateClientID()
    {
        var rand = new Random();
        var stamp = "hmqcs";

        var clientID = new string(Enumerable.Range(0, 18) // Target length 23 (5 chars for stamp)
                                            .Select(_ =>
                                            this.clientIdCharset[rand.Next(this.clientIdCharset.Length)])
                                            .ToArray());

        this.ClientId = stamp + clientID;
    }

    public void ValidateOptions()
    {
        Logger.Warn("HiveMQClientOptions.ValidateOptions() is deprecated.  Use Validate() instead.");
        this.Validate();
    }

    /// <summary>
    /// Validate that the options specified in this class are all sane.
    /// </summary>
    public void Validate()
    {
        if (this.Host == null)
        {
            throw new HiveMQttClientException("Host (broker) must be specified.");
        }

        this.KeepAlive = RangeValidateTwoByteInteger(this.KeepAlive);
        this.SessionExpiryInterval = RangeValidateFourByteInteger(this.SessionExpiryInterval);

        // IANA registered ports for MQTT are 1883 (plain) and 8883 (secure).
        // https://www.iana.org/assignments/service-names-port-numbers/service-names-port-numbers.xhtml?search=mqtt
        // If for any reason you want to disable TLS when using port 8883, use a BeforeConnect event handler.
        if (this.Port == 8883)
        {
            this.UseTLS = true;
        }

        if (this.ClientMaximumPacketSize != null)
        {
            this.ClientMaximumPacketSize = RangeValidateFourByteInteger((long)this.ClientMaximumPacketSize);

            if (this.ClientMaximumPacketSize == 0)
            {
                throw new HiveMQttClientException("Client Maximum Packet Size must be greater than 0.");
            }
        }

        this.ClientReceiveMaximum = RangeValidateTwoByteInteger(this.ClientReceiveMaximum);
        if (this.ClientReceiveMaximum == 0)
        {
            this.ClientReceiveMaximum = 65535;
        }

        if (this.ClientTopicAliasMaximum != null)
        {
            this.ClientTopicAliasMaximum = RangeValidateTwoByteInteger((int)this.ClientTopicAliasMaximum);
        }

        if (this.UserProperties is null)
        {
            this.UserProperties = new Dictionary<string, string>();
        }
        else
        {
            foreach (var property in this.UserProperties)
            {
                if (property.Key is not string key || key.Length > 65535)
                {
                    throw new HiveMQttClientException("User Property Key must be less than 65535 characters.");
                }

                if (property.Value is not string value || value.Length > 65535)
                {
                    throw new HiveMQttClientException("User Property Value must be less than 65535 characters.");
                }
            }
        }

        if (this.AuthenticationMethod != null && this.AuthenticationMethod.Length > 65535)
        {
            throw new HiveMQttClientException("Authentication Method must be less than 65535 characters.");
        }

        if (this.AuthenticationData != null && this.AuthenticationData.Length > 65535)
        {
            throw new HiveMQttClientException("Authentication Data must be less than 65535 bytes.");
        }

        if (this.ClientId == null)
        {
            this.GenerateClientID();
        }

        if (this.ClientId is not null && this.ClientId.Length > 23)
        {
            Logger.Info($"Client ID {this.ClientId} is longer than 23 characters.  This may cause issues with some brokers.");
        }
    }

    /// <summary>
    /// Validate that the value is within the range of a 2 byte unsigned integer.
    /// </summary>
    /// <param name="value">The value to be validated.</param>
    /// <returns>A corrected value or the original if it was in range.</returns>
    internal static int RangeValidateTwoByteInteger(int value)
    {
        if (value < ushort.MinValue)
        {
            value = ushort.MinValue;
        }
        else if (value > ushort.MaxValue)
        {
            value = ushort.MaxValue;
        }

        return value;
    }

    /// <summary>
    /// Validate that the value is within the range of a 4 byte unsigned integer.
    /// </summary>
    /// <param name="value">The value to be validated.</param>
    /// <returns>A corrected value or the original if it was in range.</returns>
    internal static long RangeValidateFourByteInteger(long value)
    {
        if (value < uint.MinValue)
        {
            value = uint.MinValue;
        }
        else if (value > uint.MaxValue)
        {
            value = uint.MaxValue;
        }

        return value;
    }
}
