namespace HiveMQtt.MQTT5.Connect;

using System.Buffers;
using System.IO;

/// <summary>
/// An MQTT Disconnect Control Packet as defined in:
/// https://docs.oasis-open.org/mqtt/mqtt/v5.0/os/mqtt-v5.0-os.html#_Toc3901205.
/// </summary>
internal class DisconnectPacket : ControlPacket
{
    private readonly ReadOnlySequence<byte> rawPacketData;

    public DisconnectPacket() { }

    public DisconnectPacket(ReadOnlySequence<byte> data)
    {
        this.rawPacketData = data;
        this.Decode();
    }

    public override ControlPacketType ControlPacketType => ControlPacketType.Disconnect;

    public DisconnectReasonCode DisconnectReasonCode { get; set; }

    public void Decode()
    {
        var reader = new SequenceReader<byte>(this.rawPacketData);

        // Skip past the Fixed Header
        reader.Advance(1);

        if (reader.TryRead(out var remainingLength))
        {
            if (remainingLength + 2 > this.rawPacketData.Length)
            {
                // Not enough packet data / partial packet
                // FIXME: Send back to pipeline to get more data
            }
            else if (remainingLength < 1)
            {
                // Byte 1 in the Variable Header is the Disconnect Reason Code. If the Remaining Length is less
                // than 1 the value of 0x00 (Normal disconnection) is used.
                // See <see href="https://docs.oasis-open.org/mqtt/mqtt/v5.0/os/mqtt-v5.0-os.html#_Toc3901208">
                // Disconnect Reason Codes</see>.
                this.DisconnectReasonCode = DisconnectReasonCode.NormalDisconnection;
            }
            else
            {
                if (reader.TryRead(out var reasonCode))
                {
                    this.DisconnectReasonCode = (DisconnectReasonCode)reasonCode;

                    var propertyLength = DecodeVariableByteInteger(ref reader);
                    _ = this.DecodeProperties(ref reader, propertyLength);
                }
            }
        }
    }

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
        stream.WriteByte((int)DisconnectReasonCode.NormalDisconnection);

        // Disconnect has no payload

        // Fixed Header - Add to the beginning of the stream
        var remainingLength = stream.Length - 2;

        stream.Position = 0;
        stream.WriteByte((byte)ControlPacketType.Disconnect << 4);
        EncodeVariableByteInteger(stream, (int)remainingLength);

        return stream.ToArray();
    }
}
