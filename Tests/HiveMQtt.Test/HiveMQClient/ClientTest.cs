namespace HiveMQtt.Test.HiveMQClient;

using HiveMQtt.Client;
using HiveMQtt.Client.Internal;
using Xunit;

public class ClientTest
{
    [Fact]
    public void Client_Initializes_With_Defaults()
    {
        var client = new HiveMQClient();

        Assert.NotNull(client.Options.ClientId);
        Assert.True(client.Options.ClientId.Length < 24);
        Assert.Equal("127.0.0.1", client.Options.Host);
        Assert.Equal(1883, client.Options.Port);
        Assert.Equal(60, client.Options.KeepAlive);
        Assert.True(client.Options.CleanStart);
        Assert.Null(client.Options.UserName);
        Assert.Null(client.Options.Password);

        Assert.NotNull(client);

        client.Dispose();
    }

    [Fact]
    public async Task Client_Has_Default_Connect_Async()
    {
        var options = new HiveMQClientOptionsBuilder().
                    WithClientId("Client_Has_Default_Connect_Async").Build();
        var client = new HiveMQClient(options);
        var connectResult = await client.ConnectAsync().ConfigureAwait(false);

        Assert.NotNull(connectResult);
        Assert.True(connectResult.ReasonCode == MQTT5.ReasonCodes.ConnAckReasonCode.Success);

        var result = await client.DisconnectAsync().ConfigureAwait(false);

        Assert.True(result);
    }

    [Fact]
    public async Task ClientStateAsync()
    {
        var options = new HiveMQClientOptionsBuilder().
                    WithClientId("ClientTest.ClientStateAsync").Build();
        var client = new HiveMQClient(options);

        // Validate private internals of the HiveMQtt client

        // Task Stuff
        Assert.Null(client.Connection.ConnectionWriterTask);
        Assert.Null(client.Connection.ConnectionReaderTask);
        Assert.Null(client.Connection.ReceivedPacketsHandlerTask);
        Assert.Null(client.Connection.ConnectionMonitorTask);

        // Queues
        Assert.NotNull(client.Connection.SendQueue);
        Assert.NotNull(client.Connection.ReceivedQueue);

        // State
        Assert.Equal(ConnectState.Disconnected, client.Connection.State);

        // *************************************
        // Connect and validate internals again
        // *************************************
        var connectResult = await client.ConnectAsync().ConfigureAwait(false);
        Assert.Equal(MQTT5.ReasonCodes.ConnAckReasonCode.Success, connectResult.ReasonCode);

        // Wait for connack
        await Task.Delay(1000).ConfigureAwait(false);

        // Task Stuff
        Assert.NotNull(client.Connection.ConnectionWriterTask);
        Assert.NotNull(client.Connection.ConnectionReaderTask);
        Assert.NotNull(client.Connection.ReceivedPacketsHandlerTask);
        Assert.NotNull(client.Connection.ConnectionMonitorTask);

        Assert.Equal(TaskStatus.WaitingForActivation, client.Connection.ConnectionWriterTask.Status);
        Assert.Equal(TaskStatus.WaitingForActivation, client.Connection.ConnectionReaderTask.Status);
        Assert.Equal(TaskStatus.WaitingForActivation, client.Connection.ReceivedPacketsHandlerTask.Status);
        Assert.Equal(TaskStatus.WaitingForActivation, client.Connection.ConnectionMonitorTask.Status);

        // Queues
        Assert.NotNull(client.Connection.SendQueue);
        Assert.NotNull(client.Connection.ReceivedQueue);

        // State
        Assert.Equal(ConnectState.Connected, client.Connection.State);

        // *************************************
        // Do some stuff and validate internals again
        // *************************************

        // Publish QoS 0 (At most once delivery)
        _ = await client.PublishAsync("tests/ClientTest", new string("♚ ♛ ♜ ♝ ♞ ♟ ♔ ♕ ♖ ♗ ♘ ♙")).ConfigureAwait(false);

        client.OnMessageReceived += (sender, args) => { };

        var subResult = await client.SubscribeAsync(
                                        "tests/ClientTest",
                                        MQTT5.Types.QualityOfService.AtLeastOnceDelivery).ConfigureAwait(false);

        // Publish QoS 1 (At least once delivery)
        var pubResult = await client.PublishAsync(
                                        "tests/ClientTest",
                                        new string("♚ ♛ ♜ ♝ ♞ ♟ ♔ ♕ ♖ ♗ ♘ ♙"),
                                        MQTT5.Types.QualityOfService.ExactlyOnceDelivery).ConfigureAwait(false);

        // Task Stuff
        Assert.NotNull(client.Connection.ConnectionWriterTask);
        Assert.NotNull(client.Connection.ConnectionReaderTask);
        Assert.NotNull(client.Connection.ReceivedPacketsHandlerTask);
        Assert.NotNull(client.Connection.ConnectionMonitorTask);

        Assert.Equal(TaskStatus.WaitingForActivation, client.Connection.ConnectionWriterTask.Status);
        Assert.Equal(TaskStatus.WaitingForActivation, client.Connection.ConnectionReaderTask.Status);
        Assert.Equal(TaskStatus.WaitingForActivation, client.Connection.ReceivedPacketsHandlerTask.Status);
        Assert.Equal(TaskStatus.WaitingForActivation, client.Connection.ConnectionMonitorTask.Status);

        // Queues
        Assert.NotNull(client.Connection.SendQueue);
        Assert.NotNull(client.Connection.ReceivedQueue);

        // *************************************
        // Disconnect and validate internals again
        // *************************************
        var disconnectResult = await client.DisconnectAsync().ConfigureAwait(false);

        // Wait for disconnect to take affect
        await Task.Delay(1000).ConfigureAwait(false);

        // Task Stuff
        Assert.Null(client.Connection.ConnectionWriterTask);
        Assert.Null(client.Connection.ConnectionReaderTask);
        Assert.Null(client.Connection.ReceivedPacketsHandlerTask);
        Assert.Null(client.Connection.ConnectionMonitorTask);

        // Queues
        Assert.NotNull(client.Connection.SendQueue);
        Assert.NotNull(client.Connection.ReceivedQueue);

        // State
        Assert.Equal(ConnectState.Disconnected, client.Connection.State);
    }
}
