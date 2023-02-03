namespace HiveMQtt.Client.Events;

using HiveMQtt.MQTT5.Packets;

public class OnPublishQoS1CompleteEventArgs : EventArgs
{
    public OnPublishQoS1CompleteEventArgs(PubAckPacket packet) => this.PubAckPacket = packet;

    public PubAckPacket PubAckPacket { get; set; }
}
