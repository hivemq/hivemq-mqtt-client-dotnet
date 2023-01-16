namespace HiveMQtt.Client;

using System;
using HiveMQtt.Client.Events;
using HiveMQtt.Client.Options;
using HiveMQtt.Client.Results;
using HiveMQtt.MQTT5.Packets;

/// <inheritdoc />
public partial class HiveClient : IDisposable, IHiveClient
{
    /* ========================================================================================= */
    // MQTT Client Events
    /* ========================================================================================= */

    /// <summary>
    /// Event that is fired before the client connects to the broker.
    /// </summary>
    public event EventHandler<BeforeConnectEventArgs> BeforeConnect = new EventHandler<BeforeConnectEventArgs>((client, e) => { });

    protected virtual void BeforeConnectEventLauncher(HiveClientOptions options)
    {
        var eventArgs = new BeforeConnectEventArgs(options);
        this.BeforeConnect?.Invoke(this, eventArgs);
    }

    /// <summary>
    /// Event that is fired when the client connects to the broker.
    /// </summary>
    public event EventHandler<ConnectedEventArgs> OnConnected = new EventHandler<ConnectedEventArgs>((client, e) => { });

    protected virtual void OnConnectedEventLauncher(ConnectResult connectResult)
    {
        var eventArgs = new ConnectedEventArgs(connectResult);
        this.OnConnected?.Invoke(this, eventArgs);
    }

    /// <summary>
    /// Event that is fired after the client connects to the broker.
    /// </summary>
    public event EventHandler<AfterConnectEventArgs> AfterConnect = new EventHandler<AfterConnectEventArgs>((client, e) => { });

    protected virtual void AfterConnectEventLauncher(ConnectResult results)
    {
        var eventArgs = new AfterConnectEventArgs(results);
        this.AfterConnect?.Invoke(this, eventArgs);
    }

    /* ========================================================================================= */
    // Packet Level Events
    /* ========================================================================================= */

    /// <summary>
    /// Event that is fired after the client sends a CONNECT packet to the broker.
    /// </summary>
    public event EventHandler<ConnectSentEventArgs> OnConnectSent = new EventHandler<ConnectSentEventArgs>((client, e) => { });

    protected virtual void OnConnectSentEventLauncher(ConnectPacket packet)
    {
        var eventArgs = new ConnectSentEventArgs(packet);
        this.OnConnectSent?.Invoke(this, eventArgs);
    }

    /// <summary>
    /// Event that is fired after the client receives a CONNACK packet from the broker.
    /// </summary>
    public event EventHandler<ConnAckReceivedEventArgs> OnConnAckReceived = new EventHandler<ConnAckReceivedEventArgs>((client, e) => { });

    protected virtual void OnConnAckReceivedEventLauncher(ConnAckPacket packet)
    {
        var eventArgs = new ConnAckReceivedEventArgs(packet);
        this.OnConnAckReceived?.Invoke(this, eventArgs);
    }

    /// <summary>
    /// Event that is fired after the client send a Disconnect packet from the broker.
    /// </summary>
    public event EventHandler<DisconnectSentEventArgs> OnDisconnectSent = new EventHandler<DisconnectSentEventArgs>((client, e) => { });

    protected virtual void OnDisconnectSentEventLauncher(DisconnectPacket packet)
    {
        var eventArgs = new DisconnectSentEventArgs(packet);
        this.OnDisconnectSent?.Invoke(this, eventArgs);
    }

    /// <summary>
    /// Event that is fired after the client receives a Disconnect packet from the broker.
    /// </summary>
    public event EventHandler<DisconnectReceivedEventArgs> OnDisconnectReceived = new EventHandler<DisconnectReceivedEventArgs>((client, e) => { });

    protected virtual void OnDisconnectReceivedEventLauncher(DisconnectPacket packet)
    {
        var eventArgs = new DisconnectReceivedEventArgs(packet);
        this.OnDisconnectReceived?.Invoke(this, eventArgs);
    }

    /// <summary>
    /// Event that is fired after the client send a PingReq packet from the broker.
    /// </summary>
    public event EventHandler<PingReqSentEventArgs> OnPingReqSent = new EventHandler<PingReqSentEventArgs>((client, e) => { });

    protected virtual void OnPingReqSentEventLauncher(PingReqPacket packet)
    {
        var eventArgs = new PingReqSentEventArgs(packet);
        this.OnPingReqSent?.Invoke(this, eventArgs);
    }

    /// <summary>
    /// Event that is fired after the client send a PingReq packet from the broker.
    /// </summary>
    public event EventHandler<PingRespReceivedEventArgs> OnPingRespReceived = new EventHandler<PingRespReceivedEventArgs>((client, e) => { });

    protected virtual void OnPingRespReceivedEventLauncher(PingRespPacket packet)
    {
        var eventArgs = new PingRespReceivedEventArgs(packet);
        this.OnPingRespReceived?.Invoke(this, eventArgs);
    }


}
