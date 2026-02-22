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

namespace HiveMQtt.Sparkplug.HostApplication;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HiveMQtt.Client;
using HiveMQtt.Client.Events;
using HiveMQtt.Client.Options;
using HiveMQtt.Client.Results;
using HiveMQtt.MQTT5.ReasonCodes;
using HiveMQtt.MQTT5.Types;
using HiveMQtt.Sparkplug.Payload;
using HiveMQtt.Sparkplug.Protobuf;
using HiveMQtt.Sparkplug.Topics;

/// <summary>
/// Sparkplug B Host Application: subscribes to Edge Nodes and Devices, tracks state, and publishes NCMD/DCMD.
/// </summary>
public sealed class SparkplugHostApplication : IDisposable
{
    private readonly IHiveMQClient _client;
    private readonly SparkplugHostApplicationOptions _options;
    private readonly bool _ownsClient;
    private readonly ConcurrentDictionary<string, SparkplugNodeState> _nodeStates = new();
    private readonly ConcurrentDictionary<string, SparkplugDeviceState> _deviceStates = new();
    private readonly SemaphoreSlim _startStopLock = new(1, 1);
    private bool _started;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="SparkplugHostApplication"/> class using an existing MQTT client.
    /// </summary>
    /// <param name="client">The MQTT client to use. Must not be null.</param>
    /// <param name="options">Host Application options. Must not be null.</param>
    /// <exception cref="ArgumentNullException">Thrown when client or options is null.</exception>
    public SparkplugHostApplication(IHiveMQClient client, SparkplugHostApplicationOptions options)
    {
        this._client = client ?? throw new ArgumentNullException(nameof(client));
        this._options = options ?? throw new ArgumentNullException(nameof(options));
        this._ownsClient = false;
        this._options.Validate();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SparkplugHostApplication"/> class, creating and owning the MQTT client.
    /// STATE LWT is configured on the client options if <see cref="SparkplugHostApplicationOptions.UseStateLwt"/> is true and no LWT is already set.
    /// </summary>
    /// <param name="clientOptions">The MQTT client options. Will be used to create an internal HiveMQClient.</param>
    /// <param name="sparkplugOptions">Host Application options. Must not be null.</param>
    /// <exception cref="ArgumentNullException">Thrown when clientOptions or sparkplugOptions is null.</exception>
    public SparkplugHostApplication(HiveMQClientOptions clientOptions, SparkplugHostApplicationOptions sparkplugOptions)
    {
        if (clientOptions is null)
        {
            throw new ArgumentNullException(nameof(clientOptions));
        }

        sparkplugOptions?.Validate();
        this._options = sparkplugOptions ?? throw new ArgumentNullException(nameof(sparkplugOptions));

        if (this._options.UseStateMessages && this._options.UseStateLwt && clientOptions.LastWillAndTestament is null && !string.IsNullOrWhiteSpace(this._options.HostApplicationId))
        {
            var (topic, payload) = BuildStateOfflineMessage(this._options);
            clientOptions.LastWillAndTestament = new LastWillAndTestament(topic, payload, QualityOfService.AtLeastOnceDelivery, retain: false);
        }

        this._client = new HiveMQClient(clientOptions);
        this._ownsClient = true;
    }

    /// <summary>
    /// Event raised when a Node Birth (NBIRTH) message is received.
    /// </summary>
    public event EventHandler<SparkplugMessageReceivedEventArgs>? NodeBirthReceived;

    /// <summary>
    /// Event raised when a Node Death (NDEATH) message is received.
    /// </summary>
    public event EventHandler<SparkplugMessageReceivedEventArgs>? NodeDeathReceived;

    /// <summary>
    /// Event raised when a Node Data (NDATA) message is received.
    /// </summary>
    public event EventHandler<SparkplugMessageReceivedEventArgs>? NodeDataReceived;

    /// <summary>
    /// Event raised when a Device Birth (DBIRTH) message is received.
    /// </summary>
    public event EventHandler<SparkplugMessageReceivedEventArgs>? DeviceBirthReceived;

    /// <summary>
    /// Event raised when a Device Death (DDEATH) message is received.
    /// </summary>
    public event EventHandler<SparkplugMessageReceivedEventArgs>? DeviceDeathReceived;

    /// <summary>
    /// Event raised when a Device Data (DDATA) message is received.
    /// </summary>
    public event EventHandler<SparkplugMessageReceivedEventArgs>? DeviceDataReceived;

    /// <summary>
    /// Event raised when a STATE message is received (from another Host Application).
    /// </summary>
    public event EventHandler<SparkplugMessageReceivedEventArgs>? StateMessageReceived;

    /// <summary>
    /// Event raised when a Sparkplug message is received but could not be parsed or is unsupported.
    /// </summary>
    public event EventHandler<SparkplugMessageParseErrorEventArgs>? MessageParseError;

    /// <summary>
    /// Gets the underlying MQTT client.
    /// </summary>
    public IHiveMQClient Client => this._client;

    /// <summary>
    /// Gets the Host Application options.
    /// </summary>
    public SparkplugHostApplicationOptions Options => this._options;

    /// <summary>
    /// Gets a value indicating whether the Host Application is connected and started.
    /// </summary>
    public bool IsConnected => this._started && this._client.IsConnected();

    /// <summary>
    /// Gets a snapshot of known Edge Node states (online/offline).
    /// </summary>
    public IReadOnlyDictionary<string, SparkplugNodeState> NodeStates =>
        new Dictionary<string, SparkplugNodeState>(this._nodeStates);

    /// <summary>
    /// Gets a snapshot of known Device states (online/offline). Key is "groupId/edgeNodeId/deviceId".
    /// </summary>
    public IReadOnlyDictionary<string, SparkplugDeviceState> DeviceStates =>
        new Dictionary<string, SparkplugDeviceState>(this._deviceStates);

    /// <summary>
    /// Connects the client, subscribes to Sparkplug topics, and publishes STATE (birth) if configured.
    /// </summary>
    /// <param name="connectOptions">Optional connect overrides.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task that completes when the Host Application is started.</returns>
    public async Task StartAsync(ConnectOptions? connectOptions = null, CancellationToken cancellationToken = default)
    {
        await this._startStopLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (this._started)
            {
                return;
            }

            if (!this._client.IsConnected())
            {
                var connectResult = await this._client.ConnectAsync(connectOptions).ConfigureAwait(false);
                if (connectResult.ReasonCode != ConnAckReasonCode.Success)
                {
                    throw new InvalidOperationException($"Connect failed: {connectResult.ReasonString ?? connectResult.ReasonCode.ToString()}.");
                }
            }

            this._client.OnMessageReceived += this.OnClientMessageReceived;
            var subscribeResult = await this._client.SubscribeAsync(this._options.SparkplugTopicFilter, QualityOfService.AtLeastOnceDelivery).ConfigureAwait(false);
            var firstSub = subscribeResult.GetFirstSubscription();
            var reasonCode = firstSub?.SubscribeReasonCode;
            var granted = reasonCode is SubAckReasonCode.GrantedQoS0 or SubAckReasonCode.GrantedQoS1 or SubAckReasonCode.GrantedQoS2;
            if (!granted)
            {
                this._client.OnMessageReceived -= this.OnClientMessageReceived;
                throw new InvalidOperationException($"Subscribe failed: {reasonCode}.");
            }

            if (this._options.UseStateMessages && !string.IsNullOrWhiteSpace(this._options.HostApplicationId))
            {
                await this.PublishStateBirthAsync(cancellationToken).ConfigureAwait(false);
            }

            this._started = true;
        }
        finally
        {
            this._startStopLock.Release();
        }
    }

