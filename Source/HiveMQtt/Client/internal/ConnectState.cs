namespace HiveMQtt.Client.Internal;

/// <summary>
/// The state of the MQTT connection.
/// </summary>
internal enum ConnectState
{
    /// <summary>
    /// The connection is being established.
    /// </summary>
    Connecting = 0x00,

    /// <summary>
    /// The connection is established.
    /// </summary>
    Connected = 0x01,

    /// <summary>
    /// The connection is being disconnected.
    /// </summary>
    Disconnecting = 0x02,

    /// <summary>
    /// The connection is disconnected.
    /// </summary>
    Disconnected = 0x03,
}
