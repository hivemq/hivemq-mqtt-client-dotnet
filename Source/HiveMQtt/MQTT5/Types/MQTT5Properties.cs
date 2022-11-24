namespace HiveMQtt.MQTT5.Types;

using System.Collections;

/// <summary>
/// MQTT version 5 properties as defined in
/// https://docs.oasis-open.org/mqtt/mqtt/v5.0/os/mqtt-v5.0-os.html#_Toc3901027
/// </summary>
public class MQTT5Properties
{
    public MQTT5Properties() => this.UserProperties = new Hashtable();

    public byte? PayloadFormatIndicator { get; set; }

    public int? MessageExpiryInterval { get; set; }

    public string? ContentType { get; set; }

    public string? ResponseTopic { get; set; }

    public byte[]? CorrelationData { get; set; }

    public int? SubscriptionIdentifier { get; set; }

    public int? SessionExpiryInterval { get; set; }

    public string? AssignedClientIdentifier { get; set; }

    public int? ServerKeepAlive { get; set; }

    public string? AuthenticationMethod { get; set; }

    public byte[]? AuthenticationData { get; set; }

    public byte? RequestProblemInformation { get; set; }

    public int? WillDelayInterval { get; set; }

    public byte? RequestResponseInformation { get; set; }

    public string? ResponseInformation { get; set; }

    public string? ServerReference { get; set; }

    public string? ReasonString { get; set; }

    public int? ReceiveMaximum { get; set; }

    public int? TopicAliasMaximum { get; set; }

    public int? TopicAlias { get; set; }

    public int? MaximumQoS { get; set; }

    public int? RetainAvailable { get; set; }

    public Hashtable UserProperties { get; set; }

    public int? MaximumPacketSize { get; set; }

    public bool? WildcardSubscriptionAvailable { get; set; }

    public bool? SubscriptionIdentifierAvailable { get; set; }

    public bool? SharedSubscriptionAvailable { get; set; }

}
