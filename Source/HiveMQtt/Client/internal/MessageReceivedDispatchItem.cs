namespace HiveMQtt.Client.Internal;

using System;
using System.Collections.Generic;
using HiveMQtt.Client.Events;

/// <summary>
/// A snapshot of handlers to invoke for one received publish (QoS 1 or 2).
/// </summary>
internal sealed class MessageReceivedDispatchItem
{
    public object Sender { get; init; } = null!;

    public OnMessageReceivedEventArgs EventArgs { get; init; } = null!;

    public IReadOnlyList<EventHandler<OnMessageReceivedEventArgs>> GlobalHandlers { get; init; } =
        Array.Empty<EventHandler<OnMessageReceivedEventArgs>>();

    /// <summary>
    /// Gets an optional resolver for per-subscription handlers, invoked on the dispatch consumer thread.
    /// </summary>
    public Func<IReadOnlyList<EventHandler<OnMessageReceivedEventArgs>>>? ResolveSubscriptionHandlers { get; init; }
}
