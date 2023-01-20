namespace HiveMQtt.MQTT5.Types;

/// <summary>
/// MQTT v5.0 PropertyType as defined in
/// https://docs.oasis-open.org/mqtt/mqtt/v5.0/os/mqtt-v5.0-os.html#_Toc3901027
/// </summary>
public enum MQTT5DataType
{
    /// <summary>
    /// Property is a Byte.
    /// </summary>
    Byte = 0x0,

    /// <summary>
    /// Property is a Two Byte Integer.
    /// </summary>
    TwoByteInteger = 0x1,

    /// <summary>
    /// Property is a Four Byte Integer.
    /// </summary>
    FourByteInteger = 0x2,

    /// <summary>
    /// Property is a UTF-8 encoded string.
    /// </summary>
    UTF8EncodedString = 0x3,

    /// <summary>
    /// Property is a UTF-8 encoded string pair.
    /// </summary>
    UTF8EncodedStringPair = 0x4,

    /// <summary>
    /// Property is Binary Data.
    /// </summary>
    BinaryData = 0x5,

    /// <summary>
    /// Property is Variable Byte Integer.
    /// </summary>
    VariableByteInteger = 0x6,
}
