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

    /// <summary>
    /// Validates whether a subscription already exists.
    /// </summary>
    /// <param name="subscription">The subscription to compare.</param>
    /// <returns>A boolean indicating whether the subscription exists.</returns>
    internal bool SubscriptionExists(Subscription subscription)
    {
        List<Subscription> tempList;
        try
        {
            this.SubscriptionsSemaphore.Wait();
            tempList = this.Subscriptions.ToList();
        }
        finally
        {
            _ = this.SubscriptionsSemaphore.Release();
        }

        return tempList.Any(s => s.TopicFilter.Topic == subscription.TopicFilter.Topic);
    }

    /// <summary>
    /// Gets a subscription by topic from the list of existing subscriptions.
    /// </summary>
    /// <param name="topic">The topic to match.</param>
    /// <returns>The subscription or null if not found.</returns>
    internal Subscription? GetSubscriptionByTopic(string topic)
    {
        List<Subscription> tempList;
        try
        {
            this.SubscriptionsSemaphore.Wait();
            tempList = this.Subscriptions.ToList();
        }
        finally
        {
            _ = this.SubscriptionsSemaphore.Release();
        }

        return tempList.FirstOrDefault(s => s.TopicFilter.Topic == topic);
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
            if (candidate.StartsWith("$", StringComparison.CurrentCulture))
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
            if (candidate.StartsWith("$", StringComparison.CurrentCulture) ||
                candidate.StartsWith("/", StringComparison.CurrentCulture))
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
#pragma warning disable SYSLIB1045 // Convert to 'GeneratedRegexAttribute'.
        var mlwcValidityRegex = new Regex(@"(?<!/)#");
#pragma warning restore SYSLIB1045 // Convert to 'GeneratedRegexAttribute'.

        if (pattern.Contains("/#/") | mlwcValidityRegex.IsMatch(pattern))
        {
            throw new ArgumentException(
                "The multi-level wildcard character must be specified either on its own or following a topic level separator. " +
                "In either case it must be the last character specified in the Topic Filter.");
        }

        // ^sport\/tennis\/player1(\/?|.+)$
        var regexp = "\\A" + Regex.Escape(pattern).Replace(@"\+", @"?[/][^/]*") + "\\z";

        regexp = regexp.Replace(@"/\#", @"(/?|.+)");
        regexp = regexp.EndsWith("\\z", StringComparison.CurrentCulture) ? regexp : regexp + "\\z";

        return Regex.IsMatch(candidate, regexp);
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
    /// runtime from inside finalize and you should not reference
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
                if (this.Connection.State == Internal.ConnectState.Connected)
                {
                    Logger.Trace("HiveMQClient Dispose: Disconnecting connected client.");
                    _ = Task.Run(async () => await this.DisconnectAsync().ConfigureAwait(false));
                }
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
