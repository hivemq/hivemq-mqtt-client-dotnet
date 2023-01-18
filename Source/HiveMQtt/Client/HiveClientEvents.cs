namespace HiveMQtt.Client;

using System;
using System.Diagnostics;
using HiveMQtt.Client.Events;
using HiveMQtt.Client.Options;
using HiveMQtt.Client.Results;
using HiveMQtt.MQTT5.Packets;
using HiveMQtt.MQTT5.Types;

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
        Trace.WriteLine("BeforeConnectEventLauncher");
        this.BeforeConnect?.Invoke(this, eventArgs);
    }

    /// <summary>
    /// Event that is fired after the client connects to the broker.
    /// </summary>
    public event EventHandler<AfterConnectEventArgs> AfterConnect = new EventHandler<AfterConnectEventArgs>((client, e) => { });

    protected virtual void AfterConnectEventLauncher(ConnectResult results)
    {
        var eventArgs = new AfterConnectEventArgs(results);
        Trace.WriteLine("AfterConnectEventLauncher");
        this.AfterConnect?.Invoke(this, eventArgs);
    }

    /// <summary>
    /// Event that is fired before the client sends a subscribe request.
    /// </summary>
    public event EventHandler<BeforeSubscribeEventArgs> BeforeSubscribe = new EventHandler<BeforeSubscribeEventArgs>((client, e) => { });

    protected virtual void BeforeSubscribeEventLauncher(SubscribeOptions options)
    {
        var eventArgs = new BeforeSubscribeEventArgs(options);
        Trace.WriteLine("BeforeSubscribeEventLauncher");
        this.BeforeSubscribe?.Invoke(this, eventArgs);
    }

    /// <summary>
    /// Event that is fired after the client sends a subscribe request.
    /// </summary>
    public event EventHandler<AfterSubscribeEventArgs> AfterSubscribe = new EventHandler<AfterSubscribeEventArgs>((client, e) => { });

    protected virtual void AfterSubscribeEventLauncher(SubscribeResult results)
    {
        var eventArgs = new AfterSubscribeEventArgs(results);
        Trace.WriteLine("AfterSubscribeEventLauncher");
        this.AfterSubscribe?.Invoke(this, eventArgs);
    }

    /// <summary>
    /// Event that is fired when a message is received from the broker.
    /// </summary>
    public event EventHandler<OnMessageReceivedEventArgs> OnMessageReceived = new EventHandler<OnMessageReceivedEventArgs>((client, e) => { });

    protected virtual void OnMessageReceivedEventLauncher(PublishPacket packet)
    {
        var message = new MQTT5PublishMessage(packet);
        var eventArgs = new OnMessageReceivedEventArgs(message);
        Trace.WriteLine("OnMessageReceivedEventLauncher");
        this.OnMessageReceived?.Invoke(this, eventArgs);
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
        Trace.WriteLine("ConnectSentEventLauncher");
        this.OnConnectSent?.Invoke(this, eventArgs);
    }

    /// <summary>
    /// Event that is fired after the client receives a CONNACK packet from the broker.
    /// </summary>
    public event EventHandler<ConnAckReceivedEventArgs> OnConnAckReceived = new EventHandler<ConnAckReceivedEventArgs>((client, e) => { });

    protected virtual void OnConnAckReceivedEventLauncher(ConnAckPacket packet)
    {
        var eventArgs = new ConnAckReceivedEventArgs(packet);
        Trace.WriteLine("ConnAckReceivedEventLauncher");
        this.OnConnAckReceived?.Invoke(this, eventArgs);
    }

    /// <summary>
    /// Event that is fired after the client send a Disconnect packet from the broker.
    /// </summary>
    public event EventHandler<DisconnectSentEventArgs> OnDisconnectSent = new EventHandler<DisconnectSentEventArgs>((client, e) => { });

    protected virtual void OnDisconnectSentEventLauncher(DisconnectPacket packet)
    {
        var eventArgs = new DisconnectSentEventArgs(packet);
        Trace.WriteLine("DisconnectSentEventLauncher");
        this.OnDisconnectSent?.Invoke(this, eventArgs);
    }

    /// <summary>
    /// Event that is fired after the client receives a Disconnect packet from the broker.
    /// </summary>
    public event EventHandler<DisconnectReceivedEventArgs> OnDisconnectReceived = new EventHandler<DisconnectReceivedEventArgs>((client, e) => { });

    protected virtual void OnDisconnectReceivedEventLauncher(DisconnectPacket packet)
    {
        var eventArgs = new DisconnectReceivedEventArgs(packet);
        Trace.WriteLine("DisconnectReceivedEventLauncher");
        this.OnDisconnectReceived?.Invoke(this, eventArgs);
    }

    /// <summary>
    /// Event that is fired after the client send a PingReq packet from the broker.
    /// </summary>
    public event EventHandler<PingReqSentEventArgs> OnPingReqSent = new EventHandler<PingReqSentEventArgs>((client, e) => { });

    protected virtual void OnPingReqSentEventLauncher(PingReqPacket packet)
    {
        var eventArgs = new PingReqSentEventArgs(packet);
        Trace.WriteLine("PingReqSentEventLauncher");
        this.OnPingReqSent?.Invoke(this, eventArgs);
    }

    /// <summary>
    /// Event that is fired after the client send a PingReq packet from the broker.
    /// </summary>
    public event EventHandler<PingRespReceivedEventArgs> OnPingRespReceived = new EventHandler<PingRespReceivedEventArgs>((client, e) => { });

    protected virtual void OnPingRespReceivedEventLauncher(PingRespPacket packet)
    {
        var eventArgs = new PingRespReceivedEventArgs(packet);
        Trace.WriteLine("PingRespReceivedEventLauncher");
        this.OnPingRespReceived?.Invoke(this, eventArgs);
    }

    /// <summary>
    /// Event that is fired after the client send a SubAck packet from the broker.
    /// </summary>
    public event EventHandler<SubscribeSentEventArgs> OnSubscribeSent = new EventHandler<SubscribeSentEventArgs>((client, e) => { });

    protected virtual void OnSubscribeSentEventLauncher(SubscribePacket packet)
    {
        var eventArgs = new SubscribeSentEventArgs(packet);
        Trace.WriteLine("SubscribeSentEventLauncher");
        this.OnSubscribeSent?.Invoke(this, eventArgs);
    }

    /// <summary>
    /// Event that is fired after the client send a SubAck packet from the broker.
    /// </summary>
    public event EventHandler<SubAckReceivedEventArgs> OnSubAckReceived = new EventHandler<SubAckReceivedEventArgs>((client, e) => { });

    protected virtual void OnSubAckReceivedEventLauncher(SubAckPacket packet)
    {
        var eventArgs = new SubAckReceivedEventArgs(packet);
        Trace.WriteLine("SubAckReceivedEventLauncher");
        this.OnSubAckReceived?.Invoke(this, eventArgs);
    }

}
