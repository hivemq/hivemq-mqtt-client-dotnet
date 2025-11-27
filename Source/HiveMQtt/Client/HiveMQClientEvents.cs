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
using Microsoft.Extensions.Logging;
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
        if (this.BeforeConnect == null)
        {
            return;
        }

        this.logger.LogTrace("BeforeConnectEventLauncher");
        var eventArgs = new BeforeConnectEventArgs(options);
        var handlers = this.BeforeConnect.GetInvocationList();

        foreach (var handler in handlers)
        {
            _ = Task.Run(() => ((EventHandler<BeforeConnectEventArgs>)handler)(this, eventArgs)).ContinueWith(
                t =>
                {
                    if (t.IsFaulted)
                    {
                        this.logger.LogError(t.Exception, "BeforeConnect Handler exception");
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

        this.logger.LogTrace("AfterConnectEventLauncher");
        var eventArgs = new AfterConnectEventArgs(results);
        var handlers = this.AfterConnect.GetInvocationList();

        foreach (var handler in handlers)
        {
            _ = Task.Run(() => ((EventHandler<AfterConnectEventArgs>)handler)(this, eventArgs)).ContinueWith(
                t =>
                {
                    if (t.IsFaulted)
                    {
                        this.logger.LogError(t.Exception, "AfterConnect Handler exception");
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

        this.logger.LogTrace("BeforeDisconnectEventLauncher");
        var eventArgs = new BeforeDisconnectEventArgs();
        var handlers = this.BeforeDisconnect.GetInvocationList();

        foreach (var handler in handlers)
        {
            _ = Task.Run(() => ((EventHandler<BeforeDisconnectEventArgs>)handler)(this, eventArgs)).ContinueWith(
                t =>
                {
                    if (t.IsFaulted)
                    {
                        this.logger.LogError(t.Exception, "BeforeDisconnect Handler exception");
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

        this.logger.LogTrace("AfterDisconnectEventLauncher");
        var eventArgs = new AfterDisconnectEventArgs(clean);
        var handlers = this.AfterDisconnect.GetInvocationList();

        foreach (var handler in handlers)
        {
            _ = Task.Run(() => ((EventHandler<AfterDisconnectEventArgs>)handler)(this, eventArgs)).ContinueWith(
                t =>
                {
                    if (t.IsFaulted)
                    {
                        this.logger.LogError(t.Exception, "AfterDisconnect Handler exception");
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

        this.logger.LogTrace("BeforeSubscribeEventLauncher");
        var eventArgs = new BeforeSubscribeEventArgs(options);
        var handlers = this.BeforeSubscribe.GetInvocationList();

        foreach (var handler in handlers)
        {
            _ = Task.Run(() => ((EventHandler<BeforeSubscribeEventArgs>)handler)(this, eventArgs)).ContinueWith(
                t =>
                {
                    if (t.IsFaulted)
                    {
                        this.logger.LogError(t.Exception, "BeforeSubscribe Handler exception");
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

        this.logger.LogTrace("AfterSubscribeEventLauncher");
        var eventArgs = new AfterSubscribeEventArgs(results);
        var handlers = this.AfterSubscribe.GetInvocationList();

        foreach (var handler in handlers)
        {
            _ = Task.Run(() => ((EventHandler<AfterSubscribeEventArgs>)handler)(this, eventArgs)).ContinueWith(
                t =>
                {
                    if (t.IsFaulted)
                    {
                        this.logger.LogError(t.Exception, "AfterSubscribe Handler exception");
                    }
                }, TaskScheduler.Default);
        }
    }

    /// <summary>
    /// Event that is fired before the client sends a subscribe request.
    /// </summary>
    public event EventHandler<BeforeUnsubscribeEventArgs>? BeforeUnsubscribe;

    internal virtual void BeforeUnsubscribeEventLauncher(List<Subscription> subscriptions)
    {
        if (this.BeforeUnsubscribe == null)
        {
            return;
        }

        this.logger.LogTrace("BeforeUnsubscribeEventLauncher");
        var eventArgs = new BeforeUnsubscribeEventArgs(subscriptions);
        var handlers = this.BeforeUnsubscribe.GetInvocationList();

        foreach (var handler in handlers)
        {
            _ = Task.Run(() => ((EventHandler<BeforeUnsubscribeEventArgs>)handler)(this, eventArgs)).ContinueWith(
                t =>
                {
                    if (t.IsFaulted)
                    {
                        this.logger.LogError(t.Exception, "BeforeUnsubscribe Handler exception");
                    }
                }, TaskScheduler.Default);
        }
    }

    /// <summary>
    /// Event that is fired after the client sends a subscribe request.
    /// </summary>
    public event EventHandler<AfterUnsubscribeEventArgs>? AfterUnsubscribe;

    internal virtual void AfterUnsubscribeEventLauncher(UnsubscribeResult results)
    {
        if (this.AfterUnsubscribe == null)
        {
            return;
        }

        this.logger.LogTrace("AfterUnsubscribeEventLauncher");
        var eventArgs = new AfterUnsubscribeEventArgs(results);
        var handlers = this.AfterUnsubscribe.GetInvocationList();

        foreach (var handler in handlers)
        {
            _ = Task.Run(() => ((EventHandler<AfterUnsubscribeEventArgs>)handler)(this, eventArgs)).ContinueWith(
                t =>
                {
                    if (t.IsFaulted)
                    {
                        this.logger.LogError(t.Exception, "AfterUnsubscribe Handler exception");
                    }
                }, TaskScheduler.Default);
        }
    }

    /// <summary>
    /// Event that is fired when a message is received from the broker.
    /// </summary>
    public event EventHandler<OnMessageReceivedEventArgs>? OnMessageReceived;

    internal virtual void OnMessageReceivedEventLauncher(PublishPacket packet)
    {
        var messageHandled = false;

        // Get all handlers - fast path if no handlers
        if (this.OnMessageReceived != null)
        {
            this.logger.LogTrace("OnMessageReceivedEventLauncher");
            var eventArgs = new OnMessageReceivedEventArgs(packet.Message);
            var handlers = this.OnMessageReceived.GetInvocationList();
            foreach (var handler in handlers)
            {
                _ = Task.Run(() => ((EventHandler<OnMessageReceivedEventArgs>)handler)(this, eventArgs)).ContinueWith(
                    t =>
                    {
                        if (t.IsFaulted)
                        {
                            this.logger.LogError(t.Exception, "OnMessageReceived Handler exception");
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

        // Create eventArgs only if we have subscription handlers (optimization: avoid allocation if not needed)
        if (matchingSubscriptions.Any())
        {
            var eventArgs = new OnMessageReceivedEventArgs(packet.Message);
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
                        this.logger.LogError(e, "per-subscription MessageReceivedEventLauncher faulted ({Topic})", packet.Message.Topic);
                    }
                });

                messageHandled = true;
            }
        }

        if (!messageHandled)
        {
            // We received an application message for a subscription without a MessageReceivedHandler
            // AND there is also no global OnMessageReceived event handler.  This publish is thus lost and unhandled.
            // We warn here about the lost message, but we don't throw an exception.
            this.logger.LogWarning("Lost Application Message ({Topic}): No global or subscription message handler found.  Register an event handler (before Subscribing) to receive all messages incoming.", packet.Message.Topic);
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

        this.logger.LogTrace("OnConnectSentEventLauncher");
        var eventArgs = new OnConnectSentEventArgs(packet);
        var handlers = this.OnConnectSent.GetInvocationList();

        foreach (var handler in handlers)
        {
            _ = Task.Run(() => ((EventHandler<OnConnectSentEventArgs>)handler)(this, eventArgs)).ContinueWith(
                t =>
                {
                    if (t.IsFaulted)
                    {
                        this.logger.LogError(t.Exception, "OnConnectSent Handler exception");
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

        this.logger.LogTrace("OnConnAckReceivedEventLauncher");
        var eventArgs = new OnConnAckReceivedEventArgs(packet);
        var handlers = this.OnConnAckReceived.GetInvocationList();

        foreach (var handler in handlers)
        {
            _ = Task.Run(() => ((EventHandler<OnConnAckReceivedEventArgs>)handler)(this, eventArgs)).ContinueWith(
                t =>
                {
                    if (t.IsFaulted)
                    {
                        this.logger.LogError(t.Exception, "OnConnAckReceived Handler exception");
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

        this.logger.LogTrace("OnDisconnectSentEventLauncher");
        var eventArgs = new OnDisconnectSentEventArgs(packet);
        var handlers = this.OnDisconnectSent.GetInvocationList();

        foreach (var handler in handlers)
        {
            _ = Task.Run(() => ((EventHandler<OnDisconnectSentEventArgs>)handler)(this, eventArgs)).ContinueWith(
                t =>
                {
                    if (t.IsFaulted)
                    {
                        this.logger.LogError(t.Exception, "OnDisconnectSent Handler exception");
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

        this.logger.LogTrace("OnDisconnectReceivedEventLauncher: ReasonCode: {ReasonCode} ReasonString: {ReasonString}", packet.DisconnectReasonCode, packet.Properties.ReasonString);
        var eventArgs = new OnDisconnectReceivedEventArgs(packet);
        var handlers = this.OnDisconnectReceived.GetInvocationList();

        foreach (var handler in handlers)
        {
            _ = Task.Run(() => ((EventHandler<OnDisconnectReceivedEventArgs>)handler)(this, eventArgs)).ContinueWith(
                t =>
                {
                    if (t.IsFaulted)
                    {
                        this.logger.LogError(t.Exception, "OnDisconnectReceived Handler exception");
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

        this.logger.LogTrace("OnPingReqSentEventLauncher");
        var eventArgs = new OnPingReqSentEventArgs(packet);
        var handlers = this.OnPingReqSent.GetInvocationList();

        foreach (var handler in handlers)
        {
            _ = Task.Run(() => ((EventHandler<OnPingReqSentEventArgs>)handler)(this, eventArgs)).ContinueWith(
                t =>
                {
                    if (t.IsFaulted)
                    {
                        this.logger.LogError(t.Exception, "OnPingReqSent Handler exception");
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

        this.logger.LogTrace("OnPingRespReceivedEventLauncher");
        var eventArgs = new OnPingRespReceivedEventArgs(packet);
        var handlers = this.OnPingRespReceived.GetInvocationList();

        foreach (var handler in handlers)
        {
            _ = Task.Run(() => ((EventHandler<OnPingRespReceivedEventArgs>)handler)(this, eventArgs)).ContinueWith(
                t =>
                {
                    if (t.IsFaulted)
                    {
                        this.logger.LogError(t.Exception, "OnPingRespReceived Handler exception");
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

        this.logger.LogTrace("OnSubscribeSentEventLauncher");
        var eventArgs = new OnSubscribeSentEventArgs(packet);
        var handlers = this.OnSubscribeSent.GetInvocationList();

        foreach (var handler in handlers)
        {
            _ = Task.Run(() => ((EventHandler<OnSubscribeSentEventArgs>)handler)(this, eventArgs)).ContinueWith(
                t =>
                {
                    if (t.IsFaulted)
                    {
                        this.logger.LogError(t.Exception, "OnSubscribeSent Handler exception");
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

        this.logger.LogTrace("OnSubAckReceivedEventLauncher");
        var eventArgs = new OnSubAckReceivedEventArgs(packet);
        var handlers = this.OnSubAckReceived.GetInvocationList();

        foreach (var handler in handlers)
        {
            _ = Task.Run(() => ((EventHandler<OnSubAckReceivedEventArgs>)handler)(this, eventArgs)).ContinueWith(
                t =>
                {
                    if (t.IsFaulted)
                    {
                        this.logger.LogError(t.Exception, "OnSubAckReceived Handler exception");
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

        this.logger.LogTrace("OnUnsubscribeSentEventLauncher");
        var eventArgs = new OnUnsubscribeSentEventArgs(packet);
        var handlers = this.OnUnsubscribeSent.GetInvocationList();

        foreach (var handler in handlers)
        {
            _ = Task.Run(() => ((EventHandler<OnUnsubscribeSentEventArgs>)handler)(this, eventArgs)).ContinueWith(
                t =>
                {
                    if (t.IsFaulted)
                    {
                        this.logger.LogError(t.Exception, "OnUnsubscribeSent Handler exception");
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

        this.logger.LogTrace("OnUnsubAckReceivedEventLauncher");
        var eventArgs = new OnUnsubAckReceivedEventArgs(packet);
        var handlers = this.OnUnsubAckReceived.GetInvocationList();

        foreach (var handler in handlers)
        {
            _ = Task.Run(() => ((EventHandler<OnUnsubAckReceivedEventArgs>)handler)(this, eventArgs)).ContinueWith(
                t =>
                {
                    if (t.IsFaulted)
                    {
                        this.logger.LogError(t.Exception, "OnUnsubAckReceived Handler exception");
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

        this.logger.LogTrace("OnPublishReceivedEventLauncher");
        var eventArgs = new OnPublishReceivedEventArgs(packet);
        var handlers = this.OnPublishReceived.GetInvocationList();

        foreach (var handler in handlers)
        {
            _ = Task.Run(() => ((EventHandler<OnPublishReceivedEventArgs>)handler)(this, eventArgs)).ContinueWith(
                t =>
                {
                    if (t.IsFaulted)
                    {
                        this.logger.LogError(t.Exception, "OnPublishReceived Handler exception");
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

        this.logger.LogTrace("OnPublishSentEventLauncher");
        var eventArgs = new OnPublishSentEventArgs(packet);
        var handlers = this.OnPublishSent.GetInvocationList();

        foreach (var handler in handlers)
        {
            _ = Task.Run(() => ((EventHandler<OnPublishSentEventArgs>)handler)(this, eventArgs)).ContinueWith(
                t =>
                {
                    if (t.IsFaulted)
                    {
                        this.logger.LogError(t.Exception, "OnPublishSent Handler exception");
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

        this.logger.LogTrace("OnPubAckReceivedEventLauncher");
        var eventArgs = new OnPubAckReceivedEventArgs(packet);
        var handlers = this.OnPubAckReceived.GetInvocationList();

        foreach (var handler in handlers)
        {
            _ = Task.Run(() => ((EventHandler<OnPubAckReceivedEventArgs>)handler)(this, eventArgs)).ContinueWith(
                t =>
                {
                    if (t.IsFaulted)
                    {
                        this.logger.LogError(t.Exception, "OnPubAckReceived Handler exception");
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

        this.logger.LogTrace("OnPubAckSentEventLauncher");
        var eventArgs = new OnPubAckSentEventArgs(packet);
        var handlers = this.OnPubAckSent.GetInvocationList();

        foreach (var handler in handlers)
        {
            _ = Task.Run(() => ((EventHandler<OnPubAckSentEventArgs>)handler)(this, eventArgs)).ContinueWith(
                t =>
                {
                    if (t.IsFaulted)
                    {
                        this.logger.LogError(t.Exception, "OnPubAckSent Handler exception");
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

        this.logger.LogTrace("OnPubRecReceivedEventLauncher");
        var eventArgs = new OnPubRecReceivedEventArgs(packet);
        var handlers = this.OnPubRecReceived.GetInvocationList();

        foreach (var handler in handlers)
        {
            _ = Task.Run(() => ((EventHandler<OnPubRecReceivedEventArgs>)handler)(this, eventArgs)).ContinueWith(
                t =>
                {
                    if (t.IsFaulted)
                    {
                        this.logger.LogError(t.Exception, "OnPubRecReceived Handler exception");
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

        this.logger.LogTrace("OnPubRecSentEventLauncher");
        var eventArgs = new OnPubRecSentEventArgs(packet);
        var handlers = this.OnPubRecSent.GetInvocationList();

        foreach (var handler in handlers)
        {
            _ = Task.Run(() => ((EventHandler<OnPubRecSentEventArgs>)handler)(this, eventArgs)).ContinueWith(
                t =>
                {
                    if (t.IsFaulted)
                    {
                        this.logger.LogError(t.Exception, "OnPubRecSent Handler exception");
                    }
                }, TaskScheduler.Default);
        }
    }

    /// <summary>
    /// Event that is fired after the client received a PubRel packet from the broker.
    /// </summary>
    public event EventHandler<OnPubRelReceivedEventArgs>? OnPubRelReceived;

    internal virtual void OnPubRelReceivedEventLauncher(PubRelPacket packet)
    {
        if (this.OnPubRelReceived == null)
        {
            return;
        }

        this.logger.LogTrace("OnPubRelReceivedEventLauncher");
        var eventArgs = new OnPubRelReceivedEventArgs(packet);
        var handlers = this.OnPubRelReceived.GetInvocationList();

        foreach (var handler in handlers)
        {
            _ = Task.Run(() => ((EventHandler<OnPubRelReceivedEventArgs>)handler)(this, eventArgs)).ContinueWith(
                t =>
                {
                    if (t.IsFaulted)
                    {
                        this.logger.LogError(t.Exception, "OnPubRelReceived Handler exception");
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

        this.logger.LogTrace("OnPubRelSentEventLauncher");
        var eventArgs = new OnPubRelSentEventArgs(packet);
        var handlers = this.OnPubRelSent.GetInvocationList();

        foreach (var handler in handlers)
        {
            _ = Task.Run(() => ((EventHandler<OnPubRelSentEventArgs>)handler)(this, eventArgs)).ContinueWith(
                t =>
                {
                    if (t.IsFaulted)
                    {
                        this.logger.LogError(t.Exception, "OnPubRelSent Handler exception");
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

        this.logger.LogTrace("PubCompReceivedEventLauncher");
        var eventArgs = new OnPubCompReceivedEventArgs(packet);
        var handlers = this.OnPubCompReceived.GetInvocationList();

        foreach (var handler in handlers)
        {
            _ = Task.Run(() => ((EventHandler<OnPubCompReceivedEventArgs>)handler)(this, eventArgs)).ContinueWith(
                t =>
                {
                    if (t.IsFaulted)
                    {
                        this.logger.LogError(t.Exception, "OnPubCompReceived Handler exception");
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

        this.logger.LogTrace("PubCompSentEventLauncher");
        var eventArgs = new OnPubCompSentEventArgs(packet);
        var handlers = this.OnPubCompSent.GetInvocationList();

        foreach (var handler in handlers)
        {
            _ = Task.Run(() => ((EventHandler<OnPubCompSentEventArgs>)handler)(this, eventArgs)).ContinueWith(
                t =>
                {
                    if (t.IsFaulted)
                    {
                        this.logger.LogError(t.Exception, "OnPubCompSent Handler exception");
                    }
                }, TaskScheduler.Default);
        }
    }
}
