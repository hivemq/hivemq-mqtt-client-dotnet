namespace HiveMQtt.Client.Events;

using HiveMQtt.MQTT5.Packets;

public class OnPingRespReceivedEventArgs : EventArgs
{
    public OnPingRespReceivedEventArgs(PingRespPacket pingRespPacket) => this.PingRespPacket = pingRespPacket;

    public PingRespPacket PingRespPacket { get; set; }
}
