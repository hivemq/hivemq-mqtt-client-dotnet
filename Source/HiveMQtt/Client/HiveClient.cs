namespace HiveMQtt.Client;

using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO.Pipelines;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

using HiveMQtt.Client.Events;
using HiveMQtt.Client.Internal;
using HiveMQtt.Client.Options;
using HiveMQtt.Client.Results;
using HiveMQtt.MQTT5;
using HiveMQtt.MQTT5.Packets;
using HiveMQtt.MQTT5.ReasonCodes;
using HiveMQtt.MQTT5.Types;

using Microsoft.Extensions.Logging;

/// <summary>
/// The excellent, superb and slightly wonderful HiveMQ MQTT Client.
/// Fully MQTT compliant and compatible with all respectable MQTT Brokers because sharing is caring
/// and MQTT is awesome.
/// </summary>
public partial class HiveClient : IDisposable, IHiveClient
{
    private ILogger<HiveClient> _logger;

    public void AttachLogger(ILogger<HiveClient> logger)
    {
        _logger = logger;
    }

    private readonly ConcurrentQueue<ControlPacket> sendQueue = new();
    private readonly ConcurrentQueue<ControlPacket> receiveQueue = new();

    private int lastPacketId = 0;

    private Socket? socket;
    private NetworkStream? stream;
    private PipeReader? reader;
    private PipeWriter? writer;

    private Task<bool>? trafficOutflowProcessor;

    private Task<bool>? trafficInflowProcessor;

    internal ConnectState connectState = ConnectState.Disconnected;

    private bool disposed = false;

    public HiveClient(HiveClientOptions? options = null)
    {
        Trace.Listeners.Add(new TextWriterTraceListener(Console.Out));
        Trace.AutoFlush = true;
        options ??= new HiveClientOptions();
        this.Options = options;
    }

    /// <inheritdoc />
    public Dictionary<string, string> LocalStore { get; } = new();

    public HiveClientOptions Options { get; set; }

    public List<Subscription> Subscriptions { get; } = new();

    internal MQTT5Properties? ConnectionProperties { get; }

    public bool IsConnected()
    {
        return this.connectState == ConnectState.Connected;
    }

    /// <inheritdoc />
    public async Task<ConnectResult> ConnectAsync()
    {
        this.connectState = ConnectState.Connecting;

        // Fire the corresponding event
        this.BeforeConnectEventLauncher(this.Options);

        var socketIsConnected = await this.ConnectSocketAsync().ConfigureAwait(false);

        if (!socketIsConnected || this.socket == null)
        {
            // FIXME: Use a real exception
            throw new InvalidOperationException("Failed to connect socket");
        }

        // Setup the Pipeline
        this.stream = new NetworkStream(this.socket);
        this.reader = PipeReader.Create(this.stream);
        this.writer = PipeWriter.Create(this.stream);

        // Start the traffic processors
        this.trafficOutflowProcessor = this.TrafficOutflowProcessorAsync();
        this.trafficInflowProcessor = this.TrafficInflowProcessorAsync();

        var taskCompletionSource = new TaskCompletionSource<ConnAckPacket>();

        EventHandler<OnConnAckReceivedEventArgs> eventHandler = (sender, args) =>
        {
            taskCompletionSource.SetResult(args.ConnAckPacket);
        };
        this.OnConnAckReceived += eventHandler;

        // Construct the MQTT Connect packet and queue to send
        var connPacket = new ConnectPacket(this.Options);
        this.sendQueue.Enqueue(connPacket);

        // FIXME: Cancellation token and better timeout value
        ConnAckPacket connAck;
        ConnectResult connectResult;
        try
        {
            connAck = await taskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(5)).ConfigureAwait(false);
        }
        catch (System.TimeoutException ex)
        {
            // log.Error(string.Format("Connect timeout.  No response received in time.", ex);
            throw;
        }
        finally
        {
            // Remove the event handler
            this.OnConnAckReceived -= eventHandler;
        }

        if (connAck.ReasonCode == ConnAckReasonCode.Success)
        {
            this.connectState = ConnectState.Connected;
        }
        else
        {
            this.connectState = ConnectState.Disconnected;
        }

