namespace HiveMQtt.Client.Events;

using HiveMQtt.MQTT5.Packets;

public class SubAckReceivedEventArgs : EventArgs
{
    public SubAckReceivedEventArgs(SubAckPacket subAckPacket) => this.SubAckPacket = subAckPacket;

    public SubAckPacket SubAckPacket { get; set; }
}
