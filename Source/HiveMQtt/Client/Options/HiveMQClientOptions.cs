namespace HiveMQtt.Client.Options;

using System;
using System.Linq;

using HiveMQtt.Client.Exceptions;

/// <summary>
/// A class to manage the MQTT options available in the Client.
/// </summary>
public class HiveMQClientOptions
{
    // The set of valid characters that a client identifier can consist of
    private readonly string clientIdCharset = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";

    public HiveMQClientOptions()
    {
        this.GenerateClientID();
        this.Host = "127.0.0.1";
        this.Port = 1883;
        this.KeepAlive = 60;
        this.SessionExpiryInterval = 300;
        this.CleanStart = true;
        this.UserProperties = new Dictionary<string, string>();
    }

    // Client Identifier to be used in the Client.  Will be set automatically if not specified.
    public string? ClientId { get; set; }

    // IP Address or DNS Hostname of the MQTT Broker to connect to
    public string Host { get; set; }

    // The port to connect to on the MQTT Broker
    public int Port { get; set; }

    // The the maximum time interval that is permitted to elapse between the point at which the Client
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
    /// We use long here because uint in C# is non CLS compliant.  The value range is unsigned 4 byte integer (uint32).
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
    public int? ClientReceiveMaximum { get; set; }

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
    /// Generate a semi-random client identifier to be used in <c>Client</c> connections.
    /// hivec#-pid-randomstring.
    /// </summary>
    public void GenerateClientID()
    {
        var rand = new Random();
        var pid_string = Convert.ToString(Environment.ProcessId, System.Globalization.CultureInfo.InvariantCulture);
        var stampLength = 23 - pid_string.Length;

        var clientID = new string(Enumerable.Range(0, stampLength - 1)
                                            .Select(_ =>
                                            this.clientIdCharset[rand.Next(this.clientIdCharset.Length)])
                                            .ToArray());

        this.ClientId = pid_string + "-" + clientID;
    }

    /// <summary>
    /// Validate that the options specified in this class are all sane.
    /// </summary>
    public void ValidateOptions()
    {
        this.KeepAlive = RangeValidateTwoByteInteger(this.KeepAlive);
        this.SessionExpiryInterval = RangeValidateFourByteInteger(this.SessionExpiryInterval);

        if (this.ClientReceiveMaximum != null)
        {
            this.ClientReceiveMaximum = RangeValidateTwoByteInteger((int)this.ClientReceiveMaximum);
        }

        if (this.ClientMaximumPacketSize != null)
        {
            this.ClientMaximumPacketSize = RangeValidateFourByteInteger((long)this.ClientMaximumPacketSize);
        }

        if (this.ClientTopicAliasMaximum != null)
        {
            this.ClientTopicAliasMaximum = RangeValidateTwoByteInteger((int)this.ClientTopicAliasMaximum);
        }

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
        else if (this.ClientId.Length > 23)
        {
            // FIXME: Warn on exceeded length; can use but it may not work...
        }
    }

    /// <summary>
    /// Validate that the value is within the range of a 2 byte unsigned integer.
    /// </summary>
    /// <param name="value">The value to be validated.</param>
    /// <returns>A corrected value or the original if it was in range.</returns>
    internal static int RangeValidateTwoByteInteger(int value)
    {
        if (value < UInt16.MinValue)
        {
            value = UInt16.MinValue;
        }
        else if (value > UInt16.MaxValue)
        {
            value = UInt16.MaxValue;
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
        if (value < UInt32.MinValue)
        {
            value = UInt32.MinValue;
        }
        else if (value > UInt32.MaxValue)
        {
            value = UInt32.MaxValue;
        }

        return value;
    }
}
