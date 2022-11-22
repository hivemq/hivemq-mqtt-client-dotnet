namespace HiveMQtt.MQTT5;

/// <summary>
/// Representation of a Variable Byte Integer as defined in:
/// https://docs.oasis-open.org/mqtt/mqtt/v5.0/os/mqtt-v5.0-os.html#_Toc3901011
/// </summary>
public class VariableByteInteger
{
    /// <summary>
    /// Gets or sets the value of this variable byte integer.
    /// </summary>
    public int Value { get; set; }

    /// <summary>
    /// Gets or sets the length in bytes that this variable byte integer consumes when encoded.
    /// </summary>
    public int Length { get; set; }
}
