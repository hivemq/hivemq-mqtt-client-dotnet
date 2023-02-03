namespace HiveMQtt.Test.Packets;

using HiveMQtt.Client.Options;
using HiveMQtt.MQTT5.Packets;
using Xunit;

public class ConnectPacketTest
{
    [Fact]
    public void Encoding()
    {
        var options = new HiveMQClientOptions();
        Assert.NotNull(options);

        var packet = new ConnectPacket(options);
        var packetData = packet.Encode();
        Assert.NotNull(packetData);
    }
}
