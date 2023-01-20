namespace HiveMQtt.Client.Options;

using HiveMQtt.MQTT5.Packets;
using HiveMQtt.MQTT5.ReasonCodes;

/// <summary>
/// The options class for a Disconnect call.
/// </summary>
public class DisconnectOptions
{
    public DisconnectOptions()
    {
        this.UserProperties = new Dictionary<string, string>();

        // Set the default disconnect reason
        this.ReasonCode = DisconnectReasonCode.NormalDisconnection;
    }

    /// <summary>
    /// Gets or sets the reason code for the disconnection.  The default value is
    /// <c>NormalDisconnection</c>.
    /// </summary>
    public DisconnectReasonCode ReasonCode { get; set; }

    /// <summary>
    /// Gets or sets the session expiry.  This sets the expiration for the session
    /// to the indicated value.  The value respresents the session expiration time
    /// in seconds.
    /// </summary>
    public int? SessionExpiry { get; set; }

    /// <summary>
    /// Gets or sets the reason string for the disconnection.  This is a human readable
    /// string that is used for diagnostics only.
    /// </summary>
    public string? ReasonString { get; set; }

    // FIXME: Add documentation
    public Dictionary<string, string> UserProperties { get; set; }
}
