namespace HiveMQtt.Test.Packets;

using HiveMQtt.MQTT5;
using HiveMQtt.Client;
using Xunit;

public class ConnectPacketTest
{
    [Fact]
    public void Encoding()
    {
        var options = new HiveClientOptions();
        Assert.NotNull(options);

        var packet = new ConnectPacket(options);
        var packetData = packet.Encode();
        Assert.NotNull(packetData);
    }
}