        connectResult = new ConnectResult(connAck.ReasonCode, connAck.SessionPresent, connAck.Properties);

        // Data massage: This class is used for end users.  Let's prep the data so it's easily understandable.
        // If the Session Expiry Interval is absent the value in the CONNECT Packet used.
        connectResult.Properties.SessionExpiryInterval ??= (UInt32)this.Options.SessionExpiryInterval;

        // Fire the corresponding event
        this.AfterConnectEventLauncher(connectResult);

        return connectResult;
    }

    /// <inheritdoc />
    public async Task<bool> DisconnectAsync(DisconnectOptions? options = null)
    {
        if (this.connectState != ConnectState.Connected)
        {
            // log.Error("Disconnect failed.  Client is not connected.");
            return false;
        }

        options ??= new DisconnectOptions();

        var disconnectPacket = new DisconnectPacket
        {
            DisconnectReasonCode = options.ReasonCode,
        };

        this.sendQueue.Enqueue(disconnectPacket);

        // Once this is set, no more incoming packets or outgoing will be accepted
        this.connectState = ConnectState.Disconnecting;

        await Task.Delay(1000).ConfigureAwait(false);

        // Shutdown the traffic processors
        this.trafficOutflowProcessor = null;
        this.trafficInflowProcessor = null;

        // Shutdown the pipeline
        this.reader = null;
        this.writer = null;

        // Shutdown the socket
        this.socket?.Shutdown(SocketShutdown.Both);
        this.socket?.Close();

        this.connectState = ConnectState.Disconnected;

        this.sendQueue.Clear();
        this.receiveQueue.Clear();

        return true;
    }

    /// <summary>
    /// Publish a message to an MQTT topic.
    /// </summary>
    /// <param name="message">The <seealso cref="MQTT5PublishMessage"/> for the Publish.</param>
    /// <returns>A <seealso cref="PublishResult"/> representing the result of the publish operation.</returns>
    public async Task<PublishResult> PublishAsync(MQTT5PublishMessage message)
    {
        message.Validate();

        var packetIdentifier = this.GeneratePacketIdentifier();
        var packet = new PublishPacket(message, (ushort)packetIdentifier);
        _ = await this.writer.WriteAsync(packet.Encode()).ConfigureAwait(false);

        // TODO: Get the packet identifier from the PublishAck packet
        return new PublishResult();
    }

    /// <summary>
    /// Publish a message to an MQTT topic.
    /// <para>
    /// This is a convenience method that routes to <seealso cref="PublishAsync(MQTT5PublishMessage)"/>.
    /// </para>
    /// </summary>
    /// <param name="topic">The string topic to publish to.</param>
    /// <param name="payload">The string message to publish.</param>
    /// <returns>A <seealso cref="PublishResult"/> representing the result of the publish operation.</returns>
    public async Task<PublishResult> PublishAsync(string topic, string payload, QualityOfService qos = QualityOfService.AtMostOnceDelivery)
    {
        var message = new MQTT5PublishMessage
        {
            Topic = topic,
            Payload = Encoding.ASCII.GetBytes(payload),
            QoS = qos,
        };

        return await this.PublishAsync(message).ConfigureAwait(false);
    }

    /// <summary>
    /// Publish a message to an MQTT topic.
    /// <para>
    /// This is a convenience method that routes to <seealso cref="PublishAsync(MQTT5PublishMessage)"/>.
    /// </para>
    /// </summary>
    /// <param name="topic">The string topic to publish to.</param>
    /// <param name="payload">The UTF-8 encoded array of bytes to publish.</param>
    /// <returns>A <seealso cref="PublishResult"/> representing the result of the publish operation.</returns>
    public async Task<PublishResult> PublishAsync(string topic, byte[] payload, QualityOfService qos = QualityOfService.AtMostOnceDelivery)
    {
        // Note: Should we validate encoding here?
        var message = new MQTT5PublishMessage
        {
            Topic = topic,
            Payload = payload,
            QoS = qos,
        };

        return await this.PublishAsync(message).ConfigureAwait(false);
    }

    /// <summary>
    /// Subscribe with a single topic filter on the MQTT broker.
    /// </summary>
    /// <example>
    /// Usage example:
    /// <code>
    /// var result = await client.SubscribeAsync("my/topic", QualityOfService.AtLeastOnceDelivery);
    /// </code>
    /// </example>
    /// <param name="topic">The topic filter to subscribe to.</param>
    /// <param name="qos">The <seealso cref="QualityOfService">QualityOfService</seealso> level to subscribe with.</param>
    /// <returns>SubscribeResult reflecting the result of the operation.</returns>
    public async Task<SubscribeResult> SubscribeAsync(string topic, QualityOfService qos = QualityOfService.AtMostOnceDelivery)
    {
        var options = new SubscribeOptions();

        var tf = new TopicFilter
        {
            Topic = topic,
            QoS = qos,
        };
        options.TopicFilters.Add(tf);

        return await this.SubscribeAsync(options).ConfigureAwait(false);
    }

    /// <summary>
    /// Subscribe with SubscribeOptions on the MQTT broker.
    /// </summary>
    /// <example>
    /// Usage example:
    /// <code>
    /// var options = new SubscribeOptions();
    /// options.TopicFilters.Add(new TopicFilter { Topic = "foo", QoS = QualityOfService.AtLeastOnceDelivery });
    /// options.TopicFilters.Add(new TopicFilter { Topic = "bar", QoS = QualityOfService.AtMostOnceDelivery });
    /// var result = await client.SubscribeAsync(options);
    /// </code>
    /// </example>
    /// <param name="options">The options for the subscribe request.</param>
    /// <returns>SubscribeResult reflecting the result of the operation.</returns>
    public async Task<SubscribeResult> SubscribeAsync(SubscribeOptions options)
    {
        // Fire the corresponding event
        this.BeforeSubscribeEventLauncher(options);

        var packetIdentifier = this.GeneratePacketIdentifier();
        var subscribePacket = new SubscribePacket(options, (ushort)packetIdentifier);

        var taskCompletionSource = new TaskCompletionSource<SubAckPacket>();

        // FIXME: We should only ever have one subscribe in flight at any time (for now)

        EventHandler<OnSubAckReceivedEventArgs> eventHandler = (sender, args) =>
        {
            taskCompletionSource.SetResult(args.SubAckPacket);
        };
        this.OnSubAckReceived += eventHandler;

        // Construct the MQTT Connect packet and queue to send
        this.sendQueue.Enqueue(subscribePacket);

        // FIXME: Cancellation token and better timeout value
        SubAckPacket subAck;
        SubscribeResult subscribeResult;
        try
        {
            subAck = await taskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(5)).ConfigureAwait(false);
            // FIXME: Validate that the packet identifier matches
        }
        catch (System.TimeoutException ex)
        {
            // log.Error(string.Format("Connect timeout.  No response received in time.", ex);
            throw;
        }
        finally
        {
            // Remove the event handler
            this.OnSubAckReceived -= eventHandler;
        }

        subscribeResult = new SubscribeResult(options, subAck);

        // Add the subscriptions to the client
        this.Subscriptions.AddRange(subscribeResult.Subscriptions);

        // Fire the corresponding event
        this.AfterSubscribeEventLauncher(subscribeResult);

        return subscribeResult;
    }

    public async Task<UnsubscribeResult> UnsubscribeAsync(string topic)
    {
        foreach (var subscription in this.Subscriptions)
        {
            if (subscription.TopicFilter.Topic == topic)
            {
                return await this.UnsubscribeAsync(subscription).ConfigureAwait(false);
            }
        }

        // FIXME: Exception types
        throw new KeyNotFoundException("No subscription found for topic: " + topic);
    }

    public async Task<UnsubscribeResult> UnsubscribeAsync(Subscription subscription)
    {
        var subscriptions = new List<Subscription> { subscription };
        return await this.UnsubscribeAsync(subscriptions).ConfigureAwait(false);
    }

    public async Task<UnsubscribeResult> UnsubscribeAsync(List<Subscription> subscriptions)
    {
        // Fire the corresponding event
        this.BeforeUnsubscribeEventLauncher(subscriptions);

        var packetIdentifier = this.GeneratePacketIdentifier();
        var unsubscribePacket = new UnsubscribePacket(subscriptions, (ushort)packetIdentifier);

        var taskCompletionSource = new TaskCompletionSource<UnsubAckPacket>();
        EventHandler<OnUnsubAckReceivedEventArgs> eventHandler = (sender, args) =>
        {
            taskCompletionSource.SetResult(args.UnsubAckPacket);
        };
        this.OnUnsubAckReceived += eventHandler;

        this.sendQueue.Enqueue(unsubscribePacket);

        // FIXME: Cancellation token and better timeout value
        UnsubAckPacket unsubAck;
        UnsubscribeResult unsubscribeResult;
        try
        {
            unsubAck = await taskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(5)).ConfigureAwait(false);
            // FIXME: Validate that the packet identifier matches
        }
        catch (System.TimeoutException ex)
        {
            // log.Error(string.Format("Connect timeout.  No response received in time.", ex);
            throw;
        }
        finally
        {
            // Remove the event handler
            this.OnUnsubAckReceived -= eventHandler;
        }

        // Prepare the result
        unsubscribeResult = new UnsubscribeResult
        {
            Subscriptions = unsubscribePacket.Subscriptions,
        };

        var counter = 0;
        foreach (var reasonCode in unsubAck.ReasonCodes)
        {
            unsubscribeResult.Subscriptions[counter].UnsubscribeReasonCode = reasonCode;
            if (reasonCode == UnsubAckReasonCode.Success)
            {
                // Remove the subscription from the client
                this.Subscriptions.Remove(unsubscribeResult.Subscriptions[counter]);
            }
        }

        return unsubscribeResult;
    }

    /// <summary>
    /// Make a TCP connection to a remote broker.
    /// </summary>
    /// <returns>A boolean representing the success or failure of the operation.</returns>
    internal async Task<bool> ConnectSocketAsync()
    {
        Console.WriteLine($"Connecting socket... {this.Options.Host}:{this.Options.Port}");

        var ipHostInfo = await Dns.GetHostEntryAsync(this.Options.Host).ConfigureAwait(false);
        var ipAddress = ipHostInfo.AddressList[1];
        IPEndPoint ipEndPoint = new(ipAddress, this.Options.Port);

        this.socket = new(ipEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        await this.socket.ConnectAsync(ipEndPoint).ConfigureAwait(false);

        Console.WriteLine($"Socket connected to {this.socket.RemoteEndPoint}");

        return this.socket.Connected;
    }

    /// <summary>
    /// Generate a packet identifier.
    /// </summary>
    /// <returns>A valid packet identifier.</returns>
    protected int GeneratePacketIdentifier()
    {
        if (this.lastPacketId == ushort.MaxValue)
        {
            this.lastPacketId = 0;
        }

        return Interlocked.Increment(ref this.lastPacketId);
    }

    /// <summary>
    /// https://learn.microsoft.com/en-us/dotnet/api/system.idisposable?view=net-6.0.
    /// </summary>
    public void Dispose()
    {
        this.Dispose(disposing: true);
        /*
          This object will be cleaned up by the Dispose method.
          Therefore, you should call GC.SuppressFinalize to
          take this object off the finalization queue
          and prevent finalization code for this object
          from executing a second time.
        */
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// https://learn.microsoft.com/en-us/dotnet/api/system.idisposable?view=net-6.0
    /// Dispose(bool disposing) executes in two distinct scenarios.
    /// If disposing equals true, the method has been called directly
    /// or indirectly by a user's code. Managed and unmanaged resources
    /// can be disposed.
    /// If disposing equals false, the method has been called by the
    /// runtime from inside the finalizer and you should not reference
    /// other objects. Only unmanaged resources can be disposed.
    /// </summary>
    /// <param name="disposing">fixme.</param>
    protected virtual void Dispose(bool disposing)
    {
        // Check to see if Dispose has already been called.
        if (!this.disposed)
        {
            // If disposing equals true, dispose all managed
            // and unmanaged resources.
            if (disposing)
            {
                // Dispose managed resources.
                { }
            }

            // Call the appropriate methods to clean up
            // unmanaged resources here.
            // If disposing is false,
            // only the following code is executed.
            { }

            // Note disposing has been done.
            this.disposed = true;
        }
    }
}
