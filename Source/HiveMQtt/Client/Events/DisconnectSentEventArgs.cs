namespace HiveMQtt.Client.Events;

using HiveMQtt.MQTT5.Connect;

public class DisconnectSentEventArgs : EventArgs
{
    public DisconnectSentEventArgs(DisconnectPacket packet) => this.DisconnectPacket = packet;

    public DisconnectPacket DisconnectPacket { get; set; }
}
