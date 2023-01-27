namespace HiveMQtt.Client.Events;

using HiveMQtt.MQTT5.Packets;

public class OnPubRecReceivedEventArgs : EventArgs
{
    public OnPubRecReceivedEventArgs(PubRecPacket pubRecPacket) => this.PubRecPacket = pubRecPacket;

    public PubRecPacket PubRecPacket { get; set; }
}
