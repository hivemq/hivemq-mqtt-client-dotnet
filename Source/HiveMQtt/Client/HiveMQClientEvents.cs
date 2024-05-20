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
    public event EventHandler<BeforeConnectEventArgs>? BeforeConnect;

    protected virtual void BeforeConnectEventLauncher(HiveMQClientOptions options)
    {
        if (this.BeforeConnect != null && this.BeforeConnect.GetInvocationList().Length > 0)
        {
            Logger.Trace("BeforeConnectEventLauncher");
            var eventArgs = new BeforeConnectEventArgs(options);
            _ = Task.Run(() => this.BeforeConnect?.Invoke(this, eventArgs)).ContinueWith(
                t =>
                {
                    if (t.IsFaulted)
                    {
                        Logger.Error("BeforeConnectEventLauncher exception: " + t.Exception.Message);
                    }
                },
                TaskScheduler.Default);
        }
    }

    /// <summary>
    /// Event that is fired after the client connects to the broker.
    /// </summary>
    public event EventHandler<AfterConnectEventArgs>? AfterConnect;

    protected virtual void AfterConnectEventLauncher(ConnectResult results)
    {
        if (this.AfterConnect != null && this.AfterConnect.GetInvocationList().Length > 0)
        {
            var eventArgs = new AfterConnectEventArgs(results);
            Logger.Trace("AfterConnectEventLauncher");
            _ = Task.Run(() => this.AfterConnect?.Invoke(this, eventArgs)).ContinueWith(
                t =>
                {
                    if (t.IsFaulted)
                    {
                        Logger.Error("AfterConnectEventLauncher exception: " + t.Exception.Message);
                    }
                },
                TaskScheduler.Default);
        }
    }

    /// <summary>
    /// Event that is fired before the client disconnects from the broker.
    /// </summary>
    public event EventHandler<BeforeDisconnectEventArgs>? BeforeDisconnect;

    protected virtual void BeforeDisconnectEventLauncher()
    {
        if (this.BeforeDisconnect != null && this.BeforeDisconnect.GetInvocationList().Length > 0)
        {
            var eventArgs = new BeforeDisconnectEventArgs();
            Logger.Trace("BeforeDisconnectEventLauncher");
            _ = Task.Run(() => this.BeforeDisconnect?.Invoke(this, eventArgs)).ContinueWith(
                t =>
                {
                    if (t.IsFaulted)
                    {
                        Logger.Error("BeforeDisconnectEventLauncher exception: " + t.Exception.Message);
                    }
                },
                TaskScheduler.Default);
        }
    }

    /// <summary>
    /// Event that is fired after the client is disconnected from the broker.
    /// </summary>
    public event EventHandler<AfterDisconnectEventArgs>? AfterDisconnect;

    protected virtual void AfterDisconnectEventLauncher(bool clean = false)
    {
        if (this.AfterDisconnect != null && this.AfterDisconnect.GetInvocationList().Length > 0)
        {
            var eventArgs = new AfterDisconnectEventArgs(clean);
            Logger.Trace("AfterDisconnectEventLauncher");
            _ = Task.Run(() => this.AfterDisconnect?.Invoke(this, eventArgs)).ContinueWith(
                t =>
                {
                    if (t.IsFaulted)
                    {
                        Logger.Error("AfterDisconnectEventLauncher exception: " + t.Exception.Message);
                    }
                },
                TaskScheduler.Default);
        }
    }

    /// <summary>
    /// Event that is fired before the client sends a subscribe request.
    /// </summary>
    public event EventHandler<BeforeSubscribeEventArgs>? BeforeSubscribe;

    protected virtual void BeforeSubscribeEventLauncher(SubscribeOptions options)
    {
        if (this.BeforeSubscribe != null && this.BeforeSubscribe.GetInvocationList().Length > 0)
        {
            var eventArgs = new BeforeSubscribeEventArgs(options);
            Logger.Trace("BeforeSubscribeEventLauncher");
            _ = Task.Run(() => this.BeforeSubscribe?.Invoke(this, eventArgs)).ContinueWith(
                t =>
                {
                    if (t.IsFaulted)
                    {
                        Logger.Error("BeforeSubscribeEventLauncher exception: " + t.Exception.Message);
                    }
                },
                TaskScheduler.Default);
        }
    }

    /// <summary>
    /// Event that is fired after the client sends a subscribe request.
    /// </summary>
    public event EventHandler<AfterSubscribeEventArgs>? AfterSubscribe;

    protected virtual void AfterSubscribeEventLauncher(SubscribeResult results)
    {
        if (this.AfterSubscribe != null && this.AfterSubscribe.GetInvocationList().Length > 0)
        {
            var eventArgs = new AfterSubscribeEventArgs(results);
            Logger.Trace("AfterSubscribeEventLauncher");
            _ = Task.Run(() => this.AfterSubscribe?.Invoke(this, eventArgs)).ContinueWith(
                t =>
                {
                    if (t.IsFaulted)
                    {
                        Logger.Error("AfterSubscribeEventLauncher exception: " + t.Exception.Message);
                    }
                },
                TaskScheduler.Default);
        }
    }

    /// <summary>
    /// Event that is fired before the client sends a subscribe request.
    /// </summary>
    public event EventHandler<BeforeUnsubscribeEventArgs>? BeforeUnsubscribe;

    protected virtual void BeforeUnsubscribeEventLauncher(List<Subscription> subscriptions)
    {
        if (this.BeforeUnsubscribe != null && this.BeforeUnsubscribe.GetInvocationList().Length > 0)
        {
            var eventArgs = new BeforeUnsubscribeEventArgs(subscriptions);
            Logger.Trace("BeforeUnsubscribeEventLauncher");
            _ = Task.Run(() => this.BeforeUnsubscribe?.Invoke(this, eventArgs)).ContinueWith(
                t =>
                {
                    if (t.IsFaulted)
                    {
                        Logger.Error("BeforeUnsubscribeEventLauncher exception: " + t.Exception.Message);
                    }
                },
                TaskScheduler.Default);
        }
    }

    /// <summary>
    /// Event that is fired after the client sends a subscribe request.
    /// </summary>
    public event EventHandler<AfterUnsubscribeEventArgs>? AfterUnsubscribe;

    protected virtual void AfterUnsubscribeEventLauncher(UnsubscribeResult results)
    {
        if (this.AfterUnsubscribe != null && this.AfterUnsubscribe.GetInvocationList().Length > 0)
        {
            var eventArgs = new AfterUnsubscribeEventArgs(results);
            Logger.Trace("AfterUnsubscribeEventLauncher");
            _ = Task.Run(() => this.AfterUnsubscribe?.Invoke(this, eventArgs)).ContinueWith(
                t =>
                {
                    if (t.IsFaulted)
                    {
                        Logger.Error("AfterUnsubscribeEventLauncher exception: " + t.Exception.Message);
                    }
                },
                TaskScheduler.Default);
        }
    }

    /// <summary>
    /// Event that is fired when a message is received from the broker.
    /// </summary>
    public event EventHandler<OnMessageReceivedEventArgs>? OnMessageReceived;

    protected virtual void OnMessageReceivedEventLauncher(PublishPacket packet)
    {
        var eventArgs = new OnMessageReceivedEventArgs(packet.Message);
        var messageHandled = false;

        if (this.OnMessageReceived != null && this.OnMessageReceived.GetInvocationList().Length > 0)
        {
            Logger.Trace("OnMessageReceivedEventLauncher");

            // Global Event Handler
            _ = Task.Run(() => this.OnMessageReceived?.Invoke(this, eventArgs)).ContinueWith(
                t =>
                {
                    if (t.IsFaulted)
                    {
                        Logger.Error("OnMessageReceivedEventLauncher exception: " + t.Exception.Message);
                    }
                },
                TaskScheduler.Default);

            messageHandled = true;
        }

        // Per Subscription Event Handler
        foreach (var subscription in this.Subscriptions)
        {
            if (packet.Message.Topic != null && MatchTopic(subscription.TopicFilter.Topic, packet.Message.Topic))
            {
                if (this.OnMessageReceived != null && this.OnMessageReceived.GetInvocationList().Length > 0)
                {
                    // We have a per-subscription message handler.
                    _ = Task.Run(() => subscription.MessageReceivedHandler?.Invoke(this, eventArgs)).ContinueWith(
                        t =>
                        {
                            if (t.IsFaulted)
                            {
                                Logger.Error("per-subscription OnMessageReceivedEventLauncher exception: " + t.Exception.Message);
                            }
                        },
                        TaskScheduler.Default);

                    messageHandled = true;
                }
            }
        }

        if (!messageHandled)
        {
            // We received an application message for a subscription without a MessageReceivedHandler
            // AND there is also no global OnMessageReceived event handler.  This publish is thus lost and unhandled.
            // We warn here about the lost message, but we don't throw an exception.
            Logger.Warn($"Lost Application Message ({packet.Message.Topic}): No global or subscription message handler found.  Register an event handler (before Subscribing) to receive all messages incoming.");
        }
    }

    /* ========================================================================================= */
    /* Packet Level Events                                                                       */
    /* ========================================================================================= */

    /// <summary>
    /// Event that is fired after the client sends a CONNECT packet to the broker.
    /// </summary>
    public event EventHandler<OnConnectSentEventArgs>? OnConnectSent;

    protected virtual void OnConnectSentEventLauncher(ConnectPacket packet)
    {
        if (this.OnConnectSent != null && this.OnConnectSent.GetInvocationList().Length > 0)
        {
            var eventArgs = new OnConnectSentEventArgs(packet);
            Logger.Trace("OnConnectSentEventLauncher");
            _ = Task.Run(() => this.OnConnectSent?.Invoke(this, eventArgs)).ContinueWith(
                t =>
                {
                    if (t.IsFaulted)
                    {
                        Logger.Error("OnConnectSentEventLauncher exception: " + t.Exception.Message);
                    }
                },
                TaskScheduler.Default);
        }
    }

    /// <summary>
    /// Event that is fired after the client receives a CONNACK packet from the broker.
    /// </summary>
    public event EventHandler<OnConnAckReceivedEventArgs>? OnConnAckReceived;

    protected virtual void OnConnAckReceivedEventLauncher(ConnAckPacket packet)
    {
        if (this.OnConnAckReceived != null && this.OnConnAckReceived.GetInvocationList().Length > 0)
        {
            var eventArgs = new OnConnAckReceivedEventArgs(packet);
            Logger.Trace("OnConnAckReceivedEventLauncher");
            _ = Task.Run(() => this.OnConnAckReceived?.Invoke(this, eventArgs)).ContinueWith(
                t =>
                {
                    if (t.IsFaulted)
                    {
                        Logger.Error("OnConnAckReceivedEventLauncher exception: " + t.Exception.Message);
                    }
                },
                TaskScheduler.Default);
        }
    }

    /// <summary>
    /// Event that is fired after the client send a Disconnect packet from the broker.
    /// </summary>
    public event EventHandler<OnDisconnectSentEventArgs>? OnDisconnectSent;

    protected virtual void OnDisconnectSentEventLauncher(DisconnectPacket packet)
    {
        if (this.OnDisconnectSent != null && this.OnDisconnectSent.GetInvocationList().Length > 0)
        {
            var eventArgs = new OnDisconnectSentEventArgs(packet);
            Logger.Trace("OnDisconnectSentEventLauncher");
            _ = Task.Run(() => this.OnDisconnectSent?.Invoke(this, eventArgs)).ContinueWith(
                t =>
                {
                    if (t.IsFaulted)
                    {
                        Logger.Error("OnDisconnectSentEventLauncher exception: " + t.Exception.Message);
                    }
                },
                TaskScheduler.Default);
        }
    }

    /// <summary>
    /// Event that is fired after the client receives a Disconnect packet from the broker.
    /// </summary>
    public event EventHandler<OnDisconnectReceivedEventArgs>? OnDisconnectReceived;

    protected virtual void OnDisconnectReceivedEventLauncher(DisconnectPacket packet)
    {
        if (this.OnDisconnectReceived != null && this.OnDisconnectReceived.GetInvocationList().Length > 0)
        {
            var eventArgs = new OnDisconnectReceivedEventArgs(packet);
            Logger.Trace("OnDisconnectReceivedEventLauncher: ReasonCode: " + packet.DisconnectReasonCode + " ReasonString: " + packet.Properties.ReasonString);
            _ = Task.Run(() => this.OnDisconnectReceived?.Invoke(this, eventArgs)).ContinueWith(
                t =>
                {
                    if (t.IsFaulted)
                    {
                        Logger.Error("OnDisconnectReceivedEventLauncher exception: " + t.Exception.Message);
                    }
                },
                TaskScheduler.Default);
        }
    }

    /// <summary>
    /// Event that is fired after the client send a PingReq packet from the broker.
    /// </summary>
    public event EventHandler<OnPingReqSentEventArgs>? OnPingReqSent;

    protected virtual void OnPingReqSentEventLauncher(PingReqPacket packet)
    {
        if (this.OnPingReqSent != null && this.OnPingReqSent.GetInvocationList().Length > 0)
        {
            var eventArgs = new OnPingReqSentEventArgs(packet);
            Logger.Trace("OnPingReqSentEventLauncher");
            _ = Task.Run(() => this.OnPingReqSent?.Invoke(this, eventArgs)).ContinueWith(
                t =>
                {
                    if (t.IsFaulted)
                    {
                        Logger.Error("OnPingReqSentEventLauncher exception: " + t.Exception.Message);
                    }
                },
                TaskScheduler.Default);
        }
    }

    /// <summary>
    /// Event that is fired after the client send a PingReq packet from the broker.
    /// </summary>
    public event EventHandler<OnPingRespReceivedEventArgs>? OnPingRespReceived;

    protected virtual void OnPingRespReceivedEventLauncher(PingRespPacket packet)
    {
        if (this.OnPingRespReceived != null && this.OnPingRespReceived.GetInvocationList().Length > 0)
        {
            var eventArgs = new OnPingRespReceivedEventArgs(packet);
            Logger.Trace("OnPingRespReceivedEventLauncher");
            _ = Task.Run(() => this.OnPingRespReceived?.Invoke(this, eventArgs)).ContinueWith(
                t =>
                {
                    if (t.IsFaulted)
                    {
                        Logger.Error("OnPingRespReceivedEventLauncher exception: " + t.Exception.Message);
                    }
                },
                TaskScheduler.Default);
        }
    }

    /// <summary>
    /// Event that is fired after the client sends a Subscribe packet to the broker.
    /// </summary>
    public event EventHandler<OnSubscribeSentEventArgs>? OnSubscribeSent;

    protected virtual void OnSubscribeSentEventLauncher(SubscribePacket packet)
    {
        if (this.OnSubscribeSent != null && this.OnSubscribeSent.GetInvocationList().Length > 0)
        {
            var eventArgs = new OnSubscribeSentEventArgs(packet);
            Logger.Trace("OnSubscribeSentEventLauncher");
            _ = Task.Run(() => this.OnSubscribeSent?.Invoke(this, eventArgs)).ContinueWith(
                t =>
                {
                    if (t.IsFaulted)
                    {
                        Logger.Error("OnSubscribeSentEventLauncher exception: " + t.Exception.Message);
                    }
                },
                TaskScheduler.Default);
        }
    }

    /// <summary>
    /// Event that is fired after the client receives a SubAck packet from the broker.
    /// </summary>
    public event EventHandler<OnSubAckReceivedEventArgs>? OnSubAckReceived;

    protected virtual void OnSubAckReceivedEventLauncher(SubAckPacket packet)
    {
        if (this.OnSubAckReceived != null && this.OnSubAckReceived.GetInvocationList().Length > 0)
        {
            var eventArgs = new OnSubAckReceivedEventArgs(packet);
            Logger.Trace("OnSubAckReceivedEventLauncher");
            _ = Task.Run(() => this.OnSubAckReceived?.Invoke(this, eventArgs)).ContinueWith(
                t =>
                {
                    if (t.IsFaulted)
                    {
                        Logger.Error("OnSubAckReceivedEventLauncher exception: " + t.Exception.Message);
                    }
                },
                TaskScheduler.Default);
        }
    }

    /// <summary>
    /// Event that is fired after the client sends a Unsubscribe packet to the broker.
    /// </summary>
    public event EventHandler<OnUnsubscribeSentEventArgs>? OnUnsubscribeSent;

    protected virtual void OnUnsubscribeSentEventLauncher(UnsubscribePacket packet)
    {
        if (this.OnUnsubscribeSent != null && this.OnUnsubscribeSent.GetInvocationList().Length > 0)
        {
            var eventArgs = new OnUnsubscribeSentEventArgs(packet);
            Logger.Trace("OnUnsubscribeSentEventLauncher");
            _ = Task.Run(() => this.OnUnsubscribeSent?.Invoke(this, eventArgs)).ContinueWith(
                t =>
                {
                    if (t.IsFaulted)
                    {
                        Logger.Error("OnUnsubscribeSentEventLauncher exception: " + t.Exception.Message);
                    }
                },
                TaskScheduler.Default);
        }
    }

    /// <summary>
    /// Event that is fired after the client receives a UnsubAck packet from the broker.
    /// </summary>
    public event EventHandler<OnUnsubAckReceivedEventArgs>? OnUnsubAckReceived;

    protected virtual void OnUnsubAckReceivedEventLauncher(UnsubAckPacket packet)
    {
        if (this.OnUnsubAckReceived != null && this.OnUnsubAckReceived.GetInvocationList().Length > 0)
        {
            var eventArgs = new OnUnsubAckReceivedEventArgs(packet);
            Logger.Trace("OnUnsubAckReceivedEventLauncher");
            _ = Task.Run(() => this.OnUnsubAckReceived?.Invoke(this, eventArgs)).ContinueWith(
                t =>
                {
                    if (t.IsFaulted)
                    {
                        Logger.Error("OnUnsubAckReceivedEventLauncher exception: " + t.Exception.Message);
                    }
                },
                TaskScheduler.Default);
        }
    }

    /// <summary>
    /// Event that is fired after the client receives a Publish packet from the broker.
    /// </summary>
    public event EventHandler<OnPublishReceivedEventArgs>? OnPublishReceived;

    protected virtual void OnPublishReceivedEventLauncher(PublishPacket packet)
    {
        if (this.OnPublishReceived != null && this.OnPublishReceived.GetInvocationList().Length > 0)
        {
            var eventArgs = new OnPublishReceivedEventArgs(packet);
            Logger.Trace("OnPublishReceivedEventLauncher");
            _ = Task.Run(() => this.OnPublishReceived?.Invoke(this, eventArgs)).ContinueWith(
                t =>
                {
                    if (t.IsFaulted)
                    {
                        Logger.Error("OnPublishReceivedEventLauncher exception: " + t.Exception.Message);
                    }
                },
                TaskScheduler.Default);
        }
    }

    /// <summary>
    /// Event that is fired after the client sends a Publish packet to the broker.
    /// </summary>
    public event EventHandler<OnPublishSentEventArgs>? OnPublishSent;

    protected virtual void OnPublishSentEventLauncher(PublishPacket packet)
    {
        if (this.OnPublishSent != null && this.OnPublishSent.GetInvocationList().Length > 0)
        {
            var eventArgs = new OnPublishSentEventArgs(packet);
            Logger.Trace("OnPublishSentEventLauncher");
            _ = Task.Run(() => this.OnPublishSent?.Invoke(this, eventArgs)).ContinueWith(
                t =>
                {
                    if (t.IsFaulted)
                    {
                        Logger.Error("OnPublishSentEventLauncher exception: " + t.Exception.Message);
                    }
                },
                TaskScheduler.Default);
        }
    }

    /// <summary>
    /// Event that is fired after the client receives a PubAck packet from the broker.
    /// </summary>
    public event EventHandler<OnPubAckReceivedEventArgs>? OnPubAckReceived;

    protected virtual void OnPubAckReceivedEventLauncher(PubAckPacket packet)
    {
        if (this.OnPubAckReceived != null && this.OnPubAckReceived.GetInvocationList().Length > 0)
        {
            var eventArgs = new OnPubAckReceivedEventArgs(packet);
            Logger.Trace("OnPubAckReceivedEventLauncher");
            _ = Task.Run(() => this.OnPubAckReceived?.Invoke(this, eventArgs)).ContinueWith(
                t =>
                {
                    if (t.IsFaulted)
                    {
                        Logger.Error("OnPubAckReceivedEventLauncher exception: " + t.Exception.Message);
                    }
                },
                TaskScheduler.Default);
        }
    }

    /// <summary>
    /// Event that is fired after the client sends a PubAck packet from the broker.
    /// </summary>
    public event EventHandler<OnPubAckSentEventArgs>? OnPubAckSent;

    protected virtual void OnPubAckSentEventLauncher(PubAckPacket packet)
    {
        if (this.OnPubAckSent != null && this.OnPubAckSent.GetInvocationList().Length > 0)
        {
            var eventArgs = new OnPubAckSentEventArgs(packet);
            Logger.Trace("OnPubAckSentEventLauncher");
            _ = Task.Run(() => this.OnPubAckSent?.Invoke(this, eventArgs)).ContinueWith(
                t =>
                {
                    if (t.IsFaulted)
                    {
                        Logger.Error("OnPubAckSentEventLauncher exception: " + t.Exception.Message);
                    }
                },
                TaskScheduler.Default);
        }
    }

    /// <summary>
    /// Event that is fired after the client receives a PubRec packet from the broker.
    /// </summary>
    public event EventHandler<OnPubRecReceivedEventArgs>? OnPubRecReceived;

    protected virtual void OnPubRecReceivedEventLauncher(PubRecPacket packet)
    {
        if (this.OnPubRecReceived != null && this.OnPubRecReceived.GetInvocationList().Length > 0)
        {
            var eventArgs = new OnPubRecReceivedEventArgs(packet);
            Logger.Trace("OnPubRecReceivedEventLauncher");
            _ = Task.Run(() => this.OnPubRecReceived?.Invoke(this, eventArgs)).ContinueWith(
                t =>
                {
                    if (t.IsFaulted)
                    {
                        Logger.Error("OnPubRecReceivedEventLauncher exception: " + t.Exception.Message);
                    }
                },
                TaskScheduler.Default);
        }
    }

    /// <summary>
    /// Event that is fired after the client sends a PubRec packet to the broker.
    /// </summary>
    public event EventHandler<OnPubRecSentEventArgs>? OnPubRecSent;

    protected virtual void OnPubRecSentEventLauncher(PubRecPacket packet)
    {
        if (this.OnPubRecSent != null && this.OnPubRecSent.GetInvocationList().Length > 0)
        {
            var eventArgs = new OnPubRecSentEventArgs(packet);
            Logger.Trace("OnPubRecSentEventLauncher");
            _ = Task.Run(() => this.OnPubRecSent?.Invoke(this, eventArgs)).ContinueWith(
                t =>
                {
                    if (t.IsFaulted)
                    {
                        Logger.Error("OnPubRecSentEventLauncher exception: " + t.Exception.Message);
                    }
                },
                TaskScheduler.Default);
        }
    }

    /// <summary>
    /// Event that is fired after the client received a PubRel packet from the broker.
    /// </summary>
    public event EventHandler<OnPubRelReceivedEventArgs>? OnPubRelReceived;

    protected virtual void OnPubRelReceivedEventLauncher(PubRelPacket packet)
    {
        if (this.OnPubRelReceived != null && this.OnPubRelReceived.GetInvocationList().Length > 0)
        {
            var eventArgs = new OnPubRelReceivedEventArgs(packet);
            Logger.Trace("OnPubRelReceivedEventLauncher");
            _ = Task.Run(() => this.OnPubRelReceived?.Invoke(this, eventArgs)).ContinueWith(
                t =>
                {
                    if (t.IsFaulted)
                    {
                        Logger.Error("OnPubRelReceivedEventLauncher exception: " + t.Exception.Message);
                    }
                },
                TaskScheduler.Default);
        }
    }

    /// <summary>
    /// Event that is fired after the client sent a PubRel packet to the broker.
    /// </summary>
    public event EventHandler<OnPubRelSentEventArgs>? OnPubRelSent;

    protected virtual void OnPubRelSentEventLauncher(PubRelPacket packet)
    {
        if (this.OnPubRelSent != null && this.OnPubRelSent.GetInvocationList().Length > 0)
        {
            var eventArgs = new OnPubRelSentEventArgs(packet);
            Logger.Trace("OnPubRelSentEventLauncher");
            _ = Task.Run(() => this.OnPubRelSent?.Invoke(this, eventArgs)).ContinueWith(
                t =>
                {
                    if (t.IsFaulted)
                    {
                        Logger.Error("OnPubRelSentEventLauncher exception: " + t.Exception.Message);
                    }
                },
                TaskScheduler.Default);
        }
    }

    /// <summary>
    /// Event that is fired after the client receives a PubComp packet from the broker.
    /// </summary>
    public event EventHandler<OnPubCompReceivedEventArgs>? OnPubCompReceived;

    protected virtual void OnPubCompReceivedEventLauncher(PubCompPacket packet)
    {
        if (this.OnPubCompReceived != null && this.OnPubCompReceived.GetInvocationList().Length > 0)
        {
            var eventArgs = new OnPubCompReceivedEventArgs(packet);
            Logger.Trace("PubCompReceivedEventLauncher");
            _ = Task.Run(() => this.OnPubCompReceived?.Invoke(this, eventArgs)).ContinueWith(
                t =>
                {
                    if (t.IsFaulted)
                    {
                        Logger.Error("PubCompReceivedEventLauncher exception: " + t.Exception.Message);
                    }
                },
                TaskScheduler.Default);
        }
    }

    /// <summary>
    /// Event that is fired after the client sends a PubComp packet to the broker.
    /// </summary>
    public event EventHandler<OnPubCompSentEventArgs>? OnPubCompSent;

    protected virtual void OnPubCompSentEventLauncher(PubCompPacket packet)
    {
        if (this.OnPubCompSent != null && this.OnPubCompSent.GetInvocationList().Length > 0)
        {
            var eventArgs = new OnPubCompSentEventArgs(packet);
            Logger.Trace("PubCompSentEventLauncher");
            _ = Task.Run(() => this.OnPubCompSent?.Invoke(this, eventArgs)).ContinueWith(
                t =>
                {
                    if (t.IsFaulted)
                    {
                        Logger.Error("PubCompSentEventLauncher exception: " + t.Exception.Message);
                    }
                },
                TaskScheduler.Default);
        }
    }
}
