namespace HiveMQtt.Test.Packets;

using HiveMQtt.Client.Options;
using HiveMQtt.Client.Internal;
using HiveMQtt.MQTT5.Packets;
using HiveMQtt.MQTT5;
using Xunit;

public class BoundedDictionaryXTest
{
    [Fact]
    public async Task BlockWhenSlotsFullAsync()
    {
        var dictionary = new BoundedDictionaryX<int, ControlPacket>(2);
        Assert.True(dictionary.IsEmpty);

        var options = new HiveMQClientOptions();
        Assert.NotNull(options);

        var cts = new CancellationTokenSource(TimeSpan.FromSeconds(2));
        var packet = new ConnectPacket(options);
        var operationCanceled = false;

        try
        {
            // Add the first
            await dictionary.AddAsync(1, packet, cts.Token).ConfigureAwait(false);
            Assert.False(dictionary.IsEmpty);
            Assert.Equal(1, dictionary.Count);

            // Add the second
            await dictionary.AddAsync(2, packet, cts.Token).ConfigureAwait(false);
            Assert.False(dictionary.IsEmpty);
            Assert.Equal(2, dictionary.Count);

            // The third should block and wait for the cancellation token to timeout
            await dictionary.AddAsync(3, packet, cts.Token).ConfigureAwait(false);

        }
        catch (OperationCanceledException)
        {
            operationCanceled = true;
        }

        Assert.True(operationCanceled);
        Assert.False(dictionary.IsEmpty);
        Assert.Equal(2, dictionary.Count);
    }

    [Fact]
    public async Task SlotsCanBeUpdatedAsync()
    {
        var dictionary = new BoundedDictionaryX<int, ControlPacket>(3);
        Assert.True(dictionary.IsEmpty);

        var options = new HiveMQClientOptions();
        Assert.NotNull(options);

        var cts = new CancellationTokenSource(TimeSpan.FromSeconds(2));
        var packet = new ConnectPacket(options);
        var result = false;

        // Add the first
        result = await dictionary.AddAsync(1, packet, cts.Token).ConfigureAwait(false);
        Assert.True(result);
        Assert.False(dictionary.IsEmpty);
        Assert.Equal(1, dictionary.Count);

        // Add the second
        result = await dictionary.AddAsync(2, packet, cts.Token).ConfigureAwait(false);
        Assert.True(result);
        Assert.False(dictionary.IsEmpty);
        Assert.Equal(2, dictionary.Count);

        // Add the third
        result = await dictionary.AddAsync(3, packet, cts.Token).ConfigureAwait(false);
        Assert.True(result);
        Assert.False(dictionary.IsEmpty);
        Assert.Equal(3, dictionary.Count);

        // Get the first
        result = dictionary.TryGetValue(2, out var origValue);
        Assert.True(result);
        Assert.False(dictionary.IsEmpty);
        Assert.Equal(packet, origValue);

        var cPacket = new PingReqPacket();

        // Update the second
        result = dictionary.TryUpdate(2, cPacket, packet);
        Assert.True(result);

        Assert.False(dictionary.IsEmpty);
        Assert.Equal(3, dictionary.Count);

        // Re-retrieve the second item to verify the update
        result = dictionary.TryGetValue(2, out var newValue);
        Assert.True(result);
        Assert.False(dictionary.IsEmpty);
        Assert.Equal(cPacket, newValue);
    }

    [Fact]
    public void ExposesCapacity()
    {
        var dictionary = new BoundedDictionaryX<int, ControlPacket>(3);
        Assert.True(dictionary.IsEmpty);
        Assert.Equal(3, dictionary.Capacity);
    }

    [Fact]
    public async Task CanBeClearedAsync()
    {
        var dictionary = new BoundedDictionaryX<int, ControlPacket>(3);
        Assert.True(dictionary.IsEmpty);

        var options = new HiveMQClientOptions();
        Assert.NotNull(options);

        var packetOne = new ConnectPacket(options);
        await dictionary.AddAsync(1, packetOne).ConfigureAwait(false);
        Assert.Equal(1, dictionary.Count);

        var packetTwo = new ConnectPacket(options);
        await dictionary.AddAsync(2, packetTwo).ConfigureAwait(false);
        Assert.Equal(2, dictionary.Count);

        var packetThree = new ConnectPacket(options);
        await dictionary.AddAsync(3, packetThree).ConfigureAwait(false);
        Assert.Equal(3, dictionary.Count);

        dictionary.Clear();
        Assert.True(dictionary.IsEmpty);
        Assert.Equal(0, dictionary.Count);
    }
}
