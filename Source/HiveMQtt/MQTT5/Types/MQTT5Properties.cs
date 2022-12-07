namespace HiveMQtt.MQTT5.Types;

using System.Collections;

/// <summary>
/// MQTT version 5 properties as defined in
/// https://docs.oasis-open.org/mqtt/mqtt/v5.0/os/mqtt-v5.0-os.html#_Toc3901027
/// </summary>
internal class MQTT5Properties
{
    public MQTT5Properties() => this.UserProperties = new Hashtable();

    public byte? PayloadFormatIndicator { get; set; }

    public UInt32? MessageExpiryInterval { get; set; }

    public string? ContentType { get; set; }

    public string? ResponseTopic { get; set; }

    public byte[]? CorrelationData { get; set; }

    public uint? SubscriptionIdentifier { get; set; }

    /// <summary>
    /// Gets or sets a value that represents the session expiration duration in use by the MQTT broker.
    /// </summary>
    public UInt32? SessionExpiryInterval { get; set; }

    /// <summary>
    /// Gets or sets the Client Identifier which was assigned by the Server because a zero length Client
    /// Identifier was found in the CONNECT packet.
    /// </summary>
    public string? AssignedClientIdentifier { get; set; }

    /// <summary>
    /// Gets or sets a value indicating the session expiry duration.
    /// <para>
    /// If this value is set, this overrides any Keep Alive sent in the Connect request.
    /// </para>
    /// </summary>
    public UInt16? ServerKeepAlive { get; set; }

    /// <summary>
    /// Gets or sets a string containing the name of the authentication method.
    /// </summary>
    public string? AuthenticationMethod { get; set; }

    /// <summary>
    /// Gets or sets a byte array containing authentication data.
    /// </summary>
    public byte[]? AuthenticationData { get; set; }

    public byte? RequestProblemInformation { get; set; }

    public UInt32? WillDelayInterval { get; set; }

    public byte? RequestResponseInformation { get; set; }

    /// <summary>
    /// Gets or sets a value that is in response to the RequestResponseInformation flag in the connect
    /// request.
    /// <para>
    /// If null, RequestResponseInformation wasn't set or it is not supported by this MQTT broker.
    /// </para>
    /// </summary>
    public string? ResponseInformation { get; set; }

    /// <summary>
    /// Gets or sets a String which can be used by the Client to identify another Server to use.
    /// <para>
    /// The Server uses a Server Reference in either a CONNACK or DISCONNECT packet with
    /// Reason code of 0x9C (Use another server) or Reason Code 0x9D (Server moved).
    /// </para>
    /// </summary>
    public string? ServerReference { get; set; }

    /// <summary>
    /// Gets or sets a value that is a human readable string designed for diagnostics.
    /// </summary>
    public string? ReasonString { get; set; }

    /// <summary>
    /// Gets or sets a value that indicates the maximum number of QoS 1 and QoS 2 publications that the
    /// MQTT broker is willing to process concurrently.
    /// </summary>
    public UInt16? ReceiveMaximum { get; set; }

    /// <summary>
    /// Gets or sets a value that indicates the highest value that the Server will accept as a Topic Alias sent by the Client.
    /// <para>
    /// The Server uses this value to limit the number of Topic Aliases that it is willing to hold on this Connection.
    /// A value of 0 indicates that the Server does not accept any Topic Aliases on this connection.
    /// </para>
    /// </summary>
    public UInt16? TopicAliasMaximum { get; set; }

    public UInt16? TopicAlias { get; set; }

    /// <summary>
    /// Gets or sets a value that indicates the maximum "Quality of Service" level that the MQTT
    /// broker is willing to accept.
    /// </summary>
    public UInt16? MaximumQoS { get; set; }

    /// <summary>
    /// Gets or sets a value that indicates whether the MQTT broker supports Retained Messages.
    /// </summary>
    public bool? RetainAvailable { get; set; }

    /// <summary>
    /// Gets or sets a HashTable containing the User Properties returned by the MQTT broker.
    /// </summary>
    public Hashtable UserProperties { get; set; }

    /// <summary>
    /// Gets or sets a value that indicates the maximum packet size that the MQTT broker is willing
    /// accept.
    /// </summary>
    public UInt32? MaximumPacketSize { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the Server supports Wildcard Subscriptions.
    /// </summary>
    public bool? WildcardSubscriptionAvailable { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the Server supports Subscription Identifiers.
    /// </summary>
    public bool? SubscriptionIdentifiersAvailable { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the Server supports Shared Subscriptions.
    /// </summary>
    public bool? SharedSubscriptionAvailable { get; set; }

}
