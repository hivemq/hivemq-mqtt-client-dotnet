namespace HiveMQtt.MQTT5;

using System;
using System.Buffers;
using System.IO;

/// <summary>
/// An MQTT Disconnect Control Packet as defined in:
/// https://docs.oasis-open.org/mqtt/mqtt/v5.0/os/mqtt-v5.0-os.html#_Toc3901205
/// </summary>
public class DisconnectPacket : ControlPacket
{
    private readonly ReadOnlySequence<byte> rawPacketData;

    public DisconnectPacket(ReadOnlySequence<byte> data)
    {
        this.rawPacketData = data;
        this.Decode();
    }

    public override ControlPacketType ControlPacketType => ControlPacketType.Disconnect;

    public void Decode()
    {
        var packetLength = this.rawPacketData.Length;
        var reader = new SequenceReader<byte>(this.rawPacketData);

        // Skip past the Fixed Header
        reader.Advance(2);

        if (reader.TryRead(out var ackFlags))
        {
            this.SessionPresent = (ackFlags & 0x1) == 0x1;
        }

        if (reader.TryRead(out var reasonCode))
        {
            this.ReasonCode = (ConnAckReasonCode)reasonCode;
        }

        var propertyLength = DecodeVariableByteInteger(ref reader);
        _ = this.DecodeProperties(ref reader, propertyLength);

    }

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
        stream.WriteByte(((byte)ControlPacketType.Disconnect) << 4);
        EncodeVariableByteInteger(stream, (int)remainingLength);

        var data = stream.GetBuffer();
        var segment = new ArraySegment<byte>(data, 0, (int)stream.Length);
        return segment.ToArray();
    }


}
