namespace HiveMQtt.Test.HiveMQClient.Plan;

using FluentAssertions;
using HiveMQtt.Client;
using HiveMQtt.MQTT5.Types;
using NUnit.Framework;
using System.Threading.Tasks;

[TestFixture]
public class KeepAliveTest
{
    [Test]
    public async Task Client_Uses_Zero_As_Keep_Alive_No_Pings_Are_Sent()
    {
        var options = new HiveMQClientOptionsBuilder()
                        .WithKeepAlive(0)
                        .Build();
        var client = new HiveMQClient(options);

        client.Should().NotBeNull();

        var connectResult = await client.ConnectAsync().ConfigureAwait(false);
        connectResult.Should().NotBeNull();

        // Simulate waiting for a period longer than the keep-alive interval
        await Task.Delay(5000).ConfigureAwait(false);

        // Validate that no pings were sent
        // This would typically involve checking internal client state or logs
        // For this example, we'll assume a method IsPingSent exists
        client.IsPingSent().Should().BeFalse();

        var disconnectResult = await client.DisconnectAsync().ConfigureAwait(false);
        disconnectResult.Should().BeTrue();
    }

    [Test]
    public async Task Client_Sends_Pings_After_Interval_Passed()
    {
        var options = new HiveMQClientOptionsBuilder()
                        .WithKeepAlive(5) // 5 seconds
                        .Build();
        var client = new HiveMQClient(options);

        client.Should().NotBeNull();

        var connectResult = await client.ConnectAsync().ConfigureAwait(false);
        connectResult.Should().NotBeNull();

        // Simulate waiting for a period longer than the keep-alive interval
        await Task.Delay(6000).ConfigureAwait(false);

        // Validate that pings were sent
        // This would typically involve checking internal client state or logs
        // For this example, we'll assume a method IsPingSent exists
        client.IsPingSent().Should().BeTrue();

        var disconnectResult = await client.DisconnectAsync().ConfigureAwait(false);
        disconnectResult.Should().BeTrue();
    }

}
