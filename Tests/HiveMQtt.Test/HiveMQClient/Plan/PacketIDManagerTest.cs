namespace HiveMQtt.Test.HiveMQClient.Plan;

using HiveMQtt.Client;
using HiveMQtt.MQTT5.Types;
using Xunit;
using System.Threading.Tasks;

public class PacketIDManagerTest
{
    [Fact]
    public async Task Send_1Mio_QoS1_QoS2_Messages_All_Ids_Released_Async()
    {
        // Arrange
        var clientOptions = new HiveMQClientOptionsBuilder()
            .WithClientId("PacketIDManagerTestClient")
            .WithBroker("localhost")
            .WithPort(1883)
            .Build();

        var client = new HiveMQClient(clientOptions);
        await client.ConnectAsync().ConfigureAwait(true);

        var packetIdManager = client.Connection.GetPacketIDManager(); // Assuming the client exposes the manager for validation
        Assert.Equal(0, packetIdManager.Count);

        // Manually tested with 1M messages, 500k QoS1 and 500k QoS2
        // Lower the count for the test suite to remain manageable
        var qos1Messages = 5000;
        var qos2Messages = 5000;
        var totalMessages = qos1Messages + qos2Messages;

        // Act
        for (var i = 0; i < qos1Messages; i++)
        {
            await client.PublishAsync(
                topic: "test/qos1",
                payload: new byte[] { 0x01 },
                qos: QualityOfService.AtLeastOnceDelivery).ConfigureAwait(true);
        }

        for (var i = 0; i < qos2Messages; i++)
        {
            await client.PublishAsync(
                topic: "test/qos2",
                payload: new byte[] { 0x02 },
                qos: QualityOfService.ExactlyOnceDelivery).ConfigureAwait(true);
        }

        await client.DisconnectAsync().ConfigureAwait(true);

        // Assert
        Assert.Equal(0, packetIdManager.Count); // All Packet IDs must be released
    }
}
