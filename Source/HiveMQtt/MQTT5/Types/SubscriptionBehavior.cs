/*
 * Copyright 2026-present HiveMQ and the HiveMQ Community
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

namespace HiveMQtt.MQTT5.Types;

/// <summary>
/// Defines how the client handles messages that match multiple overlapping subscriptions.
/// </summary>
/// <remarks>
/// When a client subscribes to overlapping topic patterns (e.g., "sensors/#" and "sensors/+/temperature"),
/// a single message published to a matching topic (e.g., "sensors/livingroom/temperature") will match
/// both subscriptions. This enum controls whether the message handler fires once or for each matching subscription.
/// </remarks>
public enum OverlappingSubscriptionBehavior
{
    /// <summary>
    /// Fire the message handler for every matching subscription that has a handler registered.
    /// This is the default behavior for backward compatibility.
    /// </summary>
    /// <remarks>
    /// If you have 3 overlapping subscriptions and all have handlers, the handler will fire 3 times
    /// for a message that matches all 3 subscriptions.
    /// </remarks>
    FireAllMatchingHandlers,

    /// <summary>
    /// Fire only the first matching subscription handler based on subscription order.
    /// This is the recommended behavior for most applications.
    /// </summary>
    /// <remarks>
    /// If you have 3 overlapping subscriptions and all have handlers, only the first subscription's
    /// handler (in the order they were added) will fire for a message that matches all 3 subscriptions.
    /// The global OnMessageReceived event is not affected by this setting and always fires.
    /// </remarks>
    FireFirstMatchingHandler
}
