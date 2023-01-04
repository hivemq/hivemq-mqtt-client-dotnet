namespace HiveMQtt.MQTT5.Subscribe;

using System.Buffers;
using HiveMQtt.Client.Exceptions;

/// <summary>
/// An MQTT Connect Control Packet as defined in:
/// https://docs.oasis-open.org/mqtt/mqtt/v5.0/os/mqtt-v5.0-os.html#_Toc3901074.
/// </summary>
internal class SubAckPacket : ControlPacket
{
    public bool SessionPresent { get; set; }

    public int AckFlags { get; set; }

    public List<SubAckReasonCode> ReasonCodes { get; set; }

    public SubAckPacket(ReadOnlySequence<byte> packetData)
    {
        this.ReasonCodes = new List<SubAckReasonCode>();
        this.Decode(packetData);
    }

    public override ControlPacketType ControlPacketType => ControlPacketType.SubAck;

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

        var lengthOfPropertyLength = 0;
        var propertyLength = DecodeVariableByteInteger(ref reader, out lengthOfPropertyLength);
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
