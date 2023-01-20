namespace HiveMQtt.MQTT5.ReasonCodes;

/// <summary>
/// MQTT v5.0 PUBREL Reason Codes as defined in:
/// https://docs.oasis-open.org/mqtt/mqtt/v5.0/os/mqtt-v5.0-os.html#_Toc3901144
/// </summary>
public enum PubRelReasonCode
{
    /// <summary>
    /// Message released.
    /// </summary>
    Success = 0x00,

    /// <summary>
    /// The Packet Identifier is not known. This is not an error during recovery, but at other times
    /// indicates a mismatch between the Session State on the Client and Server.
    /// </summary>
    PacketIdentifierNotFound = 0x92

}
