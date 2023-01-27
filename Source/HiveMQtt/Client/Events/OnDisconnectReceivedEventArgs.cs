namespace HiveMQtt.Client.Events;

using HiveMQtt.MQTT5.Packets;

public class OnDisconnectReceivedEventArgs : EventArgs
{
    public OnDisconnectReceivedEventArgs(DisconnectPacket packet) => this.DisconnectPacket = packet;

    public DisconnectPacket DisconnectPacket { get; set; }
}
