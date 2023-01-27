namespace HiveMQtt.Client.Events;

using HiveMQtt.MQTT5.Packets;

public class OnPubCompReceivedEventArgs : EventArgs
{
    public OnPubCompReceivedEventArgs(PubCompPacket pubCompPacket) => this.PubCompPacket = pubCompPacket;

    public PubCompPacket PubCompPacket { get; set; }
}
