namespace HiveMQtt.Client.Events;

using HiveMQtt.MQTT5.Packets;

public class OnPublishSentEventArgs : EventArgs
{
    public OnPublishSentEventArgs(PublishPacket publishPacket) => this.PublishPacket = publishPacket;

    public PublishPacket PublishPacket { get; set; }
}
