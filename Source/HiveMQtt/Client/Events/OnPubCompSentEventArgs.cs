namespace HiveMQtt.Client.Events;

using HiveMQtt.MQTT5.Packets;

public class OnPubCompSentEventArgs : EventArgs
{
    public OnPubCompSentEventArgs(PubCompPacket pubCompPacket) => this.PubCompPacket = pubCompPacket;

    public PubCompPacket PubCompPacket { get; set; }
}
