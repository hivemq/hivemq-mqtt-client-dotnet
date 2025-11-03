namespace HiveMQtt.Test.HiveMQClient.WebSocket;

using System;
using System.Threading;
using System.Threading.Tasks;
using HiveMQtt.Client;
using HiveMQtt.MQTT5.ReasonCodes;
using Xunit;

/// <summary>
/// Tests for secure WebSocket (wss://) connections.
/// These tests use broker.hivemq.com which supports WSS connections.
/// Note: These tests are marked to be run manually only as they require an internet connection.
/// </summary>
public class WebSocketTLSTest
{
    // Rate limiting: Minimum delay between connection attempts (2 seconds)
    // broker.hivemq.com has rate limiting to prevent ConnectionRateExceeded errors
    private static readonly TimeSpan MinConnectionDelay = TimeSpan.FromSeconds(2);
#pragma warning disable IDE0090 // Use 'new(...)' - SemaphoreSlim requires explicit constructor parameters
    private static readonly SemaphoreSlim RateLimitSemaphore = new SemaphoreSlim(1, 1);
#pragma warning restore IDE0090
    private static DateTime lastConnectionTime = DateTime.MinValue;

    [Fact]
    public async Task ConnectWssPublicBrokerAsync()
    {
        await RateLimitAsync().ConfigureAwait(false);

        var uniqueClientId = $"WssPublicBrokerTest_{Guid.NewGuid():N}";
        var options = new HiveMQClientOptionsBuilder()
            .WithWebSocketServer("wss://broker.hivemq.com:8884/mqtt")
            .WithClientId(uniqueClientId)
            .Build();

        var client = new HiveMQClient(options);
        var connectResult = await client.ConnectAsync().ConfigureAwait(false);

        Assert.Equal(ConnAckReasonCode.Success, connectResult.ReasonCode);
        Assert.True(client.IsConnected());

        var disconnectResult = await client.DisconnectAsync().ConfigureAwait(false);
        Assert.True(disconnectResult);
        Assert.False(client.IsConnected());

        client.Dispose();
    }

    [Fact]
    public async Task BasicPubSubWssAsync()
    {
        await RateLimitAsync().ConfigureAwait(false);

        var testTopic = "tests/BasicPubSubWssAsync";

        // JSON string is intentional test data
        var testPayload = /*lang=json*/ "{\"test\": \"wss_connection\"}";
        var uniqueClientId = $"BasicPubSubWssAsync_{Guid.NewGuid():N}";
        var options = new HiveMQClientOptionsBuilder()
            .WithWebSocketServer("wss://broker.hivemq.com:8884/mqtt")
            .WithClientId(uniqueClientId)
            .Build();
        options.ConnectTimeoutInMs = 10000; // Increase timeout for internet connections

        var client = new HiveMQClient(options);
        var connectResult = await client.ConnectAsync().ConfigureAwait(false);
        Assert.Equal(ConnAckReasonCode.Success, connectResult.ReasonCode);

        var taskCompletionSource = new TaskCompletionSource<bool>();

        client.OnMessageReceived += (sender, args) =>
        {
            Assert.Equal(testTopic, args.PublishMessage.Topic);
            Assert.Equal(testPayload, args.PublishMessage.PayloadAsString);
            taskCompletionSource.SetResult(true);
        };

        var subResult = await client.SubscribeAsync(testTopic).ConfigureAwait(false);
        Assert.NotEmpty(subResult.Subscriptions);

        _ = await client.PublishAsync(testTopic, testPayload).ConfigureAwait(false);

        var taskResult = await taskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(10)).ConfigureAwait(false);
        Assert.True(taskResult);

        await client.DisconnectAsync().ConfigureAwait(false);
        client.Dispose();
    }

    [Fact]
    public async Task ConnectWssWithAllowInvalidCertsAsync()
    {
        await RateLimitAsync().ConfigureAwait(false);

        // Test that AllowInvalidBrokerCertificates option works for WSS
        // Note: broker.hivemq.com has valid certs, but we're testing the option works
        var uniqueClientId = $"WssAllowInvalidCertsTest_{Guid.NewGuid():N}";
        var options = new HiveMQClientOptionsBuilder()
            .WithWebSocketServer("wss://broker.hivemq.com:8884/mqtt")
            .WithClientId(uniqueClientId)
            .WithAllowInvalidBrokerCertificates(true)
            .Build();
        options.ConnectTimeoutInMs = 10000; // Increase timeout for internet connections

        var client = new HiveMQClient(options);
        var connectResult = await client.ConnectAsync().ConfigureAwait(false);

        Assert.Equal(ConnAckReasonCode.Success, connectResult.ReasonCode);
        Assert.True(client.IsConnected());

        await client.DisconnectAsync().ConfigureAwait(false);
        client.Dispose();
    }

    /// <summary>
    /// Ensures rate limiting by waiting if necessary before connecting.
    /// Thread-safe implementation using SemaphoreSlim.
    /// </summary>
    private static async Task RateLimitAsync()
    {
        await RateLimitSemaphore.WaitAsync().ConfigureAwait(false);
        try
        {
            var timeSinceLastConnection = DateTime.UtcNow - lastConnectionTime;
            if (timeSinceLastConnection < MinConnectionDelay)
            {
                var delayNeeded = MinConnectionDelay - timeSinceLastConnection;
                await Task.Delay(delayNeeded).ConfigureAwait(false);
            }

            lastConnectionTime = DateTime.UtcNow;
        }
        finally
        {
            RateLimitSemaphore.Release();
        }
    }
}
