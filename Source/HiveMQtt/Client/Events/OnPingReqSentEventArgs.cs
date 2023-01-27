namespace HiveMQtt.Client.Events;

using HiveMQtt.MQTT5.Packets;

public class OnPingReqSentEventArgs : EventArgs
{
    public OnPingReqSentEventArgs(PingReqPacket pingReqPacket) => this.PingReqPacket = pingReqPacket;

    public PingReqPacket PingReqPacket { get; set; }
}
