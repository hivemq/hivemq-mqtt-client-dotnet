namespace HiveMQtt.Client.Events;

using HiveMQtt.MQTT5.Packets;

public class OnPubAckReceivedEventArgs : EventArgs
{
    public OnPubAckReceivedEventArgs(PubAckPacket pubAckPacket) => this.PubAckPacket = pubAckPacket;

    public PubAckPacket PubAckPacket { get; set; }
}
