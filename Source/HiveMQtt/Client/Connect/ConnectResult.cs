namespace HiveMQtt.Client.Connect;

using HiveMQtt.MQTT5;

/// <summary>
/// Results of the connect operation.
/// </summary>
public class ConnectResult
{
    // Results of the connect operation.
    public ConnectResult()
    {
    }

    public bool SessionPresent { get; internal set; }

    public ConnAckReasonCode ReasonCode { get; internal set; }
}
