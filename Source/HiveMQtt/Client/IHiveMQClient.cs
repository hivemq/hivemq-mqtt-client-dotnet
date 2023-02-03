namespace HiveMQtt.Client;

using System;
using System.Threading.Tasks;

using HiveMQtt.Client.Options;
using HiveMQtt.Client.Results;
using HiveMQtt.MQTT5.Types;

/// <summary>
/// Some interface.
/// </summary>
public interface IHiveMQClient : IDisposable
{
    HiveMQClientOptions Options { get; set; }

    /// <summary>
    /// Gets the local store for the client.
    /// <para>
    /// The LocalStore is a Dictionary(string, string) that can be used
    /// to store data that is specific to this HiveMQClient.
    /// </para>
    /// </summary>
    Dictionary<string, string> LocalStore { get; }

    bool IsConnected();

    /// <summary>
    /// Asynchronously makes a TCP connection to the remote specified in HiveMQClientOptions and then
    /// proceeds to make an MQTT Connect request.
    /// </summary>
    /// <returns>A ConnectResult class respresenting the result of the MQTT connect call.</returns>
    Task<ConnectResult> ConnectAsync();

    /// <summary>
    /// Asynchronous disconnect from the previously connected MQTT broker.
    /// </summary>
    /// <param name="options">The options for the MQTT Disconnect call.</param>
    /// <returns>A boolean indicating on success or failure.</returns>
    Task<bool> DisconnectAsync(DisconnectOptions options);

    // Task<SubscribeResult> SubscribeAsync(string topic);

    // Task<UnsubscribeResult> UnsubscribeAsync(string topic);

    Task<PublishResult> PublishAsync(string topic, string payload, QualityOfService qos = QualityOfService.AtMostOnceDelivery);

    Task<PublishResult> PublishAsync(string topic, byte[] payload, QualityOfService qos = QualityOfService.AtMostOnceDelivery);

    Task<PublishResult> PublishAsync(MQTT5PublishMessage message);

}
