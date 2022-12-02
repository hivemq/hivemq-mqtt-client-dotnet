namespace HiveMQtt;

using System.Collections;
using HiveMQtt.MQTT5;

/// <summary>
/// Options class for connect.
/// </summary>
public class DisconnectOptions
{
    public DisconnectOptions()
    {
        this.UserProperties = new Hashtable();
        this.DisconnectReasonCode = DisconnectReasonCode.NormalDisconnection;
    }

    public DisconnectReasonCode DisconnectReasonCode { get; set; }

    public int? SessionExpiry { get; set; }

    public string? ReasonString { get; set; }

    public Hashtable UserProperties { get; set; }
}
