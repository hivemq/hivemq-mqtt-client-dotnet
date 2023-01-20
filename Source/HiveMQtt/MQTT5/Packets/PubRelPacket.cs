namespace HiveMQtt.MQTT5.Packets;

using System.Buffers;
using HiveMQtt.MQTT5.ReasonCodes;

/// <summary>
/// An MQTT PUBREL Control Packet as defined in:
/// https://docs.oasis-open.org/mqtt/mqtt/v5.0/os/mqtt-v5.0-os.html#_Toc3901141.
/// </summary>
public class PubRelPacket : ControlPacket
{
    public PubRelPacket(ReadOnlySequence<byte> packetData)
    {
        this.Decode(packetData);
    }

    public PubRelReasonCode ReasonCode { get; set; }

    public override ControlPacketType ControlPacketType => ControlPacketType.PubRel;

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
