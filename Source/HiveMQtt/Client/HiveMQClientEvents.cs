namespace HiveMQtt.Client;

using System;
using System.Diagnostics;
using HiveMQtt.Client.Events;
using HiveMQtt.Client.Options;
using HiveMQtt.Client.Results;
using HiveMQtt.MQTT5.Packets;
using HiveMQtt.MQTT5.Types;

/// <inheritdoc />
public partial class HiveMQClient : IDisposable, IHiveMQClient
{
    /* ========================================================================================= */
    // MQTT Client Events
    /* ========================================================================================= */

    /// <summary>
    /// Event that is fired before the client connects to the broker.
    /// </summary>
    public event EventHandler<BeforeConnectEventArgs> BeforeConnect = new EventHandler<BeforeConnectEventArgs>((client, e) => { });

    protected virtual void BeforeConnectEventLauncher(HiveMQClientOptions options)
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
    /// Event that is fired before the client sends a subscribe request.
    /// </summary>
    public event EventHandler<BeforeUnsubscribeEventArgs> BeforeUnsubscribe = new EventHandler<BeforeUnsubscribeEventArgs>((client, e) => { });

    protected virtual void BeforeUnsubscribeEventLauncher(List<Subscription> subscriptions)
    {
        var eventArgs = new BeforeUnsubscribeEventArgs(subscriptions);
        Trace.WriteLine("BeforeUnsubscribeEventLauncher");
        this.BeforeUnsubscribe?.Invoke(this, eventArgs);
    }

    /// <summary>
    /// Event that is fired after the client sends a subscribe request.
    /// </summary>
    public event EventHandler<AfterUnsubscribeEventArgs> AfterUnsubscribe = new EventHandler<AfterUnsubscribeEventArgs>((client, e) => { });

    protected virtual void AfterUnsubscribeEventLauncher(UnsubscribeResult results)
    {
        var eventArgs = new AfterUnsubscribeEventArgs(results);
        Trace.WriteLine("AfterUnsubscribeEventLauncher");
        this.AfterUnsubscribe?.Invoke(this, eventArgs);
    }

    /// <summary>
    /// Event that is fired when a message is received from the broker.
    /// </summary>
    public event EventHandler<OnMessageReceivedEventArgs> OnMessageReceived = new EventHandler<OnMessageReceivedEventArgs>((client, e) => { });

    protected virtual void OnMessageReceivedEventLauncher(PublishPacket packet)
    {
        var eventArgs = new OnMessageReceivedEventArgs(packet.Message);
        Trace.WriteLine("OnMessageReceivedEventLauncher");
        this.OnMessageReceived?.Invoke(this, eventArgs);
    }

    /* ========================================================================================= */
    // Packet Level Events
    /* ========================================================================================= */

    /// <summary>
    /// Event that is fired after the client sends a CONNECT packet to the broker.
    /// </summary>
    public event EventHandler<OnConnectSentEventArgs> OnConnectSent = new EventHandler<OnConnectSentEventArgs>((client, e) => { });

    protected virtual void OnConnectSentEventLauncher(ConnectPacket packet)
    {
        var eventArgs = new OnConnectSentEventArgs(packet);
        Trace.WriteLine("OnConnectSentEventLauncher");
        this.OnConnectSent?.Invoke(this, eventArgs);
    }

    /// <summary>
    /// Event that is fired after the client receives a CONNACK packet from the broker.
    /// </summary>
    public event EventHandler<OnConnAckReceivedEventArgs> OnConnAckReceived = new EventHandler<OnConnAckReceivedEventArgs>((client, e) => { });

    protected virtual void OnConnAckReceivedEventLauncher(ConnAckPacket packet)
    {
        var eventArgs = new OnConnAckReceivedEventArgs(packet);
        Trace.WriteLine("OnConnAckReceivedEventLauncher");
        this.OnConnAckReceived?.Invoke(this, eventArgs);
    }

    /// <summary>
    /// Event that is fired after the client send a Disconnect packet from the broker.
    /// </summary>
    public event EventHandler<OnDisconnectSentEventArgs> OnDisconnectSent = new EventHandler<OnDisconnectSentEventArgs>((client, e) => { });

    protected virtual void OnDisconnectSentEventLauncher(DisconnectPacket packet)
    {
        var eventArgs = new OnDisconnectSentEventArgs(packet);
        Trace.WriteLine("OnDisconnectSentEventLauncher");
        this.OnDisconnectSent?.Invoke(this, eventArgs);
    }

    /// <summary>
    /// Event that is fired after the client receives a Disconnect packet from the broker.
    /// </summary>
    public event EventHandler<OnDisconnectReceivedEventArgs> OnDisconnectReceived = new EventHandler<OnDisconnectReceivedEventArgs>((client, e) => { });

