namespace HiveMQtt.Test.HiveMQClient.WebSocket;

using System;
using System.Threading;
using System.Threading.Tasks;
using HiveMQtt.Client;
using HiveMQtt.MQTT5.ReasonCodes;
using Xunit;

/// <summary>
/// Tests for large message handling in WebSocket transport.
/// These tests verify:
/// - Messages exceeding buffer size (8KB) - fragmented message path.
/// - Multiple large messages in sequence - ArrayPool buffer reuse.
/// - Memory pressure scenarios - no memory leaks.
/// </summary>
[Collection("Broker")]
public class LargeMessageTest
{
    private const int BufferSize = 8192; // WebSocket buffer size

    [Fact]
    public async Task FragmentedMessageExceedingBufferSizeAsync()
    {
        // Test message that exceeds the 8KB buffer size to verify fragmented message path
        var testTopic = "tests/FragmentedMessageExceedingBufferSize";
        var messageSize = 10 * 1024; // 10KB - exceeds 8KB buffer
        var testPayload = new byte[messageSize];

        // Fill with pattern for verification
        for (var i = 0; i < testPayload.Length; i++)
        {
            testPayload[i] = (byte)(i % 256);
        }

        var options = new HiveMQClientOptionsBuilder()
                            .WithWebSocketServer("ws://localhost:8000/mqtt")
                            .WithClientId("FragmentedMessageExceedingBufferSize")
                            .Build();
        var client = new HiveMQClient(options);
        var taskCompletionSource = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

        var connectResult = await client.ConnectAsync().ConfigureAwait(false);
        Assert.True(connectResult.ReasonCode == ConnAckReasonCode.Success);

        // Set the event handler for the message received event
        client.OnMessageReceived += (sender, args) =>
        {
            Assert.Equal(testTopic, args.PublishMessage.Topic);
            Assert.NotNull(args.PublishMessage.Payload);
            Assert.Equal(messageSize, args.PublishMessage.Payload.Length);

            // Verify data integrity
            for (var i = 0; i < args.PublishMessage.Payload.Length; i++)
            {
                Assert.Equal((byte)(i % 256), args.PublishMessage.Payload[i]);
            }

            taskCompletionSource.SetResult(true);
        };

        var subResult = await client.SubscribeAsync(testTopic).ConfigureAwait(false);
        var result = await client.PublishAsync(testTopic, testPayload).ConfigureAwait(false);

        var taskResult = await taskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(10)).ConfigureAwait(false);
        Assert.True(taskResult);

