namespace HiveMQtt.MQTT5.ReasonCodes;

/// <summary>
/// MQTT v5.0 AUTH Reason Codes as defined in:
/// https://docs.oasis-open.org/mqtt/mqtt/v5.0/os/mqtt-v5.0-os.html#_Toc3901220
/// </summary>
public enum AuthReasonCode
{
    /// <summary>
    /// Authentication is successful.
    /// </summary>
    Success = 0x00,

    /// <summary>
    /// Continue the authentication with another step.
    /// </summary>
    ContinueAuthenticaton = 0x18,

    /// <summary>
    /// Initiate a re-authentication.
    /// </summary>
    ReAuthenticate = 0x19
}
