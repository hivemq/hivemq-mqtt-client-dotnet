namespace HiveMQtt.Test.HiveMQClient;

using System.Threading.Tasks;
using HiveMQtt.Client;
using HiveMQtt.MQTT5.ReasonCodes;
using HiveMQtt.MQTT5.Types;
using Xunit;

public class EventExceptionHandlingTest
{
    [Fact]
    public async Task BeforeDisconnectExceptionDoesNotPreventDisconnectAsync()
    {
        var options = new HiveMQClientOptionsBuilder()
            .WithClientId("BeforeDisconnectExceptionTest")
            .Build();

        var client = new HiveMQClient(options);

        // Connect first
        var connectResult = await client.ConnectAsync().ConfigureAwait(false);
        Assert.True(connectResult.ReasonCode == ConnAckReasonCode.Success);
        Assert.True(client.IsConnected());

        // Add event handler that throws an exception
        client.BeforeDisconnect += (sender, args) => throw new InvalidOperationException("Test exception in BeforeDisconnect");

        // Attempt to disconnect - should succeed despite the exception
        var disconnectResult = await client.DisconnectAsync().ConfigureAwait(false);
        Assert.True(disconnectResult);
        Assert.False(client.IsConnected());

        client.Dispose();
    }

    [Fact]
    public async Task OnPublishSentExceptionDoesNotPreventPublishAsync()
    {
        var options = new HiveMQClientOptionsBuilder()
            .WithClientId("OnPublishSentExceptionTest")
            .Build();

        var client = new HiveMQClient(options);

        // Connect first
        var connectResult = await client.ConnectAsync().ConfigureAwait(false);
        Assert.True(connectResult.ReasonCode == ConnAckReasonCode.Success);

        // Add event handler that throws an exception
        client.OnPublishSent += (sender, args) => throw new InvalidOperationException("Test exception in OnPublishSent");

        // Attempt to publish - should succeed despite the exception
        var publishResult = await client.PublishAsync(
            "tests/OnPublishSentExceptionTest",
            "test message",
            QualityOfService.AtMostOnceDelivery)
            .ConfigureAwait(false);

        Assert.NotNull(publishResult);
        Assert.Equal(QualityOfService.AtMostOnceDelivery, publishResult.Message.QoS);

        // Clean up
        var disconnectResult = await client.DisconnectAsync().ConfigureAwait(false);
        Assert.True(disconnectResult);
        client.Dispose();
    }

    [Fact]
    public async Task BeforeSubscribeExceptionDoesNotPreventSubscribeAsync()
    {
        var options = new HiveMQClientOptionsBuilder()
            .WithClientId("BeforeSubscribeExceptionTest")
            .Build();

        var client = new HiveMQClient(options);

        // Connect first
        var connectResult = await client.ConnectAsync().ConfigureAwait(false);
        Assert.True(connectResult.ReasonCode == ConnAckReasonCode.Success);

        // Add event handler that throws an exception
        client.BeforeSubscribe += (sender, args) => throw new InvalidOperationException("Test exception in BeforeSubscribe");

        // Attempt to subscribe - should succeed despite the exception
        var subscribeResult = await client.SubscribeAsync(
            "tests/BeforeSubscribeExceptionTest",
            QualityOfService.AtMostOnceDelivery)
            .ConfigureAwait(false);

        Assert.NotEmpty(subscribeResult.Subscriptions);
        Assert.Equal(SubAckReasonCode.GrantedQoS0, subscribeResult.Subscriptions[0].SubscribeReasonCode);

        // Clean up
        var disconnectResult = await client.DisconnectAsync().ConfigureAwait(false);
        Assert.True(disconnectResult);
        client.Dispose();
    }

    [Fact]
    public async Task BeforeUnsubscribeExceptionDoesNotPreventUnsubscribeAsync()
    {
        var options = new HiveMQClientOptionsBuilder()
            .WithClientId("BeforeUnsubscribeExceptionTest")
            .Build();

        var client = new HiveMQClient(options);

        // Connect first
        var connectResult = await client.ConnectAsync().ConfigureAwait(false);
        Assert.True(connectResult.ReasonCode == ConnAckReasonCode.Success);

        // Subscribe first
        var subscribeResult = await client.SubscribeAsync(
            "tests/BeforeUnsubscribeExceptionTest",
            QualityOfService.AtMostOnceDelivery)
            .ConfigureAwait(false);
        Assert.NotEmpty(subscribeResult.Subscriptions);

        // Add event handler that throws an exception
        client.BeforeUnsubscribe += (sender, args) => throw new InvalidOperationException("Test exception in BeforeUnsubscribe");

        // Attempt to unsubscribe - should succeed despite the exception
        var unsubscribeResult = await client.UnsubscribeAsync(
            "tests/BeforeUnsubscribeExceptionTest")
            .ConfigureAwait(false);

        Assert.NotEmpty(unsubscribeResult.Subscriptions);
        Assert.Equal(UnsubAckReasonCode.Success, unsubscribeResult.Subscriptions[0].UnsubscribeReasonCode);

        // Clean up
        var disconnectResult = await client.DisconnectAsync().ConfigureAwait(false);
        Assert.True(disconnectResult);
        client.Dispose();
    }

