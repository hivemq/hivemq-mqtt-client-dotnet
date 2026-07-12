namespace HiveMQtt.Test.HiveMQClient.Plan;

using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using FluentAssertions;
using HiveMQtt.Client;
using HiveMQtt.Client.Events;
using NUnit.Framework;
using Xunit;

[TestFixture]
[Collection("Broker")]
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

    [Test]
    public async Task Client_Receives_PingResp_And_Stays_Connected_Async()
    {
        var options = new HiveMQClientOptionsBuilder()
                        .WithKeepAlive(3)
                        .Build();
        var client = new HiveMQClient(options);

        var pingReqSent = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var pingRespReceived = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

        void OnPingSent(object? sender, OnPingReqSentEventArgs e) => pingReqSent.TrySetResult(true);
        void OnPingResp(object? sender, OnPingRespReceivedEventArgs e) => pingRespReceived.TrySetResult(true);

        client.OnPingReqSent += OnPingSent;
        client.OnPingRespReceived += OnPingResp;

        try
        {
            var connectResult = await client.ConnectAsync().ConfigureAwait(false);
            connectResult.Should().NotBeNull();

            await pingReqSent.Task.WaitAsync(TimeSpan.FromSeconds(6)).ConfigureAwait(false);
            await pingRespReceived.Task.WaitAsync(TimeSpan.FromSeconds(5)).ConfigureAwait(false);

            client.IsConnected().Should().BeTrue();
        }
        finally
        {
            client.OnPingReqSent -= OnPingSent;
            client.OnPingRespReceived -= OnPingResp;
            if (client.IsConnected())
            {
                await client.DisconnectAsync().ConfigureAwait(false);
            }

            client.Dispose();
        }
    }

    [Test]
    public async Task Missing_PingResp_Triggers_Unclean_Disconnect_Async()
    {
        // Silent broker: accepts CONNECT, sends CONNACK, never answers PINGREQ.
        // This simulates a half-open / unresponsive peer for keep-alive timeout detection.
        using var silentBroker = new SilentMqttBroker();
        await silentBroker.StartAsync().ConfigureAwait(false);

        var options = new HiveMQClientOptionsBuilder()
                        .WithBroker("127.0.0.1")
                        .WithPort(silentBroker.Port)
                        .WithKeepAlive(2)
                        .Build();
        options.ResponseTimeoutInMs = 2000;
        options.ConnectTimeoutInMs = 5000;

        var client = new HiveMQClient(options);
        var afterDisconnect = new TaskCompletionSource<AfterDisconnectEventArgs>(TaskCreationOptions.RunContinuationsAsynchronously);
        var pingReqSent = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

        void OnAfterDisconnect(object? sender, AfterDisconnectEventArgs e) => afterDisconnect.TrySetResult(e);
        void OnPingSent(object? sender, OnPingReqSentEventArgs e) => pingReqSent.TrySetResult(true);

        client.AfterDisconnect += OnAfterDisconnect;
        client.OnPingReqSent += OnPingSent;

        try
        {
            var connectResult = await client.ConnectAsync().ConfigureAwait(false);
            connectResult.Should().NotBeNull();
            client.IsConnected().Should().BeTrue();

            // Wait for PINGREQ to be sent
            await pingReqSent.Task.WaitAsync(TimeSpan.FromSeconds(5)).ConfigureAwait(false);

            // Detection ≈ KeepAlive + ResponseTimeoutInMs + monitor poll (2s) + AfterDisconnect delay (1s)
            var disconnectArgs = await afterDisconnect.Task.WaitAsync(TimeSpan.FromSeconds(15)).ConfigureAwait(false);

            disconnectArgs.CleanDisconnect.Should().BeFalse(
                "missing PINGRESP should trigger an unclean disconnect");
            client.IsConnected().Should().BeFalse();
        }
        finally
        {
            client.AfterDisconnect -= OnAfterDisconnect;
            client.OnPingReqSent -= OnPingSent;
            client.Dispose();
        }
    }

    /// <summary>
    /// Minimal MQTT 5 broker that accepts a connection and CONNACK, then never sends PINGRESP.
    /// </summary>
    private sealed class SilentMqttBroker : IDisposable
    {
        private TcpListener? listener;
        private CancellationTokenSource? cts;
        private Task? acceptLoop;

        public int Port { get; private set; }

        public Task StartAsync()
        {
            this.listener = new TcpListener(IPAddress.Loopback, 0);
            this.listener.Start();
            this.Port = ((IPEndPoint)this.listener.LocalEndpoint).Port;
            this.cts = new CancellationTokenSource();
            this.acceptLoop = this.AcceptLoopAsync(this.cts.Token);
            return Task.CompletedTask;
        }

        private async Task AcceptLoopAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                TcpClient? tcpClient = null;
                try
                {
                    // Parameterless AcceptTcpClientAsync for net6.0 compatibility;
                    // listener.Stop() in Dispose unblocks and exits the loop.
                    tcpClient = await this.listener!.AcceptTcpClientAsync().ConfigureAwait(false);
                    _ = this.HandleClientAsync(tcpClient, cancellationToken);
                }
                catch (ObjectDisposedException)
                {
                    tcpClient?.Dispose();
                    break;
                }
                catch (SocketException)
                {
                    tcpClient?.Dispose();
                    if (cancellationToken.IsCancellationRequested)
                    {
                        break;
                    }
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    tcpClient?.Dispose();
                    break;
                }
                catch
                {
                    tcpClient?.Dispose();
                }
            }
        }

        private async Task HandleClientAsync(TcpClient tcpClient, CancellationToken cancellationToken)
        {
            using (tcpClient)
            {
                var stream = tcpClient.GetStream();
                var buffer = new byte[4096];

                // Read CONNECT (or enough of it)
                var read = await stream.ReadAsync(buffer.AsMemory(0, buffer.Length), cancellationToken).ConfigureAwait(false);
                if (read <= 0)
                {
                    return;
                }

                // MQTT 5 CONNACK: Success, no session present, empty properties
                // Fixed header 0x20, remaining length 3, flags 0, reason 0, property length 0
                byte[] connAck = new byte[] { 0x20, 0x03, 0x00, 0x00, 0x00 };
                await stream.WriteAsync(connAck.AsMemory(0, connAck.Length), cancellationToken).ConfigureAwait(false);
                await stream.FlushAsync(cancellationToken).ConfigureAwait(false);

                // Drain inbound packets (PINGREQ etc.) but never respond — simulates half-open / silent peer
                while (!cancellationToken.IsCancellationRequested && tcpClient.Connected)
                {
                    read = await stream.ReadAsync(buffer.AsMemory(0, buffer.Length), cancellationToken).ConfigureAwait(false);
                    if (read <= 0)
                    {
                        break;
                    }
                }
            }
        }

        public void Dispose()
        {
            try
            {
                this.cts?.Cancel();
            }
            catch
            {
                // ignore
            }

            this.listener?.Stop();
            this.cts?.Dispose();
        }
    }
}
