namespace HiveMQtt.Client.Events;

using HiveMQtt.MQTT5.Packets;

public class DisconnectReceivedEventArgs : EventArgs
{
    public DisconnectReceivedEventArgs(DisconnectPacket packet) => this.DisconnectPacket = packet;

    public DisconnectPacket DisconnectPacket { get; set; }
}
