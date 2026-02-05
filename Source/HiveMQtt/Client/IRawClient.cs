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
namespace HiveMQtt.Client;

using System;
using System.Threading.Tasks;
using HiveMQtt.Client.Events;
using HiveMQtt.Client.Exceptions;
using HiveMQtt.Client.Options;
using HiveMQtt.Client.Results;
using HiveMQtt.MQTT5.Types;

/// <summary>
/// A low-level MQTT client interface that provides direct access to MQTT v5 protocol
/// without subscription management features. This client is performance-oriented and
/// does not maintain subscription state.
/// </summary>
public interface IRawClient : IDisposable
{
    /// <summary>
    /// Gets the local store for the client.
    /// <para>
    /// The LocalStore is a Dictionary(string, string) that can be used
    /// to store data that is specific to this RawClient.
    /// </para>
    /// </summary>
    public Dictionary<string, string> LocalStore { get; }

    /// <summary>
    /// Gets or sets the options for this client.
    /// </summary>
    public HiveMQClientOptions Options { get; set; }

    /// <summary>
    /// Indicates if the client is currently connected to the MQTT broker.
    /// </summary>
    /// <returns>True if connected, false otherwise.</returns>
    public bool IsConnected();

    /// <summary>
    /// Asynchronously makes a TCP connection to the remote specified in HiveMQClientOptions and then
    /// proceeds to make an MQTT Connect request.
    /// </summary>
    /// <param name="connectOptions">The connect override options for the MQTT Connect call.  These settings
    /// will override the settings in HiveMQClientOptions.</param>
    /// <returns>A ConnectResult class representing the result of the MQTT connect call.</returns>
    public Task<ConnectResult> ConnectAsync(ConnectOptions? connectOptions = null);

    /// <summary>
    /// Asynchronous disconnect from the previously connected MQTT broker.
    /// </summary>
    /// <param name="options">The options for the MQTT Disconnect call.</param>
    /// <returns>A boolean indicating on success or failure.</returns>
    public Task<bool> DisconnectAsync(DisconnectOptions? options = null);

    /// <summary>
    /// Publish a message to an MQTT topic.
    /// </summary>
    /// <param name="message">The <seealso cref="MQTT5PublishMessage"/> for the Publish.</param>
    /// <param name="cancellationToken">A <seealso cref="CancellationToken"/> to cancel the operation.</param>
    /// <returns>A <seealso cref="PublishResult"/> representing the result of the publish operation.</returns>
    public Task<PublishResult> PublishAsync(MQTT5PublishMessage message, CancellationToken cancellationToken = default);

    /// <summary>
    /// Publish a message to an MQTT topic.
    /// <para>
    /// This is a convenience method that routes to <seealso cref="PublishAsync(MQTT5PublishMessage, CancellationToken)"/>.
    /// </para>
    /// </summary>
    /// <param name="topic">The string topic to publish to.</param>
    /// <param name="payload">The string message to publish.</param>
    /// <param name="qos">The <seealso cref="QualityOfService"/> to use for the publish.</param>
    /// <returns>A <seealso cref="PublishResult"/> representing the result of the publish operation.</returns>
    public Task<PublishResult> PublishAsync(string topic, string payload, QualityOfService qos = QualityOfService.AtMostOnceDelivery);

    /// <summary>
    /// Publish a message to an MQTT topic.
    /// <para>
    /// This is a convenience method that routes to <seealso cref="PublishAsync(MQTT5PublishMessage, CancellationToken)"/>.
    /// </para>
    /// </summary>
    /// <param name="topic">The string topic to publish to.</param>
    /// <param name="payload">The UTF-8 encoded array of bytes to publish.</param>
    /// <param name="qos">The <seealso cref="QualityOfService"/> to use for the publish.</param>
    /// <returns>A <seealso cref="PublishResult"/> representing the result of the publish operation.</returns>
    public Task<PublishResult> PublishAsync(string topic, byte[] payload, QualityOfService qos = QualityOfService.AtMostOnceDelivery);

