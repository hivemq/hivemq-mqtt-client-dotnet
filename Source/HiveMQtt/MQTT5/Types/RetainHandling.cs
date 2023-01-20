namespace HiveMQtt.MQTT5.Types;

/// <summary>
/// Defines the Retain Handling options for a subscription as defined in
/// the <see href="https://docs.oasis-open.org/mqtt/mqtt/v5.0/os/mqtt-v5.0-os.html#_Toc3901169">
/// MQTT Specification</see>.
/// </summary>
public enum RetainHandling
{
    /// <summary>
    /// Send retained messages at the time of the subscribe.
    /// </summary>
    SendAtSubscribe = 0x0,

    /// <summary>
    /// Send retained messages at subscribe only if the subscription does not currently exist.
    /// </summary>
    SendAtSubscribeIfNewSubscription = 0x1,

    /// <summary>
    /// Do not send retained messages at the time of the subscribe.
    /// </summary>
    DoNotSendAtSubscribe = 0x2,
}
