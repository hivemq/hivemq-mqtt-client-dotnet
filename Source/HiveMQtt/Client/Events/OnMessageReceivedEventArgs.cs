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
namespace HiveMQtt.Client.Events;

using System;
using HiveMQtt.MQTT5.Types;

/// <summary>
/// Event arguments for the <see cref="HiveMQClient.OnMessageReceived"/> event.
/// <para>This event is called when a message is received from the broker.</para>
/// <para><see cref="PublishMessage"/> contains the received message.</para>
/// <para><see cref="PacketIdentifier"/> is set for QoS 1 and QoS 2 messages; use it when calling AckAsync for manual acknowledgement.</para>
/// </summary>
public class OnMessageReceivedEventArgs : EventArgs
{
    /// <summary>
    /// Initializes a new instance of the <see cref="OnMessageReceivedEventArgs"/> class.
    /// </summary>
    /// <param name="message">The received publish message.</param>
    public OnMessageReceivedEventArgs(MQTT5PublishMessage message)
        : this(message, null)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="OnMessageReceivedEventArgs"/> class.
    /// </summary>
    /// <param name="message">The received publish message.</param>
    /// <param name="packetIdentifier">The packet identifier for QoS 1 and QoS 2 messages; null for QoS 0.</param>
    public OnMessageReceivedEventArgs(MQTT5PublishMessage message, ushort? packetIdentifier)
    {
        this.PublishMessage = message;
        this.PacketIdentifier = packetIdentifier;
    }

    /// <summary>
    /// Gets or sets the received publish message.
    /// </summary>
    public MQTT5PublishMessage PublishMessage { get; set; }

    /// <summary>
    /// Gets the packet identifier for QoS 1 and QoS 2 messages, or null for QoS 0.
    /// When manual acknowledgement is enabled, use this value when calling AckAsync.
    /// </summary>
    [CLSCompliant(false)]
    public ushort? PacketIdentifier { get; }
}
