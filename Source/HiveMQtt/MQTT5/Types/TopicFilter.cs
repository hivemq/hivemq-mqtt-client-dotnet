namespace HiveMQtt.MQTT5.Types;

using HiveMQtt.Client.Exceptions;

public class TopicFilter
{
    /// <summary>
    /// Gets or sets the topic for this filter.
    /// <para>
    /// The Topic is a UTF-8 encoded string that specifies a subscription topic.  It can
    /// include the wildcard characters # and + as defined in
    /// <seealso href="https://docs.oasis-open.org/mqtt/mqtt/v5.0/os/mqtt-v5.0-os.html#_Toc3901241">
    /// the MQTT specification</seealso>.
    /// </para>
    /// </summary>
    public string Topic { get; set; }

    /// <summary>
    /// Gets or sets the Quality of Service level for this filter.
    /// </summary>
    public QualityOfService QoS { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the broker will forward messages published by
    /// the client to the client itself.
    /// <para>
    /// The default value is false: the broker will forward all messages to the client.
    /// </para>
    /// </summary>
    public bool? NoLocal { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether Application Messages forwarded using this
    /// subscription keep the RETAIN flag they were published with.
    /// </summary>
    public bool? RetainAsPublished { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether retained messages are sent when the
    /// subscription is established.  This does not affect the sending of retained
    /// messages at any point after the subscribe.
    /// <para>
    /// The default value is RetainHandling.SendAtSubscribe.  See
    /// <seealso href="https://docs.oasis-open.org/mqtt/mqtt/v5.0/os/mqtt-v5.0-os.html#_Toc3901169">
    /// the MQTT specification</seealso> for more information.
    /// </para>
    /// </summary>
    public RetainHandling? RetainHandling { get; set; }

    /// <summary>
    /// Validates the filter.
    /// </summary>
    /// <exception cref="HiveMQttClientException"></exception>
    public void ValidateFilter()
    {
        if (string.IsNullOrEmpty(this.Topic))
        {
            throw new HiveMQttClientException("Topic filter must not be null or empty");
        }

    }
}
