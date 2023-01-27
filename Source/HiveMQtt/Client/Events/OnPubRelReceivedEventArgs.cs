namespace HiveMQtt.Client.Events;

using HiveMQtt.MQTT5.Packets;

public class OnPubRelReceivedEventArgs : EventArgs
{
    public OnPubRelReceivedEventArgs(PubRelPacket pubRelPacket) => this.PubRelPacket = pubRelPacket;

    public PubRelPacket PubRelPacket { get; set; }
}
