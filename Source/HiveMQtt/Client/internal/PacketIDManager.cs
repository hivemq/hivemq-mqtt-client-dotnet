namespace HiveMQtt.Client.Internal;

using System.Collections;

public class PacketIDManager
{
    private HashSet<int> PacketIDsInUse { get; } = new();

    private BitArray PacketIDBitArray { get; } = new BitArray(65536);

    private SemaphoreSlim SemLock { get; } = new(1, 1);

    private int LastPacketId { get; set; } = 1;

    public PacketIDManager() => this.PacketIDBitArray.SetAll(false);

    /// <summary>
    /// Gets the next available packet ID.
    /// </summary>
    /// <returns>The next available packet ID.</returns>
    public async Task<int> GetAvailablePacketIDAsync()
    {
        // Obtain the lock
        await this.SemLock.WaitAsync().ConfigureAwait(false);

        var candidate = this.FindNextAvailablePacketID();
        this.PacketIDsInUse.Add(candidate);
        this.PacketIDBitArray[candidate] = true;

        // Release the lock
        this.SemLock.Release();

        return candidate;
    }

    /// <summary>
    /// Marks a packet ID as available.
    /// </summary>
    /// <param name="packetId">The packet ID to mark as available.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task MarkPacketIDAsAvailableAsync(int packetId)
    {
        // Obtain the lock
        await this.SemLock.WaitAsync().ConfigureAwait(false);

        this.PacketIDsInUse.Remove(packetId);
        this.PacketIDBitArray[packetId] = false;

        // Release the lock
        this.SemLock.Release();
    }

    /// <summary>
    /// Finds the next available packet ID.
    /// </summary>
    /// <returns>The next available packet ID.</returns>
    /// <exception cref="InvalidOperationException">Thrown when no available packet IDs are available.</exception>
    internal int FindNextAvailablePacketID()
    {
        // Loop through starting at the last served packet ID
        for (var i = this.LastPacketId; i <= 65535; i++)
        {
            if (!this.PacketIDsInUse.Contains(i) && !this.PacketIDBitArray[i])
            {
                this.LastPacketId = i;
                return i;
            }
        }

        // We hit the end of the range, loop from the beginning
        for (var i = 1; i < this.LastPacketId; i++)
        {
            if (!this.PacketIDsInUse.Contains(i) && !this.PacketIDBitArray[i])
            {
                this.LastPacketId = i;
                return i;
            }
        }

        throw new InvalidOperationException("No available packet IDs");
    }

    /// <summary>
    /// Gets the number of packet IDs in use.
    /// </summary>
    public int Count => this.PacketIDsInUse.Count;
}
