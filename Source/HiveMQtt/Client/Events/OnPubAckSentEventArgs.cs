namespace HiveMQtt.Client.Events;

using HiveMQtt.MQTT5.Packets;

public class OnPubAckSentEventArgs : EventArgs
{
    public OnPubAckSentEventArgs(PubAckPacket pubAckPacket) => this.PubAckPacket = pubAckPacket;

    public PubAckPacket PubAckPacket { get; set; }
}
