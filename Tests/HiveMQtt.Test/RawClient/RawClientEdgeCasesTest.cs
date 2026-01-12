namespace HiveMQtt.Test.RawClient;

using System.Threading.Tasks;
using HiveMQtt.Client;
using HiveMQtt.Client.Exceptions;
using HiveMQtt.Client.Options;
using HiveMQtt.MQTT5.ReasonCodes;
using Xunit;

public class RawClientEdgeCasesTest
{
    [Fact]
    public async Task LocalStoreFunctionalityAsync()
    {
        var options = new HiveMQClientOptionsBuilder().WithClientId("RawClientLocalStoreFunctionalityAsync").Build();
        var client = new RawClient(options);

        // Test LocalStore is accessible and can store data
        client.LocalStore["testKey"] = "testValue";
        Assert.True(client.LocalStore.ContainsKey("testKey"));
        Assert.Equal("testValue", client.LocalStore["testKey"]);

        // LocalStore persists across operations
        var connectResult = await client.ConnectAsync().ConfigureAwait(false);
        Assert.True(connectResult.ReasonCode == ConnAckReasonCode.Success);
        Assert.True(client.LocalStore.ContainsKey("testKey"));

        await client.DisconnectAsync().ConfigureAwait(false);
        Assert.True(client.LocalStore.ContainsKey("testKey"));

        client.Dispose();
    }

    [Fact]
    public async Task OptionsPropertyGetSetAsync()
    {
        var options1 = new HiveMQClientOptionsBuilder().WithClientId("RawClientOptionsPropertyGetSetAsync1").Build();
        var client = new RawClient(options1);

        Assert.Equal("RawClientOptionsPropertyGetSetAsync1", client.Options.ClientId);

        // Change options
        var options2 = new HiveMQClientOptionsBuilder().WithClientId("RawClientOptionsPropertyGetSetAsync2").Build();
        client.Options = options2;
        Assert.Equal("RawClientOptionsPropertyGetSetAsync2", client.Options.ClientId);

        client.Dispose();
    }

    [Fact]
    public async Task DisposeBehaviorAsync()
    {
        var options = new HiveMQClientOptionsBuilder().WithClientId("RawClientDisposeBehaviorAsync").Build();
        var client = new RawClient(options);

        var connectResult = await client.ConnectAsync().ConfigureAwait(false);
        Assert.True(connectResult.ReasonCode == ConnAckReasonCode.Success);
        Assert.True(client.IsConnected());

        // Dispose should clean up resources
        client.Dispose();

        // After dispose, IsConnected should return false
        Assert.False(client.IsConnected());

        // Multiple dispose calls should not throw
        client.Dispose();
        client.Dispose();
    }

    [Fact]
    public async Task DisposeWhenConnectedAsync()
    {
        var options = new HiveMQClientOptionsBuilder().WithClientId("RawClientDisposeWhenConnectedAsync").Build();
        var client = new RawClient(options);

        var connectResult = await client.ConnectAsync().ConfigureAwait(false);
        Assert.True(connectResult.ReasonCode == ConnAckReasonCode.Success);
        Assert.True(client.IsConnected());

        // Dispose while connected should clean up gracefully
        client.Dispose();
        Assert.False(client.IsConnected());
    }

    [Fact]
    public async Task MultipleRapidOperationsAsync()
    {
        var testTopic = "tests/RawClientMultipleRapidOperationsAsync";
        var options = new HiveMQClientOptionsBuilder().WithClientId("RawClientMultipleRapidOperationsAsync").Build();
        var client = new RawClient(options);

        var connectResult = await client.ConnectAsync().ConfigureAwait(false);
        Assert.True(connectResult.ReasonCode == ConnAckReasonCode.Success);

        // Perform multiple rapid operations
        var tasks = new List<Task>();

        // Rapid subscribes
        for (var i = 0; i < 5; i++)
        {
            tasks.Add(client.SubscribeAsync($"{testTopic}/{i}"));
        }

        // Rapid publishes
        for (var i = 0; i < 5; i++)
        {
            tasks.Add(client.PublishAsync($"{testTopic}/{i}", $"message {i}"));
        }

        await Task.WhenAll(tasks).ConfigureAwait(false);

        await client.DisconnectAsync().ConfigureAwait(false);
        client.Dispose();
    }

    [Fact]
    public async Task ConstructorWithNullOptionsAsync()
    {
        // Constructor should accept null options and create defaults
        var client = new RawClient(null);
        Assert.NotNull(client);
        Assert.NotNull(client.Options);

        var connectResult = await client.ConnectAsync().ConfigureAwait(false);
        Assert.True(connectResult.ReasonCode == ConnAckReasonCode.Success);

        await client.DisconnectAsync().ConfigureAwait(false);
        client.Dispose();
    }

    [Fact]
    public async Task ConnectionPropertyCachingAsync()
    {
        // This test verifies that connection properties are cached and used correctly
        var testTopic = "tests/RawClientConnectionPropertyCachingAsync";
        var options = new HiveMQClientOptionsBuilder().WithClientId("RawClientConnectionPropertyCachingAsync").Build();
        var client = new RawClient(options);

        var connectResult = await client.ConnectAsync().ConfigureAwait(false);
        Assert.True(connectResult.ReasonCode == ConnAckReasonCode.Success);

        // Properties should be available from connect result
        Assert.NotNull(connectResult.Properties);

        // After connection, cached properties should be used for fast publish operations
        // Publish a simple QoS 0 message (should use fast path)
        await client.PublishAsync(testTopic, "test").ConfigureAwait(false);

        await client.DisconnectAsync().ConfigureAwait(false);
        client.Dispose();
    }

    [Fact]
    public async Task IsConnectedAfterFailedConnectAsync()
    {
        var options = new HiveMQClientOptionsBuilder()
            .WithPort(0) // Invalid port
            .WithClientId("RawClientIsConnectedAfterFailedConnectAsync")
            .Build();

        var client = new RawClient(options);

        Assert.False(client.IsConnected());

        await Assert.ThrowsAsync<HiveMQttClientException>(() => client.ConnectAsync()).ConfigureAwait(false);

        // Should still be disconnected after failed connect
        Assert.False(client.IsConnected());

        client.Dispose();
    }
}
