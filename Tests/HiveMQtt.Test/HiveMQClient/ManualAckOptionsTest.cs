namespace HiveMQtt.Test.HiveMQClient;

using HiveMQtt.Client;
using HiveMQtt.Client.Events;
using HiveMQtt.Client.Options;
using HiveMQtt.MQTT5.Types;
using Xunit;

public class ManualAckOptionsTest
{
    [Fact]
    public void ManualAckEnabled_DefaultsToFalse()
    {
        var options = new HiveMQClientOptions();
        Assert.False(options.ManualAckEnabled);
    }

    [Fact]
    public void WithManualAck_SetsOptionTrue()
    {
        var options = new HiveMQClientOptionsBuilder()
            .WithManualAck()
            .Build();
        Assert.True(options.ManualAckEnabled);
    }

    [Fact]
    public void WithManualAckTrue_SetsOptionTrue()
    {
        var options = new HiveMQClientOptionsBuilder()
            .WithManualAck(true)
            .Build();
        Assert.True(options.ManualAckEnabled);
    }

    [Fact]
    public void WithManualAckFalse_SetsOptionFalse()
    {
        var options = new HiveMQClientOptionsBuilder()
            .WithManualAck(false)
            .Build();
        Assert.False(options.ManualAckEnabled);
    }

    [Fact]
    public void OnMessageReceivedEventArgs_ConstructorMessageOnly_PacketIdentifierIsNull()
    {
        var message = new MQTT5PublishMessage
        {
            Topic = "test/topic",
            Payload = System.Text.Encoding.UTF8.GetBytes("payload"),
            QoS = QualityOfService.AtMostOnceDelivery,
        };
        var args = new OnMessageReceivedEventArgs(message);
        Assert.Equal(message, args.PublishMessage);
        Assert.Null(args.PacketIdentifier);
    }

    [Fact]
    public void OnMessageReceivedEventArgs_ConstructorWithPacketId_PacketIdentifierIsSet()
    {
        var message = new MQTT5PublishMessage
        {
            Topic = "test/topic",
            Payload = System.Text.Encoding.UTF8.GetBytes("payload"),
            QoS = QualityOfService.AtLeastOnceDelivery,
        };
        const ushort packetId = 42;
        var args = new OnMessageReceivedEventArgs(message, packetId);
        Assert.Equal(message, args.PublishMessage);
        Assert.NotNull(args.PacketIdentifier);
        Assert.Equal(packetId, args.PacketIdentifier!.Value);
    }

    [Fact]
    public void OnMessageReceivedEventArgs_ConstructorWithNullPacketId_PacketIdentifierIsNull()
    {
        var message = new MQTT5PublishMessage
        {
            Topic = "test/topic",
            Payload = System.Text.Encoding.UTF8.GetBytes("payload"),
            QoS = QualityOfService.AtMostOnceDelivery,
        };
        var args = new OnMessageReceivedEventArgs(message, null);
        Assert.Equal(message, args.PublishMessage);
        Assert.Null(args.PacketIdentifier);
    }
}