    /// <summary>
    /// Subscribe with SubscribeOptions on the MQTT broker.
    /// <para>
    /// Note: This client does not maintain subscription state. The subscription is sent to the broker
    /// and the result is returned, but no subscription tracking is performed.
    /// </para>
    /// </summary>
    /// <param name="options">The options for the subscribe request.</param>
    /// <returns>SubscribeResult reflecting the result of the operation.</returns>
    public Task<SubscribeResult> SubscribeAsync(SubscribeOptions options);

    /// <summary>
    /// Subscribe with a single topic filter on the MQTT broker.
    /// <para>
    /// Note: This client does not maintain subscription state. The subscription is sent to the broker
    /// and the result is returned, but no subscription tracking is performed.
    /// </para>
    /// </summary>
    /// <param name="topic">The topic filter to subscribe to.</param>
    /// <param name="qos">The <seealso cref="QualityOfService">QualityOfService</seealso> level to subscribe with.</param>
    /// <param name="noLocal">A boolean indicating whether this client will receive the messages published by this client.</param>
    /// <param name="retainAsPublished">A boolean indicating whether Application Messages forwarded using this subscription keep the RETAIN flag they were published with.</param>
    /// <param name="retainHandling">A RetainHandling value indicating whether retained messages are sent when the subscription is established.</param>
    /// <returns>SubscribeResult reflecting the result of the operation.</returns>
    public Task<SubscribeResult> SubscribeAsync(string topic, QualityOfService qos = QualityOfService.AtMostOnceDelivery, bool noLocal = false, bool retainAsPublished = false, RetainHandling retainHandling = RetainHandling.SendAtSubscribe);

    /// <summary>
    /// Unsubscribe from topic filters on the MQTT broker.
    /// <para>
    /// Note: This client does not maintain subscription state. The unsubscribe request is sent to the broker
    /// and the result is returned, but no subscription tracking is performed.
    /// </para>
    /// </summary>
    /// <param name="options">The options for the unsubscribe request.</param>
    /// <returns>UnsubscribeResult reflecting the result of the operation.</returns>
    public Task<UnsubscribeResult> UnsubscribeAsync(UnsubscribeOptions options);

    /// <summary>
    /// Acknowledge a received QoS 1 or QoS 2 publish by sending PubAck or PubRec to the broker.
    /// Only valid when ManualAckEnabled is true. Use the packet identifier from OnMessageReceivedEventArgs.PacketIdentifier.
    /// </summary>
    /// <param name="packetIdentifier">The packet identifier of the received publish to acknowledge.</param>
    /// <exception cref="HiveMQttClientException">Thrown when manual ack is not enabled or no pending incoming publish exists for the packet identifier.</exception>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public Task AckAsync(ushort packetIdentifier);

    /// <summary>
    /// Event that is fired before the client connects to the broker.
    /// </summary>
    public event EventHandler<BeforeConnectEventArgs>? BeforeConnect;

    /// <summary>
    /// Event that is fired after the client successfully connects to the broker.
    /// </summary>
    public event EventHandler<AfterConnectEventArgs>? AfterConnect;

    /// <summary>
    /// Event that is fired before the client disconnects from the broker.
    /// </summary>
    public event EventHandler<BeforeDisconnectEventArgs>? BeforeDisconnect;

    /// <summary>
    /// Event that is fired after the client is disconnected from the broker.
    /// </summary>
    public event EventHandler<AfterDisconnectEventArgs>? AfterDisconnect;

    /// <summary>
    /// Event that is fired when a message is received from the broker.
    /// <para>
    /// Note: This event fires for all received PUBLISH packets. No subscription matching is performed.
    /// </para>
    /// </summary>
    public event EventHandler<OnMessageReceivedEventArgs>? OnMessageReceived;

    /// <summary>
    /// Event that is fired after the client sends a CONNECT packet to the broker.
    /// </summary>
    public event EventHandler<OnConnectSentEventArgs>? OnConnectSent;