    /// <summary>
    /// Publishes STATE (death), unsubscribes from Sparkplug handling, and disconnects if this instance owns the client.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task that completes when the Host Application is stopped.</returns>
    public async Task StopAsync(CancellationToken cancellationToken = default)
    {
        await this._startStopLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (!this._started)
            {
                return;
            }

            this._client.OnMessageReceived -= this.OnClientMessageReceived;

            if (this._options.UseStateMessages && !string.IsNullOrWhiteSpace(this._options.HostApplicationId))
            {
                await this.PublishStateDeathAsync(cancellationToken).ConfigureAwait(false);
            }

            if (this._ownsClient)
            {
                await this._client.DisconnectAsync().ConfigureAwait(false);
            }

            this._started = false;
        }
        finally
        {
            this._startStopLock.Release();
        }
    }

    /// <summary>
    /// Publishes a Node Command (NCMD) to the specified Edge Node.
    /// </summary>
    /// <param name="groupId">The group ID.</param>
    /// <param name="edgeNodeId">The Edge Node ID.</param>
    /// <param name="payload">The Sparkplug B payload containing the command metrics.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The result of the publish.</returns>
    public Task<PublishResult> PublishNodeCommandAsync(string groupId, string edgeNodeId, Payload payload, CancellationToken cancellationToken = default)
    {
        var topic = SparkplugTopic.NodeCommand(groupId, edgeNodeId, this._options.SparkplugNamespace);
        return this.PublishSparkplugPayloadAsync(topic.Build(), payload, cancellationToken);
    }

    /// <summary>
    /// Publishes a Device Command (DCMD) to the specified Device.
    /// </summary>
    /// <param name="groupId">The group ID.</param>
    /// <param name="edgeNodeId">The Edge Node ID.</param>
    /// <param name="deviceId">The Device ID.</param>
    /// <param name="payload">The Sparkplug B payload containing the command metrics.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The result of the publish.</returns>
    public Task<PublishResult> PublishDeviceCommandAsync(string groupId, string edgeNodeId, string deviceId, Payload payload, CancellationToken cancellationToken = default)
    {
        var topic = SparkplugTopic.DeviceCommand(groupId, edgeNodeId, deviceId, this._options.SparkplugNamespace);
        return this.PublishSparkplugPayloadAsync(topic.Build(), payload, cancellationToken);
    }

    /// <summary>
    /// Publishes a Rebirth command (NCMD with Rebirth metric) to the specified Edge Node.
    /// </summary>
    /// <param name="groupId">The group ID.</param>
    /// <param name="edgeNodeId">The Edge Node ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The result of the publish.</returns>
    public async Task<PublishResult> PublishRebirthCommandAsync(string groupId, string edgeNodeId, CancellationToken cancellationToken = default)
    {
        var payload = SparkplugPayloadEncoder.CreatePayload(SparkplugPayloadEncoder.GetCurrentTimestamp(), 0);
        var rebirthMetric = SparkplugMetricBuilder.Create("Rebirth")
            .WithBooleanValue(true)
            .Build();
        payload.Metrics.Add(rebirthMetric);
        return await this.PublishNodeCommandAsync(groupId, edgeNodeId, payload, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Gets the current state of an Edge Node, or null if unknown.
    /// </summary>
    public SparkplugNodeState? GetNodeState(string groupId, string edgeNodeId)
    {
        var key = SparkplugHostApplicationStatusCache.MakeNodeKey(groupId, edgeNodeId);
        return this._nodeStates.TryGetValue(key, out var state) ? state : null;
    }

    /// <summary>
    /// Gets the current state of a Device, or null if unknown.
    /// </summary>
    public SparkplugDeviceState? GetDeviceState(string groupId, string edgeNodeId, string deviceId)
    {
        var key = SparkplugHostApplicationStatusCache.MakeDeviceKey(groupId, edgeNodeId, deviceId);
        return this._deviceStates.TryGetValue(key, out var state) ? state : null;
    }

    /// <summary>
    /// Disposes the Host Application and the underlying client if this instance owns it.
    /// </summary>
    public void Dispose()
    {
        if (this._disposed)
        {
            return;
        }

        if (this._ownsClient && this._client is IDisposable disposable)
        {
            disposable.Dispose();
        }

        this._startStopLock.Dispose();
        this._disposed = true;
        GC.SuppressFinalize(this);
    }

    private static (string Topic, byte[] Payload) BuildStateOfflineMessage(SparkplugHostApplicationOptions options)
    {
        var topic = $"{options.SparkplugNamespace}/STATE/{options.HostApplicationId}";
        var payload = new Payload
        {
            Timestamp = 0,
        };
        return (topic, SparkplugPayloadEncoder.Encode(payload));
    }

    private async Task PublishStateBirthAsync(CancellationToken cancellationToken)
    {
        var topic = $"{this._options.SparkplugNamespace}/STATE/{this._options.HostApplicationId}";
        var payload = SparkplugPayloadEncoder.CreatePayload(SparkplugPayloadEncoder.GetCurrentTimestamp(), 0);
        var bytes = SparkplugPayloadEncoder.Encode(payload);
        var message = new MQTT5PublishMessage
        {
            Topic = topic,
            Payload = bytes,
            QoS = QualityOfService.AtLeastOnceDelivery,
            Retain = false,
        };
        await this._client.PublishAsync(message, cancellationToken).ConfigureAwait(false);
    }

    private async Task PublishStateDeathAsync(CancellationToken cancellationToken)
    {
        var (topic, payload) = BuildStateOfflineMessage(this._options);
        var message = new MQTT5PublishMessage
        {
            Topic = topic,
            Payload = payload,
            QoS = QualityOfService.AtLeastOnceDelivery,
            Retain = false,
        };
        await this._client.PublishAsync(message, cancellationToken).ConfigureAwait(false);
    }

    private Task<PublishResult> PublishSparkplugPayloadAsync(string topic, Payload payload, CancellationToken cancellationToken)
    {
        var bytes = SparkplugPayloadEncoder.Encode(payload);
        var message = new MQTT5PublishMessage
        {
            Topic = topic,
            Payload = bytes,
            QoS = QualityOfService.AtLeastOnceDelivery,
            Retain = false,
        };
        return this._client.PublishAsync(message, cancellationToken);
    }

    private void OnClientMessageReceived(object? sender, OnMessageReceivedEventArgs e)
    {
        var topicStr = e.PublishMessage.Topic;
        if (string.IsNullOrEmpty(topicStr))
        {
            return;
        }

        if (!SparkplugTopic.TryParse(topicStr, out var sparkplugTopic))
        {
            this.MessageParseError?.Invoke(this, new SparkplugMessageParseErrorEventArgs(topicStr, e.PublishMessage.Payload, "Topic is not a valid Sparkplug topic."));
            return;
        }

        if (sparkplugTopic.MessageType == SparkplugMessageType.STATE)
        {
            this.RaiseStateReceived(sparkplugTopic, topicStr, e.PublishMessage.Payload);
            return;
        }

        if (e.PublishMessage.Payload is null || e.PublishMessage.Payload.Length == 0)
        {
            this.MessageParseError?.Invoke(this, new SparkplugMessageParseErrorEventArgs(topicStr, null, "Empty payload."));
            return;
        }

        if (!SparkplugPayloadEncoder.TryDecode(e.PublishMessage.Payload, out var payload))
        {
            this.MessageParseError?.Invoke(this, new SparkplugMessageParseErrorEventArgs(topicStr, e.PublishMessage.Payload, "Payload is not valid Sparkplug B protobuf."));
            return;
        }

        var args = new SparkplugMessageReceivedEventArgs(sparkplugTopic, payload!, topicStr);
        var now = DateTimeOffset.UtcNow;

        switch (sparkplugTopic.MessageType)
        {
            case SparkplugMessageType.NBIRTH:
                SparkplugHostApplicationStatusCache.UpdateNodeBirth(this._nodeStates, sparkplugTopic.GroupId, sparkplugTopic.EdgeNodeId, now);
                this.NodeBirthReceived?.Invoke(this, args);
                break;
            case SparkplugMessageType.NDEATH:
                SparkplugHostApplicationStatusCache.UpdateNodeDeath(this._nodeStates, sparkplugTopic.GroupId, sparkplugTopic.EdgeNodeId, now);
                this.NodeDeathReceived?.Invoke(this, args);
                break;
            case SparkplugMessageType.NDATA:
                SparkplugHostApplicationStatusCache.UpdateNodeData(this._nodeStates, sparkplugTopic.GroupId, sparkplugTopic.EdgeNodeId, now);
                this.NodeDataReceived?.Invoke(this, args);
                break;
            case SparkplugMessageType.DBIRTH:
                SparkplugHostApplicationStatusCache.UpdateDeviceBirth(this._deviceStates, sparkplugTopic.GroupId, sparkplugTopic.EdgeNodeId, sparkplugTopic.DeviceId!, now);
                this.DeviceBirthReceived?.Invoke(this, args);
                break;
            case SparkplugMessageType.DDEATH:
                SparkplugHostApplicationStatusCache.UpdateDeviceDeath(this._deviceStates, sparkplugTopic.GroupId, sparkplugTopic.EdgeNodeId, sparkplugTopic.DeviceId!, now);
                this.DeviceDeathReceived?.Invoke(this, args);
                break;
            case SparkplugMessageType.DDATA:
                SparkplugHostApplicationStatusCache.UpdateDeviceData(this._deviceStates, sparkplugTopic.GroupId, sparkplugTopic.EdgeNodeId, sparkplugTopic.DeviceId!, now);
                this.DeviceDataReceived?.Invoke(this, args);
                break;
            default:
                this.MessageParseError?.Invoke(this, new SparkplugMessageParseErrorEventArgs(topicStr, e.PublishMessage.Payload, $"Unsupported message type for Host: {sparkplugTopic.MessageType}."));
                break;
        }
    }

    private void RaiseStateReceived(SparkplugTopic topic, string rawTopic, byte[]? payload)
    {
        var p = payload is { Length: > 0 } && SparkplugPayloadEncoder.TryDecode(payload, out var decoded)
            ? decoded!
            : new Payload();
        var args = new SparkplugMessageReceivedEventArgs(topic, p, rawTopic);
        this.StateMessageReceived?.Invoke(this, args);
    }

    /// <summary>
    /// Internal status cache helper for thread-safe node/device state updates.
    /// </summary>
    internal static class SparkplugHostApplicationStatusCache
    {
        public static string MakeNodeKey(string groupId, string edgeNodeId) => $"{groupId}/{edgeNodeId}";

        public static string MakeDeviceKey(string groupId, string edgeNodeId, string deviceId) => $"{groupId}/{edgeNodeId}/{deviceId}";

        public static void UpdateNodeBirth(ConcurrentDictionary<string, SparkplugNodeState> cache, string groupId, string edgeNodeId, DateTimeOffset now)
        {
            var key = MakeNodeKey(groupId, edgeNodeId);
            cache[key] = new SparkplugNodeState(groupId, edgeNodeId, isOnline: true, now);
        }

        public static void UpdateNodeDeath(ConcurrentDictionary<string, SparkplugNodeState> cache, string groupId, string edgeNodeId, DateTimeOffset now)
        {
            var key = MakeNodeKey(groupId, edgeNodeId);
            cache[key] = new SparkplugNodeState(groupId, edgeNodeId, isOnline: false, cache.TryGetValue(key, out var existing) ? existing.LastSeenUtc : now);
        }

        public static void UpdateNodeData(ConcurrentDictionary<string, SparkplugNodeState> cache, string groupId, string edgeNodeId, DateTimeOffset now)
        {
            var key = MakeNodeKey(groupId, edgeNodeId);
            if (cache.TryGetValue(key, out var existing))
            {
                cache[key] = new SparkplugNodeState(groupId, edgeNodeId, existing.IsOnline, now);
            }
        }

        public static void UpdateDeviceBirth(ConcurrentDictionary<string, SparkplugDeviceState> cache, string groupId, string edgeNodeId, string deviceId, DateTimeOffset now)
        {
            var key = MakeDeviceKey(groupId, edgeNodeId, deviceId);
            cache[key] = new SparkplugDeviceState(groupId, edgeNodeId, deviceId, isOnline: true, now);
        }

        public static void UpdateDeviceDeath(ConcurrentDictionary<string, SparkplugDeviceState> cache, string groupId, string edgeNodeId, string deviceId, DateTimeOffset now)
        {
            var key = MakeDeviceKey(groupId, edgeNodeId, deviceId);
            cache[key] = new SparkplugDeviceState(groupId, edgeNodeId, deviceId, isOnline: false, cache.TryGetValue(key, out var existing) ? existing.LastSeenUtc : now);
        }

        public static void UpdateDeviceData(ConcurrentDictionary<string, SparkplugDeviceState> cache, string groupId, string edgeNodeId, string deviceId, DateTimeOffset now)
        {
            var key = MakeDeviceKey(groupId, edgeNodeId, deviceId);
            if (cache.TryGetValue(key, out var existing))
            {
                cache[key] = new SparkplugDeviceState(groupId, edgeNodeId, deviceId, existing.IsOnline, now);
            }
        }
    }
}
