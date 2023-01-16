namespace HiveMQtt.Client.Events;

using HiveMQtt.MQTT5.Packets;

public class PingReqSentEventArgs : EventArgs
{
    public PingReqSentEventArgs(PingReqPacket pingReqPacket) => this.PingReqPacket = pingReqPacket;

    public PingReqPacket PingReqPacket { get; set; }
}