    /// <summary>
    /// Event that is fired after the client receives a CONNACK packet from the broker.
    /// </summary>
    public event EventHandler<OnConnAckReceivedEventArgs>? OnConnAckReceived;

    /// <summary>
    /// Event that is fired after the client sends a DISCONNECT packet to the broker.
    /// </summary>
    public event EventHandler<OnDisconnectSentEventArgs>? OnDisconnectSent;

    /// <summary>
    /// Event that is fired after the client receives a DISCONNECT packet from the broker.
    /// </summary>
    public event EventHandler<OnDisconnectReceivedEventArgs>? OnDisconnectReceived;

    /// <summary>
    /// Event that is fired after the client sends a PINGREQ packet to the broker.
    /// </summary>
    public event EventHandler<OnPingReqSentEventArgs>? OnPingReqSent;

    /// <summary>
    /// Event that is fired after the client receives a PINGRESP packet from the broker.
    /// </summary>
    public event EventHandler<OnPingRespReceivedEventArgs>? OnPingRespReceived;

    /// <summary>
    /// Event that is fired after the client sends a SUBSCRIBE packet to the broker.
    /// </summary>
    public event EventHandler<OnSubscribeSentEventArgs>? OnSubscribeSent;

    /// <summary>
    /// Event that is fired after the client receives a SUBACK packet from the broker.
    /// </summary>
    public event EventHandler<OnSubAckReceivedEventArgs>? OnSubAckReceived;

    /// <summary>
    /// Event that is fired after the client sends an UNSUBSCRIBE packet to the broker.
    /// </summary>
    public event EventHandler<OnUnsubscribeSentEventArgs>? OnUnsubscribeSent;

    /// <summary>
    /// Event that is fired after the client receives an UNSUBACK packet from the broker.
    /// </summary>
    public event EventHandler<OnUnsubAckReceivedEventArgs>? OnUnsubAckReceived;

    /// <summary>
    /// Event that is fired after the client receives a PUBLISH packet from the broker.
    /// </summary>
    public event EventHandler<OnPublishReceivedEventArgs>? OnPublishReceived;

    /// <summary>
    /// Event that is fired after the client sends a PUBLISH packet to the broker.
    /// </summary>
    public event EventHandler<OnPublishSentEventArgs>? OnPublishSent;

    /// <summary>
    /// Event that is fired after the client receives a PUBACK packet from the broker.
    /// </summary>
    public event EventHandler<OnPubAckReceivedEventArgs>? OnPubAckReceived;

    /// <summary>
    /// Event that is fired after the client sends a PUBACK packet to the broker.
    /// </summary>
    public event EventHandler<OnPubAckSentEventArgs>? OnPubAckSent;

    /// <summary>
    /// Event that is fired after the client receives a PUBREC packet from the broker.
    /// </summary>
    public event EventHandler<OnPubRecReceivedEventArgs>? OnPubRecReceived;

    /// <summary>
    /// Event that is fired after the client sends a PUBREC packet to the broker.
    /// </summary>
    public event EventHandler<OnPubRecSentEventArgs>? OnPubRecSent;

    /// <summary>
    /// Event that is fired after the client receives a PUBREL packet from the broker.
    /// </summary>
    public event EventHandler<OnPubRelReceivedEventArgs>? OnPubRelReceived;

    /// <summary>
    /// Event that is fired after the client sends a PUBREL packet to the broker.
    /// </summary>
    public event EventHandler<OnPubRelSentEventArgs>? OnPubRelSent;

    /// <summary>
    /// Event that is fired after the client receives a PUBCOMP packet from the broker.
    /// </summary>
    public event EventHandler<OnPubCompReceivedEventArgs>? OnPubCompReceived;

    /// <summary>
    /// Event that is fired after the client sends a PUBCOMP packet to the broker.
    /// </summary>
    public event EventHandler<OnPubCompSentEventArgs>? OnPubCompSent;
}
