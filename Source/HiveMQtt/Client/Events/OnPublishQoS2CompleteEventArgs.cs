namespace HiveMQtt.Client.Events;

using HiveMQtt.MQTT5.Packets;

public class OnPublishQoS2CompleteEventArgs : EventArgs
{
    public OnPublishQoS2CompleteEventArgs(PubRecPacket packet) => this.PubRecPacket = packet;

    public PubRecPacket PubRecPacket { get; set; }
}
