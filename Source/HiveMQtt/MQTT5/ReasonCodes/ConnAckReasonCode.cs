namespace HiveMQtt.MQTT5.ReasonCodes;

/// <summary>
/// MQTT v5.0 CONNACK Reason Codes as defined in:
/// https://docs.oasis-open.org/mqtt/mqtt/v5.0/os/mqtt-v5.0-os.html#_Toc3901079
/// </summary>
public enum ConnAckReasonCode
{
    /// <summary>
    /// The Connection is accepted.
    /// </summary>
    Success = 0x0,

    /// <summary>
    /// The Server does not wish to reveal the reason for the failure, or none of the other Reason Codes apply.
    /// </summary>
    UnspecifiedError = 0x80,

    /// <summary>
    /// Data within the CONNECT packet could not be correctly parsed.
    /// </summary>
    MalformedPacket = 0x81,

    /// <summary>
    /// Data in the CONNECT packet does not conform to this specification.
    /// </summary>
    ProtocolError = 0x82,

    /// <summary>
    /// The CONNECT is valid but is not accepted by this Server.
    /// </summary>
    ImplementationSpecificError = 0x83,

    /// <summary>
    /// The Server does not support the version of the MQTT protocol requested by the Client.
    /// </summary>
    UnsupportedProtocolVersion = 0x84,

    /// <summary>
    /// The Client Identifier is a valid string but is not allowed by the Server.
    /// </summary>
    ClientIdentifierNotValid = 0x85,

    /// <summary>
    /// The Server does not accept the User Name or Password specified by the Client
    /// </summary>
    BadUserNameOrPassword = 0x86,

    /// <summary>
    /// The Client is not authorized to connect.
    /// </summary>
    NotAuthorized = 0x87,

    /// <summary>
    /// The MQTT Server is not available.
    /// </summary>
    ServerUnavailable = 0x88,

    /// <summary>
    /// The Server is busy. Try again later.
    /// </summary>
    ServerBusy = 0x89,

    /// <summary>
    /// This Client has been banned by administrative action. Contact the server administrator.
    /// </summary>
    Banned = 0x8A,

    /// <summary>
    /// The authentication method is not supported or does not match the authentication method currently in use.
    /// </summary>
    BadAuthenticationMethod = 0x8C,

    /// <summary>
    /// The Will Topic Name is not malformed, but is not accepted by this Server.
    /// </summary>
    TopicNameInvalid = 0x90,

    /// <summary>
    /// The CONNECT packet exceeded the maximum permissible size.
    /// </summary>
    PacketTooLarge = 0x95,

    /// <summary>
    /// An implementation or administrative imposed limit has been exceeded.
    /// </summary>
    QuotaExceeded = 0x97,

    /// <summary>
    /// The Will Payload does not match the specified Payload Format Indicator.
    /// </summary>
    PayloadFormatInvalid = 0x99,

    /// <summary>
    /// The Server does not support retained messages, and Will Retain was set to 1.
    /// </summary>
    RetainNotSupported = 0x9A,

    /// <summary>
    /// The Server does not support the QoS set in Will QoS.
    /// </summary>
    QoSNotSupported = 0x9B,

    /// <summary>
    /// The Client should temporarily use another server.
    /// </summary>
    UseAnotherServer = 0x9C,

    /// <summary>
    /// The Client should permanently use another server.
    /// </summary>
    ServerMoved = 0x9D,

    /// <summary>
    /// The connection rate limit has been exceeded.
    /// </summary>
    ConnectionRateExceeded = 0x9F,
}
