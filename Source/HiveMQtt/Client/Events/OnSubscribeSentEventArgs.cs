namespace HiveMQtt.Client.Events;

using HiveMQtt.MQTT5.Packets;

public class OnSubscribeSentEventArgs : EventArgs
{
    public OnSubscribeSentEventArgs(SubscribePacket subscribePacket) => this.SubscribePacket = subscribePacket;

    public SubscribePacket SubscribePacket { get; set; }
}
