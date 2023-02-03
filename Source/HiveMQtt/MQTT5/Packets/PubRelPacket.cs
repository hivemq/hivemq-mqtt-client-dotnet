namespace HiveMQtt.MQTT5.Packets;

using System.Buffers;
using HiveMQtt.MQTT5.ReasonCodes;

/// <summary>
/// An MQTT PUBREL Control Packet as defined in:
/// https://docs.oasis-open.org/mqtt/mqtt/v5.0/os/mqtt-v5.0-os.html#_Toc3901141.
/// </summary>
public class PubRelPacket : ControlPacket
{
    public PubRelPacket(ushort packetIdentifier, PubRelReasonCode reasonCode)
    {
        this.PacketIdentifier = packetIdentifier;
        this.ReasonCode = reasonCode;
    }

    public PubRelPacket(ReadOnlySequence<byte> packetData)
    {
        this.Decode(packetData);
    }

    public PubRelReasonCode ReasonCode { get; set; }

    public override ControlPacketType ControlPacketType => ControlPacketType.PubRel;

    /// <summary>
    /// Encode this packet to be sent on the wire.
    /// </summary>
    /// <returns>An array of bytes ready to be sent.</returns>
    public byte[] Encode()
    {
        var stream = new MemoryStream(100)
        {
            Position = 2,
        };

        // Variable Header - starts at byte 2
        ControlPacket.EncodeTwoByteInteger(stream, this.PacketIdentifier);
        stream.WriteByte((byte)this.ReasonCode);
        this.EncodeProperties(stream);

        var length = stream.Length - 2;

        // Fixed Header - Add to the beginning of the stream
        stream.Position = 0;
        var byte1 = (byte)ControlPacketType.PubRec << 4;
        byte1 |= 0x2;
        stream.WriteByte((byte)byte1);
        _ = EncodeVariableByteInteger(stream, (int)length);

        return stream.ToArray();
    }

    public void Decode(ReadOnlySequence<byte> packetData)
    {
        var packetLength = packetData.Length;
        var reader = new SequenceReader<byte>(packetData);

        reader.Advance(1);

        reader.TryRead(out var remainingLength);

        var packetIdentifier = DecodeTwoByteInteger(ref reader);
        if (packetIdentifier != null)
        {
            // FIXME: validate packet identifier value (e.g. not zero)
            this.PacketIdentifier = packetIdentifier.Value;
        }

        if (reader.TryRead(out var reasonCode))
        {
            this.ReasonCode = (PubRelReasonCode)reasonCode;
        }

        var propertyLength = DecodeVariableByteInteger(ref reader);
        _ = this.DecodeProperties(ref reader, propertyLength);
    }

}
