/*
 * Copyright 2025-present HiveMQ and the HiveMQ Community
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
using System.Threading.Tasks;
using HiveMQtt.Client.Events;
using HiveMQtt.Client.Internal;
using HiveMQtt.Client.Options;
using HiveMQtt.Client.Results;
using HiveMQtt.MQTT5.Packets;
using HiveMQtt.MQTT5.Types;

/// <inheritdoc />
public partial class RawClient : IDisposable, IRawClient, IBaseMQTTClient
{
    /* ========================================================================================= */
    // MQTT Client Events
    /* ========================================================================================= */

    /// <summary>
    /// Event that is fired before the client connects to the broker.
    /// </summary>
    public event EventHandler<BeforeConnectEventArgs>? BeforeConnect;

    internal virtual void BeforeConnectEventLauncher(HiveMQClientOptions options)
    {
        if (this.BeforeConnect == null)
        {
            return;
        }

        Logger.Trace("BeforeConnectEventLauncher");
        var eventArgs = new BeforeConnectEventArgs(options);
        var handlers = this.BeforeConnect.GetInvocationList();

        foreach (var handler in handlers)
        {
            _ = Task.Run(() => ((EventHandler<BeforeConnectEventArgs>)handler)(this, eventArgs)).ContinueWith(
                t =>
                {
                    if (t.IsFaulted)
                    {
                        Logger.Error($"BeforeConnect Handler exception: {t.Exception?.Message}");
                    }
                }, TaskScheduler.Default);
        }
    }

    /// <summary>
    /// Event that is fired after the client connects to the broker.
    /// </summary>
    public event EventHandler<AfterConnectEventArgs>? AfterConnect;

    internal virtual void AfterConnectEventLauncher(ConnectResult results)
    {
        if (this.AfterConnect == null)
        {
            return;
        }

        Logger.Trace("AfterConnectEventLauncher");
        var eventArgs = new AfterConnectEventArgs(results);
        var handlers = this.AfterConnect.GetInvocationList();

        foreach (var handler in handlers)
        {
            _ = Task.Run(() => ((EventHandler<AfterConnectEventArgs>)handler)(this, eventArgs)).ContinueWith(
                t =>
                {
                    if (t.IsFaulted)
                    {
                        Logger.Error($"AfterConnect Handler exception: {t.Exception?.Message}");
                    }
                }, TaskScheduler.Default);
        }
    }

    /// <summary>
    /// Event that is fired before the client disconnects from the broker.
    /// </summary>
    public event EventHandler<BeforeDisconnectEventArgs>? BeforeDisconnect;

    internal virtual void BeforeDisconnectEventLauncher()
    {
        if (this.BeforeDisconnect == null)
        {
            return;
        }

        Logger.Trace("BeforeDisconnectEventLauncher");
        var eventArgs = new BeforeDisconnectEventArgs();
        var handlers = this.BeforeDisconnect.GetInvocationList();

        foreach (var handler in handlers)
        {
            _ = Task.Run(() => ((EventHandler<BeforeDisconnectEventArgs>)handler)(this, eventArgs)).ContinueWith(
                t =>
                {
                    if (t.IsFaulted)
                    {
                        Logger.Error($"BeforeDisconnect Handler exception: {t.Exception?.Message}");
                    }
                }, TaskScheduler.Default);
        }
    }

    /// <summary>
    /// Event that is fired after the client is disconnected from the broker.
    /// </summary>
    public event EventHandler<AfterDisconnectEventArgs>? AfterDisconnect;

    internal virtual void AfterDisconnectEventLauncher(bool clean = false)
    {
        if (this.AfterDisconnect == null)
        {
            return;
        }

        Logger.Trace("AfterDisconnectEventLauncher");
        var eventArgs = new AfterDisconnectEventArgs(clean);
        var handlers = this.AfterDisconnect.GetInvocationList();

        foreach (var handler in handlers)
        {
            _ = Task.Run(() => ((EventHandler<AfterDisconnectEventArgs>)handler)(this, eventArgs)).ContinueWith(
                t =>
                {
                    if (t.IsFaulted)
                    {
                        Logger.Error($"AfterDisconnect Handler exception: {t.Exception?.Message}");
                    }
                }, TaskScheduler.Default);
        }
    }

    /// <summary>
    /// Event that is fired before the client sends a subscribe request.
    /// </summary>
    public event EventHandler<BeforeSubscribeEventArgs>? BeforeSubscribe;

    internal virtual void BeforeSubscribeEventLauncher(SubscribeOptions options)
    {
        if (this.BeforeSubscribe == null)
        {
            return;
        }

        Logger.Trace("BeforeSubscribeEventLauncher");
        var eventArgs = new BeforeSubscribeEventArgs(options);
        var handlers = this.BeforeSubscribe.GetInvocationList();

        foreach (var handler in handlers)
        {
            _ = Task.Run(() => ((EventHandler<BeforeSubscribeEventArgs>)handler)(this, eventArgs)).ContinueWith(
                t =>
                {
                    if (t.IsFaulted)
                    {
                        Logger.Error($"BeforeSubscribe Handler exception: {t.Exception?.Message}");
                    }
                }, TaskScheduler.Default);
        }
    }

    /// <summary>
    /// Event that is fired after the client sends a subscribe request.
    /// </summary>
    public event EventHandler<AfterSubscribeEventArgs>? AfterSubscribe;

    internal virtual void AfterSubscribeEventLauncher(SubscribeResult results)
    {
        if (this.AfterSubscribe == null)
        {
            return;
        }

        Logger.Trace("AfterSubscribeEventLauncher");
        var eventArgs = new AfterSubscribeEventArgs(results);
        var handlers = this.AfterSubscribe.GetInvocationList();

        foreach (var handler in handlers)
        {
            _ = Task.Run(() => ((EventHandler<AfterSubscribeEventArgs>)handler)(this, eventArgs)).ContinueWith(
                t =>
                {
                    if (t.IsFaulted)
                    {
                        Logger.Error($"AfterSubscribe Handler exception: {t.Exception?.Message}");
                    }
                }, TaskScheduler.Default);
        }
    }

    /// <summary>
    /// Event that is fired before the client sends an unsubscribe request.
    /// </summary>
    public event EventHandler<BeforeUnsubscribeEventArgs>? BeforeUnsubscribe;

    internal virtual void BeforeUnsubscribeEventLauncher(List<Subscription> subscriptions)
    {
        if (this.BeforeUnsubscribe == null)
        {
            return;
        }

        Logger.Trace("BeforeUnsubscribeEventLauncher");
        var eventArgs = new BeforeUnsubscribeEventArgs(subscriptions);
        var handlers = this.BeforeUnsubscribe.GetInvocationList();

        foreach (var handler in handlers)
        {
            _ = Task.Run(() => ((EventHandler<BeforeUnsubscribeEventArgs>)handler)(this, eventArgs)).ContinueWith(
                t =>
                {
                    if (t.IsFaulted)
                    {
                        Logger.Error($"BeforeUnsubscribe Handler exception: {t.Exception?.Message}");
                    }
                }, TaskScheduler.Default);
        }
    }

    /// <summary>
    /// Event that is fired after the client sends an unsubscribe request.
    /// </summary>
    public event EventHandler<AfterUnsubscribeEventArgs>? AfterUnsubscribe;

    internal virtual void AfterUnsubscribeEventLauncher(UnsubscribeResult results)
    {
        if (this.AfterUnsubscribe == null)
        {
            return;
        }

        Logger.Trace("AfterUnsubscribeEventLauncher");
        var eventArgs = new AfterUnsubscribeEventArgs(results);
        var handlers = this.AfterUnsubscribe.GetInvocationList();

        foreach (var handler in handlers)
        {
            _ = Task.Run(() => ((EventHandler<AfterUnsubscribeEventArgs>)handler)(this, eventArgs)).ContinueWith(
                t =>
                {
                    if (t.IsFaulted)
                    {
                        Logger.Error($"AfterUnsubscribe Handler exception: {t.Exception?.Message}");
                    }
                }, TaskScheduler.Default);
        }
    }

    /// <summary>
    /// Event that is fired when a message is received from the broker.
    /// <para>
    /// Note: RawClient does not perform subscription matching. This event fires for all received PUBLISH packets.
    /// </para>
    /// </summary>
    public event EventHandler<OnMessageReceivedEventArgs>? OnMessageReceived;

    internal virtual void OnMessageReceivedEventLauncher(PublishPacket packet)
    {
        // RawClient does not maintain subscription state, so we simply fire the event if handlers are registered
        if (this.OnMessageReceived != null)
        {
            Logger.Trace("OnMessageReceivedEventLauncher");
            var eventArgs = new OnMessageReceivedEventArgs(
                packet.Message,
                packet.Message.QoS == QualityOfService.AtMostOnceDelivery ? null : packet.PacketIdentifier);
            var handlers = this.OnMessageReceived.GetInvocationList();
            foreach (var handler in handlers)
            {
                _ = Task.Run(() => ((EventHandler<OnMessageReceivedEventArgs>)handler)(this, eventArgs)).ContinueWith(
                    t =>
                    {
                        if (t.IsFaulted)
                        {
                            Logger.Error($"OnMessageReceived Handler exception: {t.Exception?.Message}");
                        }
                    }, TaskScheduler.Default);
            }
        }
    }

    /* ========================================================================================= */
    /* Packet Level Events                                                                       */
    /* ========================================================================================= */

    /// <summary>
    /// Event that is fired after the client sends a CONNECT packet to the broker.
    /// </summary>
    public event EventHandler<OnConnectSentEventArgs>? OnConnectSent;

    internal virtual void OnConnectSentEventLauncher(ConnectPacket packet)
    {
        if (this.OnConnectSent == null)
        {
            return;
        }

        Logger.Trace("OnConnectSentEventLauncher");
        var eventArgs = new OnConnectSentEventArgs(packet);
        var handlers = this.OnConnectSent.GetInvocationList();

        foreach (var handler in handlers)
        {
            _ = Task.Run(() => ((EventHandler<OnConnectSentEventArgs>)handler)(this, eventArgs)).ContinueWith(
                t =>
                {
                    if (t.IsFaulted)
                    {
                        Logger.Error($"OnConnectSent Handler exception: {t.Exception?.Message}");
                    }
                }, TaskScheduler.Default);
        }
    }

    /// <summary>
    /// Event that is fired after the client receives a CONNACK packet from the broker.
    /// </summary>
    public event EventHandler<OnConnAckReceivedEventArgs>? OnConnAckReceived;

    internal virtual void OnConnAckReceivedEventLauncher(ConnAckPacket packet)
    {
        if (this.OnConnAckReceived == null)
        {
            return;
        }

        Logger.Trace("OnConnAckReceivedEventLauncher");
        var eventArgs = new OnConnAckReceivedEventArgs(packet);
        var handlers = this.OnConnAckReceived.GetInvocationList();

        foreach (var handler in handlers)
        {
            _ = Task.Run(() => ((EventHandler<OnConnAckReceivedEventArgs>)handler)(this, eventArgs)).ContinueWith(
                t =>
                {
                    if (t.IsFaulted)
                    {
                        Logger.Error($"OnConnAckReceived Handler exception: {t.Exception?.Message}");
                    }
                }, TaskScheduler.Default);
        }
    }

    /// <summary>
    /// Event that is fired after the client send a Disconnect packet from the broker.
    /// </summary>
    public event EventHandler<OnDisconnectSentEventArgs>? OnDisconnectSent;

    internal virtual void OnDisconnectSentEventLauncher(DisconnectPacket packet)
    {
        if (this.OnDisconnectSent == null)
        {
            return;
        }

        Logger.Trace("OnDisconnectSentEventLauncher");
        var eventArgs = new OnDisconnectSentEventArgs(packet);
        var handlers = this.OnDisconnectSent.GetInvocationList();

        foreach (var handler in handlers)
        {
            _ = Task.Run(() => ((EventHandler<OnDisconnectSentEventArgs>)handler)(this, eventArgs)).ContinueWith(
                t =>
                {
                    if (t.IsFaulted)
                    {
                        Logger.Error($"OnDisconnectSent Handler exception: {t.Exception?.Message}");
                    }
                }, TaskScheduler.Default);
        }
    }

    /// <summary>
    /// Event that is fired after the client receives a Disconnect packet from the broker.
    /// </summary>
    public event EventHandler<OnDisconnectReceivedEventArgs>? OnDisconnectReceived;

    internal virtual void OnDisconnectReceivedEventLauncher(DisconnectPacket packet)
    {
        if (this.OnDisconnectReceived == null)
        {
            return;
        }

        Logger.Trace("OnDisconnectReceivedEventLauncher: ReasonCode: " + packet.DisconnectReasonCode + " ReasonString: " + packet.Properties.ReasonString);
        var eventArgs = new OnDisconnectReceivedEventArgs(packet);
        var handlers = this.OnDisconnectReceived.GetInvocationList();

        foreach (var handler in handlers)
        {
            _ = Task.Run(() => ((EventHandler<OnDisconnectReceivedEventArgs>)handler)(this, eventArgs)).ContinueWith(
                t =>
                {
                    if (t.IsFaulted)
                    {
                        Logger.Error($"OnDisconnectReceived Handler exception: {t.Exception?.Message}");
                    }
                }, TaskScheduler.Default);
        }
    }

    /// <summary>
    /// Event that is fired after the client send a PingReq packet from the broker.
    /// </summary>
    public event EventHandler<OnPingReqSentEventArgs>? OnPingReqSent;

    internal virtual void OnPingReqSentEventLauncher(PingReqPacket packet)
    {
        if (this.OnPingReqSent == null)
        {
            return;
        }

        Logger.Trace("OnPingReqSentEventLauncher");
        var eventArgs = new OnPingReqSentEventArgs(packet);
        var handlers = this.OnPingReqSent.GetInvocationList();

        foreach (var handler in handlers)
        {
            _ = Task.Run(() => ((EventHandler<OnPingReqSentEventArgs>)handler)(this, eventArgs)).ContinueWith(
                t =>
                {
                    if (t.IsFaulted)
                    {
                        Logger.Error($"OnPingReqSent Handler exception: {t.Exception?.Message}");
                    }
                }, TaskScheduler.Default);
        }
    }

    /// <summary>
    /// Event that is fired after the client send a PingReq packet from the broker.
    /// </summary>
    public event EventHandler<OnPingRespReceivedEventArgs>? OnPingRespReceived;

    internal virtual void OnPingRespReceivedEventLauncher(PingRespPacket packet)
    {
        if (this.OnPingRespReceived == null)
        {
            return;
        }

        Logger.Trace("OnPingRespReceivedEventLauncher");
        var eventArgs = new OnPingRespReceivedEventArgs(packet);
        var handlers = this.OnPingRespReceived.GetInvocationList();

        foreach (var handler in handlers)
        {
            _ = Task.Run(() => ((EventHandler<OnPingRespReceivedEventArgs>)handler)(this, eventArgs)).ContinueWith(
                t =>
                {
                    if (t.IsFaulted)
                    {
                        Logger.Error($"OnPingRespReceived Handler exception: {t.Exception?.Message}");
                    }
                }, TaskScheduler.Default);
        }
    }

    /// <summary>
    /// Event that is fired after the client sends a Subscribe packet to the broker.
    /// </summary>
    public event EventHandler<OnSubscribeSentEventArgs>? OnSubscribeSent;

    internal virtual void OnSubscribeSentEventLauncher(SubscribePacket packet)
    {
        if (this.OnSubscribeSent == null)
        {
            return;
        }

        Logger.Trace("OnSubscribeSentEventLauncher");
        var eventArgs = new OnSubscribeSentEventArgs(packet);
        var handlers = this.OnSubscribeSent.GetInvocationList();

        foreach (var handler in handlers)
        {
            _ = Task.Run(() => ((EventHandler<OnSubscribeSentEventArgs>)handler)(this, eventArgs)).ContinueWith(
                t =>
                {
                    if (t.IsFaulted)
                    {
                        Logger.Error($"OnSubscribeSent Handler exception: {t.Exception?.Message}");
                    }
                }, TaskScheduler.Default);
        }
    }

    /// <summary>
    /// Event that is fired after the client receives a SubAck packet from the broker.
    /// </summary>
    public event EventHandler<OnSubAckReceivedEventArgs>? OnSubAckReceived;

    internal virtual void OnSubAckReceivedEventLauncher(SubAckPacket packet)
    {
        if (this.OnSubAckReceived == null)
        {
            return;
        }

        Logger.Trace("OnSubAckReceivedEventLauncher");
        var eventArgs = new OnSubAckReceivedEventArgs(packet);
        var handlers = this.OnSubAckReceived.GetInvocationList();

        foreach (var handler in handlers)
        {
            _ = Task.Run(() => ((EventHandler<OnSubAckReceivedEventArgs>)handler)(this, eventArgs)).ContinueWith(
                t =>
                {
                    if (t.IsFaulted)
                    {
                        Logger.Error($"OnSubAckReceived Handler exception: {t.Exception?.Message}");
                    }
                }, TaskScheduler.Default);
        }
    }

    /// <summary>
    /// Event that is fired after the client sends a Unsubscribe packet to the broker.
    /// </summary>
    public event EventHandler<OnUnsubscribeSentEventArgs>? OnUnsubscribeSent;

    internal virtual void OnUnsubscribeSentEventLauncher(UnsubscribePacket packet)
    {
        if (this.OnUnsubscribeSent == null)
        {
            return;
        }

        Logger.Trace("OnUnsubscribeSentEventLauncher");
        var eventArgs = new OnUnsubscribeSentEventArgs(packet);
        var handlers = this.OnUnsubscribeSent.GetInvocationList();

        foreach (var handler in handlers)
        {
            _ = Task.Run(() => ((EventHandler<OnUnsubscribeSentEventArgs>)handler)(this, eventArgs)).ContinueWith(
                t =>
                {
                    if (t.IsFaulted)
                    {
                        Logger.Error($"OnUnsubscribeSent Handler exception: {t.Exception?.Message}");
                    }
                }, TaskScheduler.Default);
        }
    }

    /// <summary>
    /// Event that is fired after the client receives a UnsubAck packet from the broker.
    /// </summary>
    public event EventHandler<OnUnsubAckReceivedEventArgs>? OnUnsubAckReceived;

    internal virtual void OnUnsubAckReceivedEventLauncher(UnsubAckPacket packet)
    {
        if (this.OnUnsubAckReceived == null)
        {
            return;
        }

        Logger.Trace("OnUnsubAckReceivedEventLauncher");
        var eventArgs = new OnUnsubAckReceivedEventArgs(packet);
        var handlers = this.OnUnsubAckReceived.GetInvocationList();

        foreach (var handler in handlers)
        {
            _ = Task.Run(() => ((EventHandler<OnUnsubAckReceivedEventArgs>)handler)(this, eventArgs)).ContinueWith(
                t =>
                {
                    if (t.IsFaulted)
                    {
                        Logger.Error($"OnUnsubAckReceived Handler exception: {t.Exception?.Message}");
                    }
                }, TaskScheduler.Default);
        }
    }

    /// <summary>
    /// Event that is fired after the client receives a Publish packet from the broker.
    /// </summary>
    public event EventHandler<OnPublishReceivedEventArgs>? OnPublishReceived;

    internal virtual void OnPublishReceivedEventLauncher(PublishPacket packet)
    {
        if (this.OnPublishReceived == null)
        {
            return;
        }

        Logger.Trace("OnPublishReceivedEventLauncher");
        var eventArgs = new OnPublishReceivedEventArgs(packet);
        var handlers = this.OnPublishReceived.GetInvocationList();

        foreach (var handler in handlers)
        {
            _ = Task.Run(() => ((EventHandler<OnPublishReceivedEventArgs>)handler)(this, eventArgs)).ContinueWith(
                t =>
                {
                    if (t.IsFaulted)
                    {
                        Logger.Error($"OnPublishReceived Handler exception: {t.Exception?.Message}");
                    }
                }, TaskScheduler.Default);
        }
    }

    /// <summary>
    /// Event that is fired after the client sends a Publish packet to the broker.
    /// </summary>
    public event EventHandler<OnPublishSentEventArgs>? OnPublishSent;

    internal virtual void OnPublishSentEventLauncher(PublishPacket packet)
    {
        if (this.OnPublishSent == null)
        {
            return;
        }

        Logger.Trace("OnPublishSentEventLauncher");
        var eventArgs = new OnPublishSentEventArgs(packet);
        var handlers = this.OnPublishSent.GetInvocationList();

        foreach (var handler in handlers)
        {
            _ = Task.Run(() => ((EventHandler<OnPublishSentEventArgs>)handler)(this, eventArgs)).ContinueWith(
                t =>
                {
                    if (t.IsFaulted)
                    {
                        Logger.Error($"OnPublishSent Handler exception: {t.Exception?.Message}");
                    }
                }, TaskScheduler.Default);
        }
    }

    /// <summary>
    /// Event that is fired after the client receives a PubAck packet from the broker.
    /// </summary>
    public event EventHandler<OnPubAckReceivedEventArgs>? OnPubAckReceived;

    internal virtual void OnPubAckReceivedEventLauncher(PubAckPacket packet)
    {
        if (this.OnPubAckReceived == null)
        {
            return;
        }

        Logger.Trace("OnPubAckReceivedEventLauncher");
        var eventArgs = new OnPubAckReceivedEventArgs(packet);
        var handlers = this.OnPubAckReceived.GetInvocationList();

        foreach (var handler in handlers)
        {
            _ = Task.Run(() => ((EventHandler<OnPubAckReceivedEventArgs>)handler)(this, eventArgs)).ContinueWith(
                t =>
                {
                    if (t.IsFaulted)
                    {
                        Logger.Error($"OnPubAckReceived Handler exception: {t.Exception?.Message}");
                    }
                }, TaskScheduler.Default);
        }
    }

    /// <summary>
    /// Event that is fired after the client sends a PubAck packet from the broker.
    /// </summary>
    public event EventHandler<OnPubAckSentEventArgs>? OnPubAckSent;

    internal virtual void OnPubAckSentEventLauncher(PubAckPacket packet)
    {
        if (this.OnPubAckSent == null)
        {
            return;
        }

        Logger.Trace("OnPubAckSentEventLauncher");
        var eventArgs = new OnPubAckSentEventArgs(packet);
        var handlers = this.OnPubAckSent.GetInvocationList();

        foreach (var handler in handlers)
        {
            _ = Task.Run(() => ((EventHandler<OnPubAckSentEventArgs>)handler)(this, eventArgs)).ContinueWith(
                t =>
                {
                    if (t.IsFaulted)
                    {
                        Logger.Error($"OnPubAckSent Handler exception: {t.Exception?.Message}");
                    }
                }, TaskScheduler.Default);
        }
    }

    /// <summary>
    /// Event that is fired after the client receives a PubRec packet from the broker.
    /// </summary>
    public event EventHandler<OnPubRecReceivedEventArgs>? OnPubRecReceived;

    internal virtual void OnPubRecReceivedEventLauncher(PubRecPacket packet)
    {
        if (this.OnPubRecReceived == null)
        {
            return;
        }

        Logger.Trace("OnPubRecReceivedEventLauncher");
        var eventArgs = new OnPubRecReceivedEventArgs(packet);
        var handlers = this.OnPubRecReceived.GetInvocationList();

        foreach (var handler in handlers)
        {
            _ = Task.Run(() => ((EventHandler<OnPubRecReceivedEventArgs>)handler)(this, eventArgs)).ContinueWith(
                t =>
                {
                    if (t.IsFaulted)
                    {
                        Logger.Error($"OnPubRecReceived Handler exception: {t.Exception?.Message}");
                    }
                }, TaskScheduler.Default);
        }
    }

    /// <summary>
    /// Event that is fired after the client sends a PubRec packet to the broker.
    /// </summary>
    public event EventHandler<OnPubRecSentEventArgs>? OnPubRecSent;

    internal virtual void OnPubRecSentEventLauncher(PubRecPacket packet)
    {
        if (this.OnPubRecSent == null)
        {
            return;
        }

        Logger.Trace("OnPubRecSentEventLauncher");
        var eventArgs = new OnPubRecSentEventArgs(packet);
        var handlers = this.OnPubRecSent.GetInvocationList();

        foreach (var handler in handlers)
        {
            _ = Task.Run(() => ((EventHandler<OnPubRecSentEventArgs>)handler)(this, eventArgs)).ContinueWith(
                t =>
                {
                    if (t.IsFaulted)
                    {
                        Logger.Error($"OnPubRecSent Handler exception: {t.Exception?.Message}");
                    }
                }, TaskScheduler.Default);
        }
    }

    /// <summary>
    /// Event that is fired after the client receives a PubRel packet from the broker.
    /// </summary>
    public event EventHandler<OnPubRelReceivedEventArgs>? OnPubRelReceived;

    internal virtual void OnPubRelReceivedEventLauncher(PubRelPacket packet)
    {
        if (this.OnPubRelReceived == null)
        {
            return;
        }

        Logger.Trace("OnPubRelReceivedEventLauncher");
        var eventArgs = new OnPubRelReceivedEventArgs(packet);
        var handlers = this.OnPubRelReceived.GetInvocationList();

        foreach (var handler in handlers)
        {
            _ = Task.Run(() => ((EventHandler<OnPubRelReceivedEventArgs>)handler)(this, eventArgs)).ContinueWith(
                t =>
                {
                    if (t.IsFaulted)
                    {
                        Logger.Error($"OnPubRelReceived Handler exception: {t.Exception?.Message}");
                    }
                }, TaskScheduler.Default);
        }
    }

    /// <summary>
    /// Event that is fired after the client sent a PubRel packet to the broker.
    /// </summary>
    public event EventHandler<OnPubRelSentEventArgs>? OnPubRelSent;

    internal virtual void OnPubRelSentEventLauncher(PubRelPacket packet)
    {
        if (this.OnPubRelSent == null)
        {
            return;
        }

        Logger.Trace("OnPubRelSentEventLauncher");
        var eventArgs = new OnPubRelSentEventArgs(packet);
        var handlers = this.OnPubRelSent.GetInvocationList();

        foreach (var handler in handlers)
        {
            _ = Task.Run(() => ((EventHandler<OnPubRelSentEventArgs>)handler)(this, eventArgs)).ContinueWith(
                t =>
                {
                    if (t.IsFaulted)
                    {
                        Logger.Error($"OnPubRelSent Handler exception: {t.Exception?.Message}");
                    }
                }, TaskScheduler.Default);
        }
    }

    /// <summary>
    /// Event that is fired after the client receives a PubComp packet from the broker.
    /// </summary>
    public event EventHandler<OnPubCompReceivedEventArgs>? OnPubCompReceived;

    internal virtual void OnPubCompReceivedEventLauncher(PubCompPacket packet)
    {
        if (this.OnPubCompReceived == null)
        {
            return;
        }

        Logger.Trace("PubCompReceivedEventLauncher");
        var eventArgs = new OnPubCompReceivedEventArgs(packet);
        var handlers = this.OnPubCompReceived.GetInvocationList();

        foreach (var handler in handlers)
        {
            _ = Task.Run(() => ((EventHandler<OnPubCompReceivedEventArgs>)handler)(this, eventArgs)).ContinueWith(
                t =>
                {
                    if (t.IsFaulted)
                    {
                        Logger.Error($"OnPubCompReceived Handler exception: {t.Exception?.Message}");
                    }
                }, TaskScheduler.Default);
        }
    }

    /// <summary>
    /// Event that is fired after the client sends a PubComp packet to the broker.
    /// </summary>
    public event EventHandler<OnPubCompSentEventArgs>? OnPubCompSent;

    internal virtual void OnPubCompSentEventLauncher(PubCompPacket packet)
    {
        if (this.OnPubCompSent == null)
        {
            return;
        }

        Logger.Trace("PubCompSentEventLauncher");
        var eventArgs = new OnPubCompSentEventArgs(packet);
        var handlers = this.OnPubCompSent.GetInvocationList();

        foreach (var handler in handlers)
        {
            _ = Task.Run(() => ((EventHandler<OnPubCompSentEventArgs>)handler)(this, eventArgs)).ContinueWith(
                t =>
                {
                    if (t.IsFaulted)
                    {
                        Logger.Error($"OnPubCompSent Handler exception: {t.Exception?.Message}");
                    }
                }, TaskScheduler.Default);
        }
    }

    /* ========================================================================================= */

    // Explicit interface implementations for IBaseMQTTClient
    /* ========================================================================================= */

    void IBaseMQTTClient.OnConnAckReceivedEventLauncher(ConnAckPacket packet) => this.OnConnAckReceivedEventLauncher(packet);

    void IBaseMQTTClient.OnDisconnectReceivedEventLauncher(DisconnectPacket packet) => this.OnDisconnectReceivedEventLauncher(packet);

    void IBaseMQTTClient.OnPublishReceivedEventLauncher(PublishPacket packet) => this.OnPublishReceivedEventLauncher(packet);

    void IBaseMQTTClient.OnMessageReceivedEventLauncher(PublishPacket packet) => this.OnMessageReceivedEventLauncher(packet);

    void IBaseMQTTClient.OnPubAckReceivedEventLauncher(PubAckPacket packet) => this.OnPubAckReceivedEventLauncher(packet);

    void IBaseMQTTClient.OnPubRecReceivedEventLauncher(PubRecPacket packet) => this.OnPubRecReceivedEventLauncher(packet);

    void IBaseMQTTClient.OnPubRelReceivedEventLauncher(PubRelPacket packet) => this.OnPubRelReceivedEventLauncher(packet);

    void IBaseMQTTClient.OnPubCompReceivedEventLauncher(PubCompPacket packet) => this.OnPubCompReceivedEventLauncher(packet);

    void IBaseMQTTClient.OnSubAckReceivedEventLauncher(SubAckPacket packet) => this.OnSubAckReceivedEventLauncher(packet);

    void IBaseMQTTClient.OnUnsubAckReceivedEventLauncher(UnsubAckPacket packet) => this.OnUnsubAckReceivedEventLauncher(packet);

    void IBaseMQTTClient.OnPingRespReceivedEventLauncher(PingRespPacket packet) => this.OnPingRespReceivedEventLauncher(packet);

    void IBaseMQTTClient.OnConnectSentEventLauncher(ConnectPacket packet) => this.OnConnectSentEventLauncher(packet);

    void IBaseMQTTClient.OnDisconnectSentEventLauncher(DisconnectPacket packet) => this.OnDisconnectSentEventLauncher(packet);

    void IBaseMQTTClient.OnPublishSentEventLauncher(PublishPacket packet) => this.OnPublishSentEventLauncher(packet);

    void IBaseMQTTClient.OnPubAckSentEventLauncher(PubAckPacket packet) => this.OnPubAckSentEventLauncher(packet);

    void IBaseMQTTClient.OnPubRecSentEventLauncher(PubRecPacket packet) => this.OnPubRecSentEventLauncher(packet);

    void IBaseMQTTClient.OnPubRelSentEventLauncher(PubRelPacket packet) => this.OnPubRelSentEventLauncher(packet);

    void IBaseMQTTClient.OnPubCompSentEventLauncher(PubCompPacket packet) => this.OnPubCompSentEventLauncher(packet);

    void IBaseMQTTClient.OnSubscribeSentEventLauncher(SubscribePacket packet) => this.OnSubscribeSentEventLauncher(packet);

    void IBaseMQTTClient.OnUnsubscribeSentEventLauncher(UnsubscribePacket packet) => this.OnUnsubscribeSentEventLauncher(packet);

    void IBaseMQTTClient.OnPingReqSentEventLauncher(PingReqPacket packet) => this.OnPingReqSentEventLauncher(packet);

    void IBaseMQTTClient.AfterDisconnectEventLauncher(bool clean) => this.AfterDisconnectEventLauncher(clean);
}
