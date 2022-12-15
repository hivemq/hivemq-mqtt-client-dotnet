namespace HiveMQtt.MQTT5.Types;

/// <summary>
/// MQTT v5 Payload Format Indicator.
/// </summary>
public enum MQTT5PayloadFormatIndicator
{
    /// <summary>
    /// Indicates that the Payload is unspecified bytes, which is equivalent to not sending a Payload Format Indicator.
    /// </summary>
    Unspecified = 0,

    /// <summary>
    /// Indicates that the Payload is UTF-8 Encoded Character Data. The UTF-8 data in the Payload MUST be well-formed
    /// UTF-8 as defined by the Unicode specification [Unicode] and restated in RFC 3629 [RFC3629].
    /// </summary>
    UTF8Encoded = 1,
}
