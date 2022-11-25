namespace HiveMQtt;

using System;
using System.Collections.Concurrent;
using System.IO.Pipelines;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

using HiveMQtt.MQTT5;

/// <summary>
/// The excellent, superb and slightly wonderful HiveMQ MQTT Client.
/// Fully MQTT compliant and compatible with all respectable MQTT Brokers because sharing is caring
/// and MQTT is awesome.
/// </summary>
public class Client : IDisposable, IClient
{
    private readonly ConcurrentQueue<byte[]> sendQueue;
    private readonly ConcurrentQueue<ControlPacket> receiveQueue;

    private Socket? socket;
    private NetworkStream? stream;
    private PipeReader? reader;
    private PipeWriter? writer;

    private bool disposed;

    public Client(ClientOptions? options = null)
    {
        options ??= new ClientOptions();
        this.Options = options;

        this.sendQueue = new ConcurrentQueue<byte[]>();
        this.receiveQueue = new ConcurrentQueue<ControlPacket>();

        this.disposed = false;
    }

    public ClientOptions Options { get; set; }

    public bool IsConnected()
    {
        // FIXME: Add MQTT connection state check
        if ((this.socket is not null) && this.socket.Connected)
        {
            return true;
        }

        return false;
    }

    public async Task<ConnectResult> ConnectAsync()
    {
        var socketIsConnected = await this.ConnectSocketAsync().ConfigureAwait(false);

        var connectResult = new ConnectResult();

        if (socketIsConnected && this.socket != null)
        {
            this.stream = new NetworkStream(this.socket);
            this.reader = PipeReader.Create(this.stream);
            this.writer = PipeWriter.Create(this.stream);

            // Construct the MQTT Connect packet
            var packet = new ConnectPacket(this.Options);
            var x = await this.writer.WriteAsync(packet.Encode()).ConfigureAwait(false);

            var result = await this.reader.ReadAsync().ConfigureAwait(false);
            var connAck = PacketDecoder.Decode(result.Buffer);
            Console.WriteLine(result);
        }
        return connectResult;
    }

    public async Task DisconnectAsync(DisconnectOptions options)
    {
        // var disconnectPacket = new Disconn
        // await this.socket.SendAsync(segment, SocketFlags.None).ConfigureAwait(false);
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
