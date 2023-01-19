namespace HiveMQtt.MQTT5.Packets;

using System.Buffers;
using HiveMQtt.MQTT5.ReasonCodes;
using HiveMQtt.Client.Exceptions;

/// <summary>
/// An MQTT Connect Control Packet as defined in:
/// https://docs.oasis-open.org/mqtt/mqtt/v5.0/os/mqtt-v5.0-os.html#_Toc3901074.
/// </summary>
public class UnsubAckPacket : ControlPacket
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UnsubAckPacket"/> class.
    /// </summary>
    /// <param name="packetData">The raw packet data off the wire.</param>
    public UnsubAckPacket(ReadOnlySequence<byte> packetData)
    {
        this.ReasonCodes = new List<UnsubAckReasonCode>();
        this.Decode(packetData);
    }

    /// <summary>
    /// Gets or sets the list of Reason Codes in this packet.
    /// </summary>
    public List<UnsubAckReasonCode> ReasonCodes { get; set; }

    /// <inheritdoc/>
    public override ControlPacketType ControlPacketType => ControlPacketType.SubAck;

    /// <summary>
    /// Decodes the raw packet data.
    /// </summary>
    /// <param name="packetData">The raw packet data off the wire.</param>
    public void Decode(ReadOnlySequence<byte> packetData)
    {
        var packetLength = packetData.Length;
        var reader = new SequenceReader<byte>(packetData);

        if (reader.TryRead(out var headerByte))
        {
            if (headerByte != 0xB0)
            {
                throw new HiveMQttClientException("Invalid UnsubAck header byte");
            }
        }

        var remainingLength = DecodeVariableByteInteger(ref reader);
        var packetIdentifier = DecodeTwoByteInteger(ref reader);

        var propertyLength = DecodeVariableByteInteger(ref reader, out var lengthOfPropertyLength);
        _ = this.DecodeProperties(ref reader, propertyLength);

        // Payload
        var payloadLength = remainingLength - lengthOfPropertyLength - propertyLength - 2;

        // The Payload contains a list of Reason Codes.
        for (var x = 0; x < payloadLength; x++)
        {
            reader.TryRead(out var reasonCode);
            this.ReasonCodes.Add((UnsubAckReasonCode)reasonCode);
        }
    }

}
