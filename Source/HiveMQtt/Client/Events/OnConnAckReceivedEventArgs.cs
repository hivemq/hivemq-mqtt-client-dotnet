namespace HiveMQtt.Client.Events;

using HiveMQtt.MQTT5.Packets;

public class OnConnAckReceivedEventArgs : EventArgs
{
    public OnConnAckReceivedEventArgs(ConnAckPacket connAckPacket) => this.ConnAckPacket = connAckPacket;

    public ConnAckPacket ConnAckPacket { get; set; }
}