        await client.DisconnectAsync().ConfigureAwait(false);
        client.Dispose();
    }

    [Fact]
    public async Task MultipleLargeMessagesInSequenceAsync()
    {
        // Test multiple large messages in sequence to verify ArrayPool buffer reuse
        var testTopic = "tests/MultipleLargeMessagesInSequence";
        var messageSize = 20 * 1024; // 20KB - exceeds buffer and requires fragmentation
        var messageCount = 10;
        var messagesReceived = 0;

        var options = new HiveMQClientOptionsBuilder()
                            .WithWebSocketServer("ws://localhost:8000/mqtt")
                            .WithClientId("MultipleLargeMessagesInSequence")
                            .Build();
        var client = new HiveMQClient(options);
        var taskCompletionSource = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

        var connectResult = await client.ConnectAsync().ConfigureAwait(false);
        Assert.True(connectResult.ReasonCode == ConnAckReasonCode.Success);

        // Set the event handler for the message received event
        client.OnMessageReceived += (sender, args) =>
        {
            Assert.Equal(testTopic, args.PublishMessage.Topic);
            Assert.NotNull(args.PublishMessage.Payload);
            Assert.Equal(messageSize, args.PublishMessage.Payload.Length);

            // Verify payload pattern - all bytes should be the same
            // Note: Using QoS 0 (default = AtMostOnceDelivery), which does NOT guarantee message ordering
            // per MQTT v5 spec section 4.6. QoS 0 is "best effort" and messages can arrive out of order.
            // For strict ordering guarantees, use QoS 2 (ExactlyOnceDelivery)
            var firstByte = args.PublishMessage.Payload[0];
            for (var i = 1; i < args.PublishMessage.Payload.Length; i++)
            {
                Assert.Equal(firstByte, args.PublishMessage.Payload[i]);
            }

            // Verify pattern is in expected range (0-9)
            Assert.True(firstByte < messageCount, $"Pattern {firstByte} should be less than {messageCount}");

            var received = Interlocked.Increment(ref messagesReceived);
            if (received == messageCount)
            {
                taskCompletionSource.SetResult(true);
            }
        };

        var subResult = await client.SubscribeAsync(testTopic).ConfigureAwait(false);

        // Publish multiple large messages in sequence
        for (var msgIndex = 0; msgIndex < messageCount; msgIndex++)
        {
            var testPayload = new byte[messageSize];
            var pattern = (byte)(msgIndex % 256);

            // Fill with unique pattern per message
            for (var i = 0; i < testPayload.Length; i++)
            {
                testPayload[i] = pattern;
            }

            var result = await client.PublishAsync(testTopic, testPayload).ConfigureAwait(false);
            Assert.NotNull(result);
        }

        var taskResult = await taskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(30)).ConfigureAwait(false);
        Assert.True(taskResult);
        Assert.Equal(messageCount, messagesReceived);

        await client.DisconnectAsync().ConfigureAwait(false);
        client.Dispose();
    }

    [Fact]
    public async Task MemoryPressureScenarioManyMessagesAsync()
    {
        // Test memory pressure scenario with many messages to validate no memory leaks
        var testTopic = "tests/MemoryPressureScenarioManyMessages";
        var messageSize = 15 * 1024; // 15KB - requires fragmentation
        var messageCount = 50; // Many messages to stress test ArrayPool reuse
        var messagesReceived = 0;

        var options = new HiveMQClientOptionsBuilder()
                            .WithWebSocketServer("ws://localhost:8000/mqtt")
                            .WithClientId("MemoryPressureScenarioManyMessages")
                            .Build();
        var client = new HiveMQClient(options);
        var taskCompletionSource = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

        var connectResult = await client.ConnectAsync().ConfigureAwait(false);
        Assert.True(connectResult.ReasonCode == ConnAckReasonCode.Success);

        // Capture initial memory
        var initialMemory = GC.GetTotalMemory(forceFullCollection: true);

        // Set the event handler for the message received event
        client.OnMessageReceived += (sender, args) =>
        {
            Assert.Equal(testTopic, args.PublishMessage.Topic);
            Assert.NotNull(args.PublishMessage.Payload);
            Assert.Equal(messageSize, args.PublishMessage.Payload.Length);

            var received = Interlocked.Increment(ref messagesReceived);

            if (received == messageCount)
            {
                taskCompletionSource.SetResult(true);
            }
        };

        var subResult = await client.SubscribeAsync(testTopic).ConfigureAwait(false);

        // Publish many messages rapidly
        for (var msgIndex = 0; msgIndex < messageCount; msgIndex++)
        {
            var testPayload = new byte[messageSize];

            // Fill with pattern
            for (var i = 0; i < testPayload.Length; i++)
            {
                testPayload[i] = (byte)(msgIndex % 256);
            }

            var result = await client.PublishAsync(testTopic, testPayload).ConfigureAwait(false);
            Assert.NotNull(result);

            // Force GC occasionally to test ArrayPool behavior under pressure
            if (msgIndex % 10 == 0)
            {
                GC.Collect(GC.MaxGeneration, GCCollectionMode.Optimized);
            }
        }

        var taskResult = await taskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(60)).ConfigureAwait(false);
        Assert.True(taskResult);
        Assert.Equal(messageCount, messagesReceived);

        // Force full GC and check memory
        GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, true, true);
        var finalMemory = GC.GetTotalMemory(forceFullCollection: true);

        // Memory should not grow excessively (allowing for some variance)
        // If ArrayPool is working correctly, memory should be relatively stable
        var memoryGrowth = finalMemory - initialMemory;
        var memoryGrowthMB = memoryGrowth / (1024.0 * 1024.0);

        // Log for debugging (actual assertion is that messages were received correctly)
        // Memory growth should be reasonable given ArrayPool reuse
        Assert.True(memoryGrowthMB < 100, $"Memory growth of {memoryGrowthMB:F2} MB seems excessive. This may indicate a memory leak.");

        await client.DisconnectAsync().ConfigureAwait(false);
        client.Dispose();
    }

    [Fact]
    public async Task VeryLargeMessageAsync()
    {
        // Test with a very large message (50KB) to stress test fragmentation
        var testTopic = "tests/VeryLargeMessage";
        var messageSize = 50 * 1024; // 50KB - requires multiple fragments
        var testPayload = new byte[messageSize];

        // Fill with pattern for verification
        var random = new Random(42); // Fixed seed for reproducibility
        random.NextBytes(testPayload);

        var options = new HiveMQClientOptionsBuilder()
                            .WithWebSocketServer("ws://localhost:8000/mqtt")
                            .WithClientId("VeryLargeMessage")
                            .Build();
        var client = new HiveMQClient(options);
        var taskCompletionSource = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

        var connectResult = await client.ConnectAsync().ConfigureAwait(false);
        Assert.True(connectResult.ReasonCode == ConnAckReasonCode.Success);

        // Set the event handler for the message received event
        client.OnMessageReceived += (sender, args) =>
        {
            Assert.Equal(testTopic, args.PublishMessage.Topic);
            Assert.NotNull(args.PublishMessage.Payload);
            Assert.Equal(messageSize, args.PublishMessage.Payload.Length);

            // Verify data integrity
            for (var i = 0; i < args.PublishMessage.Payload.Length; i++)
            {
                Assert.Equal(testPayload[i], args.PublishMessage.Payload[i]);
            }

            taskCompletionSource.SetResult(true);
        };

        var subResult = await client.SubscribeAsync(testTopic).ConfigureAwait(false);
        var result = await client.PublishAsync(testTopic, testPayload).ConfigureAwait(false);

        var taskResult = await taskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(15)).ConfigureAwait(false);
        Assert.True(taskResult);

        await client.DisconnectAsync().ConfigureAwait(false);
        client.Dispose();
    }

    [Fact]
    public async Task BoundaryConditionExactlyBufferSizeAsync()
    {
        // Test message exactly at buffer boundary (8KB) - may or may not fragment
        var testTopic = "tests/BoundaryConditionExactlyBufferSize";
        var messageSize = BufferSize; // Exactly 8KB
        var testPayload = new byte[messageSize];

        // Fill with pattern
        for (var i = 0; i < testPayload.Length; i++)
        {
            testPayload[i] = (byte)(i % 256);
        }

        var options = new HiveMQClientOptionsBuilder()
                            .WithWebSocketServer("ws://localhost:8000/mqtt")
                            .WithClientId("BoundaryConditionExactlyBufferSize")
                            .Build();
        var client = new HiveMQClient(options);
        var taskCompletionSource = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

        var connectResult = await client.ConnectAsync().ConfigureAwait(false);
        Assert.True(connectResult.ReasonCode == ConnAckReasonCode.Success);

        // Set the event handler for the message received event
        client.OnMessageReceived += (sender, args) =>
        {
            Assert.Equal(testTopic, args.PublishMessage.Topic);
            Assert.NotNull(args.PublishMessage.Payload);
            Assert.Equal(messageSize, args.PublishMessage.Payload.Length);

            // Verify data integrity
            for (var i = 0; i < args.PublishMessage.Payload.Length; i++)
            {
                Assert.Equal((byte)(i % 256), args.PublishMessage.Payload[i]);
            }

            taskCompletionSource.SetResult(true);
        };

        var subResult = await client.SubscribeAsync(testTopic).ConfigureAwait(false);
        var result = await client.PublishAsync(testTopic, testPayload).ConfigureAwait(false);

        var taskResult = await taskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(10)).ConfigureAwait(false);
        Assert.True(taskResult);

        await client.DisconnectAsync().ConfigureAwait(false);
        client.Dispose();
    }

    [Fact]
    public async Task BoundaryConditionJustOverBufferSizeAsync()
    {
        // Test message just over buffer size (8KB + 1 byte) - should fragment
        var testTopic = "tests/BoundaryConditionJustOverBufferSize";
        var messageSize = BufferSize + 1; // Just over 8KB
        var testPayload = new byte[messageSize];

        // Fill with pattern
        for (var i = 0; i < testPayload.Length; i++)
        {
            testPayload[i] = (byte)(i % 256);
        }

        var options = new HiveMQClientOptionsBuilder()
                            .WithWebSocketServer("ws://localhost:8000/mqtt")
                            .WithClientId("BoundaryConditionJustOverBufferSize")
                            .Build();
        var client = new HiveMQClient(options);
        var taskCompletionSource = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

        var connectResult = await client.ConnectAsync().ConfigureAwait(false);
        Assert.True(connectResult.ReasonCode == ConnAckReasonCode.Success);

        // Set the event handler for the message received event
        client.OnMessageReceived += (sender, args) =>
        {
            Assert.Equal(testTopic, args.PublishMessage.Topic);
            Assert.NotNull(args.PublishMessage.Payload);
            Assert.Equal(messageSize, args.PublishMessage.Payload.Length);

            // Verify data integrity
            for (var i = 0; i < args.PublishMessage.Payload.Length; i++)
            {
                Assert.Equal((byte)(i % 256), args.PublishMessage.Payload[i]);
            }

            taskCompletionSource.SetResult(true);
        };

        var subResult = await client.SubscribeAsync(testTopic).ConfigureAwait(false);
        var result = await client.PublishAsync(testTopic, testPayload).ConfigureAwait(false);

        var taskResult = await taskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(10)).ConfigureAwait(false);
        Assert.True(taskResult);

        await client.DisconnectAsync().ConfigureAwait(false);
        client.Dispose();
    }
}
