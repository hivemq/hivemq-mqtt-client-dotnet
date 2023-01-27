namespace HiveMQtt.Client.Events;

using HiveMQtt.MQTT5.Packets;

public class OnConnectSentEventArgs : EventArgs
{
    public OnConnectSentEventArgs(ConnectPacket connectPacket) => this.ConnectPacket = connectPacket;

    public ConnectPacket ConnectPacket { get; set; }
}
