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

/// <summary>
/// Event arguments for the <see cref="HiveMQClient.AfterDisconnect"/> event.
/// <para>
/// This event is called after a disconnect from the MQTT broker.  This can be happen because
/// of a call to <see cref="HiveMQClient.DisconnectAsync"/> or because of a failure.
/// </para>
/// <para>
/// If the disconnect was caused by a call to <see cref="HiveMQClient.DisconnectAsync"/>, then
/// <see cref="AfterDisconnectEventArgs.CleanDisconnect"/> will be <c>true</c>.  If the disconnect
/// was caused by a failure, then <see cref="AfterDisconnectEventArgs.CleanDisconnect"/> will be
/// <c>false</c>.
/// </para>
/// </summary>
public class AfterDisconnectEventArgs : EventArgs
{
    public AfterDisconnectEventArgs(bool clean = false) => this.CleanDisconnect = clean;

    public bool CleanDisconnect { get; set; }
}
