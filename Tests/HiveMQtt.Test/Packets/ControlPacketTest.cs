namespace HiveMQtt.Test.Packets;

using System.Buffers;
using System.IO;
using HiveMQtt.MQTT5;
using HiveMQtt.MQTT5.Exceptions;
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
        stringCandidate = string.Empty;
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
        Assert.Equal(uint.MinValue, decodedInteger);

        // Max Value
        stream = new MemoryStream(2);
        EncodeFourByteInteger(stream, uint.MaxValue);
        buffer = stream.ToArray();

        // Decode back
        sequence = new ReadOnlySequence<byte>(buffer);
        reader = new SequenceReader<byte>(sequence);
        decodedInteger = DecodeFourByteInteger(ref reader);
        Assert.NotNull(decodedInteger);
        Assert.Equal(uint.MaxValue, decodedInteger);
    }

    [Fact]
    public void GetVariableByteIntegerSize_ReturnsCorrectSize()
    {
        // 1-byte values (0 to 127)
        Assert.Equal(1, GetVariableByteIntegerSize(0));
        Assert.Equal(1, GetVariableByteIntegerSize(1));
        Assert.Equal(1, GetVariableByteIntegerSize(127));
        Assert.Equal(1, GetVariableByteIntegerSize(0x7F));

        // 2-byte values (128 to 16383)
        Assert.Equal(2, GetVariableByteIntegerSize(128));
        Assert.Equal(2, GetVariableByteIntegerSize(0x80));
        Assert.Equal(2, GetVariableByteIntegerSize(16383));
        Assert.Equal(2, GetVariableByteIntegerSize(0x3FFF));

        // 3-byte values (16384 to 2097151)
        Assert.Equal(3, GetVariableByteIntegerSize(16384));
        Assert.Equal(3, GetVariableByteIntegerSize(0x4000));
        Assert.Equal(3, GetVariableByteIntegerSize(2097151));
        Assert.Equal(3, GetVariableByteIntegerSize(0x1FFFFF));

        // 4-byte values (2097152 to 268435455)
        Assert.Equal(4, GetVariableByteIntegerSize(2097152));
        Assert.Equal(4, GetVariableByteIntegerSize(0x200000));
        Assert.Equal(4, GetVariableByteIntegerSize(268435455));
        Assert.Equal(4, GetVariableByteIntegerSize(0xFFFFFFF));

        // Boundary values
        Assert.Equal(1, GetVariableByteIntegerSize(0x7F));
        Assert.Equal(2, GetVariableByteIntegerSize(0x80));
        Assert.Equal(2, GetVariableByteIntegerSize(0x3FFF));
        Assert.Equal(3, GetVariableByteIntegerSize(0x4000));
        Assert.Equal(3, GetVariableByteIntegerSize(0x1FFFFF));
        Assert.Equal(4, GetVariableByteIntegerSize(0x200000));
    }

    [Fact]
    public void GetVariableByteIntegerSize_ThrowsOnInvalidRange()
    {
        // Negative values
        Assert.Throws<MQTTProtocolException>(() => GetVariableByteIntegerSize(-1));
        Assert.Throws<MQTTProtocolException>(() => GetVariableByteIntegerSize(int.MinValue));

        // Values exceeding maximum
        Assert.Throws<MQTTProtocolException>(() => GetVariableByteIntegerSize(268435456));
        Assert.Throws<MQTTProtocolException>(() => GetVariableByteIntegerSize(int.MaxValue));
    }

    [Fact]
    public void EncodeVariableByteIntegerToSpan_EncodesCorrectly()
    {
        byte[] buffer;
        int bytesWritten;
        int decodedValue;
        ReadOnlySequence<byte> sequence;
        SequenceReader<byte> reader;
        MemoryStream stream;
        byte[] streamBuffer;

        // 1-byte encoding (0 to 127)
        buffer = new byte[4];
        bytesWritten = EncodeVariableByteIntegerToSpan(buffer, 0);
        Assert.Equal(1, bytesWritten);
        Assert.Equal(0x00, buffer[0]);

        // Verify it can be decoded
        sequence = new ReadOnlySequence<byte>(buffer[..bytesWritten]);
        reader = new SequenceReader<byte>(sequence);
        decodedValue = DecodeVariableByteInteger(ref reader);
        Assert.Equal(0, decodedValue);

        // Compare with MemoryStream version
        stream = new MemoryStream(4);
        _ = EncodeVariableByteInteger(stream, 0);
        streamBuffer = stream.ToArray();
        Assert.Equal(streamBuffer, buffer[..bytesWritten]);

        // Test value 127 (max 1-byte)
        buffer = new byte[4];
        bytesWritten = EncodeVariableByteIntegerToSpan(buffer, 127);
        Assert.Equal(1, bytesWritten);
        Assert.Equal(0x7F, buffer[0]);

        sequence = new ReadOnlySequence<byte>(buffer[..bytesWritten]);
        reader = new SequenceReader<byte>(sequence);
        decodedValue = DecodeVariableByteInteger(ref reader);
        Assert.Equal(127, decodedValue);

        // 2-byte encoding (128 to 16383)
        buffer = new byte[4];
        bytesWritten = EncodeVariableByteIntegerToSpan(buffer, 128);
        Assert.Equal(2, bytesWritten);
        Assert.Equal(0x80, buffer[0]);
        Assert.Equal(0x01, buffer[1]);

        sequence = new ReadOnlySequence<byte>(buffer[..bytesWritten]);
        reader = new SequenceReader<byte>(sequence);
        decodedValue = DecodeVariableByteInteger(ref reader);
        Assert.Equal(128, decodedValue);

        // Compare with MemoryStream version
        stream = new MemoryStream(4);
        _ = EncodeVariableByteInteger(stream, 128);
        streamBuffer = stream.ToArray();
        Assert.Equal(streamBuffer, buffer[..bytesWritten]);

        // Test value 16383 (max 2-byte)
        buffer = new byte[4];
        bytesWritten = EncodeVariableByteIntegerToSpan(buffer, 16383);
        Assert.Equal(2, bytesWritten);
        Assert.Equal(0xFF, buffer[0]);
        Assert.Equal(0x7F, buffer[1]);

        sequence = new ReadOnlySequence<byte>(buffer[..bytesWritten]);
        reader = new SequenceReader<byte>(sequence);
        decodedValue = DecodeVariableByteInteger(ref reader);
        Assert.Equal(16383, decodedValue);

        // 3-byte encoding (16384 to 2097151)
        buffer = new byte[4];
        bytesWritten = EncodeVariableByteIntegerToSpan(buffer, 16384);
        Assert.Equal(3, bytesWritten);
        Assert.Equal(0x80, buffer[0]);
        Assert.Equal(0x80, buffer[1]);
        Assert.Equal(0x01, buffer[2]);

        sequence = new ReadOnlySequence<byte>(buffer[..bytesWritten]);
        reader = new SequenceReader<byte>(sequence);
        decodedValue = DecodeVariableByteInteger(ref reader);
        Assert.Equal(16384, decodedValue);

        // Compare with MemoryStream version
        stream = new MemoryStream(4);
        _ = EncodeVariableByteInteger(stream, 16384);
        streamBuffer = stream.ToArray();
        Assert.Equal(streamBuffer, buffer[..bytesWritten]);

        // Test value 2097151 (max 3-byte)
        buffer = new byte[4];
        bytesWritten = EncodeVariableByteIntegerToSpan(buffer, 2097151);
        Assert.Equal(3, bytesWritten);

        sequence = new ReadOnlySequence<byte>(buffer[..bytesWritten]);
        reader = new SequenceReader<byte>(sequence);
        decodedValue = DecodeVariableByteInteger(ref reader);
        Assert.Equal(2097151, decodedValue);

        // 4-byte encoding (2097152 to 268435455)
        buffer = new byte[4];
        bytesWritten = EncodeVariableByteIntegerToSpan(buffer, 2097152);
        Assert.Equal(4, bytesWritten);
        Assert.Equal(0x80, buffer[0]);
        Assert.Equal(0x80, buffer[1]);
        Assert.Equal(0x80, buffer[2]);
        Assert.Equal(0x01, buffer[3]);

        sequence = new ReadOnlySequence<byte>(buffer[..bytesWritten]);
        reader = new SequenceReader<byte>(sequence);
        decodedValue = DecodeVariableByteInteger(ref reader);
        Assert.Equal(2097152, decodedValue);

        // Compare with MemoryStream version
        stream = new MemoryStream(4);
        _ = EncodeVariableByteInteger(stream, 2097152);
        streamBuffer = stream.ToArray();
        Assert.Equal(streamBuffer, buffer[..bytesWritten]);

        // Test value 268435455 (max 4-byte)
        buffer = new byte[4];
        bytesWritten = EncodeVariableByteIntegerToSpan(buffer, 268435455);
        Assert.Equal(4, bytesWritten);
        Assert.Equal(0xFF, buffer[0]);
        Assert.Equal(0xFF, buffer[1]);
        Assert.Equal(0xFF, buffer[2]);
        Assert.Equal(0x7F, buffer[3]);

        sequence = new ReadOnlySequence<byte>(buffer[..bytesWritten]);
        reader = new SequenceReader<byte>(sequence);
        decodedValue = DecodeVariableByteInteger(ref reader);
        Assert.Equal(268435455, decodedValue);

        // Compare with MemoryStream version
        stream = new MemoryStream(4);
        _ = EncodeVariableByteInteger(stream, 268435455);
        streamBuffer = stream.ToArray();
        Assert.Equal(streamBuffer, buffer[..bytesWritten]);
    }

    [Fact]
    public void EncodeVariableByteIntegerToSpan_ThrowsOnInvalidRange()
    {
        var buffer = new byte[4];

        // Negative values
        Assert.Throws<MQTTProtocolException>(() => EncodeVariableByteIntegerToSpan(buffer, -1));
        Assert.Throws<MQTTProtocolException>(() => EncodeVariableByteIntegerToSpan(buffer, int.MinValue));

        // Values exceeding maximum
        Assert.Throws<MQTTProtocolException>(() => EncodeVariableByteIntegerToSpan(buffer, 268435456));
        Assert.Throws<MQTTProtocolException>(() => EncodeVariableByteIntegerToSpan(buffer, int.MaxValue));
    }

    [Fact]
    public void EncodeVariableByteIntegerToSpan_MatchesMemoryStreamVersion()
    {
        // Test a variety of values to ensure both methods produce identical output
        var testValues = new[]
        {
            0, 1, 127, 128, 255, 256, 1000, 16383, 16384, 32767, 65535, 100000, 2097151, 2097152,
            1000000, 268435455,
        };

        foreach (var value in testValues)
        {
            // Encode using span
            var spanBuffer = new byte[4];
            var spanBytesWritten = EncodeVariableByteIntegerToSpan(spanBuffer, value);

            // Encode using MemoryStream
            var stream = new MemoryStream(4);
            var streamBytesWritten = EncodeVariableByteInteger(stream, value);
            var streamBuffer = stream.ToArray();

            // Verify sizes match
            Assert.Equal(streamBytesWritten, spanBytesWritten);
            Assert.Equal(streamBuffer.Length, spanBytesWritten);

            // Verify byte content matches
            Assert.Equal(streamBuffer, spanBuffer[..spanBytesWritten]);

            // Verify both can be decoded to the same value
            var spanSequence = new ReadOnlySequence<byte>(spanBuffer[..spanBytesWritten]);
            var spanReader = new SequenceReader<byte>(spanSequence);
            var spanDecoded = DecodeVariableByteInteger(ref spanReader);

            var streamSequence = new ReadOnlySequence<byte>(streamBuffer);
            var streamReader = new SequenceReader<byte>(streamSequence);
            var streamDecoded = DecodeVariableByteInteger(ref streamReader);

            Assert.Equal(value, spanDecoded);
            Assert.Equal(value, streamDecoded);
        }
    }
}
