namespace HiveMQtt.Client.Internal;

using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

/// <summary>
/// A finite (bounded) dictionary that can be awaited on for slots to become available.
/// </summary>
/// <typeparam name="TKey">The type of items to index with.</typeparam>
/// <typeparam name="TVal">The type of items to store as values.</typeparam>
public class BoundedDictionaryX<TKey, TVal> : IDisposable
    where TKey : notnull
{
    private readonly ILogger logger;

    /// <summary>
    /// The semaphore used to signal when items are enqueued.
    /// </summary>
    private readonly SemaphoreSlim semaphore;

    /// <summary>
    /// The internal queue of items.
    /// </summary>
    private readonly ConcurrentDictionary<TKey, TVal> dictionary;

    /// <summary>
    /// Gets the capacity of the queue.
    /// </summary>
    public int Capacity { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="BoundedDictionaryX{I, T}"/> class.
    /// </summary>
    /// <param name="capacity">The capacity of the queue.</param>
    /// <param name="loggerFactory">Optional logger factory for logging. If not provided, NullLogger will be used.</param>
    public BoundedDictionaryX(int capacity, ILoggerFactory? loggerFactory = null)
    {
        this.Capacity = capacity;
        this.semaphore = new SemaphoreSlim(capacity);
        this.dictionary = new ConcurrentDictionary<TKey, TVal>();
        this.logger = loggerFactory?.CreateLogger<BoundedDictionaryX<TKey, TVal>>()
            ?? Microsoft.Extensions.Logging.Abstractions.NullLoggerFactory.Instance.CreateLogger<BoundedDictionaryX<TKey, TVal>>();
    }

    /// <summary>
    /// Attempts to add an item to the dictionary.  If there is not slot available, this method will asynchronously wait for an available slot.
    /// </summary>
    /// <param name="key">The key to add.</param>
    /// <param name="value">The value to add.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns><c>true</c> if the item was added; otherwise, <c>false</c>.</returns>
    public async Task<bool> AddAsync(TKey key, TVal value, CancellationToken cancellationToken = default)
    {
        bool errorDetected;

        this.logger.LogTrace("Adding item {Key}", key);
        this.logger.LogTrace("Open slots: {OpenSlots}  Dictionary Count: {Count}", this.semaphore.CurrentCount, this.dictionary.Count);

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
                this.logger.LogWarning("Duplicate key: {Key}", key);

                errorDetected = true;
            }
        }
        catch (ArgumentNullException ex)
        {
            this.logger.LogWarning(ex, "ArgumentNull Exception");
            errorDetected = true;
        }
        catch (OverflowException ex)
        {
            this.logger.LogWarning(ex, "Overflow Exception");
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
        this.logger.LogTrace("Removing item {Key}", key);
        this.logger.LogTrace("Open slots: {OpenSlots}  Dictionary Count: {Count}", this.semaphore.CurrentCount, this.dictionary.Count);

        try
        {
            if (this.dictionary.TryRemove(key, out var removedValue))
            {
                // Item successfully removed, release the slot
                this.semaphore.Release();
                value = removedValue;
                return true;
            }
            else
            {
                this.logger.LogWarning("Key not found: {Key}", key);
            }
        }
        catch (ArgumentNullException ex)
        {
            this.logger.LogWarning(ex, "ArgumentNull Exception");
        }
        catch (OverflowException ex)
        {
            this.logger.LogWarning(ex, "Overflow Exception");
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
    public bool TryUpdate(TKey key, TVal newValue, TVal comparisonValue) => this.dictionary.TryUpdate(key, newValue, comparisonValue);

    /// <summary>
    /// Attempts to get a value from the dictionary.
    /// </summary>
    /// <param name="key">The key to get.</param>
    /// <param name="value">The value retrieved.</param>
    /// <returns><c>true</c> if the item was retrieved; otherwise, <c>false</c>.</returns>
    public bool TryGetValue(TKey key, out TVal value)
    {
        var result = this.dictionary.TryGetValue(key, out var retrievedValue);
        value = retrievedValue is null ? default! : retrievedValue;
        return result;
    }

    /// <summary>
    /// Determines whether the dictionary contains the specified key.
    /// </summary>
    /// <param name="key">The key to locate in the dictionary.</param>
    /// <returns><c>true</c> if the dictionary contains an element with the specified key; otherwise, <c>false</c>.</returns>
    public bool ContainsKey(TKey key) => this.dictionary.ContainsKey(key);

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

            if (numItems > 0)
            {
                this.semaphore.Release(numItems);
            }

            return true;
        }
        catch (ArgumentNullException ex)
        {
            this.logger.LogWarning(ex, "ArgumentNull Exception");
        }
        catch (OverflowException ex)
        {
            this.logger.LogWarning(ex, "Overflow Exception");
        }
        catch (Exception ex)
        {
            this.logger.LogWarning(ex, "Exception");
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
