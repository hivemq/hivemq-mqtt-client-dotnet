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
using System.Text.RegularExpressions;
using HiveMQtt.MQTT5.Types;

/// <inheritdoc />
public partial class HiveMQClient : IDisposable, IHiveMQClient
{
    private bool disposed;
    private int lastPacketId;

    /// <summary>
    /// Validates whether a subscription already exists.
    /// </summary>
    /// <param name="subscription">The subscription to compare.</param>
    /// <returns>A boolean indicating whether the subscription exists.</returns>
    internal bool SubscriptionExists(Subscription subscription)
    {
        if (this.Subscriptions.Contains(subscription))
        {
            return true;
        }

        foreach (var candidate in this.Subscriptions)
        {
            if (candidate.TopicFilter.Topic == subscription.TopicFilter.Topic)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Gets a subscription by topic from the list of existing subscriptions.
    /// </summary>
    /// <param name="topic">The topic to match.</param>
    /// <returns>The subscription or null if not found.</returns>
    internal Subscription? GetSubscriptionByTopic(string topic)
    {
        foreach (var subscription in this.Subscriptions)
        {
            if (subscription.TopicFilter.Topic == topic)
            {
                return subscription;
            }
        }

        return null;
    }

    /// <summary>
    /// This method is used to determine if a topic filter matches a topic.
    ///
    /// It implements the MQTT 5.0 specification definitions for single-level
    /// and multi-level wildcard characters (and related rules).
    ///
    /// </summary>
    /// <param name="pattern">The topic filter.</param>
    /// <param name="candidate">The topic to match.</param>
    /// <returns>A boolean indicating whether the topic filter matches the topic.</returns>
    /// <exception cref="ArgumentException">Thrown when the topic filter is invalid.</exception>
    public static bool MatchTopic(string pattern, string candidate)
    {
        if (pattern == candidate)
        {
            return true;
        }

        if (pattern == "#")
        {
            // A subscription to “#” will not receive any messages published to a topic beginning with a $
            if (candidate.StartsWith("$", System.StringComparison.CurrentCulture))
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        if (pattern == "+")
        {
            // A subscription to “+” will not receive any messages published to a topic beginning with a $ or /
            if (candidate.StartsWith("$", System.StringComparison.CurrentCulture) ||
                candidate.StartsWith("/", System.StringComparison.CurrentCulture))
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        // If pattern contains a multi-level wildcard character, it must be the last character in the pattern
        // and it must be preceded by a topic level separator.
        var mlwcValidityRegex = new Regex(@"(?<!/)#");

        if (pattern.Contains("/#/") | mlwcValidityRegex.IsMatch(pattern))
        {
            throw new ArgumentException(
                "The multi-level wildcard character must be specified either on its own or following a topic level separator. " +
                "In either case it must be the last character specified in the Topic Filter.");
        }

        // ^sport\/tennis\/player1(\/?|.+)$
        var regexp = "\\A" + Regex.Escape(pattern).Replace(@"\+", @"?[/][^/]*") + "\\z";

        regexp = regexp.Replace(@"/\#", @"(/?|.+)");
        regexp = regexp.EndsWith("\\z", System.StringComparison.CurrentCulture) ? regexp : regexp + "\\z";

        return Regex.IsMatch(candidate, regexp);
    }

    /// <summary>
    /// Generate a packet identifier.
    /// </summary>
    /// <returns>A valid packet identifier.</returns>
    protected int GeneratePacketIdentifier()
    {
        if (this.lastPacketId == ushort.MaxValue)
        {
            this.lastPacketId = 0;
        }

        return Interlocked.Increment(ref this.lastPacketId);
    }

    /// <summary>
    /// https://learn.microsoft.com/en-us/dotnet/api/system.idisposable?view=net-6.0.
    /// </summary>
    public void Dispose()
    {
        this.Dispose(disposing: true);
        /*
          This object will be cleaned up by the Dispose method.
          Therefore, you should call GC.SuppressFinalize to
          take this object off the finalization queue
          and prevent finalization code for this object
          from executing a second time.
        */
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// https://learn.microsoft.com/en-us/dotnet/api/system.idisposable?view=net-6.0
    /// Dispose(bool disposing) executes in two distinct scenarios.
    /// If disposing equals true, the method has been called directly
    /// or indirectly by a user's code. Managed and unmanaged resources
    /// can be disposed.
    /// If disposing equals false, the method has been called by the
    /// runtime from inside the finalizer and you should not reference
    /// other objects. Only unmanaged resources can be disposed.
    /// </summary>
    /// <param name="disposing">True if called from user code.</param>
    protected virtual void Dispose(bool disposing)
    {
        Logger.Trace("Disposing HiveMQClient");

        // Check to see if Dispose has already been called.
        if (!this.disposed)
        {
            // If disposing equals true, dispose all managed
            // and unmanaged resources.
            if (disposing)
            {
                if (this.ConnectState == Internal.ConnectState.Connected)
                {
                    Logger.Trace("HiveMQClient Dispose: Disconnecting connected client.");
                    _ = Task.Run(async () => await this.DisconnectAsync().ConfigureAwait(false));
                }

                // Dispose managed resources.
                this.cancellationTokenSource.Cancel();
                this.cancellationTokenSource.Dispose();
            }

            // Call the appropriate methods to clean up
            // unmanaged resources here.
            // If disposing is false,
            // only the following code is executed.
            // { }

            // Note disposing has been done.
            this.disposed = true;
        }
    }
}
