namespace HiveMQtt.Client.Events;

using HiveMQtt.MQTT5.Packets;

public class OnPublishReceivedEventArgs : EventArgs
{
    public OnPublishReceivedEventArgs(PublishPacket publishPacket) => this.PublishPacket = publishPacket;

    public PublishPacket PublishPacket { get; set; }
}
