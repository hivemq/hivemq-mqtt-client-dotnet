namespace HiveMQtt.Client.Connect;

using HiveMQtt.MQTT5;
using HiveMQtt.MQTT5.Types;

/// <summary>
/// Results of the connect operation.
/// </summary>
public class ConnectResult
{
    // Results of the connect operation.
    public ConnectResult(ConnAckReasonCode reasonCode, bool sessionPresent, MQTT5Properties properties)
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
    /// Gets the MQTT Properties returned from the connection request.
    /// <para>
    /// This class holds the specific properties
    /// <see href="https://docs.oasis-open.org/mqtt/mqtt/v5.0/os/mqtt-v5.0-os.html#_Toc3901080">
    /// returned in the CONNACK response
    /// </see>.
    /// </para>
    /// </summary>
    public MQTT5Properties Properties { get; internal set; }
}
