namespace HiveMQtt.Client.Events;

using HiveMQtt.MQTT5.Connect;

public class ConnAckReceivedEventArgs : EventArgs
{
    public ConnAckReceivedEventArgs(ConnAckPacket connAckPacket) => this.ConnAckPacket = connAckPacket;

    public ConnAckPacket ConnAckPacket { get; set; }
}
