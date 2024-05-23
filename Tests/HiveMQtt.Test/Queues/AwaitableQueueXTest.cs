namespace HiveMQtt.Test.Packets;

using HiveMQtt.Client.Options;
using HiveMQtt.Client.Internal;
using HiveMQtt.MQTT5.Packets;
using HiveMQtt.MQTT5;
using Xunit;

public class AwaitableQueueXTest
{
    [Fact]
    public async Task WaitWhenEmptyAsync()
    {
        var queue = new AwaitableQueueX<ControlPacket>();
        Assert.True(queue.IsEmpty);

        var options = new HiveMQClientOptions();
        Assert.NotNull(options);

        var packet = new ConnectPacket(options);
        queue.Enqueue(packet);
        Assert.Equal(1, queue.Count);

        var cts = new CancellationTokenSource(TimeSpan.FromSeconds(2));

        ControlPacket? firstResult = null;
        ControlPacket? secondResult = null;
        var operationCanceled = false;
        try
        {
            // Get the first packet
            firstResult = await queue.DequeueAsync(cts.Token).ConfigureAwait(false);
            Assert.NotNull(firstResult);
            Assert.Equal(packet, firstResult);

            // The second dequeue call should wait until the cancellation token is timeout canceled
            secondResult = await queue.DequeueAsync(cts.Token).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            operationCanceled = true;
            Assert.NotNull(firstResult);
            Assert.Null(secondResult);
        }

        Assert.True(operationCanceled);
    }

    [Fact]
    public void CanBeCleared()
    {
        var queue = new AwaitableQueueX<ControlPacket>();
        Assert.True(queue.IsEmpty);

        var options = new HiveMQClientOptions();
        Assert.NotNull(options);

        var packetOne = new ConnectPacket(options);
        queue.Enqueue(packetOne);
        Assert.Equal(1, queue.Count);

        var packetTwo = new ConnectPacket(options);
        queue.Enqueue(packetTwo);
        Assert.Equal(2, queue.Count);

        var packetThree = new ConnectPacket(options);
        queue.Enqueue(packetThree);
        Assert.Equal(3, queue.Count);

        queue.Clear();
        Assert.True(queue.IsEmpty);
        Assert.Equal(0, queue.Count);
    }
}
