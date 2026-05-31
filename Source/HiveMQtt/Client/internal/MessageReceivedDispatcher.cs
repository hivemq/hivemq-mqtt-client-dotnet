namespace HiveMQtt.Client.Internal;

using System;
using System.Threading;
using System.Threading.Tasks;
using HiveMQtt.Client.Events;

/// <summary>
/// Serializes QoS 1/2 <see cref="HiveMQClient.OnMessageReceived"/> handler invocation in FIFO order.
/// </summary>
internal sealed class MessageReceivedDispatcher : IDisposable
{
    private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

    private static readonly AsyncLocal<bool> IsDispatchThread = new();

    private readonly AwaitableQueueX<MessageReceivedDispatchItem> queue = new();
    private readonly CancellationTokenSource cts = new();
    private readonly Task consumerTask;
    private int quiescing;
    private int disposed;
    private int inFlight;

    public MessageReceivedDispatcher() =>
        this.consumerTask = Task.Run(() => this.ConsumerLoopAsync(this.cts.Token));

    /// <summary>
    /// Gets a value indicating whether the current thread is executing a message dispatch handler.
    /// </summary>
    internal static bool IsOnDispatchThread => IsDispatchThread.Value;

    /// <summary>
    /// Resets quiesce state so handlers can be enqueued again after a successful connect.
    /// </summary>
    public void ResetForConnect()
    {
        if (Volatile.Read(ref this.disposed) == 0)
        {
            Interlocked.Exchange(ref this.quiescing, 0);
        }
    }

    /// <summary>
    /// Enqueues a dispatch item. Returns false when quiescing, disposed, or no handlers are registered.
    /// </summary>
    /// <param name="item">The handlers and event args to dispatch.</param>
    /// <returns><see langword="true"/> when the item was enqueued; otherwise <see langword="false"/>.</returns>
    public bool TryEnqueue(MessageReceivedDispatchItem item)
    {
        if (Volatile.Read(ref this.quiescing) != 0 || Volatile.Read(ref this.disposed) != 0)
        {
            return false;
        }

        if (item.GlobalHandlers.Count == 0 && item.ResolveSubscriptionHandlers is null)
        {
            return false;
        }

        this.queue.Enqueue(item);
        return true;
    }

    /// <summary>
    /// Stops accepting new items, drops pending items, and waits for the in-flight handler to finish.
    /// When called from a dispatch handler thread, pending items are dropped but this method does not
    /// wait for the current handler (avoids self-deadlock).
    /// </summary>
    /// <param name="timeout">Maximum time to wait for the in-flight handler.</param>
    /// <returns>A task that completes when quiesce finishes or times out.</returns>
    public async Task QuiesceAsync(TimeSpan timeout)
    {
        Interlocked.Exchange(ref this.quiescing, 1);
        this.queue.Clear();

        if (IsDispatchThread.Value)
        {
            return;
        }

        var deadline = Environment.TickCount64 + (long)timeout.TotalMilliseconds;
        while (Interlocked.CompareExchange(ref this.inFlight, 0, 0) > 0)
        {
            if (Environment.TickCount64 >= deadline)
            {
                Logger.Warn("MessageReceivedDispatcher quiesce timed out waiting for in-flight handler.");
                break;
            }

            await Task.Delay(10).ConfigureAwait(false);
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (Interlocked.Exchange(ref this.disposed, 1) != 0)
        {
            return;
        }

        Interlocked.Exchange(ref this.quiescing, 1);
        this.queue.Clear();
        this.cts.Cancel();

        try
        {
#pragma warning disable VSTHRD002 // Dispose must synchronously stop the consumer task
            this.consumerTask.Wait(TimeSpan.FromSeconds(5));
#pragma warning restore VSTHRD002
        }
        catch (Exception ex)
        {
            Logger.Warn($"MessageReceivedDispatcher dispose wait failed: {ex.Message}");
        }

        this.cts.Dispose();
        this.queue.Dispose();
    }

    private static void DispatchItem(MessageReceivedDispatchItem item)
    {
#pragma warning disable IDE0301 // Collection initialization - Array.Empty is required for StyleCop SA1010
        var subscriptionHandlers =
            item.ResolveSubscriptionHandlers?.Invoke() ?? Array.Empty<EventHandler<OnMessageReceivedEventArgs>>();
#pragma warning restore IDE0301

        if (item.GlobalHandlers.Count == 0 && subscriptionHandlers.Count == 0)
        {
            Logger.Warn(
                $"Lost Application Message ({item.EventArgs.PublishMessage.Topic}): No global or subscription message handler found.  Register an event handler (before Subscribing) to receive all messages incoming.");
            return;
        }

        foreach (var handler in item.GlobalHandlers)
        {
            try
            {
                handler(item.Sender, item.EventArgs);
            }
            catch (Exception ex)
            {
                Logger.Error($"OnMessageReceived Handler exception: {ex.Message}");
            }
        }

        foreach (var handler in subscriptionHandlers)
        {
            try
            {
                handler(item.Sender, item.EventArgs);
            }
            catch (Exception ex)
            {
                Logger.Error(
                    $"per-subscription MessageReceivedEventLauncher faulted ({item.EventArgs.PublishMessage.Topic}): {ex.Message}");
            }
        }
    }

    private async Task ConsumerLoopAsync(CancellationToken cancellationToken)
    {
        IsDispatchThread.Value = true;
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                MessageReceivedDispatchItem item;
                try
                {
                    item = await this.queue.DequeueAsync(cancellationToken).ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    break;
                }

                Interlocked.Increment(ref this.inFlight);
                try
                {
                    DispatchItem(item);
                }
                finally
                {
                    Interlocked.Decrement(ref this.inFlight);
                }
            }
        }
        finally
        {
            IsDispatchThread.Value = false;
        }
    }
}
