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
using System.Linq;
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
using HiveMQtt.Sparkplug.Validation;

/// <summary>
/// Sparkplug B Edge Node: publishes NBIRTH/NDATA/NDEATH, DBIRTH/DDATA/DDEATH; receives NCMD/DCMD; manages sequence and lifecycle.
/// </summary>
public sealed class SparkplugEdgeNode : IDisposable
{
    private readonly IHiveMQClient client;
    private readonly SparkplugEdgeNodeOptions options;
    private readonly bool ownsClient;
    private readonly SemaphoreSlim startStopLock = new(1, 1);
    private readonly SemaphoreSlim publishSequenceLock = new(1, 1);
    private int sequenceNumber;
    private ulong bdSeq;
    private ulong? currentSessionBdSeq;
    private bool started;
    private bool disposed;
    private TaskCompletionSource<bool>? primaryHostOnlineWait;
    private long? lastPrimaryHostOnlineTimestamp;
    private bool isPrimaryHostOnline;
    private int primaryHostOfflineShutdownQueued;

    /// <summary>
    /// Initializes a new instance of the <see cref="SparkplugEdgeNode"/> class using an existing MQTT client.
    /// </summary>
    /// <param name="client">The MQTT client to use. Must not be null.</param>
    /// <param name="options">Edge Node options. Must not be null.</param>
    /// <exception cref="ArgumentNullException">Thrown when client or options is null.</exception>
    public SparkplugEdgeNode(IHiveMQClient client, SparkplugEdgeNodeOptions options)
    {
        this.client = client ?? throw new ArgumentNullException(nameof(client));
        this.options = options ?? throw new ArgumentNullException(nameof(options));
        this.ownsClient = false;
        this.options.Validate();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SparkplugEdgeNode"/> class, creating and owning the MQTT client.
    /// NDEATH LWT is configured in <see cref="StartAsync"/> (with the session bdSeq) when <see cref="SparkplugEdgeNodeOptions.UseDeathLwt"/> is true and no LWT is already set.
    /// </summary>
    /// <param name="clientOptions">The MQTT client options. Will be used to create an internal HiveMQClient.</param>
    /// <param name="sparkplugOptions">Edge Node options. Must not be null.</param>
    /// <exception cref="ArgumentNullException">Thrown when clientOptions or sparkplugOptions is null.</exception>
    public SparkplugEdgeNode(HiveMQClientOptions clientOptions, SparkplugEdgeNodeOptions sparkplugOptions)
    {
        ArgumentNullException.ThrowIfNull(clientOptions);
        sparkplugOptions?.Validate();
        this.options = sparkplugOptions ?? throw new ArgumentNullException(nameof(sparkplugOptions));
        this.client = new HiveMQClient(clientOptions);
        this.ownsClient = true;
    }

    private static (string Topic, byte[] Payload) BuildDeathLwtMessage(SparkplugEdgeNodeOptions options, ulong bdSeq)
    {
        var topic = SparkplugTopic.NodeDeath(options.GroupId!, options.EdgeNodeId!, options.SparkplugNamespace).Build();
        var payload = SparkplugPayloadEncoder.CreatePayload(timestamp: 0, sequenceNumber: 0);
        payload.Metrics.Insert(0, SparkplugPayloadEncoder.CreateBdSeqMetric(bdSeq));
        return (topic, SparkplugPayloadEncoder.Encode(payload));
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
    /// Event raised when a Primary Host Application STATE message is received for the configured
    /// <see cref="SparkplugEdgeNodeOptions.PrimaryHostApplicationId"/>.
    /// </summary>
    public event EventHandler<SparkplugMessageReceivedEventArgs>? StateMessageReceived;

    /// <summary>
    /// Event raised when a command or STATE message is received but could not be parsed.
    /// </summary>
    public event EventHandler<SparkplugMessageParseErrorEventArgs>? MessageParseError;

    /// <summary>
    /// Gets the underlying MQTT client.
    /// </summary>
    public IHiveMQClient Client => this.client;

    /// <summary>
    /// Gets the Edge Node options.
    /// </summary>
    public SparkplugEdgeNodeOptions Options => this.options;

    /// <summary>
    /// Gets a value indicating whether the Edge Node is connected and started.
    /// </summary>
    public bool IsConnected => this.started && this.client.IsConnected();

    /// <summary>
    /// Gets the current sequence number (0-255). Incremented after each publish.
    /// </summary>
    public int SequenceNumber => this.sequenceNumber;

    /// <summary>
    /// Gets the birth/death sequence (bdSeq) for the current session, or null if not started. Set when <see cref="StartAsync"/> runs; used in NBIRTH, NDEATH, DBIRTH, and DDEATH payloads.
    /// </summary>
    public ulong? CurrentSessionBdSeq => this.currentSessionBdSeq;

    /// <summary>
    /// Gets a value indicating whether the configured Primary Host Application is currently considered online.
    /// Always false when <see cref="SparkplugEdgeNodeOptions.PrimaryHostApplicationId"/> is not set.
    /// </summary>
    public bool IsPrimaryHostOnline => this.isPrimaryHostOnline;

    private bool UsesPrimaryHost => !string.IsNullOrWhiteSpace(this.options.PrimaryHostApplicationId);

    /// <summary>
    /// Connects the client, subscribes to NCMD and DCMD topics for this node, and publishes Node Birth (NBIRTH).
    /// When <see cref="SparkplugEdgeNodeOptions.PrimaryHostApplicationId"/> is set, also subscribes to that Host's STATE
    /// topic and waits for an online STATE (including a retained one) before publishing NBIRTH. If the Primary Host later
    /// goes offline with a valid timestamp, the Edge Node publishes NDEATH and disconnects; call <see cref="StartAsync"/>
    /// again after reconnect to resume (single-broker Primary Host support).
    /// After a reconnect (e.g. following connection loss and automatic or manual reconnection), call <see cref="StartAsync"/> again
    /// to publish a new NBIRTH (re-birth) and resume command subscriptions; the underlying client must be connected before calling.
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
        var connectedByThisStart = false;
        await this.startStopLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (this.started)
            {
                return;
            }

            var sessionBdSeq = this.bdSeq;
            this.bdSeq++;
            Interlocked.Exchange(ref this.primaryHostOfflineShutdownQueued, 0);

            if (this.ownsClient && this.options.UseDeathLwt && this.client.Options is { } opts && opts.LastWillAndTestament is null && !string.IsNullOrWhiteSpace(this.options.GroupId) && !string.IsNullOrWhiteSpace(this.options.EdgeNodeId))
            {
                var (topic, payload) = BuildDeathLwtMessage(this.options, sessionBdSeq);
                opts.LastWillAndTestament = new LastWillAndTestament(topic, payload, QualityOfService.AtLeastOnceDelivery, retain: false);
            }

            if (!this.client.IsConnected())
            {
                var connectResult = await this.client.ConnectAsync(connectOptions).ConfigureAwait(false);
                if (connectResult.ReasonCode != ConnAckReasonCode.Success)
                {
                    throw new InvalidOperationException($"Connect failed: {connectResult.ReasonString ?? connectResult.ReasonCode.ToString()}.");
                }

                connectedByThisStart = true;
            }

            this.client.OnMessageReceived += this.OnClientMessageReceived;

            var ncmdTopic = SparkplugTopic.NodeCommand(this.options.GroupId!, this.options.EdgeNodeId!, this.options.SparkplugNamespace);
            var dcmdFilter = $"{this.options.SparkplugNamespace}/{this.options.GroupId}/DCMD/{this.options.EdgeNodeId}/#";

            var subOptions = new SubscribeOptions();
            subOptions.TopicFilters.Add(new TopicFilter(ncmdTopic.Build(), QualityOfService.AtLeastOnceDelivery));
            subOptions.TopicFilters.Add(new TopicFilter(dcmdFilter, QualityOfService.AtLeastOnceDelivery));

            TaskCompletionSource<bool>? onlineWait = null;
            if (this.UsesPrimaryHost)
            {
                var stateTopic = $"{this.options.SparkplugNamespace}/STATE/{this.options.PrimaryHostApplicationId}";
                subOptions.TopicFilters.Add(new TopicFilter(stateTopic, QualityOfService.AtLeastOnceDelivery));
                onlineWait = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
                this.primaryHostOnlineWait = onlineWait;
                this.isPrimaryHostOnline = false;
            }

            var subscribeResult = await this.client.SubscribeAsync(subOptions).ConfigureAwait(false);

            if (subscribeResult.Subscriptions.Count != subOptions.TopicFilters.Count)
            {
                throw new InvalidOperationException($"Subscribe failed: expected {subOptions.TopicFilters.Count} subscription results, got {subscribeResult.Subscriptions.Count}.");
            }

            for (var i = 0; i < subscribeResult.Subscriptions.Count; i++)
            {
                var reasonCode = subscribeResult.Subscriptions[i].SubscribeReasonCode;
                var granted = reasonCode is SubAckReasonCode.GrantedQoS0 or SubAckReasonCode.GrantedQoS1 or SubAckReasonCode.GrantedQoS2;
                if (!granted)
                {
                    throw new InvalidOperationException($"Subscribe failed for topic filter index {i}: {reasonCode}.");
                }
            }

            if (onlineWait is not null)
            {
                try
                {
                    await onlineWait.Task.WaitAsync(cancellationToken).ConfigureAwait(false);
                }
                finally
                {
                    if (ReferenceEquals(this.primaryHostOnlineWait, onlineWait))
                    {
                        this.primaryHostOnlineWait = null;
                    }
                }
            }

            await this.publishSequenceLock.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                this.sequenceNumber = 0;
                var birthPayload = SparkplugPayloadEncoder.CreatePayload(SparkplugPayloadEncoder.GetCurrentTimestamp(), 0);
                birthPayload.Metrics.Insert(0, SparkplugPayloadEncoder.CreateBdSeqMetric(sessionBdSeq));

                // Add the required Node Control/Rebirth metric per Sparkplug B 3.0 spec (tck-id-topics-nbirth-rebirth-metric)
                birthPayload.Metrics.Add(
                    SparkplugMetricBuilder.Create(SparkplugPayloadEncoder.NodeControlRebirthMetricName)
                        .WithBooleanValue(false)
                        .Build());

                if (nodeBirthMetrics != null)
                {
                    foreach (var m in nodeBirthMetrics)
                    {
                        birthPayload.Metrics.Add(m);
                    }
                }

                await this.PublishPayloadAsync(SparkplugTopic.NodeBirth(this.options.GroupId!, this.options.EdgeNodeId!, this.options.SparkplugNamespace), birthPayload, cancellationToken).ConfigureAwait(false);
                this.sequenceNumber = SparkplugPayloadEncoder.NextSequenceNumber(this.sequenceNumber);
                this.currentSessionBdSeq = sessionBdSeq;

                this.started = true;
            }
            finally
            {
                this.publishSequenceLock.Release();
            }
        }
        catch
        {
            // If startup fails after wiring, always unwind the handler to avoid duplicate command processing on retry.
            this.client.OnMessageReceived -= this.OnClientMessageReceived;
            this.primaryHostOnlineWait = null;
            this.isPrimaryHostOnline = false;

            // If this instance created the connection for this startup attempt, best-effort disconnect on failure.
            if (connectedByThisStart && this.ownsClient && this.client.IsConnected())
            {
                try
                {
                    await this.client.DisconnectAsync().ConfigureAwait(false);
                }
                catch
                {
                    // Preserve the original startup exception.
                }
            }

            throw;
        }
        finally
        {
            this.startStopLock.Release();
        }
    }

    /// <summary>
    /// Publishes Node Death (NDEATH), unsubscribes from command handling, and disconnects if this instance owns the client.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task that completes when the Edge Node is stopped.</returns>
    public async Task StopAsync(CancellationToken cancellationToken = default)
    {
        await this.startStopLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (!this.started)
            {
                return;
            }

            this.client.OnMessageReceived -= this.OnClientMessageReceived;

            await this.publishSequenceLock.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                var deathPayload = SparkplugPayloadEncoder.CreatePayload(SparkplugPayloadEncoder.GetCurrentTimestamp(), this.sequenceNumber);
                if (this.currentSessionBdSeq.HasValue)
                {
                    deathPayload.Metrics.Insert(0, SparkplugPayloadEncoder.CreateBdSeqMetric(this.currentSessionBdSeq.Value));
                }

                await this.PublishPayloadAsync(SparkplugTopic.NodeDeath(this.options.GroupId!, this.options.EdgeNodeId!, this.options.SparkplugNamespace), deathPayload, cancellationToken).ConfigureAwait(false);
                this.sequenceNumber = SparkplugPayloadEncoder.NextSequenceNumber(this.sequenceNumber);
                this.currentSessionBdSeq = null;
            }
            finally
            {
                this.publishSequenceLock.Release();
            }

            if (this.ownsClient)
            {
                await this.client.DisconnectAsync().ConfigureAwait(false);
            }

            this.started = false;
        }
        finally
        {
            this.startStopLock.Release();
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
        var topic = SparkplugTopic.NodeBirth(this.options.GroupId!, this.options.EdgeNodeId!, this.options.SparkplugNamespace);
        return this.PublishPayloadAndAdvanceSequenceAsync(
            topic,
            seq =>
            {
                var payload = SparkplugPayloadEncoder.CreatePayload(SparkplugPayloadEncoder.GetCurrentTimestamp(), seq);
                if (this.currentSessionBdSeq.HasValue)
                {
                    payload.Metrics.Insert(0, SparkplugPayloadEncoder.CreateBdSeqMetric(this.currentSessionBdSeq.Value));
                }

                // Add the required Node Control/Rebirth metric per Sparkplug B 3.0 spec (tck-id-topics-nbirth-rebirth-metric)
                payload.Metrics.Add(
                    SparkplugMetricBuilder.Create(SparkplugPayloadEncoder.NodeControlRebirthMetricName)
                        .WithBooleanValue(false)
                        .Build());

                if (metrics != null)
                {
                    foreach (var m in metrics)
                    {
                        payload.Metrics.Add(m);
                    }
                }

                return payload;
            },
            cancellationToken);
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
#pragma warning disable IDE0305 // Collection initialization - ToList() is appropriate for .NET 6
        var metricList = metrics as IList<Payload.Types.Metric> ?? metrics.ToList();
#pragma warning restore IDE0305
        if (metricList.Count == 0)
        {
            throw new ArgumentException("At least one metric is required.", nameof(metrics));
        }

        var topic = SparkplugTopic.NodeData(this.options.GroupId!, this.options.EdgeNodeId!, this.options.SparkplugNamespace);
        return this.PublishPayloadAndAdvanceSequenceAsync(
            topic,
            seq =>
            {
                var payload = SparkplugPayloadEncoder.CreatePayload(SparkplugPayloadEncoder.GetCurrentTimestamp(), seq);
                foreach (var m in metricList)
                {
                    payload.Metrics.Add(m);
                }

                return payload;
            },
            cancellationToken);
    }

    /// <summary>
    /// Publishes Node Death (NDEATH). Prefer <see cref="StopAsync"/> for graceful shutdown, which publishes NDEATH automatically.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The result of the publish.</returns>
    public Task<PublishResult> PublishNodeDeathAsync(CancellationToken cancellationToken = default)
    {
        var topic = SparkplugTopic.NodeDeath(this.options.GroupId!, this.options.EdgeNodeId!, this.options.SparkplugNamespace);
        return this.PublishPayloadAndAdvanceSequenceAsync(
            topic,
            seq =>
            {
                var payload = SparkplugPayloadEncoder.CreatePayload(SparkplugPayloadEncoder.GetCurrentTimestamp(), seq);
                if (this.currentSessionBdSeq.HasValue)
                {
                    payload.Metrics.Insert(0, SparkplugPayloadEncoder.CreateBdSeqMetric(this.currentSessionBdSeq.Value));
                }

                return payload;
            },
            cancellationToken);
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
        SparkplugIdValidator.ValidateDeviceId(deviceId, nameof(deviceId), this.options.StrictIdentifierValidation);

        var topic = SparkplugTopic.DeviceBirth(this.options.GroupId!, this.options.EdgeNodeId!, deviceId, this.options.SparkplugNamespace);
        return this.PublishPayloadAndAdvanceSequenceAsync(
            topic,
            seq =>
            {
                var payload = SparkplugPayloadEncoder.CreatePayload(SparkplugPayloadEncoder.GetCurrentTimestamp(), seq);
                if (this.currentSessionBdSeq.HasValue)
                {
                    payload.Metrics.Insert(0, SparkplugPayloadEncoder.CreateBdSeqMetric(this.currentSessionBdSeq.Value));
                }

                if (metrics != null)
                {
                    foreach (var m in metrics)
                    {
                        payload.Metrics.Add(m);
                    }
                }

                return payload;
            },
            cancellationToken);
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
        SparkplugIdValidator.ValidateDeviceId(deviceId, nameof(deviceId), this.options.StrictIdentifierValidation);
        ArgumentNullException.ThrowIfNull(metrics);
#pragma warning disable IDE0305 // Collection initialization - ToList() is appropriate for .NET 6
        var metricList = metrics as IList<Payload.Types.Metric> ?? metrics.ToList();
#pragma warning restore IDE0305
        if (metricList.Count == 0)
        {
            throw new ArgumentException("At least one metric is required.", nameof(metrics));
        }

        var topic = SparkplugTopic.DeviceData(this.options.GroupId!, this.options.EdgeNodeId!, deviceId, this.options.SparkplugNamespace);
        return this.PublishPayloadAndAdvanceSequenceAsync(
            topic,
            seq =>
            {
                var payload = SparkplugPayloadEncoder.CreatePayload(SparkplugPayloadEncoder.GetCurrentTimestamp(), seq);
                foreach (var m in metricList)
                {
                    payload.Metrics.Add(m);
                }

                return payload;
            },
            cancellationToken);
    }

    /// <summary>
    /// Publishes Device Death (DDEATH).
    /// </summary>
    /// <param name="deviceId">The Device ID. Must not be null or empty.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The result of the publish.</returns>
    public Task<PublishResult> PublishDeviceDeathAsync(string deviceId, CancellationToken cancellationToken = default)
    {
        SparkplugIdValidator.ValidateDeviceId(deviceId, nameof(deviceId), this.options.StrictIdentifierValidation);

        var topic = SparkplugTopic.DeviceDeath(this.options.GroupId!, this.options.EdgeNodeId!, deviceId, this.options.SparkplugNamespace);
        return this.PublishPayloadAndAdvanceSequenceAsync(
            topic,
            seq =>
            {
                var payload = SparkplugPayloadEncoder.CreatePayload(SparkplugPayloadEncoder.GetCurrentTimestamp(), seq);
                if (this.currentSessionBdSeq.HasValue)
                {
                    payload.Metrics.Insert(0, SparkplugPayloadEncoder.CreateBdSeqMetric(this.currentSessionBdSeq.Value));
                }

                return payload;
            },
            cancellationToken);
    }

    /// <summary>
    /// Disposes the Edge Node and the underlying client if this instance owns it.
    /// </summary>
    public void Dispose()
    {
        if (this.disposed)
        {
            return;
        }

        if (this.ownsClient && this.client is IDisposable disposable)
        {
            disposable.Dispose();
        }

        this.startStopLock.Dispose();
        this.publishSequenceLock.Dispose();
        this.disposed = true;
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

        if (sparkplugTopic.MessageType == SparkplugMessageType.STATE)
        {
            this.HandleState(topicStr, e.PublishMessage.Payload, sparkplugTopic);
            return;
        }

        if (sparkplugTopic.GroupId != this.options.GroupId || sparkplugTopic.EdgeNodeId != this.options.EdgeNodeId)
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

    private void HandleState(string rawTopic, byte[]? payloadBytes, SparkplugTopic topic)
    {
        if (!this.UsesPrimaryHost)
        {
            return;
        }

        if (!string.Equals(topic.PrimaryHostId, this.options.PrimaryHostApplicationId, StringComparison.Ordinal))
        {
            return;
        }

        if (payloadBytes is null || payloadBytes.Length == 0)
        {
            this.MessageParseError?.Invoke(this, new SparkplugMessageParseErrorEventArgs(rawTopic, null, "Empty STATE payload."));
            return;
        }

        if (!SparkplugStatePayload.TryDecode(payloadBytes, out var statePayload) || statePayload is null)
        {
            this.MessageParseError?.Invoke(
                this,
                new SparkplugMessageParseErrorEventArgs(
                    rawTopic,
                    payloadBytes,
                    "STATE payload is not valid Sparkplug 3.0 JSON (expected { \"online\": true|false, \"timestamp\": <ms> })."));
            return;
        }

        var args = new SparkplugMessageReceivedEventArgs(topic, rawTopic, statePayload);
        this.StateMessageReceived?.Invoke(this, args);

        if (statePayload.Online)
        {
            this.lastPrimaryHostOnlineTimestamp = statePayload.Timestamp;
            this.isPrimaryHostOnline = true;
            this.primaryHostOnlineWait?.TrySetResult(true);
            return;
        }

        // Offline: only act when timestamp is greater than or equal to the last online STATE timestamp.
        if (!this.lastPrimaryHostOnlineTimestamp.HasValue || statePayload.Timestamp < this.lastPrimaryHostOnlineTimestamp.Value)
        {
            return;
        }

        this.isPrimaryHostOnline = false;

        if (!this.started)
        {
            return;
        }

        if (Interlocked.CompareExchange(ref this.primaryHostOfflineShutdownQueued, 1, 0) != 0)
        {
            return;
        }

        // Do not block the MQTT receive path on start/stop lock or network I/O.
        _ = this.TerminateForPrimaryHostOfflineAsync();
    }

    private async Task TerminateForPrimaryHostOfflineAsync()
    {
        try
        {
            await this.startStopLock.WaitAsync().ConfigureAwait(false);
            try
            {
                if (!this.started)
                {
                    return;
                }

                this.client.OnMessageReceived -= this.OnClientMessageReceived;

                try
                {
                    await this.publishSequenceLock.WaitAsync().ConfigureAwait(false);
                    try
                    {
                        var deathPayload = SparkplugPayloadEncoder.CreatePayload(SparkplugPayloadEncoder.GetCurrentTimestamp(), this.sequenceNumber);
                        if (this.currentSessionBdSeq.HasValue)
                        {
                            deathPayload.Metrics.Insert(0, SparkplugPayloadEncoder.CreateBdSeqMetric(this.currentSessionBdSeq.Value));
                        }

                        await this.PublishPayloadAsync(
                            SparkplugTopic.NodeDeath(this.options.GroupId!, this.options.EdgeNodeId!, this.options.SparkplugNamespace),
                            deathPayload,
                            CancellationToken.None).ConfigureAwait(false);
                        this.sequenceNumber = SparkplugPayloadEncoder.NextSequenceNumber(this.sequenceNumber);
                        this.currentSessionBdSeq = null;
                    }
                    finally
                    {
                        this.publishSequenceLock.Release();
                    }
                }
                catch
                {
                    // Best-effort NDEATH; still disconnect and clear started.
                }

                try
                {
                    if (this.client.IsConnected())
                    {
                        await this.client.DisconnectAsync().ConfigureAwait(false);
                    }
                }
                catch
                {
                    // Best-effort disconnect.
                }

                this.started = false;
                this.isPrimaryHostOnline = false;
            }
            finally
            {
                this.startStopLock.Release();
            }
        }
        finally
        {
            Interlocked.Exchange(ref this.primaryHostOfflineShutdownQueued, 0);
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
        return this.client.PublishAsync(message, cancellationToken);
    }

    private async Task<PublishResult> PublishPayloadAndAdvanceSequenceAsync(
        SparkplugTopic topic,
        Func<int, Payload> buildPayload,
        CancellationToken cancellationToken)
    {
        await this.publishSequenceLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            var payload = buildPayload(this.sequenceNumber);
            var result = await this.PublishPayloadAsync(topic, payload, cancellationToken).ConfigureAwait(false);
            this.sequenceNumber = SparkplugPayloadEncoder.NextSequenceNumber(this.sequenceNumber);
            return result;
        }
        finally
        {
            this.publishSequenceLock.Release();
        }
    }
}
