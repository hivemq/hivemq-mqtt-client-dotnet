namespace HiveMQtt.Test.HiveMQClient.Plan;

using FluentAssertions;
using HiveMQtt.Client;
using HiveMQtt.MQTT5.ReasonCodes;
using HiveMQtt.MQTT5.Types;
using NUnit.Framework;
using System.Threading.Tasks;

[TestFixture]
public class SessionExpiryTest
{
    [Test]
    public async Task Disconnect_With_Custom_Session_Expiry_Async()
    {
        var options = new HiveMQClientOptionsBuilder()
                        .WithSessionExpiryInterval(30)
                        .Build();
        var client = new HiveMQClient(options);

        var connectResult = await client.ConnectAsync().ConfigureAwait(false);
        connectResult.Should().NotBeNull();
        connectResult.ReasonCode.Should().Be(ConnAckReasonCode.Success);
        connectResult.SessionExpiryInterval.Should().Be(30);
        connectResult.SessionPresent.Should().BeFalse();

        // Subscribe to a topic
        var subscribeResult = await client.SubscribeAsync(new SubscribeOptionsBuilder().WithSubscription("test/topic", QualityOfService.AtLeastOnceDelivery).Build()).ConfigureAwait(false);
        subscribeResult.Should().NotBeNull();
        subscribeResult.Subscriptions.Should().HaveCount(1);
        subscribeResult.Subscriptions[0].TopicFilter.Topic.Should().Be("test/topic");
        subscribeResult.Subscriptions[0].SubscribeReasonCode.Should().Be(SubAckReasonCode.GrantedQoS1);

        // Normal disconnect and reconnect.  We should get session present
        var disconnectResult = await client.DisconnectAsync().ConfigureAwait(false);
        disconnectResult.Should().BeTrue();

        var connectOptions = new ConnectOptionsBuilder().WithCleanStart(false).Build();
        var connectResult2 = await client.ConnectAsync(connectOptions).ConfigureAwait(false);
        connectResult2.Should().NotBeNull();
        connectResult2.ReasonCode.Should().Be(ConnAckReasonCode.Success);
        connectResult2.SessionExpiryInterval.Should().Be(30);
        connectResult2.SessionPresent.Should().BeTrue();

        // Disconnect with custom session expiry, delay longer than the session expiry and reconnect.  We should get session not present
        var disconnectOptions = new DisconnectOptionsBuilder().WithSessionExpiryInterval(5).Build();
        disconnectResult = await client.DisconnectAsync(disconnectOptions).ConfigureAwait(false);
        disconnectResult.Should().BeTrue();

        await Task.Delay(10000).ConfigureAwait(false);

        var connectResult3 = await client.ConnectAsync().ConfigureAwait(false);
        connectResult3.Should().NotBeNull();
        connectResult3.ReasonCode.Should().Be(ConnAckReasonCode.Success);
        connectResult3.SessionExpiryInterval.Should().Be(30);
        connectResult3.SessionPresent.Should().BeFalse();
    }
}
