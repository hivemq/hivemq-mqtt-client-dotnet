namespace HiveMQtt.MQTT5.Packets;

using System.Buffers;
using HiveMQtt.Client.Exceptions;
using HiveMQtt.MQTT5.ReasonCodes;

/// <summary>
/// An MQTT SUBACK Control Packet as defined in:
/// https://docs.oasis-open.org/mqtt/mqtt/v5.0/os/mqtt-v5.0-os.html#_Toc3901187.
/// </summary>
public class SubAckPacket : ControlPacket
{
    public SubAckPacket(ReadOnlySequence<byte> packetData)
    {
        this.ReasonCodes = new List<SubAckReasonCode>();
        this.Decode(packetData);
    }

    /// <summary>
    /// Gets or sets the Session Present flag.
    /// </summary>
    public bool SessionPresent { get; set; }

    /// <summary>
    /// Gets or sets the Ack Flags.
    /// </summary>
    public int AckFlags { get; set; }

    /// <summary>
    /// Gets or sets the list of Reason Codes.
    /// </summary>
    public List<SubAckReasonCode> ReasonCodes { get; set; }

    /// <inheritdoc/>
    public override ControlPacketType ControlPacketType => ControlPacketType.SubAck;

    /// <summary>
    /// Decodes the raw packet data.
    /// </summary>
    public void Decode(ReadOnlySequence<byte> packetData)
    {
        var packetLength = packetData.Length;
        var reader = new SequenceReader<byte>(packetData);

        if (reader.TryRead(out var headerByte))
        {
            if (headerByte != 0x90)
            {
                throw new HiveMQttClientException("Invalid SubAck header byte");
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
            this.ReasonCodes.Add((SubAckReasonCode)reasonCode);
        }
    }

}
