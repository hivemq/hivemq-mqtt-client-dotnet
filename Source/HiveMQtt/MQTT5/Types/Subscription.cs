namespace HiveMQtt.MQTT5.Types;

using HiveMQtt.MQTT5.Subscribe;

public class Subscription
{
    public Subscription(TopicFilter topicFilter)
    {
        this.Topic = topicFilter;
    }

    /// <summary>
    /// Gets the topic filter for the subscription.
    /// </summary>
    public TopicFilter Topic { get; }

    /// <summary>
    /// Gets the reason code (result) for the subscription.
    /// <para>
    /// See <seealso cref="SubAckReasonCode">SubAckReasonCode</seealso> or the
    /// <seealso href="https://docs.oasis-open.org/mqtt/mqtt/v5.0/os/mqtt-v5.0-os.html#_Toc3901178">
    /// MQTT specification</seealso> for more information.
    /// </para>
    /// </summary>
    public SubAckReasonCode ReasonCode { get; set; }
}
