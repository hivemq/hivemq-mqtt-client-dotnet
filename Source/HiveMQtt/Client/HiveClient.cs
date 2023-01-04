namespace HiveMQtt.Client;

using System;
using System.Collections.Concurrent;
using System.IO.Pipelines;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using HiveMQtt.Client.Options;
using HiveMQtt.Client.Results;
using HiveMQtt.MQTT5;
using HiveMQtt.MQTT5.Connect;
using HiveMQtt.MQTT5.Publish;
using HiveMQtt.MQTT5.Subscribe;
using HiveMQtt.MQTT5.Types;

/// <summary>
/// The excellent, superb and slightly wonderful HiveMQ MQTT Client.
/// Fully MQTT compliant and compatible with all respectable MQTT Brokers because sharing is caring
/// and MQTT is awesome.
/// </summary>
public class HiveClient : IDisposable, IHiveClient
{
    private int lastPacketId;

    private readonly ConcurrentQueue<byte[]> sendQueue;
    private readonly ConcurrentQueue<ControlPacket> receiveQueue;

    private Socket? socket;
    private NetworkStream? stream;
    private PipeReader? reader;
    private PipeWriter? writer;

    private bool disposed;

    public HiveClient(HiveClientOptions? options = null)
    {
        this.lastPacketId = 0;
        options ??= new HiveClientOptions();
        this.Options = options;

        this.Subscriptions = new List<Subscription>();

        this.sendQueue = new ConcurrentQueue<byte[]>();
        this.receiveQueue = new ConcurrentQueue<ControlPacket>();

        this.disposed = false;
    }

    public HiveClientOptions Options { get; set; }

    internal MQTT5Properties? ConnectionProperties { get; }

    public List<Subscription> Subscriptions { get; }

    public bool IsConnected()
    {
        // FIXME: Add MQTT connection state check
        if ((this.socket is not null) && this.socket.Connected)
        {
            return true;
        }

        return false;
    }

    /// <inheritdoc />
    public async Task<ConnectResult> ConnectAsync()
    {
        var socketIsConnected = await this.ConnectSocketAsync().ConfigureAwait(false);

        if (!socketIsConnected || this.socket == null)
        {
            // FIXME: Use a real exception
            throw new InvalidOperationException("Failed to connect socket");
        }

        this.stream = new NetworkStream(this.socket);
        this.reader = PipeReader.Create(this.stream);
        this.writer = PipeWriter.Create(this.stream);

        // Construct the MQTT Connect packet
        var connPacket = new ConnectPacket(this.Options);
        var writeResult = await this.writer.WriteAsync(connPacket.Encode()).ConfigureAwait(false);
        var flushResult = await this.writer.FlushAsync().ConfigureAwait(false);

        // FIXME: check writeResult and flushResult - IsCompleted & IsCanceled

        // Read the MQTT ConnAck packet
        SequencePosition consumed;

        ReadResult readResult;
        ControlPacket receivedPacket;
        while(true)
        {
            readResult = await this.reader.ReadAsync().ConfigureAwait(false);
            receivedPacket = PacketDecoder.Decode(readResult.Buffer, out consumed);

            if (receivedPacket.ControlPacketType == ControlPacketType.ConnAck)
            {
                var connAck = (ConnAckPacket)receivedPacket;
                this.reader.AdvanceTo(consumed);

                var connectResult = new ConnectResult(connAck.ReasonCode, connAck.SessionPresent, connAck.Properties);

                // Data massage: This class is used for end users.  Let's prep the data so it's easily understandable.
                // If the Session Expiry Interval is absent the value in the CONNECT Packet used.
                connectResult.Properties.SessionExpiryInterval ??= (UInt32)this.Options.SessionExpiryInterval;
                return connectResult;
            }
            else if (receivedPacket.ControlPacketType == ControlPacketType.Reserved)
            {
                if (consumed.Equals(readResult.Buffer.End))
                {
                    // FIXME: Define and change to a real exception
                    throw new InvalidOperationException("Malformed packet");
                }
                continue;
            }
            else
            {
                throw new InvalidOperationException("Unexpected packet");
            }
        }
    }

    /// <inheritdoc />
    public async Task<bool> DisconnectAsync(DisconnectOptions? options = null)
    {
        options ??= new DisconnectOptions();

        var packet = new DisconnectPacket
        {
            DisconnectReasonCode = options.ReasonCode,
        };

        if (this.socket != null && this.socket.Connected)
        {
            var writeResult = await this.writer.WriteAsync(packet.Encode()).ConfigureAwait(false);
            var flushResult = await this.writer.FlushAsync().ConfigureAwait(false);
        }
        else
        {
            // FIXME: Throw some exception
            return false;
        }

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
        var packetIdentifier = this.GeneratePacketIdentifier();
        var packet = new SubscribePacket(options, (ushort)packetIdentifier);

        var writeResult = await this.writer.WriteAsync(packet.Encode()).ConfigureAwait(false);
        var flushResult = await this.writer.FlushAsync().ConfigureAwait(false);

        var readResult = await this.reader.ReadAsync().ConfigureAwait(false);
        var subAck = (SubAckPacket)PacketDecoder.Decode(readResult.Buffer, out var consumed);
        this.reader.AdvanceTo(consumed);

        // FIXME: Published packets can arrive before the SUBACK.

        var subResult = new SubscribeResult(options, subAck);

        // Add the subscriptions to the client
        this.Subscriptions.AddRange(subResult.Subscriptions);

        return subResult;
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
        var packetIdentifier = this.GeneratePacketIdentifier();
        var packet = new UnsubscribePacket(subscriptions, (ushort)packetIdentifier);

        var writeResult = await this.writer.WriteAsync(packet.Encode()).ConfigureAwait(false);
        var flushResult = await this.writer.FlushAsync().ConfigureAwait(false);

        var readResult = await this.reader.ReadAsync().ConfigureAwait(false);
        var unsubAck = (UnsubAckPacket)PacketDecoder.Decode(readResult.Buffer, out var consumed);
        this.reader.AdvanceTo(consumed);

        // Prepare the result
        var unsubResult = new UnsubscribeResult
        {
            Subscriptions = packet.Subscriptions,
        };

        var counter = 0;
        foreach (var reasonCode in unsubAck.ReasonCodes)
        {
            unsubResult.Subscriptions[counter].UnsubscribeReasonCode = reasonCode;
            if (reasonCode == UnsubAckReasonCode.Success)
            {
                // Remove the subscription from the client
                this.Subscriptions.Remove(unsubResult.Subscriptions[counter]);
            }
        }

        return unsubResult;
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
