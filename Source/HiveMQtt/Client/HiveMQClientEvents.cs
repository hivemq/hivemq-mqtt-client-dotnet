/*
 * Copyright 2023-present HiveMQ and the HiveMQ Community
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
namespace HiveMQtt.Client;

using System;
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
    public event EventHandler<BeforeConnectEventArgs> BeforeConnect = new((client, e) => { });

    protected virtual void BeforeConnectEventLauncher(HiveMQClientOptions options)
    {
        var eventArgs = new BeforeConnectEventArgs(options);
        Logger.Trace("BeforeConnectEventLauncher");
        this.BeforeConnect?.Invoke(this, eventArgs);
    }

    /// <summary>
    /// Event that is fired after the client connects to the broker.
    /// </summary>
    public event EventHandler<AfterConnectEventArgs> AfterConnect = new((client, e) => { });

    protected virtual void AfterConnectEventLauncher(ConnectResult results)
    {
        var eventArgs = new AfterConnectEventArgs(results);
        Logger.Trace("AfterConnectEventLauncher");
        this.AfterConnect?.Invoke(this, eventArgs);
    }

    /// <summary>
    /// Event that is fired before the client disconnects from the broker.
    /// </summary>
    public event EventHandler<BeforeDisconnectEventArgs> BeforeDisconnect = new((client, e) => { });

    protected virtual void BeforeDisconnectEventLauncher()
    {
        var eventArgs = new BeforeDisconnectEventArgs();
        Logger.Trace("BeforeDisconnectEventLauncher");
        this.BeforeDisconnect?.Invoke(this, eventArgs);
    }

    /// <summary>
    /// Event that is fired after the client is disconnected from the broker.
    /// </summary>
    public event EventHandler<AfterDisconnectEventArgs> AfterDisconnect = new((client, e) => { });

    protected virtual void AfterDisconnectEventLauncher(bool clean = false)
    {
        var eventArgs = new AfterDisconnectEventArgs(clean);
        Logger.Trace("AfterDisconnectEventLauncher");
        this.AfterDisconnect?.Invoke(this, eventArgs);
    }

    /// <summary>
    /// Event that is fired before the client sends a subscribe request.
    /// </summary>
    public event EventHandler<BeforeSubscribeEventArgs> BeforeSubscribe = new((client, e) => { });

    protected virtual void BeforeSubscribeEventLauncher(SubscribeOptions options)
    {
        var eventArgs = new BeforeSubscribeEventArgs(options);
        Logger.Trace("BeforeSubscribeEventLauncher");
        this.BeforeSubscribe?.Invoke(this, eventArgs);
    }

    /// <summary>
    /// Event that is fired after the client sends a subscribe request.
    /// </summary>
    public event EventHandler<AfterSubscribeEventArgs> AfterSubscribe = new((client, e) => { });

    protected virtual void AfterSubscribeEventLauncher(SubscribeResult results)
    {
        var eventArgs = new AfterSubscribeEventArgs(results);
        Logger.Trace("AfterSubscribeEventLauncher");
        this.AfterSubscribe?.Invoke(this, eventArgs);
    }

    /// <summary>
    /// Event that is fired before the client sends a subscribe request.
    /// </summary>
    public event EventHandler<BeforeUnsubscribeEventArgs> BeforeUnsubscribe = new((client, e) => { });

    protected virtual void BeforeUnsubscribeEventLauncher(List<Subscription> subscriptions)
    {
        var eventArgs = new BeforeUnsubscribeEventArgs(subscriptions);
        Logger.Trace("BeforeUnsubscribeEventLauncher");
        this.BeforeUnsubscribe?.Invoke(this, eventArgs);
    }

    /// <summary>
    /// Event that is fired after the client sends a subscribe request.
    /// </summary>
    public event EventHandler<AfterUnsubscribeEventArgs> AfterUnsubscribe = new((client, e) => { });

    protected virtual void AfterUnsubscribeEventLauncher(UnsubscribeResult results)
    {
        var eventArgs = new AfterUnsubscribeEventArgs(results);
        Logger.Trace("AfterUnsubscribeEventLauncher");
        this.AfterUnsubscribe?.Invoke(this, eventArgs);
    }

    /// <summary>
    /// Event that is fired when a message is received from the broker.
    /// </summary>
    public event EventHandler<OnMessageReceivedEventArgs> OnMessageReceived = new((client, e) => { });

    protected virtual void OnMessageReceivedEventLauncher(PublishPacket packet)
    {
        var eventArgs = new OnMessageReceivedEventArgs(packet.Message);
        Logger.Trace("OnMessageReceivedEventLauncher");
        this.OnMessageReceived?.Invoke(this, eventArgs);
    }

    /* ========================================================================================= */
    /* Packet Level Events                                                                       */
    /* ========================================================================================= */

    /// <summary>
    /// Event that is fired after the client sends a CONNECT packet to the broker.
    /// </summary>
    public event EventHandler<OnConnectSentEventArgs> OnConnectSent = new((client, e) => { });

    protected virtual void OnConnectSentEventLauncher(ConnectPacket packet)
    {
        var eventArgs = new OnConnectSentEventArgs(packet);
        Logger.Trace("OnConnectSentEventLauncher");
        this.OnConnectSent?.Invoke(this, eventArgs);
    }

    /// <summary>
    /// Event that is fired after the client receives a CONNACK packet from the broker.
    /// </summary>
    public event EventHandler<OnConnAckReceivedEventArgs> OnConnAckReceived = new((client, e) => { });

    protected virtual void OnConnAckReceivedEventLauncher(ConnAckPacket packet)
    {
        var eventArgs = new OnConnAckReceivedEventArgs(packet);
        Logger.Trace("OnConnAckReceivedEventLauncher");
        this.OnConnAckReceived?.Invoke(this, eventArgs);
    }

    /// <summary>
    /// Event that is fired after the client send a Disconnect packet from the broker.
    /// </summary>
    public event EventHandler<OnDisconnectSentEventArgs> OnDisconnectSent = new((client, e) => { });

    protected virtual void OnDisconnectSentEventLauncher(DisconnectPacket packet)
    {
        var eventArgs = new OnDisconnectSentEventArgs(packet);
        Logger.Trace("OnDisconnectSentEventLauncher");
        this.OnDisconnectSent?.Invoke(this, eventArgs);
    }

    /// <summary>
    /// Event that is fired after the client receives a Disconnect packet from the broker.
    /// </summary>
    public event EventHandler<OnDisconnectReceivedEventArgs> OnDisconnectReceived = new((client, e) => { });

    protected virtual void OnDisconnectReceivedEventLauncher(DisconnectPacket packet)
    {
        var eventArgs = new OnDisconnectReceivedEventArgs(packet);
        Logger.Trace("OnDisconnectReceivedEventLauncher: ReasonCode: " + packet.DisconnectReasonCode + " ReasonString: " + packet.Properties.ReasonString);
        this.OnDisconnectReceived?.Invoke(this, eventArgs);
    }

    /// <summary>
    /// Event that is fired after the client send a PingReq packet from the broker.
    /// </summary>
    public event EventHandler<OnPingReqSentEventArgs> OnPingReqSent = new((client, e) => { });

    protected virtual void OnPingReqSentEventLauncher(PingReqPacket packet)
    {
        var eventArgs = new OnPingReqSentEventArgs(packet);
        Logger.Trace("OnPingReqSentEventLauncher");
        this.OnPingReqSent?.Invoke(this, eventArgs);
    }

    /// <summary>
    /// Event that is fired after the client send a PingReq packet from the broker.
    /// </summary>
    public event EventHandler<OnPingRespReceivedEventArgs> OnPingRespReceived = new((client, e) => { });

    protected virtual void OnPingRespReceivedEventLauncher(PingRespPacket packet)
    {
        var eventArgs = new OnPingRespReceivedEventArgs(packet);
        Logger.Trace("OnPingRespReceivedEventLauncher");
        this.OnPingRespReceived?.Invoke(this, eventArgs);
    }

    /// <summary>
    /// Event that is fired after the client sends a Subscribe packet to the broker.
    /// </summary>
    public event EventHandler<OnSubscribeSentEventArgs> OnSubscribeSent = new((client, e) => { });

    protected virtual void OnSubscribeSentEventLauncher(SubscribePacket packet)
    {
        var eventArgs = new OnSubscribeSentEventArgs(packet);
        Logger.Trace("OnSubscribeSentEventLauncher");
        this.OnSubscribeSent?.Invoke(this, eventArgs);
    }

    /// <summary>
    /// Event that is fired after the client receives a SubAck packet from the broker.
    /// </summary>
    public event EventHandler<OnSubAckReceivedEventArgs> OnSubAckReceived = new((client, e) => { });

    protected virtual void OnSubAckReceivedEventLauncher(SubAckPacket packet)
    {
        var eventArgs = new OnSubAckReceivedEventArgs(packet);
        Logger.Trace("OnSubAckReceivedEventLauncher");
        this.OnSubAckReceived?.Invoke(this, eventArgs);
    }

    /// <summary>
    /// Event that is fired after the client sends a Unsubscribe packet to the broker.
    /// </summary>
    public event EventHandler<OnUnsubscribeSentEventArgs> OnUnsubscribeSent = new((client, e) => { });

    protected virtual void OnUnsubscribeSentEventLauncher(UnsubscribePacket packet)
    {
        var eventArgs = new OnUnsubscribeSentEventArgs(packet);
        Logger.Trace("OnUnsubscribeSentEventLauncher");
        this.OnUnsubscribeSent?.Invoke(this, eventArgs);
    }

    /// <summary>
    /// Event that is fired after the client receives a UnsubAck packet from the broker.
    /// </summary>
    public event EventHandler<OnUnsubAckReceivedEventArgs> OnUnsubAckReceived = new((client, e) => { });

    protected virtual void OnUnsubAckReceivedEventLauncher(UnsubAckPacket packet)
    {
        var eventArgs = new OnUnsubAckReceivedEventArgs(packet);
        Logger.Trace("OnUnsubAckReceivedEventLauncher");
        this.OnUnsubAckReceived?.Invoke(this, eventArgs);
    }

    /// <summary>
    /// Event that is fired after the client receives a Publish packet from the broker.
    /// </summary>
    public event EventHandler<OnPublishReceivedEventArgs> OnPublishReceived = new((client, e) => { });

    protected virtual void OnPublishReceivedEventLauncher(PublishPacket packet)
    {
        var eventArgs = new OnPublishReceivedEventArgs(packet);
        Logger.Trace("OnPublishReceivedEventLauncher");
        this.OnPublishReceived?.Invoke(this, eventArgs);
    }

    /// <summary>
    /// Event that is fired after the client sends a Publish packet to the broker.
    /// </summary>
    public event EventHandler<OnPublishSentEventArgs> OnPublishSent = new((client, e) => { });

    protected virtual void OnPublishSentEventLauncher(PublishPacket packet)
    {
        var eventArgs = new OnPublishSentEventArgs(packet);
        Logger.Trace("OnPublishSentEventLauncher");
        this.OnPublishSent?.Invoke(this, eventArgs);
    }

    /// <summary>
    /// Event that is fired after the client receives a PubAck packet from the broker.
    /// </summary>
    public event EventHandler<OnPubAckReceivedEventArgs> OnPubAckReceived = new((client, e) => { });

    protected virtual void OnPubAckReceivedEventLauncher(PubAckPacket packet)
    {
        var eventArgs = new OnPubAckReceivedEventArgs(packet);
        Logger.Trace("OnPubAckReceivedEventLauncher");
        this.OnPubAckReceived?.Invoke(this, eventArgs);
    }

    /// <summary>
    /// Event that is fired after the client sends a PubAck packet from the broker.
    /// </summary>
    public event EventHandler<OnPubAckSentEventArgs> OnPubAckSent = new((client, e) => { });

    protected virtual void OnPubAckSentEventLauncher(PubAckPacket packet)
    {
        var eventArgs = new OnPubAckSentEventArgs(packet);
        Logger.Trace("OnPubAckSentEventLauncher");
        this.OnPubAckSent?.Invoke(this, eventArgs);
    }

    /// <summary>
    /// Event that is fired after the client receives a PubRec packet from the broker.
    /// </summary>
    public event EventHandler<OnPubRecReceivedEventArgs> OnPubRecReceived = new((client, e) => { });

    protected virtual void OnPubRecReceivedEventLauncher(PubRecPacket packet)
    {
        var eventArgs = new OnPubRecReceivedEventArgs(packet);
        Logger.Trace("OnPubRecReceivedEventLauncher");
        this.OnPubRecReceived?.Invoke(this, eventArgs);
    }

    /// <summary>
    /// Event that is fired after the client sends a PubRec packet to the broker.
    /// </summary>
    public event EventHandler<OnPubRecSentEventArgs> OnPubRecSent = new((client, e) => { });

    protected virtual void OnPubRecSentEventLauncher(PubRecPacket packet)
    {
        var eventArgs = new OnPubRecSentEventArgs(packet);
        Logger.Trace("OnPubRecSentEventLauncher");
        this.OnPubRecSent?.Invoke(this, eventArgs);
    }

    /// <summary>
    /// Event that is fired after the client received a PubRel packet from the broker.
    /// </summary>
    public event EventHandler<OnPubRelReceivedEventArgs> OnPubRelReceived = new((client, e) => { });

    protected virtual void OnPubRelReceivedEventLauncher(PubRelPacket packet)
    {
        var eventArgs = new OnPubRelReceivedEventArgs(packet);
        Logger.Trace("OnPubRelReceivedEventLauncher");
        this.OnPubRelReceived?.Invoke(this, eventArgs);
    }

    /// <summary>
    /// Event that is fired after the client sent a PubRel packet to the broker.
    /// </summary>
    public event EventHandler<OnPubRelSentEventArgs> OnPubRelSent = new((client, e) => { });

    protected virtual void OnPubRelSentEventLauncher(PubRelPacket packet)
    {
        var eventArgs = new OnPubRelSentEventArgs(packet);
        Logger.Trace("OnPubRelSentEventLauncher");
        this.OnPubRelSent?.Invoke(this, eventArgs);
    }

    /// <summary>
    /// Event that is fired after the client receives a PubComp packet from the broker.
    /// </summary>
    public event EventHandler<OnPubCompReceivedEventArgs> OnPubCompReceived = new((client, e) => { });

    protected virtual void OnPubCompReceivedEventLauncher(PubCompPacket packet)
    {
        var eventArgs = new OnPubCompReceivedEventArgs(packet);
        Logger.Trace("PubCompReceivedEventLauncher");
        this.OnPubCompReceived?.Invoke(this, eventArgs);
    }

    /// <summary>
    /// Event that is fired after the client sends a PubComp packet to the broker.
    /// </summary>
    public event EventHandler<OnPubCompSentEventArgs> OnPubCompSent = new((client, e) => { });

    protected virtual void OnPubCompSentEventLauncher(PubCompPacket packet)
    {
        var eventArgs = new OnPubCompSentEventArgs(packet);
        Logger.Trace("PubCompSentEventLauncher");
        this.OnPubCompSent?.Invoke(this, eventArgs);
    }
}
