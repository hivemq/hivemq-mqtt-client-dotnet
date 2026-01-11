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
    HiveMQClientOptions Options { get; }

    /// <summary>
    /// Updates the cached connection properties for fast access during publish operations.
    /// </summary>
    /// <param name="properties">The connection properties to cache.</param>
    void UpdateConnectionPropertyCache(MQTT5Properties? properties);

    /// <summary>
    /// Clears all tracked subscriptions. This is a no-op for RawClient.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task ClearSubscriptionsAsync();

    /// <summary>
    /// Disconnects from the broker.
    /// </summary>
    /// <param name="options">The disconnect options.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task<bool> DisconnectAsync(DisconnectOptions? options = null);

    // Event launcher methods
    void OnConnAckReceivedEventLauncher(ConnAckPacket packet);

    void OnDisconnectReceivedEventLauncher(DisconnectPacket packet);

    void OnPublishReceivedEventLauncher(PublishPacket packet);

    void OnMessageReceivedEventLauncher(PublishPacket packet);

    void OnPubAckReceivedEventLauncher(PubAckPacket packet);

    void OnPubRecReceivedEventLauncher(PubRecPacket packet);

    void OnPubRelReceivedEventLauncher(PubRelPacket packet);

    void OnPubCompReceivedEventLauncher(PubCompPacket packet);

    void OnSubAckReceivedEventLauncher(SubAckPacket packet);

    void OnUnsubAckReceivedEventLauncher(UnsubAckPacket packet);

    void OnPingRespReceivedEventLauncher(PingRespPacket packet);

    void OnConnectSentEventLauncher(ConnectPacket packet);

    void OnDisconnectSentEventLauncher(DisconnectPacket packet);

    void OnPublishSentEventLauncher(PublishPacket packet);

    void OnPubAckSentEventLauncher(PubAckPacket packet);

    void OnPubRecSentEventLauncher(PubRecPacket packet);

    void OnPubRelSentEventLauncher(PubRelPacket packet);

    void OnPubCompSentEventLauncher(PubCompPacket packet);

    void OnSubscribeSentEventLauncher(SubscribePacket packet);

    void OnUnsubscribeSentEventLauncher(UnsubscribePacket packet);

    void OnPingReqSentEventLauncher(PingReqPacket packet);

    void AfterDisconnectEventLauncher(bool clean = false);
}
