namespace HiveMQtt.Test;

using HiveMQtt.MQTT5;
using Xunit;

public class ControlPacketTest : ControlPacket
{
    public override ControlPacketType ControlPacketType => ControlPacketType.Connect;

    [Fact]
    public void TwoByteIntegerEncoding()
    {
        var stream = new MemoryStream(2);
        var value = 128;

        EncodeTwoByteInteger(stream, value);
        var buffer = stream.GetBuffer();

        Assert.Equal(0x8, buffer[0]);
        Assert.Equal(0x0, buffer[1]);
    }
}
