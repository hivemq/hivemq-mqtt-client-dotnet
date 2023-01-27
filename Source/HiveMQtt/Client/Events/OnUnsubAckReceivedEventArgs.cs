namespace HiveMQtt.Client.Events;

using HiveMQtt.MQTT5.Packets;

public class OnUnsubAckReceivedEventArgs : EventArgs
{
    public OnUnsubAckReceivedEventArgs(UnsubAckPacket unsubAckPacket) => this.UnsubAckPacket = unsubAckPacket;

    public UnsubAckPacket UnsubAckPacket { get; set; }
}
