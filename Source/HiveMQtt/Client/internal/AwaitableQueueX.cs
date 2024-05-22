namespace HiveMQtt.Client.Internal;

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public class AwaitableQueueX<T> : IDisposable
{
    private readonly SemaphoreSlim semaphore;
    private readonly ConcurrentQueue<T> queue;

    public AwaitableQueueX()
    {
        this.semaphore = new SemaphoreSlim(0);
        this.queue = new ConcurrentQueue<T>();
    }

    public void Enqueue(T item)
    {
        this.queue.Enqueue(item);
        this.semaphore.Release();
    }

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

    public void Clear()
    {
        while (this.queue.TryDequeue(out _))
        {
            this.semaphore.Release();
        }
    }

    public int Count => this.queue.Count;

    public bool IsEmpty => this.queue.IsEmpty;

    public void Dispose()
    {
        this.semaphore.Dispose();
        GC.SuppressFinalize(this);
    }
}
