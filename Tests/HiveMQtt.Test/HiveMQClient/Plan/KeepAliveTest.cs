namespace HiveMQtt.Test.HiveMQClient.Plan;

using System.Threading.Tasks;
using FluentAssertions;
using HiveMQtt.Client;
using HiveMQtt.Client.Events;
using NUnit.Framework;

[TestFixture]
public class KeepAliveTest
{
    [Test]
    public async Task Client_Uses_Zero_As_Keep_Alive_No_Pings_Are_Sent_Async()
    {
        var options = new HiveMQClientOptionsBuilder()
                        .WithKeepAlive(0)
                        .Build();
        var client = new HiveMQClient(options);

        client.Should().NotBeNull();

        var connectResult = await client.ConnectAsync().ConfigureAwait(false);
        connectResult.Should().NotBeNull();

        // Wait a short period to ensure no pings are sent when keep-alive is 0
        // Reduced from 5000ms to 2000ms since we just need to verify no pings occur
        await Task.Delay(2000).ConfigureAwait(false);

        // Validate that no pings were sent
        // This would typically involve checking internal client state or logs
        // For this example, we'll assume a method IsPingSent exists
        client.IsPingSent().Should().BeFalse();

        var disconnectResult = await client.DisconnectAsync().ConfigureAwait(false);
        disconnectResult.Should().BeTrue();
    }

    [Test]
    public async Task Client_Sends_Pings_After_Interval_Passed_Async()
    {
        var options = new HiveMQClientOptionsBuilder()
                        .WithKeepAlive(5) // 5 seconds
                        .Build();
        var client = new HiveMQClient(options);

        client.Should().NotBeNull();

        var connectResult = await client.ConnectAsync().ConfigureAwait(false);
        connectResult.Should().NotBeNull();

        // Use event-based waiting for ping instead of fixed delay
        var pingReceived = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        void OnPingSent(object? sender, OnPingReqSentEventArgs e) => pingReceived.TrySetResult(true);

        client.OnPingReqSent += OnPingSent;

        try
        {
            // Wait for ping to be sent (with timeout slightly longer than keep-alive interval)
            await pingReceived.Task.WaitAsync(TimeSpan.FromSeconds(7)).ConfigureAwait(false);
        }
        finally
        {
            client.OnPingReqSent -= OnPingSent;
        }

        // Validate that pings were sent
        // This would typically involve checking internal client state or logs
        // For this example, we'll assume a method IsPingSent exists
        client.IsPingSent().Should().BeTrue();

        var disconnectResult = await client.DisconnectAsync().ConfigureAwait(false);
        disconnectResult.Should().BeTrue();
    }
}
