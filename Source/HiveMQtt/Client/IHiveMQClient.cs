/*
 * Copyright 2022-present HiveMQ and the HiveMQ Community
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

using HiveMQtt.Client.Exceptions;
using HiveMQtt.Client.Options;
using HiveMQtt.Client.Results;
using HiveMQtt.MQTT5.Types;

/// <summary>
/// The HiveMQClient interface.
/// </summary>
public interface IHiveMQClient : IDisposable
{
    /// <summary>
    /// Gets the local store for the client.
    /// <para>
    /// The LocalStore is a Dictionary(string, string) that can be used
    /// to store data that is specific to this HiveMQClient.
    /// </para>
    /// </summary>
    Dictionary<string, string> LocalStore { get; }

    /// <summary>
    /// Gets or sets the options for this client.
    /// </summary>
    HiveMQClientOptions Options { get; set; }

    /// <summary>
    /// Gets the list of subscriptions for this client.
    /// </summary>
    List<Subscription> Subscriptions { get; }

    /// <summary>
    /// Indicates if the client is currently connected to the MQTT broker.
    /// </summary>
    /// <returns>True if connected, false otherwise.</returns>
    bool IsConnected();

    /// <summary>
    /// Asynchronously makes a TCP connection to the remote specified in HiveMQClientOptions and then
    /// proceeds to make an MQTT Connect request.
    /// </summary>
    /// <returns>A ConnectResult class representing the result of the MQTT connect call.</returns>
    Task<ConnectResult> ConnectAsync();

    /// <summary>
    /// Asynchronous disconnect from the previously connected MQTT broker.
    /// </summary>
    /// <param name="options">The options for the MQTT Disconnect call.</param>
    /// <returns>A boolean indicating on success or failure.</returns>
    Task<bool> DisconnectAsync(DisconnectOptions options);

    /// <summary>
    /// Publish a message to an MQTT topic.
    /// </summary>
    /// <param name="message">The <seealso cref="MQTT5PublishMessage"/> for the Publish.</param>
    /// <returns>A <seealso cref="PublishResult"/> representing the result of the publish operation.</returns>
    Task<PublishResult> PublishAsync(MQTT5PublishMessage message);

    /// <summary>
    /// Publish a message to an MQTT topic.
    /// <para>
    /// This is a convenience method that routes to <seealso cref="PublishAsync(MQTT5PublishMessage)"/>.
    /// </para>
    /// </summary>
    /// <param name="topic">The string topic to publish to.</param>
    /// <param name="payload">The string message to publish.</param>
    /// <param name="qos">The <seealso cref="QualityOfService"/> to use for the publish.</param>
    /// <returns>A <seealso cref="PublishResult"/> representing the result of the publish operation.</returns>
    Task<PublishResult> PublishAsync(string topic, string payload, QualityOfService qos = QualityOfService.AtMostOnceDelivery);

    /// <summary>
    /// Publish a message to an MQTT topic.
    /// <para>
    /// This is a convenience method that routes to <seealso cref="PublishAsync(MQTT5PublishMessage)"/>.
    /// </para>
    /// </summary>
    /// <param name="topic">The string topic to publish to.</param>
    /// <param name="payload">The UTF-8 encoded array of bytes to publish.</param>
    /// <param name="qos">The <seealso cref="QualityOfService"/> to use for the publish.</param>
    /// <returns>A <seealso cref="PublishResult"/> representing the result of the publish operation.</returns>
    Task<PublishResult> PublishAsync(string topic, byte[] payload, QualityOfService qos = QualityOfService.AtMostOnceDelivery);

    /// <summary>
    /// Subscribe with a single topic filter on the MQTT broker.
    /// </summary>
    /// <example>
    /// Usage example:
    /// <code>
    /// var result = await client.SubscribeAsync("my/topic", QualityOfService.AtLeastOnceDelivery);
    /// </code>
    /// </example>
    /// <param name="topic">The topic filter to subscribe to.</param>
    /// <param name="qos">The <seealso cref="QualityOfService">QualityOfService</seealso> level to subscribe with.</param>
    /// <param name="noLocal">A boolean indicating whether this client will receive the messages published by this client.</param>
    /// <param name="retainAsPublished">A boolean indicating whether Application Messages forwarded using this subscription keep the RETAIN flag they were published with.</param>
    /// <param name="retainHandling">A RetainHandling value indicating whether retained messages are sent when the subscription is established.</param>
    /// <returns>SubscribeResult reflecting the result of the operation.</returns>
    Task<SubscribeResult> SubscribeAsync(string topic, QualityOfService qos = QualityOfService.AtMostOnceDelivery, bool noLocal = false, bool retainAsPublished = false, RetainHandling retainHandling = RetainHandling.SendAtSubscribe);

    /// <summary>
    /// Subscribe with SubscribeOptions on the MQTT broker.
    /// </summary>
    /// <example>
    /// Usage example:
    /// <code>
    /// var options = new SubscribeOptions();
    /// options.TopicFilters.Add(new TopicFilter { Topic = "foo", QoS = QualityOfService.AtLeastOnceDelivery });
    /// options.TopicFilters.Add(new TopicFilter { Topic = "bar", QoS = QualityOfService.AtMostOnceDelivery });
    /// var result = await client.SubscribeAsync(options);
    /// </code>
    /// </example>
    /// <param name="options">The options for the subscribe request.</param>
    /// <returns>SubscribeResult reflecting the result of the operation.</returns>
    Task<SubscribeResult> SubscribeAsync(SubscribeOptions options);

    /// <summary>
    /// Unsubscribe from a single topic filter on the MQTT broker.
    /// </summary>
    /// <exception cref="HiveMQttClientException">Thrown if no subscription is found for the topic.</exception>
    /// <param name="topic">The topic filter to unsubscribe from.</param>
    /// <returns>UnsubscribeResult reflecting the result of the operation.</returns>
    Task<UnsubscribeResult> UnsubscribeAsync(string topic);

    /// <summary>
    /// Unsubscribe from a single topic filter on the MQTT broker.
    /// </summary>
    /// <exception cref="HiveMQttClientException">Thrown if no subscription is found for the topic.</exception>
    /// <param name="subscription">The subscription from client.Subscriptions to unsubscribe from.</param>
    /// <returns>UnsubscribeResult reflecting the result of the operation.</returns>
    Task<UnsubscribeResult> UnsubscribeAsync(Subscription subscription);

    /// <summary>
    /// Unsubscribe from a single topic filter on the MQTT broker.
    /// </summary>
    /// <exception cref="HiveMQttClientException">Thrown if no subscription is found for the topic.</exception>
    /// <param name="subscriptions">The subscriptions from client.Subscriptions to unsubscribe from.</param>
    /// <returns>UnsubscribeResult reflecting the result of the operation.</returns>
    Task<UnsubscribeResult> UnsubscribeAsync(List<Subscription> subscriptions);
}
