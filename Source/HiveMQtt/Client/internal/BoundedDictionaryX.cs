namespace HiveMQtt.Client.Internal;

using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// A finite (bounded) dictionary that can be awaited on for slots to become available.
/// </summary>
/// <typeparam name="TKey">The type of items to index with.</typeparam>
/// <typeparam name="TVal">The type of items to store as values.</typeparam>
public class BoundedDictionaryX<TKey, TVal> : IDisposable
    where TKey : notnull
{
    private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

    /// <summary>
    /// The semaphore used to signal when items are enqueued.
    /// </summary>
    private readonly SemaphoreSlim semaphore;

    /// <summary>
    /// The internal queue of items.
    /// </summary>
    private readonly ConcurrentDictionary<TKey, TVal> dictionary;

    /// <summary>
    /// Initializes a new instance of the <see cref="BoundedDictionaryX{I, T}"/> class.
    /// </summary>
    /// <param name="capacity">The capacity of the queue.</param>
    public BoundedDictionaryX(int capacity)
    {
        this.semaphore = new SemaphoreSlim(capacity);
        this.dictionary = new ConcurrentDictionary<TKey, TVal>();
    }

    /// <summary>
    /// Attempts to add an item to the dictionary.
    /// </summary>
    /// <param name="key">The key to add.</param>
    /// <param name="value">The value to add.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns><c>true</c> if the item was added; otherwise, <c>false</c>.</returns>
    public async Task<bool> AddAsync(TKey key, TVal value, CancellationToken cancellationToken = default)
    {
        var errorDetected = false;

        // Wait for an available slot
        await this.semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            if (this.dictionary.TryAdd(key, value))
            {
                return true;
            }
            else
            {
                Logger.Warn("Duplicate key: {0}", key);

                // We failed to add the item, release the slot
                this.semaphore.Release();
                return false;
            }
        }
        catch (ArgumentNullException ex)
        {
            Logger.Warn("ArgumentNull Exception: {0}", ex);
            errorDetected = true;
        }
        catch (OverflowException ex)
        {
            Logger.Warn("Overflow Exception: {0}", ex);
            errorDetected = true;
        }

        if (errorDetected)
        {
            // We failed to add the item, release the slot
            this.semaphore.Release();
        }

        return false;
    }

    /// <summary>
    /// Attempts to remove an item from the dictionary.
    /// </summary>
    /// <param name="key">The key to remove.</param>
    /// <param name="value">The value removed.</param>
    /// <returns><c>true</c> if the item was removed; otherwise, <c>false</c>.</returns>
    public bool Remove(TKey key, out TVal value)
    {
        try
        {
            if (this.dictionary.TryRemove(key, out value))
            {
                // Item successfully removed, release the slot
                this.semaphore.Release();
                return true;
            }
            else
            {
                Logger.Warn("Key not found: {0}", key);
            }
        }
        catch (ArgumentNullException ex)
        {
            Logger.Warn("ArgumentNull Exception: {0}", ex);
        }
        catch (OverflowException ex)
        {
            Logger.Warn("Overflow Exception: {0}", ex);
        }

        value = default!;
        return false;
    }

    /// <summary>
    /// Attempts to update an item in the dictionary.
    /// </summary>
    /// <param name="key">The key to update.</param>
    /// <param name="newValue">The new value.</param>
    /// <param name="comparisonValue">The value to compare against.</param>
    /// <returns><c>true</c> if the item was updated; otherwise, <c>false</c>.</returns>
    public bool TryUpdate(TKey key, TVal newValue, TVal comparisonValue)
    {
        // This is just a pass-through for the underlying ConcurrentDictionary
        return this.dictionary.TryUpdate(key, newValue, comparisonValue);
    }

    /// <summary>
    /// Removes all items from the dictionary.
    /// </summary>
    /// <returns><c>true</c> if the dictionary was cleared; otherwise, <c>false</c>.</returns>
    public bool Clear()
    {
        try
        {
            var numItems = this.dictionary.Count;
            this.dictionary.Clear();
            this.semaphore.Release(numItems);
            return true;
        }
        catch (ArgumentNullException ex)
        {
            Logger.Warn("ArgumentNull Exception: {0}", ex);
        }
        catch (OverflowException ex)
        {
            Logger.Warn("Overflow Exception: {0}", ex);
        }
        catch (Exception ex)
        {
            Logger.Warn("Exception: {0}", ex);
        }

        return false;
    }

    /// <summary>
    /// Gets the number of items in the queue.
    /// </summary>
    /// <value>The number of items in the queue.</value>
    public int Count => this.dictionary.Count;

    /// <summary>
    /// Gets a value indicating whether the queue is empty.
    /// </summary>
    public bool IsEmpty => this.dictionary.IsEmpty;

    /// <inheritdoc />
    public void Dispose()
    {
        this.semaphore.Dispose();
        GC.SuppressFinalize(this);
    }
}
