namespace HiveMQtt.Client.Events;

using HiveMQtt.MQTT5.Packets;

public class PubRelReceivedEventArgs : EventArgs
{
    public PubRelReceivedEventArgs(PubRelPacket pubRelPacket) => this.PubRelPacket = pubRelPacket;

    public PubRelPacket PubRelPacket { get; set; }
}
