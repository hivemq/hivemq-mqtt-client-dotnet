namespace HiveMQtt.Client.Events;

using HiveMQtt.MQTT5.Packets;

public class ConnectSentEventArgs : EventArgs
{
    public ConnectSentEventArgs(ConnectPacket connectPacket) => this.ConnectPacket = connectPacket;

    public ConnectPacket ConnectPacket { get; set; }
}
