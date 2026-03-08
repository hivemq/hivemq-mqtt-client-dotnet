// Copyright 2026-present HiveMQ and the HiveMQ Community
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

namespace HiveMQtt.Sparkplug.Test.HostApplication;

using System;
using System.Buffers;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using HiveMQtt.Client;
using HiveMQtt.Client.Events;
using HiveMQtt.Client.Options;
using HiveMQtt.Client.Results;
using HiveMQtt.MQTT5.Packets;
using HiveMQtt.MQTT5.ReasonCodes;
using HiveMQtt.MQTT5.Types;

/// <summary>
/// Fake MQTT client for unit testing Sparkplug Host Application without a broker.
/// Uses HiveMQtt internals (via InternalsVisibleTo) to create ConnectResult and SubscribeResult.
/// </summary>
internal sealed class FakeHiveMQClient : IHiveMQClient
{
    private bool connected;
    private bool disposed;

    public FakeHiveMQClient()
    {
        this.Options = new HiveMQClientOptions();
        this.LocalStore = new Dictionary<string, string>();
        this.Subscriptions = new List<Subscription>();
    }

    public Dictionary<string, string> LocalStore { get; }

    public HiveMQClientOptions Options { get; set; }

    public List<Subscription> Subscriptions { get; }

    public List<MQTT5PublishMessage> PublishedMessages { get; } = new();

    public bool IsConnected() => this.connected && !this.disposed;

    public Task<ConnectResult> ConnectAsync(ConnectOptions? connectOptions = null)
    {
        var props = new MQTT5Properties();
        var result = new ConnectResult(ConnAckReasonCode.Success, false, props);
        this.connected = true;
        return Task.FromResult(result);
    }

    public Task<bool> DisconnectAsync(DisconnectOptions? options = null)
    {
        this.connected = false;
        return Task.FromResult(true);
    }

    public Task<PublishResult> PublishAsync(MQTT5PublishMessage message, CancellationToken cancellationToken = default)
    {
        this.PublishedMessages.Add(message);
        return Task.FromResult(new PublishResult(message));
    }

    public Task<PublishResult> PublishAsync(string topic, string payload, QualityOfService qos = QualityOfService.AtMostOnceDelivery)
    {
        var msg = new MQTT5PublishMessage(topic, qos) { PayloadAsString = payload };
        return this.PublishAsync(msg);
    }

    public Task<PublishResult> PublishAsync(string topic, byte[] payload, QualityOfService qos = QualityOfService.AtMostOnceDelivery)
    {
        var msg = new MQTT5PublishMessage(topic, qos) { Payload = payload };
        return this.PublishAsync(msg);
    }

    public Task<SubscribeResult> SubscribeAsync(string topic, QualityOfService qos = QualityOfService.AtMostOnceDelivery, bool noLocal = false, bool retainAsPublished = false, RetainHandling retainHandling = RetainHandling.SendAtSubscribe)
    {
        var options = new SubscribeOptions();
        options.TopicFilters.Add(new TopicFilter(topic, qos));
        return this.SubscribeAsync(options);
    }

    public Task<SubscribeResult> SubscribeAsync(SubscribeOptions options)
    {
        // SUBACK: fixed 0x90, remaining length = 3 + N (N = reason codes), packet id 0x00 0x01, prop len 0x00, N reason codes (0x01)
        var n = options.TopicFilters.Count;
        var subAckBytes = new byte[6 + n];
        subAckBytes[0] = 0x90;
        subAckBytes[1] = (byte)(3 + n);
        subAckBytes[2] = 0x00;
        subAckBytes[3] = 0x01;
        subAckBytes[4] = 0x00;
        for (var i = 0; i < n; i++)
        {
            subAckBytes[5 + i] = 0x01; // GrantedQoS1
        }

        var sequence = new ReadOnlySequence<byte>(subAckBytes);
        var subAck = new SubAckPacket(sequence);
        var result = new SubscribeResult(options, subAck);
        foreach (var sub in result.Subscriptions)
        {
            this.Subscriptions.Add(sub);
        }

        return Task.FromResult(result);
    }

    public Task<UnsubscribeResult> UnsubscribeAsync(string topic) => throw new NotImplementedException("Not used in Host tests");

    public Task<UnsubscribeResult> UnsubscribeAsync(Subscription subscription) => throw new NotImplementedException("Not used in Host tests");

    public Task<UnsubscribeResult> UnsubscribeAsync(List<Subscription> subscriptions) => throw new NotImplementedException("Not used in Host tests");

    public Task AckAsync(ushort packetIdentifier) => Task.CompletedTask;

    public Task AckAsync(OnMessageReceivedEventArgs eventArgs) => Task.CompletedTask;

    public event EventHandler<BeforeConnectEventArgs>? BeforeConnect;
    public event EventHandler<AfterConnectEventArgs>? AfterConnect;
    public event EventHandler<BeforeDisconnectEventArgs>? BeforeDisconnect;
    public event EventHandler<AfterDisconnectEventArgs>? AfterDisconnect;
    public event EventHandler<BeforeSubscribeEventArgs>? BeforeSubscribe;
    public event EventHandler<AfterSubscribeEventArgs>? AfterSubscribe;
    public event EventHandler<BeforeUnsubscribeEventArgs>? BeforeUnsubscribe;
    public event EventHandler<AfterUnsubscribeEventArgs>? AfterUnsubscribe;
    public event EventHandler<OnMessageReceivedEventArgs>? OnMessageReceived;
    public event EventHandler<OnConnectSentEventArgs>? OnConnectSent;
    public event EventHandler<OnConnAckReceivedEventArgs>? OnConnAckReceived;
    public event EventHandler<OnDisconnectSentEventArgs>? OnDisconnectSent;
    public event EventHandler<OnDisconnectReceivedEventArgs>? OnDisconnectReceived;
    public event EventHandler<OnPingReqSentEventArgs>? OnPingReqSent;
    public event EventHandler<OnPingRespReceivedEventArgs>? OnPingRespReceived;
    public event EventHandler<OnSubscribeSentEventArgs>? OnSubscribeSent;
    public event EventHandler<OnSubAckReceivedEventArgs>? OnSubAckReceived;
    public event EventHandler<OnUnsubscribeSentEventArgs>? OnUnsubscribeSent;
    public event EventHandler<OnUnsubAckReceivedEventArgs>? OnUnsubAckReceived;
    public event EventHandler<OnPublishReceivedEventArgs>? OnPublishReceived;
    public event EventHandler<OnPublishSentEventArgs>? OnPublishSent;
    public event EventHandler<OnPubAckReceivedEventArgs>? OnPubAckReceived;
    public event EventHandler<OnPubAckSentEventArgs>? OnPubAckSent;
    public event EventHandler<OnPubRecReceivedEventArgs>? OnPubRecReceived;
    public event EventHandler<OnPubRecSentEventArgs>? OnPubRecSent;
    public event EventHandler<OnPubRelReceivedEventArgs>? OnPubRelReceived;
    public event EventHandler<OnPubRelSentEventArgs>? OnPubRelSent;
    public event EventHandler<OnPubCompReceivedEventArgs>? OnPubCompReceived;
    public event EventHandler<OnPubCompSentEventArgs>? OnPubCompSent;

    public void SimulateMessageReceived(string topic, byte[] payload)
    {
        var msg = new MQTT5PublishMessage(topic, QualityOfService.AtLeastOnceDelivery) { Payload = payload };
        this.OnMessageReceived?.Invoke(this, new OnMessageReceivedEventArgs(msg, 1));
    }

    public void Dispose()
    {
        this.disposed = true;
        this.connected = false;
    }
}