    [Fact]
    public async Task OnMessageReceivedExceptionDoesNotPreventMessageDeliveryAsync()
    {
        var options = new HiveMQClientOptionsBuilder()
            .WithClientId("OnMessageReceivedExceptionTest")
            .Build();

        var client = new HiveMQClient(options);
        var messageReceived = false;

        // Connect first
        var connectResult = await client.ConnectAsync().ConfigureAwait(false);
        Assert.True(connectResult.ReasonCode == ConnAckReasonCode.Success);

        // Subscribe to topic
        var subscribeResult = await client.SubscribeAsync(
            "tests/OnMessageReceivedExceptionTest",
            QualityOfService.AtMostOnceDelivery)
            .ConfigureAwait(false);
        Assert.NotEmpty(subscribeResult.Subscriptions);

        // Add event handler that throws an exception
        client.OnMessageReceived += (sender, args) => throw new InvalidOperationException("Test exception in OnMessageReceived");

        // Add another event handler to verify message was still delivered
        client.OnMessageReceived += (sender, args) => messageReceived = true;

        // Publish a message
        var publishResult = await client.PublishAsync(
            "tests/OnMessageReceivedExceptionTest",
            "test message",
            QualityOfService.AtMostOnceDelivery)
            .ConfigureAwait(false);

        // Wait a bit for message delivery
        await Task.Delay(2000).ConfigureAwait(false);

        // Verify message was still delivered despite the exception
        Assert.True(messageReceived);

        // Clean up
        var disconnectResult = await client.DisconnectAsync().ConfigureAwait(false);
        Assert.True(disconnectResult);
        client.Dispose();
    }

    [Fact]
    public async Task BeforeConnectExceptionDoesNotPreventConnectAsync()
    {
        var options = new HiveMQClientOptionsBuilder()
            .WithClientId("BeforeConnectExceptionTest")
            .Build();

        var client = new HiveMQClient(options);

        // Add event handler that throws an exception
        client.BeforeConnect += (sender, args) => throw new InvalidOperationException("Test exception in BeforeConnect");

        // Attempt to connect - should succeed despite the exception
        var connectResult = await client.ConnectAsync().ConfigureAwait(false);
        Assert.True(connectResult.ReasonCode == ConnAckReasonCode.Success);
        Assert.True(client.IsConnected());

        // Clean up
        var disconnectResult = await client.DisconnectAsync().ConfigureAwait(false);
        Assert.True(disconnectResult);
        client.Dispose();
    }

    [Fact]
    public async Task OnPubAckReceivedExceptionDoesNotPreventQoS1DeliveryAsync()
    {
        var options = new HiveMQClientOptionsBuilder()
            .WithClientId("OnPubAckReceivedExceptionTest")
            .Build();

        var client = new HiveMQClient(options);
        var pubAckReceived = false;

        // Connect first
        var connectResult = await client.ConnectAsync().ConfigureAwait(false);
        Assert.True(connectResult.ReasonCode == ConnAckReasonCode.Success);

        // Add event handler that throws an exception
        client.OnPubAckReceived += (sender, args) => throw new InvalidOperationException("Test exception in OnPubAckReceived");

        // Add another event handler to verify PubAck was still received
        client.OnPubAckReceived += (sender, args) => pubAckReceived = true;

        // Publish a QoS 1 message
        var publishResult = await client.PublishAsync(
            "tests/OnPubAckReceivedExceptionTest",
            "test message",
            QualityOfService.AtLeastOnceDelivery)
            .ConfigureAwait(false);

        // Wait a bit for PubAck delivery
        await Task.Delay(500).ConfigureAwait(false);

        // Verify PubAck was still received despite the exception
        Assert.True(pubAckReceived);
        Assert.NotNull(publishResult.QoS1ReasonCode);

        // Clean up
        var disconnectResult = await client.DisconnectAsync().ConfigureAwait(false);
        Assert.True(disconnectResult);
        client.Dispose();
    }
}
