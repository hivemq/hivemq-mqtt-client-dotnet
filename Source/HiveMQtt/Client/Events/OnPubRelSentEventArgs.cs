namespace HiveMQtt.Client.Events;

using HiveMQtt.MQTT5.Packets;

public class OnPubRelSentEventArgs : EventArgs
{
    public OnPubRelSentEventArgs(PubRelPacket pubRelPacket) => this.PubRelPacket = pubRelPacket;

    public PubRelPacket PubRelPacket { get; set; }
}
