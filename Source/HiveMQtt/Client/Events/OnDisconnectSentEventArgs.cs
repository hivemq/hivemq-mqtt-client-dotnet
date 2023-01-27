namespace HiveMQtt.Client.Events;

using HiveMQtt.MQTT5.Packets;

public class OnDisconnectSentEventArgs : EventArgs
{
    public OnDisconnectSentEventArgs(DisconnectPacket packet) => this.DisconnectPacket = packet;

    public DisconnectPacket DisconnectPacket { get; set; }
}
