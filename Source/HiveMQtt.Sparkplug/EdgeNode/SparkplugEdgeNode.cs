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

namespace HiveMQtt.Sparkplug.EdgeNode;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using HiveMQtt.Client;
using HiveMQtt.Client.Events;
using HiveMQtt.Client.Options;
using HiveMQtt.Client.Results;
using HiveMQtt.MQTT5.ReasonCodes;
using HiveMQtt.MQTT5.Types;
using HiveMQtt.Sparkplug.HostApplication;
using HiveMQtt.Sparkplug.Payload;
using HiveMQtt.Sparkplug.Protobuf;
using HiveMQtt.Sparkplug.Topics;

/// <summary>
/// Sparkplug B Edge Node: publishes NBIRTH/NDATA/NDEATH, DBIRTH/DDATA/DDEATH; receives NCMD/DCMD; manages sequence and lifecycle.
/// </summary>
public sealed class SparkplugEdgeNode : IDisposable
{
    private readonly IHiveMQClient _client;
    private readonly SparkplugEdgeNodeOptions _options;
    private readonly bool _ownsClient;
    private readonly SemaphoreSlim _startStopLock = new(1, 1);
    private int _sequenceNumber;
    private bool _started;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="SparkplugEdgeNode"/> class using an existing MQTT client.
    /// </summary>
    /// <param name="client">The MQTT client to use. Must not be null.</param>
    /// <param name="options">Edge Node options. Must not be null.</param>
    /// <exception cref="ArgumentNullException">Thrown when client or options is null.</exception>
    public SparkplugEdgeNode(IHiveMQClient client, SparkplugEdgeNodeOptions options)
    {
        this._client = client ?? throw new ArgumentNullException(nameof(client));
        this._options = options ?? throw new ArgumentNullException(nameof(options));
        this._ownsClient = false;
        this._options.Validate();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SparkplugEdgeNode"/> class, creating and owning the MQTT client.
    /// </summary>
    /// <param name="clientOptions">The MQTT client options. Will be used to create an internal HiveMQClient.</param>
    /// <param name="sparkplugOptions">Edge Node options. Must not be null.</param>
    /// <exception cref="ArgumentNullException">Thrown when clientOptions or sparkplugOptions is null.</exception>
    public SparkplugEdgeNode(HiveMQClientOptions clientOptions, SparkplugEdgeNodeOptions sparkplugOptions)
    {
        ArgumentNullException.ThrowIfNull(clientOptions);
        sparkplugOptions?.Validate();
        this._options = sparkplugOptions ?? throw new ArgumentNullException(nameof(sparkplugOptions));
        this._client = new HiveMQClient(clientOptions);
        this._ownsClient = true;
    }

    /// <summary>
    /// Event raised when a Node Command (NCMD) is received for this Edge Node.
    /// </summary>
    public event EventHandler<SparkplugMessageReceivedEventArgs>? NodeCommandReceived;

    /// <summary>
    /// Event raised when a Device Command (DCMD) is received for a device of this Edge Node.
    /// </summary>
    public event EventHandler<SparkplugMessageReceivedEventArgs>? DeviceCommandReceived;

    /// <summary>
    /// Event raised when a command message is received but could not be parsed.
    /// </summary>
    public event EventHandler<SparkplugMessageParseErrorEventArgs>? MessageParseError;

    /// <summary>
    /// Gets the underlying MQTT client.
    /// </summary>
    public IHiveMQClient Client => this._client;

    /// <summary>
    /// Gets the Edge Node options.
    /// </summary>
    public SparkplugEdgeNodeOptions Options => this._options;

    /// <summary>
    /// Gets a value indicating whether the Edge Node is connected and started.
    /// </summary>
    public bool IsConnected => this._started && this._client.IsConnected();

    /// <summary>
    /// Gets the current sequence number (0-255). Incremented after each publish.
    /// </summary>
    public int SequenceNumber => this._sequenceNumber;

    /// <summary>
    /// Connects the client, subscribes to NCMD and DCMD topics for this node, and publishes Node Birth (NBIRTH).
    /// </summary>
    /// <param name="connectOptions">Optional connect overrides.</param>
    /// <param name="nodeBirthMetrics">Optional metrics to include in the initial NBIRTH certificate. Can be null or empty.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task that completes when the Edge Node is started.</returns>
    public async Task StartAsync(
        ConnectOptions? connectOptions = null,
        IEnumerable<Payload.Types.Metric>? nodeBirthMetrics = null,
        CancellationToken cancellationToken = default)
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

            var ncmdTopic = SparkplugTopic.NodeCommand(this._options.GroupId!, this._options.EdgeNodeId!, this._options.SparkplugNamespace);
            var dcmdFilter = $"{this._options.SparkplugNamespace}/{this._options.GroupId}/DCMD/{this._options.EdgeNodeId}/#";

            var subOptions = new SubscribeOptions();
            subOptions.TopicFilters.Add(new TopicFilter(ncmdTopic.Build(), QualityOfService.AtLeastOnceDelivery));
            subOptions.TopicFilters.Add(new TopicFilter(dcmdFilter, QualityOfService.AtLeastOnceDelivery));
            var subscribeResult = await this._client.SubscribeAsync(subOptions).ConfigureAwait(false);

            var firstSub = subscribeResult.GetFirstSubscription();
            var reasonCode = firstSub?.SubscribeReasonCode;
            var granted = reasonCode is SubAckReasonCode.GrantedQoS0 or SubAckReasonCode.GrantedQoS1 or SubAckReasonCode.GrantedQoS2;
            if (!granted)
            {
                this._client.OnMessageReceived -= this.OnClientMessageReceived;
                throw new InvalidOperationException($"Subscribe failed: {reasonCode}.");
            }

            this._sequenceNumber = 0;
            var birthPayload = SparkplugPayloadEncoder.CreatePayload(SparkplugPayloadEncoder.GetCurrentTimestamp(), 0);
            if (nodeBirthMetrics != null)
            {
                foreach (var m in nodeBirthMetrics)
                {
                    birthPayload.Metrics.Add(m);
                }
            }

            await this.PublishPayloadAsync(SparkplugTopic.NodeBirth(this._options.GroupId!, this._options.EdgeNodeId!, this._options.SparkplugNamespace), birthPayload, cancellationToken).ConfigureAwait(false);
            this._sequenceNumber = SparkplugPayloadEncoder.NextSequenceNumber(this._sequenceNumber);

            this._started = true;
        }
        finally
        {
            this._startStopLock.Release();
        }
    }

    /// <summary>
    /// Publishes Node Death (NDEATH), unsubscribes from command handling, and disconnects if this instance owns the client.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task that completes when the Edge Node is stopped.</returns>
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

            var deathPayload = SparkplugPayloadEncoder.CreatePayload(SparkplugPayloadEncoder.GetCurrentTimestamp(), this._sequenceNumber);
            await this.PublishPayloadAsync(SparkplugTopic.NodeDeath(this._options.GroupId!, this._options.EdgeNodeId!, this._options.SparkplugNamespace), deathPayload, cancellationToken).ConfigureAwait(false);
            this._sequenceNumber = SparkplugPayloadEncoder.NextSequenceNumber(this._sequenceNumber);

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
    /// Publishes Node Birth (NBIRTH). Typically used for rebirth after receiving a Rebirth command.
    /// </summary>
    /// <param name="metrics">The metrics to include in the birth certificate. Can be null or empty.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The result of the publish.</returns>
    public Task<PublishResult> PublishNodeBirthAsync(IEnumerable<Payload.Types.Metric>? metrics, CancellationToken cancellationToken = default)
    {
        var payload = SparkplugPayloadEncoder.CreatePayload(SparkplugPayloadEncoder.GetCurrentTimestamp(), this._sequenceNumber);
        if (metrics != null)
        {
            foreach (var m in metrics)
            {
                payload.Metrics.Add(m);
            }
        }

        var topic = SparkplugTopic.NodeBirth(this._options.GroupId!, this._options.EdgeNodeId!, this._options.SparkplugNamespace);
        return this.PublishPayloadAndAdvanceSequenceAsync(topic, payload, cancellationToken);
    }

    /// <summary>
    /// Publishes Node Data (NDATA).
    /// </summary>
    /// <param name="metrics">The metrics to publish. Must not be null or empty.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The result of the publish.</returns>
    public Task<PublishResult> PublishNodeDataAsync(IEnumerable<Payload.Types.Metric> metrics, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(metrics);
        var payload = SparkplugPayloadEncoder.CreatePayload(SparkplugPayloadEncoder.GetCurrentTimestamp(), this._sequenceNumber);
        foreach (var m in metrics)
        {
            payload.Metrics.Add(m);
        }

        var topic = SparkplugTopic.NodeData(this._options.GroupId!, this._options.EdgeNodeId!, this._options.SparkplugNamespace);
        return this.PublishPayloadAndAdvanceSequenceAsync(topic, payload, cancellationToken);
    }

    /// <summary>
    /// Publishes Node Death (NDEATH). Prefer <see cref="StopAsync"/> for graceful shutdown, which publishes NDEATH automatically.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The result of the publish.</returns>
    public Task<PublishResult> PublishNodeDeathAsync(CancellationToken cancellationToken = default)
    {
        var payload = SparkplugPayloadEncoder.CreatePayload(SparkplugPayloadEncoder.GetCurrentTimestamp(), this._sequenceNumber);
        var topic = SparkplugTopic.NodeDeath(this._options.GroupId!, this._options.EdgeNodeId!, this._options.SparkplugNamespace);
        return this.PublishPayloadAndAdvanceSequenceAsync(topic, payload, cancellationToken);
    }

    /// <summary>
    /// Publishes Device Birth (DBIRTH).
    /// </summary>
    /// <param name="deviceId">The Device ID. Must not be null or empty.</param>
    /// <param name="metrics">The metrics to include in the device birth. Can be null or empty.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The result of the publish.</returns>
    public Task<PublishResult> PublishDeviceBirthAsync(string deviceId, IEnumerable<Payload.Types.Metric>? metrics, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(deviceId))
        {
            throw new ArgumentException("Device ID cannot be null or empty.", nameof(deviceId));
        }

        var payload = SparkplugPayloadEncoder.CreatePayload(SparkplugPayloadEncoder.GetCurrentTimestamp(), this._sequenceNumber);
        if (metrics != null)
        {
            foreach (var m in metrics)
            {
                payload.Metrics.Add(m);
            }
        }

        var topic = SparkplugTopic.DeviceBirth(this._options.GroupId!, this._options.EdgeNodeId!, deviceId, this._options.SparkplugNamespace);
        return this.PublishPayloadAndAdvanceSequenceAsync(topic, payload, cancellationToken);
    }

    /// <summary>
    /// Publishes Device Data (DDATA).
    /// </summary>
    /// <param name="deviceId">The Device ID. Must not be null or empty.</param>
    /// <param name="metrics">The metrics to publish. Must not be null or empty.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The result of the publish.</returns>
    public Task<PublishResult> PublishDeviceDataAsync(string deviceId, IEnumerable<Payload.Types.Metric> metrics, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(deviceId))
        {
            throw new ArgumentException("Device ID cannot be null or empty.", nameof(deviceId));
        }

        ArgumentNullException.ThrowIfNull(metrics);
        var payload = SparkplugPayloadEncoder.CreatePayload(SparkplugPayloadEncoder.GetCurrentTimestamp(), this._sequenceNumber);
        foreach (var m in metrics)
        {
            payload.Metrics.Add(m);
        }

        var topic = SparkplugTopic.DeviceData(this._options.GroupId!, this._options.EdgeNodeId!, deviceId, this._options.SparkplugNamespace);
        return this.PublishPayloadAndAdvanceSequenceAsync(topic, payload, cancellationToken);
    }

    /// <summary>
    /// Publishes Device Death (DDEATH).
    /// </summary>
    /// <param name="deviceId">The Device ID. Must not be null or empty.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The result of the publish.</returns>
    public Task<PublishResult> PublishDeviceDeathAsync(string deviceId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(deviceId))
        {
            throw new ArgumentException("Device ID cannot be null or empty.", nameof(deviceId));
        }

        var payload = SparkplugPayloadEncoder.CreatePayload(SparkplugPayloadEncoder.GetCurrentTimestamp(), this._sequenceNumber);
        var topic = SparkplugTopic.DeviceDeath(this._options.GroupId!, this._options.EdgeNodeId!, deviceId, this._options.SparkplugNamespace);
        return this.PublishPayloadAndAdvanceSequenceAsync(topic, payload, cancellationToken);
    }

    /// <summary>
    /// Disposes the Edge Node and the underlying client if this instance owns it.
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

        if (sparkplugTopic.GroupId != this._options.GroupId || sparkplugTopic.EdgeNodeId != this._options.EdgeNodeId)
        {
            return;
        }

        if (sparkplugTopic.MessageType == SparkplugMessageType.NCMD)
        {
            this.HandleCommand(topicStr, e.PublishMessage.Payload, sparkplugTopic);
            return;
        }

        if (sparkplugTopic.MessageType == SparkplugMessageType.DCMD)
        {
            this.HandleCommand(topicStr, e.PublishMessage.Payload, sparkplugTopic);
        }
    }

    private void HandleCommand(string rawTopic, byte[]? payloadBytes, SparkplugTopic topic)
    {
        if (payloadBytes is null || payloadBytes.Length == 0)
        {
            this.MessageParseError?.Invoke(this, new SparkplugMessageParseErrorEventArgs(rawTopic, null, "Empty payload."));
            return;
        }

        if (!SparkplugPayloadEncoder.TryDecode(payloadBytes, out var payload))
        {
            this.MessageParseError?.Invoke(this, new SparkplugMessageParseErrorEventArgs(rawTopic, payloadBytes, "Payload is not valid Sparkplug B protobuf."));
            return;
        }

        var args = new SparkplugMessageReceivedEventArgs(topic, payload!, rawTopic);
        if (topic.MessageType == SparkplugMessageType.NCMD)
        {
            this.NodeCommandReceived?.Invoke(this, args);
        }
        else
        {
            this.DeviceCommandReceived?.Invoke(this, args);
        }
    }

    private Task<PublishResult> PublishPayloadAsync(SparkplugTopic topic, Payload payload, CancellationToken cancellationToken)
    {
        var bytes = SparkplugPayloadEncoder.Encode(payload);
        var message = new MQTT5PublishMessage
        {
            Topic = topic.Build(),
            Payload = bytes,
            QoS = QualityOfService.AtLeastOnceDelivery,
            Retain = false,
        };
        return this._client.PublishAsync(message, cancellationToken);
    }

    private async Task<PublishResult> PublishPayloadAndAdvanceSequenceAsync(SparkplugTopic topic, Payload payload, CancellationToken cancellationToken)
    {
        var result = await this.PublishPayloadAsync(topic, payload, cancellationToken).ConfigureAwait(false);
        this._sequenceNumber = SparkplugPayloadEncoder.NextSequenceNumber(this._sequenceNumber);
        return result;
    }
}
