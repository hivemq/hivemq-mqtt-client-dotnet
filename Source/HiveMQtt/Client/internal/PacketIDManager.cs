namespace HiveMQtt.Client.Internal;

using System.Collections;

public class PacketIDManager
{
    // Use only BitArray for O(1) operations - more memory efficient than HashSet
    private BitArray PacketIDBitArray { get; } = new BitArray(65536);

    private SemaphoreSlim SemLock { get; } = new(1, 1);

    // Circular allocation starting from 1 (0 is reserved)
    private int NextPacketId { get; set; } = 1;

    // Queue for recently freed packet IDs to enable immediate reuse
    private Queue<int> FreedPacketIds { get; } = new();

    public PacketIDManager() => this.PacketIDBitArray.SetAll(false);

    /// <summary>
    /// Gets the next available packet ID with O(1) performance.
    /// </summary>
    /// <returns>The next available packet ID.</returns>
    public async Task<int> GetAvailablePacketIDAsync()
    {
        // Obtain the lock
        await this.SemLock.WaitAsync().ConfigureAwait(false);

        try
        {
            // First, try to reuse a recently freed packet ID
            if (this.FreedPacketIds.Count > 0)
            {
                var reusedId = this.FreedPacketIds.Dequeue();
                this.PacketIDBitArray[reusedId] = true;
                return reusedId;
            }

            // Otherwise, find the next available packet ID using circular allocation
            var candidate = this.FindNextAvailablePacketID();
            this.PacketIDBitArray[candidate] = true;
            return candidate;
        }
        finally
        {
            // Release the lock
            this.SemLock.Release();
        }
    }

    /// <summary>
    /// Marks a packet ID as available and adds it to the reuse queue.
    /// </summary>
    /// <param name="packetId">The packet ID to mark as available.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task MarkPacketIDAsAvailableAsync(int packetId)
    {
        // Obtain the lock
        await this.SemLock.WaitAsync().ConfigureAwait(false);

        try
        {
            // Mark as available in the bit array
            this.PacketIDBitArray[packetId] = false;

            // Add to reuse queue for immediate availability
            this.FreedPacketIds.Enqueue(packetId);
        }
        finally
        {
            // Release the lock
            this.SemLock.Release();
        }
    }

    /// <summary>
    /// Finds the next available packet ID using efficient circular allocation.
    /// </summary>
    /// <returns>The next available packet ID.</returns>
    /// <exception cref="InvalidOperationException">Thrown when no available packet IDs are available.</exception>
    internal int FindNextAvailablePacketID()
    {
        // Start from the last allocated packet ID and search forward
        for (var i = this.NextPacketId; i <= 65535; i++)
        {
            if (!this.PacketIDBitArray[i])
            {
                this.NextPacketId = i + 1;
                return i;
            }
        }

        // Wrap around and search from 1 to the last allocated ID
        for (var i = 1; i < this.NextPacketId; i++)
        {
            if (!this.PacketIDBitArray[i])
            {
                this.NextPacketId = i + 1;
                return i;
            }
        }

        throw new InvalidOperationException("No available packet IDs");
    }

    /// <summary>
    /// Gets the number of packet IDs in use.
    /// </summary>
    public int Count
    {
        get
        {
            var count = 0;
            for (var i = 1; i <= 65535; i++)
            {
                if (this.PacketIDBitArray[i])
                {
                    count++;
                }
            }

            return count;
        }
    }
}
