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
namespace HiveMQtt.Client.Internal;

using System.Threading.Tasks;
using HiveMQtt.Client.Options;
using HiveMQtt.MQTT5.Packets;
using HiveMQtt.MQTT5.Types;

/// <summary>
/// Internal interface used by ConnectionManager to interact with MQTT clients.
/// This interface abstracts the common functionality needed by ConnectionManager
/// from both HiveMQClient and RawClient.
/// </summary>
internal interface IBaseMQTTClient
{
    /// <summary>
    /// Gets the options for this client.
    /// </summary>
    public HiveMQClientOptions Options { get; }

    /// <summary>
    /// Updates the cached connection properties for fast access during publish operations.
    /// </summary>
    /// <param name="properties">The connection properties to cache.</param>
    public void UpdateConnectionPropertyCache(MQTT5Properties? properties);

    /// <summary>
    /// Clears all tracked subscriptions. This is a no-op for RawClient.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public Task ClearSubscriptionsAsync();

    /// <summary>
    /// Disconnects from the broker.
    /// </summary>
    /// <param name="options">The disconnect options.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public Task<bool> DisconnectAsync(DisconnectOptions? options = null);

    // Event launcher methods
    public void OnConnAckReceivedEventLauncher(ConnAckPacket packet);

    public void OnDisconnectReceivedEventLauncher(DisconnectPacket packet);

    public void OnPublishReceivedEventLauncher(PublishPacket packet);

    public void OnMessageReceivedEventLauncher(PublishPacket packet);

    public void OnPubAckReceivedEventLauncher(PubAckPacket packet);

    public void OnPubRecReceivedEventLauncher(PubRecPacket packet);

    public void OnPubRelReceivedEventLauncher(PubRelPacket packet);

    public void OnPubCompReceivedEventLauncher(PubCompPacket packet);

    public void OnSubAckReceivedEventLauncher(SubAckPacket packet);

    public void OnUnsubAckReceivedEventLauncher(UnsubAckPacket packet);

    public void OnPingRespReceivedEventLauncher(PingRespPacket packet);

    public void OnConnectSentEventLauncher(ConnectPacket packet);

    public void OnDisconnectSentEventLauncher(DisconnectPacket packet);

    public void OnPublishSentEventLauncher(PublishPacket packet);

    public void OnPubAckSentEventLauncher(PubAckPacket packet);

    public void OnPubRecSentEventLauncher(PubRecPacket packet);

    public void OnPubRelSentEventLauncher(PubRelPacket packet);

    public void OnPubCompSentEventLauncher(PubCompPacket packet);

    public void OnSubscribeSentEventLauncher(SubscribePacket packet);

    public void OnUnsubscribeSentEventLauncher(UnsubscribePacket packet);

    public void OnPingReqSentEventLauncher(PingReqPacket packet);

    public void AfterDisconnectEventLauncher(bool clean = false);
}
