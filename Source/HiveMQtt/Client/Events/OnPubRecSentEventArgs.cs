namespace HiveMQtt.Client.Events;

using HiveMQtt.MQTT5.Packets;

public class OnPubRecSentEventArgs : EventArgs
{
    public OnPubRecSentEventArgs(PubRecPacket pubRecPacket) => this.PubRecPacket = pubRecPacket;

    public PubRecPacket PubRecPacket { get; set; }
}
