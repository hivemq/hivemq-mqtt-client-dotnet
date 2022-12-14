namespace HiveMQtt.Client;

using System;
using System.Threading.Tasks;

using HiveMQtt.Client.Options;
using HiveMQtt.Client.Results;

/// <summary>
/// Some interface.
/// </summary>
public interface IHiveClient : IDisposable
{
    HiveClientOptions Options { get; set; }

    bool IsConnected();

    /// <summary>
    /// Asyncronously connect to an MQTT broker.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the result of the operation.</returns>
    Task<ConnectResult> ConnectAsync();

    Task<bool> DisconnectAsync(DisconnectOptions options);

    Task<SubscribeResult> SubscribeAsync(string topic);

    Task<UnsubscribeResult> UnsubscribeAsync(string topic);

    Task<PublishResult> PublishAsync(string topic, string payload);

}
