namespace HiveMQtt;

using HiveMQtt.Connect;

/// <summary>
/// Some interface.
/// </summary>
public interface IClient : IDisposable
{
    ClientOptions Options { get; set; }

    bool IsConnected();

    /// <summary>
    /// Asyncronously connect to an MQTT broker.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the result of the operation.</returns>
    Task<ConnectResult> ConnectAsync();

    Task DisconnectAsync(DisconnectOptions options);
}
