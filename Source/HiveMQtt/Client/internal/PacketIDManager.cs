namespace HiveMQtt.Client.Internal;

using System.Collections;
using System.Collections.Concurrent;
using System.Threading;

/// <summary>
/// Manages MQTT packet identifiers with optimized lock-free operations for high-performance scenarios.
/// <para>
/// This implementation uses a hybrid approach combining lock-free operations with minimal locking:
/// - Lock-free operations: Packet ID counter updates, freed ID queue operations.
/// - Minimal locking: Only for BitArray updates to ensure thread safety.
/// - Fast path: Reuses recently freed packet IDs without allocation overhead.
/// </para>
/// <para>
/// Performance characteristics:
/// - O(1) packet ID allocation in the common case (reusing freed IDs).
/// - O(1) Count property using atomic counter (vs O(n) iteration).
/// - Reduced lock contention compared to previous SemaphoreSlim-based implementation.
/// </para>
/// </summary>
public class PacketIDManager
{
    // Use only BitArray for O(1) operations - more memory efficient than HashSet
    private BitArray PacketIDBitArray { get; } = new BitArray(65536);

    // Lightweight lock for BitArray operations (only when needed)
#if NET9_0_OR_GREATER
    private readonly Lock bitArrayLock = new();
#else
    private readonly object bitArrayLock = new();
#endif

    // Circular allocation starting from 1 (0 is reserved) - uses Interlocked for lock-free updates.
    private int nextPacketId = 1;

    // Lock-free queue for recently freed packet IDs to enable immediate reuse.
    private ConcurrentQueue<int> FreedPacketIds { get; } = new();

    // Counter for active packet IDs - O(1) instead of O(n) iteration
    private int activeCount;

    public PacketIDManager() => this.PacketIDBitArray.SetAll(false);

    /// <summary>
    /// Gets the next available packet ID with optimized lock-free performance.
    /// <para>
    /// This method uses a two-phase approach:
    /// 1. Fast path: Attempts to reuse a recently freed packet ID (lock-free dequeue).
    /// 2. Allocation path: If no freed IDs available, allocates a new ID using circular allocation.
    /// </para>
    /// <para>
    /// The method is synchronous but returns a Task for API compatibility.
    /// Lock contention is minimized by only locking during BitArray updates.
    /// </para>
    /// </summary>
    /// <returns>A Task that completes with the next available packet ID (1-65535).</returns>
    /// <exception cref="InvalidOperationException">Thrown when no packet IDs are available (all 65535 IDs in use).</exception>
    public Task<int> GetAvailablePacketIDAsync()
    {
        // Fast path: Try to reuse a freed packet ID (lock-free)
        if (this.FreedPacketIds.TryDequeue(out var reusedId))
        {
            // Only lock for BitArray update
            lock (this.bitArrayLock)
            {
                // Double-check the ID is still available (could have been reallocated)
                if (!this.PacketIDBitArray[reusedId])
                {
                    this.PacketIDBitArray[reusedId] = true;
                    Interlocked.Increment(ref this.activeCount);
                    return Task.FromResult(reusedId);
                }
            }

            // If the ID was already taken, fall through to allocation
        }

        // Allocate a new packet ID using lock-free circular allocation
        return Task.FromResult(this.AllocateNewPacketID());
    }

    /// <summary>
    /// Allocates a new packet ID using lock-free circular allocation with minimal locking.
    /// <para>
    /// This method implements a circular allocation strategy:
    /// - Starts from the last allocated packet ID position.
    /// - Searches forward for the next available ID.
    /// - Wraps around to 1 when reaching 65535.
    /// - Uses lock-free reads of the next packet ID position.
    /// - Only locks when updating the BitArray to mark an ID as in-use.
    /// </para>
    /// <para>
    /// Thread safety is ensured by:
    /// - Volatile reads for the next packet ID position.
    /// - Interlocked operations for updating the position.
    /// - Lock synchronization for BitArray updates.
    /// </para>
    /// </summary>
    /// <returns>The allocated packet ID (1-65535).</returns>
    /// <exception cref="InvalidOperationException">Thrown when no packet IDs are available after exhaustive search.</exception>
    private int AllocateNewPacketID()
    {
        const int maxRetries = 65535;
        var retries = 0;

        while (retries < maxRetries)
        {
            // Lock-free read of next packet ID
            var startId = Volatile.Read(ref this.nextPacketId);
            var candidate = startId;

            // Search forward from the current position
            for (var i = 0; i < 65535; i++)
            {
                if (candidate > 65535)
                {
                    candidate = 1; // Wrap around
                }

                // Try to claim this ID atomically
                lock (this.bitArrayLock)
                {
                    if (!this.PacketIDBitArray[candidate])
                    {
                        this.PacketIDBitArray[candidate] = true;
                        Interlocked.Increment(ref this.activeCount);

                        // Update next packet ID (lock-free update outside the lock scope)
                        var nextId = candidate + 1;
                        if (nextId > 65535)
                        {
                            nextId = 1;
                        }

                        // Update atomically using Interlocked (lock-free)
                        Interlocked.Exchange(ref this.nextPacketId, nextId);

                        return candidate;
                    }
                }

                candidate++;
            }

            retries++;
        }

        throw new InvalidOperationException("No available packet IDs");
    }

    /// <summary>
    /// Marks a packet ID as available and adds it to the reuse queue for immediate reuse.
    /// <para>
    /// This method performs two operations:
    /// 1. Updates the BitArray to mark the ID as available (requires lock).
    /// 2. Enqueues the ID to the reuse queue (lock-free operation).
    /// </para>
    /// <para>
    /// The freed ID will be immediately available for reuse in subsequent calls to
    /// <see cref="GetAvailablePacketIDAsync"/>, improving performance by avoiding allocation overhead.
    /// </para>
    /// <para>
    /// The method is synchronous but returns a Task for API compatibility.
    /// </para>
    /// </summary>
    /// <param name="packetId">The packet ID to mark as available (must be in range 1-65535).</param>
    /// <returns>A completed Task representing the asynchronous operation.</returns>
    public Task MarkPacketIDAsAvailableAsync(int packetId)
    {
        // Lock only for BitArray update
        lock (this.bitArrayLock)
        {
            if (this.PacketIDBitArray[packetId])
            {
                this.PacketIDBitArray[packetId] = false;
                Interlocked.Decrement(ref this.activeCount);
            }
        }

        // Lock-free enqueue to reuse queue
        this.FreedPacketIds.Enqueue(packetId);

        return Task.CompletedTask;
    }

    /// <summary>
    /// Gets the number of packet IDs currently in use.
    /// <para>
    /// This property is optimized to O(1) performance using an atomic counter that is
    /// maintained during allocation and deallocation operations.
    /// </para>
    /// <para>
    /// The counter is updated atomically using <see cref="Interlocked"/> operations,
    /// ensuring thread-safe access without locking.
    /// </para>
    /// </summary>
    /// <value>The number of packet IDs currently allocated (0-65535).</value>
    public int Count => Volatile.Read(ref this.activeCount);
}
