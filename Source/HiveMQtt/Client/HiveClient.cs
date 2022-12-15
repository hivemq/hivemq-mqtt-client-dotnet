namespace HiveMQtt.Client;

using System;
using System.Collections.Concurrent;
using System.IO.Pipelines;
using System.Net;
using System.Net.Sockets;
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

        this.sendQueue = new ConcurrentQueue<byte[]>();
        this.receiveQueue = new ConcurrentQueue<ControlPacket>();

        this.disposed = false;
    }

    public HiveClientOptions Options { get; set; }

    internal MQTT5Properties? ConnectionProperties { get; }

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
        var packet = new ConnectPacket(this.Options);
        var x = await this.writer.WriteAsync(packet.Encode()).ConfigureAwait(false);

        var result = await this.reader.ReadAsync().ConfigureAwait(false);
        var connAck = (ConnAckPacket)PacketDecoder.Decode(result.Buffer);

        var connectResult = new ConnectResult(connAck.ReasonCode, connAck.SessionPresent, connAck.Properties);

        // Data massage: This class is used for end users.  Let's prep the data so it's easily understandable.
        // If the Session Expiry Interval is absent the value in the CONNECT Packet used.
        connectResult.Properties.SessionExpiryInterval ??= (UInt32)this.Options.SessionExpiryInterval;

        return connectResult;
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
            var x = await this.writer.WriteAsync(packet.Encode()).ConfigureAwait(false);

        }
        else
        {
            // FIXME: Throw some exception
        }

        return true;
    }

    /// <summary>
    /// Publish a message to the MQTT broker.
    /// </summary>
    /// <param name="options">The <seealso cref="PublishOptions"/> for the Publish.</param>
    /// <returns>A <seealso cref="PublishResult"/> representing the result of the publish operation.</returns>
    public async Task<PublishResult> PublishAsync(byte[] message, PublishOptions options)
    {
        var packetIdentifier = this.GeneratePacketIdentifier();
        var packet = new PublishPacket(options, (ushort)packetIdentifier);
        _ = await this.writer.WriteAsync(packet.Encode()).ConfigureAwait(false);
        // TODO: Get the packet identifier from the PublishAck packet
        // TODO:
        return new PublishResult();
    }

    /// <summary>
    /// Subscribe to a topic on the MQTT broker.
    /// </summary>
    /// <param name="options"></param>
    /// <returns></returns>
    /// TODO: Implement the SubscribeResult class
    public async Task<SubscribeResult> SubscribeAsync(string topic, QoS qos = QoS.AtMostOnce)
    {
        var options = new SubscribeOptions
        {
            TopicFilters = new List<TopicFilter>
            {
                new()
                {
                    Topic = topic,
                    QoS = qos,
                },
            },
        };

        return await this.SubscribeAsync(options).ConfigureAwait(false);
    }

    public async Task<SubscribeResult> SubscribeAsync(SubscribeOptions options)
    {
        var packet = new SubscribePacket(options);

        var x = await this.writer.WriteAsync(packet.Encode()).ConfigureAwait(false);

        return new SubscribeResult();
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