    protected virtual void OnDisconnectReceivedEventLauncher(DisconnectPacket packet)
    {
        var eventArgs = new OnDisconnectReceivedEventArgs(packet);
        Trace.WriteLine("OnDisconnectReceivedEventLauncher");
        this.OnDisconnectReceived?.Invoke(this, eventArgs);
    }

    /// <summary>
    /// Event that is fired after the client send a PingReq packet from the broker.
    /// </summary>
    public event EventHandler<OnPingReqSentEventArgs> OnPingReqSent = new EventHandler<OnPingReqSentEventArgs>((client, e) => { });

    protected virtual void OnPingReqSentEventLauncher(PingReqPacket packet)
    {
        var eventArgs = new OnPingReqSentEventArgs(packet);
        Trace.WriteLine("OnPingReqSentEventLauncher");
        this.OnPingReqSent?.Invoke(this, eventArgs);
    }

    /// <summary>
    /// Event that is fired after the client send a PingReq packet from the broker.
    /// </summary>
    public event EventHandler<OnPingRespReceivedEventArgs> OnPingRespReceived = new EventHandler<OnPingRespReceivedEventArgs>((client, e) => { });

    protected virtual void OnPingRespReceivedEventLauncher(PingRespPacket packet)
    {
        var eventArgs = new OnPingRespReceivedEventArgs(packet);
        Trace.WriteLine("OnPingRespReceivedEventLauncher");
        this.OnPingRespReceived?.Invoke(this, eventArgs);
    }

    /// <summary>
    /// Event that is fired after the client sends a Subscribe packet to the broker.
    /// </summary>
    public event EventHandler<OnSubscribeSentEventArgs> OnSubscribeSent = new EventHandler<OnSubscribeSentEventArgs>((client, e) => { });

    protected virtual void OnSubscribeSentEventLauncher(SubscribePacket packet)
    {
        var eventArgs = new OnSubscribeSentEventArgs(packet);
        Trace.WriteLine("OnSubscribeSentEventLauncher");
        this.OnSubscribeSent?.Invoke(this, eventArgs);
    }

    /// <summary>
    /// Event that is fired after the client receives a SubAck packet from the broker.
    /// </summary>
    public event EventHandler<OnSubAckReceivedEventArgs> OnSubAckReceived = new EventHandler<OnSubAckReceivedEventArgs>((client, e) => { });

    protected virtual void OnSubAckReceivedEventLauncher(SubAckPacket packet)
    {
        var eventArgs = new OnSubAckReceivedEventArgs(packet);
        Trace.WriteLine("OnSubAckReceivedEventLauncher");
        this.OnSubAckReceived?.Invoke(this, eventArgs);
    }

    /// <summary>
    /// Event that is fired after the client sends a Unsubscribe packet to the broker.
    /// </summary>
    public event EventHandler<OnUnsubscribeSentEventArgs> OnUnsubscribeSent = new EventHandler<OnUnsubscribeSentEventArgs>((client, e) => { });

    protected virtual void OnUnsubscribeSentEventLauncher(UnsubscribePacket packet)
    {
        var eventArgs = new OnUnsubscribeSentEventArgs(packet);
        Trace.WriteLine("OnUnsubscribeSentEventLauncher");
        this.OnUnsubscribeSent?.Invoke(this, eventArgs);
    }

    /// <summary>
    /// Event that is fired after the client receives a UnsubAck packet from the broker.
    /// </summary>
    public event EventHandler<OnUnsubAckReceivedEventArgs> OnUnsubAckReceived = new EventHandler<OnUnsubAckReceivedEventArgs>((client, e) => { });

    protected virtual void OnUnsubAckReceivedEventLauncher(UnsubAckPacket packet)
    {
        var eventArgs = new OnUnsubAckReceivedEventArgs(packet);
        Trace.WriteLine("OnUnsubAckReceivedEventLauncher");
        this.OnUnsubAckReceived?.Invoke(this, eventArgs);
    }

    /// <summary>
    /// Event that is fired after the client receives a Publish packet from the broker.
    /// </summary>
    public event EventHandler<OnPublishReceivedEventArgs> OnPublishReceived = new EventHandler<OnPublishReceivedEventArgs>((client, e) => { });

    protected virtual void OnPublishReceivedEventLauncher(PublishPacket packet)
    {
        var eventArgs = new OnPublishReceivedEventArgs(packet);
        Trace.WriteLine("OnPublishReceivedEventLauncher");
        this.OnPublishReceived?.Invoke(this, eventArgs);
    }

    /// <summary>
    /// Event that is fired after the client sends a Publish packet to the broker.
    /// </summary>
    public event EventHandler<OnPublishSentEventArgs> OnPublishSent = new EventHandler<OnPublishSentEventArgs>((client, e) => { });

