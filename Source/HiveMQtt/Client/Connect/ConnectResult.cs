namespace HiveMQtt.Client.Connect;

using System.Collections;
using HiveMQtt.MQTT5.Connect;
using HiveMQtt.MQTT5.Types;

/// <summary>
/// Results of the connect operation.
/// </summary>
public class ConnectResult
{
    // Results of the connect operation.
    internal ConnectResult(ConnAckReasonCode reasonCode, bool sessionPresent, MQTT5Properties properties)
    {
        this.ReasonCode = reasonCode;
        this.SessionPresent = sessionPresent;
        this.Properties = properties;
    }

    /// <summary>
    /// Gets a value indicating whether the server is using a session state from a previous connection.
    /// </summary>
    public bool SessionPresent { get; internal set; }

    /// <summary>
    /// Gets a value that represents the reason code for the connection success or failure.
    /// <para>
    /// When an MQTT broker accepts a connection without error, this value will be
    /// <c>ConnAckReasonCode.Success</c>.  In error cases, see
    /// <seealso cref="ConnAckReasonCode">ConnAckReasonCode</seealso>.
    /// </para>
    /// </summary>
    public ConnAckReasonCode ReasonCode { get; internal set; }

    /// <summary>
    /// Gets the Session Expiry Interval in use by the MQTT broker for this connection.
    /// </summary>
    /// <returns>The Session Expiry Interval in seconds or null if unspecified.</returns>
    public int? SessionExpiryInterval => (int?)this.Properties.SessionExpiryInterval;

    /// <summary>
    /// Gets the maximum number of QoS 1 and QoS 2 publications that the
    /// MQTT broker is willing to process concurrently for this client.
    /// <para>
    /// If the Receive Maximum value is absent, then its value defaults to 65,535. It does
    /// not provide a mechanism to limit the QoS 0 publications that the Client might try to send.
    /// </para>
    /// </summary>
    /// <returns>The integer value of the maximum number of QoS 1 + QoS 2 concurrent messages.</returns>
    public int? BrokerReceiveMaximum => this.Properties.ReceiveMaximum;

    /// <summary>
    /// Gets the Maximum Packet Size the MQTT broker is willing to accept.
    /// <para>
    /// If the Maximum Packet Size is not present, there is no limit on the packet size imposed beyond
    /// the limitations in the protocol as a result of the remaining length encoding and the protocol
    /// header sizes.
    /// </para>
    /// </summary>
    public int? BrokerMaximumPacketSize => (int?)this.Properties.MaximumPacketSize;

    /// <summary>
    /// Gets the maximum the maximum quality of service (QoS) that this MQTT broker
    /// supports.
    /// <para>
    /// QoS 0 (At Most Once Delivery) = The message is delivered according to the capabilities of the underlying network.
    /// The message arrives at the receiver either once or not at all.
    /// </para>
    /// <para>
    /// QoS 1 (At Least Once Delivery) = Ensures that the message arrives at the receiver at least once.
    /// </para>
    /// <para>
    /// QoS 2 (Exactly Once Delivery) = The highest Quality of Service level, for use when neither loss nor duplication of messages are acceptable.
    /// </para>
    /// </summary>
    /// <returns>The maximum Quality of Service level supported by the MQTT broker.</returns>
    public int? MaximumQoS => this.Properties.MaximumQoS;

    /// <summary>
    /// Gets a value indicating whether gets the flags indicating whether the MQTT broker supports Retained Messages.
    /// </summary>
    public bool RetainAvailable
    {
        get
        {
            if (this.Properties.RetainAvailable is null or true)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }

    /// <summary>
    /// Gets the Client Identifier which was assigned by the Server because a zero length Client
    /// Identifier was found in the CONNECT packet.
    /// </summary>
    public string? AssignedClientIdentifier => this.Properties.AssignedClientIdentifier;

    /// <summary>
    /// Gets the value that indicates the highest value that the Server will accept as a Topic
    /// Alias sent by the Client.
    /// </summary>
    public int BrokerTopicAliasMaximum
    {
        get
        {
            if (this.Properties.TopicAliasMaximum is null)
            {
                return 0;
            }
            else
            {
                return (int)this.Properties.TopicAliasMaximum;
            }
        }
    }

    /// <summary>
    /// Gets a value that is a human readable string designed for diagnostics.
    /// </summary>
    public string? ReasonString => this.Properties.ReasonString;

    /// <summary>
    /// Gets a HashTable containing the User Properties returned by the MQTT broker.
    /// </summary>
    public Hashtable UserProperties => this.Properties.UserProperties;

    /// <summary>
    /// Gets a value indicating whether the Server supports Wildcard Subscriptions.
    /// </summary>
    public bool WildcardSubscriptionAvailable
    {
        get
        {
            if (this.Properties.WildcardSubscriptionAvailable is null or true)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }

    /// <summary>
    /// Gets a value indicating whether the Server supports Subscription Identifiers.
    /// </summary>
    public bool SubscriptionIdentifiersAvailable
    {
        get
        {
            if (this.Properties.SubscriptionIdentifiersAvailable is null or true)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }

    /// <summary>
    /// Gets a value indicating whether the Server supports Shared Subscriptions.
    /// </summary>
    public bool SharedSubscriptionAvailable
    {
        get
        {
            if (this.Properties.SharedSubscriptionAvailable is null or true)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }

    /// <summary>
    /// Gets a value indicating the session expiry duration in seconds.
    /// <para>
    /// If this value is set, this overrides any Keep Alive sent in the Connect request.
    /// </para>
    /// </summary>
    public int? ServerKeepAlive => this.Properties.ServerKeepAlive;

    /// <summary>
    /// Gets a value that is in response to the RequestResponseInformation flag in the connect
    /// request.
    /// <para>
    /// If null, RequestResponseInformation wasn't set or it is not supported by this MQTT broker.
    /// </para>
    /// </summary>
    public string? ResponseInformation => this.Properties.ResponseInformation;

    /// <summary>
    /// Gets a String which can be used by the Client to identify another Server to use.
    /// <para>
    /// The Server uses a Server Reference in either a CONNACK or DISCONNECT packet with
    /// Reason code of 0x9C (Use another server) or Reason Code 0x9D (Server moved).
    /// </para>
    /// </summary>
    public string? ServerReference => this.Properties.ServerReference;

    /// <summary>
    /// Gets a string containing the name of the authentication method.
    /// </summary>
    public string? AuthenticationMethod => this.Properties.AuthenticationMethod;

    /// <summary>
    /// Gets a byte array containing authentication data.
    /// </summary>
    public byte[]? AuthenticationData => this.Properties.AuthenticationData;

    /// <summary>
    /// Gets or sets the MQTT Properties returned from the connection request.
    /// <para>
    /// This class holds the specific properties
    /// <see href="https://docs.oasis-open.org/mqtt/mqtt/v5.0/os/mqtt-v5.0-os.html#_Toc3901080">
    /// returned in the CONNACK response
    /// </see>.
    /// </para>
    /// </summary>
    internal MQTT5Properties Properties { get; set; }
}
