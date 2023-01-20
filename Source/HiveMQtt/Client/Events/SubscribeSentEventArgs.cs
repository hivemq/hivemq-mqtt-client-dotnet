namespace HiveMQtt.Client.Events;

using HiveMQtt.MQTT5.Packets;

public class SubscribeSentEventArgs : EventArgs
{
    public SubscribeSentEventArgs(SubscribePacket subscribePacket) => this.SubscribePacket = subscribePacket;

    public SubscribePacket SubscribePacket { get; set; }
}
