namespace HiveMQtt.Client.Events;

using HiveMQtt.MQTT5.Packets;

public class PubCompReceivedEventArgs : EventArgs
{
    public PubCompReceivedEventArgs(PubCompPacket pubCompPacket) => this.PubCompPacket = pubCompPacket;

    public PubCompPacket PubCompPacket { get; set; }
}
