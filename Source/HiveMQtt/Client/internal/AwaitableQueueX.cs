namespace HiveMQtt.Client.Internal;

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// A queue that can be awaited for items to be enqueued.
/// </summary>
/// <typeparam name="T">The type of items to queue.</typeparam>
public class AwaitableQueueX<T> : IDisposable
{
    /// <summary>
    /// The semaphore used to signal when items are enqueued.
    /// </summary>
    private readonly SemaphoreSlim semaphore;

    /// <summary>
    /// The internal queue of items.
    /// </summary>
    private readonly ConcurrentQueue<T> queue;

    /// <summary>
    /// Initializes a new instance of the <see cref="AwaitableQueueX{T}"/> class.
    /// </summary>
    public AwaitableQueueX()
    {
        this.semaphore = new SemaphoreSlim(0);
        this.queue = new ConcurrentQueue<T>();
    }

    /// <summary>
    /// Enqueues an item.
    /// </summary>
    /// <param name="item">The item to enqueue.</param>
    public void Enqueue(T item)
    {
        this.queue.Enqueue(item);
        this.semaphore.Release();
    }

    /// <summary>
    /// Enqueues a range of items.
    /// </summary>
    /// <param name="source">The items to enqueue.</param>
    public void EnqueueRange(IEnumerable<T> source)
    {
        var n = 0;
        foreach (var item in source)
        {
            this.queue.Enqueue(item);
            n++;
        }

        this.semaphore.Release(n);
    }

    /// <summary>
    /// Dequeues an item.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The dequeued item.</returns>
    public async Task<T> DequeueAsync(CancellationToken cancellationToken)
    {
        while (true)
        {
            await this.semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);

            if (this.queue.TryDequeue(out var item))
            {
                return item;
            }
        }
    }

    /// <summary>
    /// Clears the queue.
    /// </summary>
    public void Clear()
    {
        while (this.queue.TryDequeue(out _))
        {
            this.semaphore.Release();
        }
    }

    /// <summary>
    /// Gets the number of items in the queue.
    /// </summary>
    /// <value>The number of items in the queue.</value>
    public int Count => this.queue.Count;

    /// <summary>
    /// Gets a value indicating whether the queue is empty.
    /// </summary>
    public bool IsEmpty => this.queue.IsEmpty;

    /// <inheritdoc />
    public void Dispose()
    {
        this.semaphore.Dispose();
        GC.SuppressFinalize(this);
    }
}
