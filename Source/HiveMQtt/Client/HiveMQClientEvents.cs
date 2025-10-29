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

    internal virtual void BeforeConnectEventLauncher(HiveMQClientOptions options)
    {
        var eventArgs = new BeforeConnectEventArgs(options);
        var handlers = this.BeforeConnect?.GetInvocationList();
        if (handlers != null)
        {
            Logger.Trace("BeforeConnectEventLauncher");
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
    }

    /// <summary>
    /// Event that is fired after the client connects to the broker.
    /// </summary>
    public event EventHandler<AfterConnectEventArgs>? AfterConnect;

    internal virtual void AfterConnectEventLauncher(ConnectResult results)
    {
        var eventArgs = new AfterConnectEventArgs(results);
        var handlers = this.AfterConnect?.GetInvocationList();
        if (handlers != null)
        {
            Logger.Trace("AfterConnectEventLauncher");
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
    }

    /// <summary>
    /// Event that is fired before the client disconnects from the broker.
    /// </summary>
    public event EventHandler<BeforeDisconnectEventArgs>? BeforeDisconnect;

    internal virtual void BeforeDisconnectEventLauncher()
    {
        var eventArgs = new BeforeDisconnectEventArgs();
        var handlers = this.BeforeDisconnect?.GetInvocationList();
        if (handlers != null)
        {
            Logger.Trace("BeforeDisconnectEventLauncher");
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
    }

    /// <summary>
    /// Event that is fired after the client is disconnected from the broker.
    /// </summary>
    public event EventHandler<AfterDisconnectEventArgs>? AfterDisconnect;

    internal virtual void AfterDisconnectEventLauncher(bool clean = false)
    {
        var eventArgs = new AfterDisconnectEventArgs(clean);
        var handlers = this.AfterDisconnect?.GetInvocationList();
        if (handlers != null)
        {
            Logger.Trace("AfterDisconnectEventLauncher");
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
    }

    /// <summary>
    /// Event that is fired before the client sends a subscribe request.
    /// </summary>
    public event EventHandler<BeforeSubscribeEventArgs>? BeforeSubscribe;

    internal virtual void BeforeSubscribeEventLauncher(SubscribeOptions options)
    {
        var eventArgs = new BeforeSubscribeEventArgs(options);
        var handlers = this.BeforeSubscribe?.GetInvocationList();
        if (handlers != null)
        {
            Logger.Trace("BeforeSubscribeEventLauncher");
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
    }

    /// <summary>
    /// Event that is fired after the client sends a subscribe request.
    /// </summary>
    public event EventHandler<AfterSubscribeEventArgs>? AfterSubscribe;

    internal virtual void AfterSubscribeEventLauncher(SubscribeResult results)
    {
        var eventArgs = new AfterSubscribeEventArgs(results);
        var handlers = this.AfterSubscribe?.GetInvocationList();
        if (handlers != null)
        {
            Logger.Trace("AfterSubscribeEventLauncher");
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
    }

    /// <summary>
    /// Event that is fired before the client sends a subscribe request.
    /// </summary>
    public event EventHandler<BeforeUnsubscribeEventArgs>? BeforeUnsubscribe;

    internal virtual void BeforeUnsubscribeEventLauncher(List<Subscription> subscriptions)
    {
        var eventArgs = new BeforeUnsubscribeEventArgs(subscriptions);
        var handlers = this.BeforeUnsubscribe?.GetInvocationList();
        if (handlers != null)
        {
            Logger.Trace("BeforeUnsubscribeEventLauncher");
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
    }

    /// <summary>
    /// Event that is fired after the client sends a subscribe request.
    /// </summary>
    public event EventHandler<AfterUnsubscribeEventArgs>? AfterUnsubscribe;

    internal virtual void AfterUnsubscribeEventLauncher(UnsubscribeResult results)
    {
        var eventArgs = new AfterUnsubscribeEventArgs(results);
        var handlers = this.AfterUnsubscribe?.GetInvocationList();
        if (handlers != null)
        {
            Logger.Trace("AfterUnsubscribeEventLauncher");
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
    }

    /// <summary>
    /// Event that is fired when a message is received from the broker.
    /// </summary>
    public event EventHandler<OnMessageReceivedEventArgs>? OnMessageReceived;

    internal virtual void OnMessageReceivedEventLauncher(PublishPacket packet)
    {
        var eventArgs = new OnMessageReceivedEventArgs(packet.Message);
        var messageHandled = false;

        // Get all handlers
        var handlers = this.OnMessageReceived?.GetInvocationList();
        if (handlers != null)
        {
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

            messageHandled = true;
        }

        if (packet.Message.Topic is null)
        {
            return;
        }

        // Per Subscription Event Handler
        // use ToList, so the iteration goes through a copy and changes at the list make not problems
        // otherwise it would be necessary to lock the Subscriptions with the semaphore of HiveMQClient
        List<Subscription> tempList;
        try
        {
            this.SubscriptionsSemaphore.Wait();
#pragma warning disable IDE0305 // Collection initialization - ToList() is appropriate for .NET 6
            tempList = this.Subscriptions.ToList();
#pragma warning restore IDE0305
        }
        finally
        {
            _ = this.SubscriptionsSemaphore.Release();
        }

        var matchingSubscriptions = tempList.Where(sub =>
            sub.MessageReceivedHandler is not null &&
            MatchTopic(sub.TopicFilter.Topic, packet.Message.Topic));

        foreach (var subscription in matchingSubscriptions)
        {
            // We have a per-subscription message handler.
            _ = Task.Run(() =>
            {
                try
                {
                    subscription.MessageReceivedHandler?.Invoke(this, eventArgs);
                }
                catch (Exception e)
                {
                    Logger.Error(
                        $"per-subscription MessageReceivedEventLauncher faulted ({packet.Message.Topic}): {e.Message}");
                }
            });

            messageHandled = true;
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

    internal virtual void OnConnectSentEventLauncher(ConnectPacket packet)
    {
        var eventArgs = new OnConnectSentEventArgs(packet);
        var handlers = this.OnConnectSent?.GetInvocationList();
        if (handlers != null)
        {
            Logger.Trace("OnConnectSentEventLauncher");
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
    }

    /// <summary>
    /// Event that is fired after the client receives a CONNACK packet from the broker.
    /// </summary>
    public event EventHandler<OnConnAckReceivedEventArgs>? OnConnAckReceived;

    internal virtual void OnConnAckReceivedEventLauncher(ConnAckPacket packet)
    {
        var eventArgs = new OnConnAckReceivedEventArgs(packet);
        var handlers = this.OnConnAckReceived?.GetInvocationList();
        if (handlers != null)
        {
            Logger.Trace("OnConnAckReceivedEventLauncher");
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
    }

    /// <summary>
    /// Event that is fired after the client send a Disconnect packet from the broker.
    /// </summary>
    public event EventHandler<OnDisconnectSentEventArgs>? OnDisconnectSent;

    internal virtual void OnDisconnectSentEventLauncher(DisconnectPacket packet)
    {
        var eventArgs = new OnDisconnectSentEventArgs(packet);
        var handlers = this.OnDisconnectSent?.GetInvocationList();
        if (handlers != null)
        {
            Logger.Trace("OnDisconnectSentEventLauncher");
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
    }

    /// <summary>
    /// Event that is fired after the client receives a Disconnect packet from the broker.
    /// </summary>
    public event EventHandler<OnDisconnectReceivedEventArgs>? OnDisconnectReceived;

    internal virtual void OnDisconnectReceivedEventLauncher(DisconnectPacket packet)
    {
        var eventArgs = new OnDisconnectReceivedEventArgs(packet);
        var handlers = this.OnDisconnectReceived?.GetInvocationList();
        if (handlers != null)
        {
            Logger.Trace("OnDisconnectReceivedEventLauncher: ReasonCode: " + packet.DisconnectReasonCode + " ReasonString: " + packet.Properties.ReasonString);
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
    }

    /// <summary>
    /// Event that is fired after the client send a PingReq packet from the broker.
    /// </summary>
    public event EventHandler<OnPingReqSentEventArgs>? OnPingReqSent;

    internal virtual void OnPingReqSentEventLauncher(PingReqPacket packet)
    {
        var eventArgs = new OnPingReqSentEventArgs(packet);
        var handlers = this.OnPingReqSent?.GetInvocationList();
        if (handlers != null)
        {
            Logger.Trace("OnPingReqSentEventLauncher");
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
    }

    /// <summary>
    /// Event that is fired after the client send a PingReq packet from the broker.
    /// </summary>
    public event EventHandler<OnPingRespReceivedEventArgs>? OnPingRespReceived;

    internal virtual void OnPingRespReceivedEventLauncher(PingRespPacket packet)
    {
        var eventArgs = new OnPingRespReceivedEventArgs(packet);
        var handlers = this.OnPingRespReceived?.GetInvocationList();
        if (handlers != null)
        {
            Logger.Trace("OnPingRespReceivedEventLauncher");
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
    }

    /// <summary>
    /// Event that is fired after the client sends a Subscribe packet to the broker.
    /// </summary>
    public event EventHandler<OnSubscribeSentEventArgs>? OnSubscribeSent;

    internal virtual void OnSubscribeSentEventLauncher(SubscribePacket packet)
    {
        var eventArgs = new OnSubscribeSentEventArgs(packet);
        var handlers = this.OnSubscribeSent?.GetInvocationList();
        if (handlers != null)
        {
            Logger.Trace("OnSubscribeSentEventLauncher");
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
    }

    /// <summary>
    /// Event that is fired after the client receives a SubAck packet from the broker.
    /// </summary>
    public event EventHandler<OnSubAckReceivedEventArgs>? OnSubAckReceived;

    internal virtual void OnSubAckReceivedEventLauncher(SubAckPacket packet)
    {
        var eventArgs = new OnSubAckReceivedEventArgs(packet);
        var handlers = this.OnSubAckReceived?.GetInvocationList();
        if (handlers != null)
        {
            Logger.Trace("OnSubAckReceivedEventLauncher");
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
    }

    /// <summary>
    /// Event that is fired after the client sends a Unsubscribe packet to the broker.
    /// </summary>
    public event EventHandler<OnUnsubscribeSentEventArgs>? OnUnsubscribeSent;

    internal virtual void OnUnsubscribeSentEventLauncher(UnsubscribePacket packet)
    {
        var eventArgs = new OnUnsubscribeSentEventArgs(packet);
        var handlers = this.OnUnsubscribeSent?.GetInvocationList();
        if (handlers != null)
        {
            Logger.Trace("OnUnsubscribeSentEventLauncher");
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
    }

    /// <summary>
    /// Event that is fired after the client receives a UnsubAck packet from the broker.
    /// </summary>
    public event EventHandler<OnUnsubAckReceivedEventArgs>? OnUnsubAckReceived;

    internal virtual void OnUnsubAckReceivedEventLauncher(UnsubAckPacket packet)
    {
        var eventArgs = new OnUnsubAckReceivedEventArgs(packet);
        var handlers = this.OnUnsubAckReceived?.GetInvocationList();
        if (handlers != null)
        {
            Logger.Trace("OnUnsubAckReceivedEventLauncher");
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
    }

    /// <summary>
    /// Event that is fired after the client receives a Publish packet from the broker.
    /// </summary>
    public event EventHandler<OnPublishReceivedEventArgs>? OnPublishReceived;

    internal virtual void OnPublishReceivedEventLauncher(PublishPacket packet)
    {
        var eventArgs = new OnPublishReceivedEventArgs(packet);
        var handlers = this.OnPublishReceived?.GetInvocationList();
        if (handlers != null)
        {
            Logger.Trace("OnPublishReceivedEventLauncher");
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
    }

    /// <summary>
    /// Event that is fired after the client sends a Publish packet to the broker.
    /// </summary>
    public event EventHandler<OnPublishSentEventArgs>? OnPublishSent;

    internal virtual void OnPublishSentEventLauncher(PublishPacket packet)
    {
        var eventArgs = new OnPublishSentEventArgs(packet);
        var handlers = this.OnPublishSent?.GetInvocationList();
        if (handlers != null)
        {
            Logger.Trace("OnPublishSentEventLauncher");
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
    }

    /// <summary>
    /// Event that is fired after the client receives a PubAck packet from the broker.
    /// </summary>
    public event EventHandler<OnPubAckReceivedEventArgs>? OnPubAckReceived;

    internal virtual void OnPubAckReceivedEventLauncher(PubAckPacket packet)
    {
        var eventArgs = new OnPubAckReceivedEventArgs(packet);
        var handlers = this.OnPubAckReceived?.GetInvocationList();
        if (handlers != null)
        {
            Logger.Trace("OnPubAckReceivedEventLauncher");
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
    }

    /// <summary>
    /// Event that is fired after the client sends a PubAck packet from the broker.
    /// </summary>
    public event EventHandler<OnPubAckSentEventArgs>? OnPubAckSent;

    internal virtual void OnPubAckSentEventLauncher(PubAckPacket packet)
    {
        var eventArgs = new OnPubAckSentEventArgs(packet);
        var handlers = this.OnPubAckSent?.GetInvocationList();
        if (handlers != null)
        {
            Logger.Trace("OnPubAckSentEventLauncher");
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
    }

    /// <summary>
    /// Event that is fired after the client receives a PubRec packet from the broker.
    /// </summary>
    public event EventHandler<OnPubRecReceivedEventArgs>? OnPubRecReceived;

    internal virtual void OnPubRecReceivedEventLauncher(PubRecPacket packet)
    {
        var eventArgs = new OnPubRecReceivedEventArgs(packet);
        var handlers = this.OnPubRecReceived?.GetInvocationList();
        if (handlers != null)
        {
            Logger.Trace("OnPubRecReceivedEventLauncher");
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
    }

    /// <summary>
    /// Event that is fired after the client sends a PubRec packet to the broker.
    /// </summary>
    public event EventHandler<OnPubRecSentEventArgs>? OnPubRecSent;

    internal virtual void OnPubRecSentEventLauncher(PubRecPacket packet)
    {
        var eventArgs = new OnPubRecSentEventArgs(packet);
        var handlers = this.OnPubRecSent?.GetInvocationList();
        if (handlers != null)
        {
            Logger.Trace("OnPubRecSentEventLauncher");
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
    }

    /// <summary>
    /// Event that is fired after the client received a PubRel packet from the broker.
    /// </summary>
    public event EventHandler<OnPubRelReceivedEventArgs>? OnPubRelReceived;

    internal virtual void OnPubRelReceivedEventLauncher(PubRelPacket packet)
    {
        var eventArgs = new OnPubRelReceivedEventArgs(packet);
        var handlers = this.OnPubRelReceived?.GetInvocationList();
        if (handlers != null)
        {
            Logger.Trace("OnPubRelReceivedEventLauncher");
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
    }

    /// <summary>
    /// Event that is fired after the client sent a PubRel packet to the broker.
    /// </summary>
    public event EventHandler<OnPubRelSentEventArgs>? OnPubRelSent;

    internal virtual void OnPubRelSentEventLauncher(PubRelPacket packet)
    {
        var eventArgs = new OnPubRelSentEventArgs(packet);
        var handlers = this.OnPubRelSent?.GetInvocationList();
        if (handlers != null)
        {
            Logger.Trace("OnPubRelSentEventLauncher");
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
    }

    /// <summary>
    /// Event that is fired after the client receives a PubComp packet from the broker.
    /// </summary>
    public event EventHandler<OnPubCompReceivedEventArgs>? OnPubCompReceived;

    internal virtual void OnPubCompReceivedEventLauncher(PubCompPacket packet)
    {
        var eventArgs = new OnPubCompReceivedEventArgs(packet);
        var handlers = this.OnPubCompReceived?.GetInvocationList();
        if (handlers != null)
        {
            Logger.Trace("PubCompReceivedEventLauncher");
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
    }

    /// <summary>
    /// Event that is fired after the client sends a PubComp packet to the broker.
    /// </summary>
    public event EventHandler<OnPubCompSentEventArgs>? OnPubCompSent;

    internal virtual void OnPubCompSentEventLauncher(PubCompPacket packet)
    {
        var eventArgs = new OnPubCompSentEventArgs(packet);
        var handlers = this.OnPubCompSent?.GetInvocationList();
        if (handlers != null)
        {
            Logger.Trace("PubCompSentEventLauncher");
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
    }
}
