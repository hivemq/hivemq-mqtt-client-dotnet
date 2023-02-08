namespace HiveMQtt.Client.ReasonCodes;

using HiveMQtt.Client.Results;
using HiveMQtt.MQTT5.ReasonCodes;

/// <summary>
/// Reason codes for QoS 2 publishes.
/// <para>
/// This is a helper enum for the <see cref="PublishResult"/> class.  The reason codes in here
/// are the same as and duplicate of the reason codes in <see cref="PubRecReasonCode"/>.
/// </para>
/// </summary>
public enum QoS2ReasonCode
{
    /// <summary>
    /// The message is accepted. Publication of the QoS 2 message proceeds.
    /// </summary>
    Success = 0x00,

    /// <summary>
    /// The message is accepted but there are no subscribers. This is sent only by the Server.
    /// If the Server knows that there are no matching subscribers, it MAY use this Reason Code instead of 0x00 (Success).
    /// </summary>
    NoMatchingSubscribers = 0x10,

    /// <summary>
    /// The receiver does not accept the publish but either does not want to reveal the reason, or it does not match one of the other values.
    /// </summary>
    UnspecifiedError = 0x80,

    /// <summary>
    /// The PUBLISH is valid but the receiver is not willing to accept it.
    /// </summary>
    ImplementationSpecificError = 0x83,

    /// <summary>
    /// The PUBLISH is not authorized.
    /// </summary>
    NotAuthorized = 0x87,

    /// <summary>
    /// The Topic Name is not malformed, but is not accepted by this Server.
    /// </summary>
    TopicNameInvalid = 0x90,

    /// <summary>
    /// The Packet Identifier is already in use. This might indicate a mismatch in the Session State between the Client and Server.
    /// </summary>
    PacketIdentifierInUse = 0x91,

    /// <summary>
    /// An implementation or administrative imposed limit has been exceeded.
    /// </summary>
    QuotaExceeded = 0x97,

    /// <summary>
    /// The payload format does not match the one specified by the payload format indicator.
    /// </summary>
    PayloadFormatInvalid = 0x99
}
