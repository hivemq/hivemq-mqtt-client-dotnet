namespace HiveMQtt.Client.Events;

using HiveMQtt.MQTT5.Packets;

public class PublishReceivedEventArgs : EventArgs
{
    public PublishReceivedEventArgs(PublishPacket publishPacket) => this.PublishPacket = publishPacket;

    public PublishPacket PublishPacket { get; set; }
}
