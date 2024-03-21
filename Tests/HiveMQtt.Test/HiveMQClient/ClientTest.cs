namespace HiveMQtt.Test.HiveMQClient;

using HiveMQtt.Client;
using HiveMQtt.Client.Internal;
using Xunit;

public class ClientTest
{
    [Fact]
    public void Client_Initializes_With_Defaults()
    {
        // var clientOptions = new HiveMQClientOptions();
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
        var client = new HiveMQClient();
        var connectResult = await client.ConnectAsync().ConfigureAwait(false);

        Assert.NotNull(connectResult);
        Assert.True(connectResult.ReasonCode == MQTT5.ReasonCodes.ConnAckReasonCode.Success);

        var result = await client.DisconnectAsync().ConfigureAwait(false);

        Assert.True(result);
    }

    [Fact]
    public async Task ClientStateAsync()
    {
        var client = new HiveMQClient();

        // Validate private internals of the HiveMQtt client

        // Socket Stuff
        Assert.Null(client.Socket);
        Assert.Null(client.Stream);
        Assert.Null(client.Reader);
        Assert.Null(client.Writer);

        // Task Stuff
        Assert.Null(client.ConnectionWriterTask);
        Assert.Null(client.ConnectionReaderTask);
        Assert.Null(client.ReceivedPacketsHandlerTask);
        Assert.Null(client.ConnectionMonitorTask);

        // Queues
        Assert.NotNull(client.SendQueue);
        Assert.NotNull(client.ReceivedQueue);

        // State
        Assert.Equal(ConnectState.Disconnected, client.ConnectState);

        // *************************************
        // Connect and validate internals again
        // *************************************
        var connectResult = await client.ConnectAsync().ConfigureAwait(false);
        Assert.Equal(MQTT5.ReasonCodes.ConnAckReasonCode.Success, connectResult.ReasonCode);

        // Wait for connack
        await Task.Delay(1000).ConfigureAwait(false);

        // Socket Stuff
        Assert.NotNull(client.Socket);
        Assert.NotNull(client.Stream);
        Assert.NotNull(client.Reader);
        Assert.NotNull(client.Writer);

        // Task Stuff
        Assert.NotNull(client.ConnectionWriterTask);
        Assert.NotNull(client.ConnectionReaderTask);
        Assert.NotNull(client.ReceivedPacketsHandlerTask);
        Assert.NotNull(client.ConnectionMonitorTask);

        Assert.Equal(TaskStatus.WaitingForActivation, client.ConnectionWriterTask.Status);
        Assert.Equal(TaskStatus.WaitingForActivation, client.ConnectionReaderTask.Status);
        Assert.Equal(TaskStatus.Running, client.ReceivedPacketsHandlerTask.Status);
        Assert.Equal(TaskStatus.WaitingForActivation, client.ConnectionMonitorTask.Status);

        // Queues
        Assert.NotNull(client.SendQueue);
        Assert.NotNull(client.ReceivedQueue);

        // State
        Assert.Equal(ConnectState.Connected, client.ConnectState);

        // *************************************
        // Do some stuff and validate internals again
        // *************************************

        // Publish QoS 0 (At most once delivery)
        _ = await client.PublishAsync("tests/ClientTest", new string("♚ ♛ ♜ ♝ ♞ ♟ ♔ ♕ ♖ ♗ ♘ ♙")).ConfigureAwait(false);

        var subResult = await client.SubscribeAsync(
                                        "tests/ClientTest",
                                        MQTT5.Types.QualityOfService.AtLeastOnceDelivery).ConfigureAwait(false);

        // Publish QoS 1 (At least once delivery)
        var pubResult = await client.PublishAsync(
                                        "tests/ClientTest",
                                        new string("♚ ♛ ♜ ♝ ♞ ♟ ♔ ♕ ♖ ♗ ♘ ♙"),
                                        MQTT5.Types.QualityOfService.ExactlyOnceDelivery).ConfigureAwait(false);

        // Socket Stuff
        Assert.NotNull(client.Socket);
        Assert.NotNull(client.Stream);
        Assert.NotNull(client.Reader);
        Assert.NotNull(client.Writer);

        // Task Stuff
        Assert.NotNull(client.ConnectionWriterTask);
        Assert.NotNull(client.ConnectionReaderTask);
        Assert.NotNull(client.ReceivedPacketsHandlerTask);
        Assert.NotNull(client.ConnectionMonitorTask);

        Assert.Equal(TaskStatus.WaitingForActivation, client.ConnectionWriterTask.Status);
        Assert.Equal(TaskStatus.WaitingForActivation, client.ConnectionReaderTask.Status);
        Assert.Equal(TaskStatus.Running, client.ReceivedPacketsHandlerTask.Status);
        Assert.Equal(TaskStatus.WaitingForActivation, client.ConnectionMonitorTask.Status);

        // Queues
        Assert.NotNull(client.SendQueue);
        Assert.NotNull(client.ReceivedQueue);

        // *************************************
        // Disconnect and validate internals again
        // *************************************
        var disconnectResult = await client.DisconnectAsync().ConfigureAwait(false);

        // Wait for disconnect to take affect
        await Task.Delay(1000).ConfigureAwait(false);

        // Socket Stuff
        Assert.Null(client.Socket);
        Assert.Null(client.Stream);
        Assert.Null(client.Reader);
        Assert.Null(client.Writer);

        // Task Stuff
        Assert.Null(client.ConnectionWriterTask);
        Assert.Null(client.ConnectionReaderTask);
        Assert.Null(client.ReceivedPacketsHandlerTask);
        Assert.Null(client.ConnectionMonitorTask);

        // Queues
        Assert.NotNull(client.SendQueue);
        Assert.NotNull(client.ReceivedQueue);

        // State
        Assert.Equal(ConnectState.Disconnected, client.ConnectState);
    }
}