    protected virtual void OnPublishSentEventLauncher(PublishPacket packet)
    {
        var eventArgs = new OnPublishSentEventArgs(packet);
        Trace.WriteLine("OnPublishSentEventLauncher");
        this.OnPublishSent?.Invoke(this, eventArgs);
    }

    /// <summary>
    /// Event that is fired after the client receives a PubAck packet from the broker.
    /// </summary>
    public event EventHandler<OnPubAckReceivedEventArgs> OnPubAckReceived = new EventHandler<OnPubAckReceivedEventArgs>((client, e) => { });

    protected virtual void OnPubAckReceivedEventLauncher(PubAckPacket packet)
    {
        var eventArgs = new OnPubAckReceivedEventArgs(packet);
        Trace.WriteLine("OnPubAckReceivedEventLauncher");
        this.OnPubAckReceived?.Invoke(this, eventArgs);
    }

    /// <summary>
    /// Event that is fired after the client sends a PubAck packet from the broker.
    /// </summary>
    public event EventHandler<OnPubAckSentEventArgs> OnPubAckSent = new EventHandler<OnPubAckSentEventArgs>((client, e) => { });

    protected virtual void OnPubAckSentEventLauncher(PubAckPacket packet)
    {
        var eventArgs = new OnPubAckSentEventArgs(packet);
        Trace.WriteLine("OnPubAckSentEventLauncher");
        this.OnPubAckSent?.Invoke(this, eventArgs);
    }

    /// <summary>
    /// Event that is fired after the client receives a PubRec packet from the broker.
    /// </summary>
    public event EventHandler<OnPubRecReceivedEventArgs> OnPubRecReceived = new EventHandler<OnPubRecReceivedEventArgs>((client, e) => { });

    protected virtual void OnPubRecReceivedEventLauncher(PubRecPacket packet)
    {
        var eventArgs = new OnPubRecReceivedEventArgs(packet);
        Trace.WriteLine("OnPubRecReceivedEventLauncher");
        this.OnPubRecReceived?.Invoke(this, eventArgs);
    }

    /// <summary>
    /// Event that is fired after the client sends a PubRec packet to the broker.
    /// </summary>
    public event EventHandler<OnPubRecSentEventArgs> OnPubRecSent = new EventHandler<OnPubRecSentEventArgs>((client, e) => { });

    protected virtual void OnPubRecSentEventLauncher(PubRecPacket packet)
    {
        var eventArgs = new OnPubRecSentEventArgs(packet);
        Trace.WriteLine("OnPubRecSentEventLauncher");
        this.OnPubRecSent?.Invoke(this, eventArgs);
    }

    /// <summary>
    /// Event that is fired after the client received a PubRel packet from the broker.
    /// </summary>
    public event EventHandler<OnPubRelReceivedEventArgs> OnPubRelReceived = new EventHandler<OnPubRelReceivedEventArgs>((client, e) => { });

    protected virtual void OnPubRelReceivedEventLauncher(PubRelPacket packet)
    {
        var eventArgs = new OnPubRelReceivedEventArgs(packet);
        Trace.WriteLine("OnPubRelReceivedEventLauncher");
        this.OnPubRelReceived?.Invoke(this, eventArgs);
    }

    /// <summary>
    /// Event that is fired after the client sent a PubRel packet to the broker.
    /// </summary>
    public event EventHandler<OnPubRelSentEventArgs> OnPubRelSent = new EventHandler<OnPubRelSentEventArgs>((client, e) => { });

    protected virtual void OnPubRelSentEventLauncher(PubRelPacket packet)
    {
        var eventArgs = new OnPubRelSentEventArgs(packet);
        Trace.WriteLine("OnPubRelSentEventLauncher");
        this.OnPubRelSent?.Invoke(this, eventArgs);
    }

    /// <summary>
    /// Event that is fired after the client receives a PubComp packet from the broker.
    /// </summary>
    public event EventHandler<OnPubCompReceivedEventArgs> OnPubCompReceived = new EventHandler<OnPubCompReceivedEventArgs>((client, e) => { });

    protected virtual void OnPubCompReceivedEventLauncher(PubCompPacket packet)
    {
        var eventArgs = new OnPubCompReceivedEventArgs(packet);
        Trace.WriteLine("PubCompReceivedEventLauncher");
        this.OnPubCompReceived?.Invoke(this, eventArgs);
    }

    /// <summary>
    /// Event that is fired after the client sends a PubComp packet to the broker.
    /// </summary>
    public event EventHandler<OnPubCompSentEventArgs> OnPubCompSent = new EventHandler<OnPubCompSentEventArgs>((client, e) => { });

    protected virtual void OnPubCompSentEventLauncher(PubCompPacket packet)
    {
        var eventArgs = new OnPubCompSentEventArgs(packet);
        Trace.WriteLine("PubCompSentEventLauncher");
        this.OnPubCompSent?.Invoke(this, eventArgs);
    }
}
