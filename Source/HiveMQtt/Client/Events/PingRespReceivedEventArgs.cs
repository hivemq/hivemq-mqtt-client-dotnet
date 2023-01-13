namespace HiveMQtt.Client.Events;

using HiveMQtt.MQTT5.Ping;

public class PingRespReceivedEventArgs : EventArgs
{
    public PingRespReceivedEventArgs(PingRespPacket pingRespPacket) => this.PingRespPacket = pingRespPacket;

    public PingRespPacket PingRespPacket { get; set; }
}
