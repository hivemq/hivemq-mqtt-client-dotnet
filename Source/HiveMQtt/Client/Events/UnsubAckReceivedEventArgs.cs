namespace HiveMQtt.Client.Events;

using HiveMQtt.MQTT5.Packets;

public class UnsubAckReceivedEventArgs : EventArgs
{
    public UnsubAckReceivedEventArgs(UnsubAckPacket unsubAckPacket) => this.UnsubAckPacket = unsubAckPacket;

    public UnsubAckPacket UnsubAckPacket { get; set; }
}
