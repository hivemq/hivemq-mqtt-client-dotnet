namespace HiveMQtt.Test.Packets;

using System.Buffers;
using System.IO;
using HiveMQtt.MQTT5;
using Xunit;

public class ControlPacketTest : ControlPacket
{
    public override ControlPacketType ControlPacketType => ControlPacketType.Connect;

    [Fact]
    public void TwoByteIntegerEncoding()
    {
        MemoryStream stream;
        byte[] buffer;
        ushort? decodedInteger;
        ReadOnlySequence<byte> sequence;
        SequenceReader<byte> reader;

        // Random value in range
        stream = new MemoryStream(2);
        EncodeTwoByteInteger(stream, 128);
        buffer = stream.ToArray();
        Assert.Equal(0x0, buffer[0]);
        Assert.Equal(0x80, buffer[1]);

        // Random value in range
        stream = new MemoryStream(2);
        EncodeTwoByteInteger(stream, 1789);
        buffer = stream.ToArray();
        Assert.Equal(0x06, buffer[0]);
        Assert.Equal(0xFD, buffer[1]);

        // Max Value
        stream = new MemoryStream(2);
        EncodeTwoByteInteger(stream, 65535);
        buffer = stream.ToArray();
        Assert.Equal(0xFF, buffer[0]);
        Assert.Equal(0xFF, buffer[1]);

        // Decode back
        sequence = new ReadOnlySequence<byte>(buffer);
        reader = new SequenceReader<byte>(sequence);
        decodedInteger = DecodeTwoByteInteger(ref reader);
        Assert.NotNull(decodedInteger);
        Assert.Equal((ushort)65535, decodedInteger);
    }

    [Fact]
    public void UTF8Encoding()
    {
        MemoryStream stream;
        byte[] buffer;
        string stringCandidate;
        string? decodedString;
        ReadOnlySequence<byte> sequence;
        SequenceReader<byte> reader;

        // Random String
        stream = new MemoryStream(2);
        stringCandidate = "MQTT This!";
        EncodeUTF8String(stream, stringCandidate);
        buffer = stream.ToArray();

        Assert.Equal(0x00, buffer[0]);
        Assert.Equal(0x0A, buffer[1]);
        Assert.Equal(stringCandidate.Length + 2, buffer.Length);

        sequence = new ReadOnlySequence<byte>(buffer);
        reader = new SequenceReader<byte>(sequence);
        decodedString = DecodeUTF8String(ref reader);
        Assert.Equal(stringCandidate, decodedString);

        // Empty String
        stream = new MemoryStream(2);
        stringCandidate = "";
        EncodeUTF8String(stream, stringCandidate);
        buffer = stream.ToArray();

        Assert.Equal(0x00, buffer[0]);
        Assert.Equal(0x00, buffer[1]);
        Assert.Equal(stringCandidate.Length + 2, buffer.Length);

        sequence = new ReadOnlySequence<byte>(buffer);
        reader = new SequenceReader<byte>(sequence);
        decodedString = DecodeUTF8String(ref reader);
        Assert.Equal(stringCandidate, decodedString);
    }

    [Fact]
    public void VBIEncoding()
    {
        MemoryStream stream;
        short byteCount;
        int decodedValue;
        byte[] buffer;
        ReadOnlySequence<byte> sequence;
        SequenceReader<byte> reader;

        // Min
        stream = new MemoryStream(100);
        byteCount = EncodeVariableByteInteger(stream, 0);
        Assert.Equal(1, byteCount);
        buffer = stream.ToArray();

        sequence = new ReadOnlySequence<byte>(buffer);
        reader = new SequenceReader<byte>(sequence);
        decodedValue = DecodeVariableByteInteger(ref reader);
        Assert.Equal(0, decodedValue);

        // Mid
        stream = new MemoryStream(100);
        byteCount = EncodeVariableByteInteger(stream, 16383);
        Assert.Equal(2, byteCount);
        buffer = stream.ToArray();

        sequence = new ReadOnlySequence<byte>(buffer);
        reader = new SequenceReader<byte>(sequence);
        decodedValue = DecodeVariableByteInteger(ref reader);
        Assert.Equal(16383, decodedValue);

        // Max
        stream = new MemoryStream(100);
        byteCount = EncodeVariableByteInteger(stream, 268435455);
        Assert.Equal(4, byteCount);
        buffer = stream.ToArray();

        sequence = new ReadOnlySequence<byte>(buffer);
        reader = new SequenceReader<byte>(sequence);
        decodedValue = DecodeVariableByteInteger(ref reader);
        Assert.Equal(268435455, decodedValue);
    }

    [Fact]
    public void FourByteIntegerEncoding()
    {
        MemoryStream stream;
        byte[] buffer;
        uint? decodedInteger;
        ReadOnlySequence<byte> sequence;
        SequenceReader<byte> reader;

        // Min Value
        stream = new MemoryStream(2);
        EncodeFourByteInteger(stream, 0);
        buffer = stream.ToArray();

        // Decode back
        sequence = new ReadOnlySequence<byte>(buffer);
        reader = new SequenceReader<byte>(sequence);
        decodedInteger = DecodeFourByteInteger(ref reader);
        Assert.NotNull(decodedInteger);
        Assert.Equal(UInt32.MinValue, decodedInteger);

        // Max Value
        stream = new MemoryStream(2);
        EncodeFourByteInteger(stream, UInt32.MaxValue);
        buffer = stream.ToArray();

        // Decode back
        sequence = new ReadOnlySequence<byte>(buffer);
        reader = new SequenceReader<byte>(sequence);
        decodedInteger = DecodeFourByteInteger(ref reader);
        Assert.NotNull(decodedInteger);
        Assert.Equal(UInt32.MaxValue, decodedInteger);
    }



}
