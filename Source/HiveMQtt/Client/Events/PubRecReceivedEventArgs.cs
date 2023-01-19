namespace HiveMQtt.Client.Events;

using HiveMQtt.MQTT5.Packets;

public class PubRecReceivedEventArgs : EventArgs
{
    public PubRecReceivedEventArgs(PubRecPacket pubRecPacket) => this.PubRecPacket = pubRecPacket;

    public PubRecPacket PubRecPacket { get; set; }
}
