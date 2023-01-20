namespace HiveMQtt.Client.Events;

using HiveMQtt.MQTT5.Packets;

public class PubAckReceivedEventArgs : EventArgs
{
    public PubAckReceivedEventArgs(PubAckPacket pubAckPacket) => this.PubAckPacket = pubAckPacket;

    public PubAckPacket PubAckPacket { get; set; }
}
