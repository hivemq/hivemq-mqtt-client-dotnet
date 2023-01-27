namespace HiveMQtt.Client.Events;

using HiveMQtt.MQTT5.Packets;

public class OnUnsubscribeSentEventArgs : EventArgs
{
    public OnUnsubscribeSentEventArgs(UnsubscribePacket subscribePacket) => this.UnsubscribePacket = subscribePacket;

    public UnsubscribePacket UnsubscribePacket { get; set; }
}
