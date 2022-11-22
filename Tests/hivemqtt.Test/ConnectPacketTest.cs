namespace HiveMQtt.Test;

using HiveMQtt.MQTT5;
using Microsoft.VisualStudio.TestPlatform.Common.Utilities;
using Xunit;

public class ConnectPacketTest
{
    [Fact]
    public void Encoding()
    {
        var options = new ClientOptions();
        Assert.NotNull(options);

        var packet = new ConnectPacket(options);
        var stream = packet.Encode();
        Assert.NotNull(stream);
    }

}
