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

using HiveMQtt.MQTT5.Types;

/// <summary>
/// Event arguments for the <see cref="HiveMQClient.OnMessageReceived"/> event.
/// <para>This event is called when a message is received from the broker.</para>
/// <para><see cref="OnMessageReceivedEventArgs.PublishMessage"/> contains the received message.</para>
/// </summary>
public class OnMessageReceivedEventArgs : EventArgs
{
    public OnMessageReceivedEventArgs(MQTT5PublishMessage message) => this.PublishMessage = message;

    public MQTT5PublishMessage PublishMessage { get; set; }
}
