namespace HiveMQtt.Test;

using HiveMQtt.MQTT5;
using Xunit;

public class ConnectPacketTest
{
    [Fact]
    public void Encoding()
    {
        var options = new ClientOptions();
        Assert.NotNull(options);

        var packet = new ConnectPacket(options);
        var packetData = packet.Encode();
        Assert.NotNull(packetData);
    }
}
