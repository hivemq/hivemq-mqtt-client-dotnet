namespace HiveMQtt.Client.Events;

using HiveMQtt.MQTT5.Packets;

public class OnSubAckReceivedEventArgs : EventArgs
{
    public OnSubAckReceivedEventArgs(SubAckPacket subAckPacket) => this.SubAckPacket = subAckPacket;

    public SubAckPacket SubAckPacket { get; set; }
}
