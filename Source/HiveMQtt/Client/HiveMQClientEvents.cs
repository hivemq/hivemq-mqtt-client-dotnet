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
using Microsoft.Extensions.Logging;

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

        LogBeforeConnectEventLauncher(this.logger);
        var eventArgs = new BeforeConnectEventArgs(options);
        var handlers = this.BeforeConnect.GetInvocationList();

        foreach (var handler in handlers)
        {
            _ = Task.Run(() => ((EventHandler<BeforeConnectEventArgs>)handler)(this, eventArgs)).ContinueWith(
                t =>
                {
                    if (t.IsFaulted)
                    {
                        LogBeforeConnectHandlerException(this.logger, t.Exception);
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

        LogAfterConnectEventLauncher(this.logger);
        var eventArgs = new AfterConnectEventArgs(results);
        var handlers = this.AfterConnect.GetInvocationList();

        foreach (var handler in handlers)
        {
            _ = Task.Run(() => ((EventHandler<AfterConnectEventArgs>)handler)(this, eventArgs)).ContinueWith(
                t =>
                {
                    if (t.IsFaulted)
                    {
                        LogAfterConnectHandlerException(this.logger, t.Exception);
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

        LogBeforeDisconnectEventLauncher(this.logger);
        var eventArgs = new BeforeDisconnectEventArgs();
        var handlers = this.BeforeDisconnect.GetInvocationList();

        foreach (var handler in handlers)
        {
            _ = Task.Run(() => ((EventHandler<BeforeDisconnectEventArgs>)handler)(this, eventArgs)).ContinueWith(
                t =>
                {
                    if (t.IsFaulted)
                    {
                        LogBeforeDisconnectHandlerException(this.logger, t.Exception);
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

        LogAfterDisconnectEventLauncher(this.logger);
        var eventArgs = new AfterDisconnectEventArgs(clean);
        var handlers = this.AfterDisconnect.GetInvocationList();

        foreach (var handler in handlers)
        {
            _ = Task.Run(() => ((EventHandler<AfterDisconnectEventArgs>)handler)(this, eventArgs)).ContinueWith(
                t =>
                {
                    if (t.IsFaulted)
                    {
                        LogAfterDisconnectHandlerException(this.logger, t.Exception);
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

        LogBeforeSubscribeEventLauncher(this.logger);
        var eventArgs = new BeforeSubscribeEventArgs(options);
        var handlers = this.BeforeSubscribe.GetInvocationList();

        foreach (var handler in handlers)
        {
            _ = Task.Run(() => ((EventHandler<BeforeSubscribeEventArgs>)handler)(this, eventArgs)).ContinueWith(
                t =>
                {
                    if (t.IsFaulted)
                    {
                        LogBeforeSubscribeHandlerException(this.logger, t.Exception);
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

        LogAfterSubscribeEventLauncher(this.logger);
        var eventArgs = new AfterSubscribeEventArgs(results);
        var handlers = this.AfterSubscribe.GetInvocationList();

        foreach (var handler in handlers)
        {
            _ = Task.Run(() => ((EventHandler<AfterSubscribeEventArgs>)handler)(this, eventArgs)).ContinueWith(
                t =>
                {
                    if (t.IsFaulted)
                    {
                        LogAfterSubscribeHandlerException(this.logger, t.Exception);
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

        LogBeforeUnsubscribeEventLauncher(this.logger);
        var eventArgs = new BeforeUnsubscribeEventArgs(subscriptions);
        var handlers = this.BeforeUnsubscribe.GetInvocationList();

        foreach (var handler in handlers)
        {
            _ = Task.Run(() => ((EventHandler<BeforeUnsubscribeEventArgs>)handler)(this, eventArgs)).ContinueWith(
                t =>
                {
                    if (t.IsFaulted)
                    {
                        LogBeforeUnsubscribeHandlerException(this.logger, t.Exception);
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

        LogAfterUnsubscribeEventLauncher(this.logger);
        var eventArgs = new AfterUnsubscribeEventArgs(results);
        var handlers = this.AfterUnsubscribe.GetInvocationList();

        foreach (var handler in handlers)
        {
            _ = Task.Run(() => ((EventHandler<AfterUnsubscribeEventArgs>)handler)(this, eventArgs)).ContinueWith(
                t =>
                {
                    if (t.IsFaulted)
                    {
                        LogAfterUnsubscribeHandlerException(this.logger, t.Exception);
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
            if (this.logger.IsEnabled(LogLevel.Trace))
            {
                LogOnMessageReceivedEventLauncher(this.logger);
            }

            var eventArgs = new OnMessageReceivedEventArgs(packet.Message);
            var handlers = this.OnMessageReceived.GetInvocationList();
            foreach (var handler in handlers)
            {
                _ = Task.Run(() => ((EventHandler<OnMessageReceivedEventArgs>)handler)(this, eventArgs)).ContinueWith(
                    t =>
                    {
                        if (t.IsFaulted)
                        {
                            LogOnMessageReceivedHandlerException(this.logger, t.Exception);
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
                        LogPerSubscriptionMessageReceivedFaulted(this.logger, e, packet.Message.Topic);
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
            LogLostApplicationMessage(this.logger, packet.Message.Topic);
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

        if (this.logger.IsEnabled(LogLevel.Trace))
        {
            LogOnConnectSentEventLauncher(this.logger);
        }

        var eventArgs = new OnConnectSentEventArgs(packet);
        var handlers = this.OnConnectSent.GetInvocationList();

        foreach (var handler in handlers)
        {
            _ = Task.Run(() => ((EventHandler<OnConnectSentEventArgs>)handler)(this, eventArgs)).ContinueWith(
                t =>
                {
                    if (t.IsFaulted)
                    {
                        LogOnConnectSentHandlerException(this.logger, t.Exception);
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

        if (this.logger.IsEnabled(LogLevel.Trace))
        {
            LogOnConnAckReceivedEventLauncher(this.logger);
        }

        var eventArgs = new OnConnAckReceivedEventArgs(packet);
        var handlers = this.OnConnAckReceived.GetInvocationList();

        foreach (var handler in handlers)
        {
            _ = Task.Run(() => ((EventHandler<OnConnAckReceivedEventArgs>)handler)(this, eventArgs)).ContinueWith(
                t =>
                {
                    if (t.IsFaulted)
                    {
                        LogOnConnAckReceivedHandlerException(this.logger, t.Exception);
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

        if (this.logger.IsEnabled(LogLevel.Trace))
        {
            LogOnDisconnectSentEventLauncher(this.logger);
        }

        var eventArgs = new OnDisconnectSentEventArgs(packet);
        var handlers = this.OnDisconnectSent.GetInvocationList();

        foreach (var handler in handlers)
        {
            _ = Task.Run(() => ((EventHandler<OnDisconnectSentEventArgs>)handler)(this, eventArgs)).ContinueWith(
                t =>
                {
                    if (t.IsFaulted)
                    {
                        LogOnDisconnectSentHandlerException(this.logger, t.Exception);
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

        if (this.logger.IsEnabled(LogLevel.Trace))
        {
            LogOnDisconnectReceivedEventLauncher(this.logger, packet.DisconnectReasonCode, packet.Properties.ReasonString);
        }

        var eventArgs = new OnDisconnectReceivedEventArgs(packet);
        var handlers = this.OnDisconnectReceived.GetInvocationList();

        foreach (var handler in handlers)
        {
            _ = Task.Run(() => ((EventHandler<OnDisconnectReceivedEventArgs>)handler)(this, eventArgs)).ContinueWith(
                t =>
                {
                    if (t.IsFaulted)
                    {
                        LogOnDisconnectReceivedHandlerException(this.logger, t.Exception);
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

        if (this.logger.IsEnabled(LogLevel.Trace))
        {
            LogOnPingReqSentEventLauncher(this.logger);
        }

        var eventArgs = new OnPingReqSentEventArgs(packet);
        var handlers = this.OnPingReqSent.GetInvocationList();

        foreach (var handler in handlers)
        {
            _ = Task.Run(() => ((EventHandler<OnPingReqSentEventArgs>)handler)(this, eventArgs)).ContinueWith(
                t =>
                {
                    if (t.IsFaulted)
                    {
                        LogOnPingReqSentHandlerException(this.logger, t.Exception);
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

        if (this.logger.IsEnabled(LogLevel.Trace))
        {
            LogOnPingRespReceivedEventLauncher(this.logger);
        }

        var eventArgs = new OnPingRespReceivedEventArgs(packet);
        var handlers = this.OnPingRespReceived.GetInvocationList();

        foreach (var handler in handlers)
        {
            _ = Task.Run(() => ((EventHandler<OnPingRespReceivedEventArgs>)handler)(this, eventArgs)).ContinueWith(
                t =>
                {
                    if (t.IsFaulted)
                    {
                        LogOnPingRespReceivedHandlerException(this.logger, t.Exception);
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

        if (this.logger.IsEnabled(LogLevel.Trace))
        {
            LogOnSubscribeSentEventLauncher(this.logger);
        }

        var eventArgs = new OnSubscribeSentEventArgs(packet);
        var handlers = this.OnSubscribeSent.GetInvocationList();

        foreach (var handler in handlers)
        {
            _ = Task.Run(() => ((EventHandler<OnSubscribeSentEventArgs>)handler)(this, eventArgs)).ContinueWith(
                t =>
                {
                    if (t.IsFaulted)
                    {
                        LogOnSubscribeSentHandlerException(this.logger, t.Exception);
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

        if (this.logger.IsEnabled(LogLevel.Trace))
        {
            LogOnSubAckReceivedEventLauncher(this.logger);
        }

        var eventArgs = new OnSubAckReceivedEventArgs(packet);
        var handlers = this.OnSubAckReceived.GetInvocationList();

        foreach (var handler in handlers)
        {
            _ = Task.Run(() => ((EventHandler<OnSubAckReceivedEventArgs>)handler)(this, eventArgs)).ContinueWith(
                t =>
                {
                    if (t.IsFaulted)
                    {
                        LogOnSubAckReceivedHandlerException(this.logger, t.Exception);
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

        if (this.logger.IsEnabled(LogLevel.Trace))
        {
            LogOnUnsubscribeSentEventLauncher(this.logger);
        }

        var eventArgs = new OnUnsubscribeSentEventArgs(packet);
        var handlers = this.OnUnsubscribeSent.GetInvocationList();

        foreach (var handler in handlers)
        {
            _ = Task.Run(() => ((EventHandler<OnUnsubscribeSentEventArgs>)handler)(this, eventArgs)).ContinueWith(
                t =>
                {
                    if (t.IsFaulted)
                    {
                        LogOnUnsubscribeSentHandlerException(this.logger, t.Exception);
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

        if (this.logger.IsEnabled(LogLevel.Trace))
        {
            LogOnUnsubAckReceivedEventLauncher(this.logger);
        }

        var eventArgs = new OnUnsubAckReceivedEventArgs(packet);
        var handlers = this.OnUnsubAckReceived.GetInvocationList();

        foreach (var handler in handlers)
        {
            _ = Task.Run(() => ((EventHandler<OnUnsubAckReceivedEventArgs>)handler)(this, eventArgs)).ContinueWith(
                t =>
                {
                    if (t.IsFaulted)
                    {
                        LogOnUnsubAckReceivedHandlerException(this.logger, t.Exception);
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

        if (this.logger.IsEnabled(LogLevel.Trace))
        {
            LogOnPublishReceivedEventLauncher(this.logger);
        }

        var eventArgs = new OnPublishReceivedEventArgs(packet);
        var handlers = this.OnPublishReceived.GetInvocationList();

        foreach (var handler in handlers)
        {
            _ = Task.Run(() => ((EventHandler<OnPublishReceivedEventArgs>)handler)(this, eventArgs)).ContinueWith(
                t =>
                {
                    if (t.IsFaulted)
                    {
                        LogOnPublishReceivedHandlerException(this.logger, t.Exception);
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

        if (this.logger.IsEnabled(LogLevel.Trace))
        {
            LogOnPublishSentEventLauncher(this.logger);
        }

        var eventArgs = new OnPublishSentEventArgs(packet);
        var handlers = this.OnPublishSent.GetInvocationList();

        foreach (var handler in handlers)
        {
            _ = Task.Run(() => ((EventHandler<OnPublishSentEventArgs>)handler)(this, eventArgs)).ContinueWith(
                t =>
                {
                    if (t.IsFaulted)
                    {
                        LogOnPublishSentHandlerException(this.logger, t.Exception);
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

        if (this.logger.IsEnabled(LogLevel.Trace))
        {
            LogOnPubAckReceivedEventLauncher(this.logger);
        }

        var eventArgs = new OnPubAckReceivedEventArgs(packet);
        var handlers = this.OnPubAckReceived.GetInvocationList();

        foreach (var handler in handlers)
        {
            _ = Task.Run(() => ((EventHandler<OnPubAckReceivedEventArgs>)handler)(this, eventArgs)).ContinueWith(
                t =>
                {
                    if (t.IsFaulted)
                    {
                        LogOnPubAckReceivedHandlerException(this.logger, t.Exception);
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

        if (this.logger.IsEnabled(LogLevel.Trace))
        {
            LogOnPubAckSentEventLauncher(this.logger);
        }

        var eventArgs = new OnPubAckSentEventArgs(packet);
        var handlers = this.OnPubAckSent.GetInvocationList();

        foreach (var handler in handlers)
        {
            _ = Task.Run(() => ((EventHandler<OnPubAckSentEventArgs>)handler)(this, eventArgs)).ContinueWith(
                t =>
                {
                    if (t.IsFaulted)
                    {
                        LogOnPubAckSentHandlerException(this.logger, t.Exception);
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

        if (this.logger.IsEnabled(LogLevel.Trace))
        {
            LogOnPubRecReceivedEventLauncher(this.logger);
        }

        var eventArgs = new OnPubRecReceivedEventArgs(packet);
        var handlers = this.OnPubRecReceived.GetInvocationList();

        foreach (var handler in handlers)
        {
            _ = Task.Run(() => ((EventHandler<OnPubRecReceivedEventArgs>)handler)(this, eventArgs)).ContinueWith(
                t =>
                {
                    if (t.IsFaulted)
                    {
                        LogOnPubRecReceivedHandlerException(this.logger, t.Exception);
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

        if (this.logger.IsEnabled(LogLevel.Trace))
        {
            LogOnPubRecSentEventLauncher(this.logger);
        }

        var eventArgs = new OnPubRecSentEventArgs(packet);
        var handlers = this.OnPubRecSent.GetInvocationList();

        foreach (var handler in handlers)
        {
            _ = Task.Run(() => ((EventHandler<OnPubRecSentEventArgs>)handler)(this, eventArgs)).ContinueWith(
                t =>
                {
                    if (t.IsFaulted)
                    {
                        LogOnPubRecSentHandlerException(this.logger, t.Exception);
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

        if (this.logger.IsEnabled(LogLevel.Trace))
        {
            LogOnPubRelReceivedEventLauncher(this.logger);
        }

        var eventArgs = new OnPubRelReceivedEventArgs(packet);
        var handlers = this.OnPubRelReceived.GetInvocationList();

        foreach (var handler in handlers)
        {
            _ = Task.Run(() => ((EventHandler<OnPubRelReceivedEventArgs>)handler)(this, eventArgs)).ContinueWith(
                t =>
                {
                    if (t.IsFaulted)
                    {
                        LogOnPubRelReceivedHandlerException(this.logger, t.Exception);
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

        if (this.logger.IsEnabled(LogLevel.Trace))
        {
            LogOnPubRelSentEventLauncher(this.logger);
        }

        var eventArgs = new OnPubRelSentEventArgs(packet);
        var handlers = this.OnPubRelSent.GetInvocationList();

        foreach (var handler in handlers)
        {
            _ = Task.Run(() => ((EventHandler<OnPubRelSentEventArgs>)handler)(this, eventArgs)).ContinueWith(
                t =>
                {
                    if (t.IsFaulted)
                    {
                        LogOnPubRelSentHandlerException(this.logger, t.Exception);
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

        if (this.logger.IsEnabled(LogLevel.Trace))
        {
            LogPubCompReceivedEventLauncher(this.logger);
        }

        var eventArgs = new OnPubCompReceivedEventArgs(packet);
        var handlers = this.OnPubCompReceived.GetInvocationList();

        foreach (var handler in handlers)
        {
            _ = Task.Run(() => ((EventHandler<OnPubCompReceivedEventArgs>)handler)(this, eventArgs)).ContinueWith(
                t =>
                {
                    if (t.IsFaulted)
                    {
                        LogOnPubCompReceivedHandlerException(this.logger, t.Exception);
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

        if (this.logger.IsEnabled(LogLevel.Trace))
        {
            LogPubCompSentEventLauncher(this.logger);
        }

        var eventArgs = new OnPubCompSentEventArgs(packet);
        var handlers = this.OnPubCompSent.GetInvocationList();

        foreach (var handler in handlers)
        {
            _ = Task.Run(() => ((EventHandler<OnPubCompSentEventArgs>)handler)(this, eventArgs)).ContinueWith(
                t =>
                {
                    if (t.IsFaulted)
                    {
                        LogOnPubCompSentHandlerException(this.logger, t.Exception);
                    }
                }, TaskScheduler.Default);
        }
    }
}
