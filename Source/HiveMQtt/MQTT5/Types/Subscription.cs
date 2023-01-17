namespace HiveMQtt.MQTT5.Types;

using HiveMQtt.MQTT5.ReasonCodes;

public class Subscription
{
    public Subscription(TopicFilter topicFilter) => this.TopicFilter = topicFilter;

    /// <summary>
    /// Gets the topic filter for the subscription.
    /// </summary>
    public TopicFilter TopicFilter { get; }

    /// <summary>
    /// Gets or sets the reason code (result) for the subscribe call.
    /// <para>
    /// See <seealso cref="SubAckReasonCode">SubAckReasonCode</seealso> or the
    /// <seealso href="https://docs.oasis-open.org/mqtt/mqtt/v5.0/os/mqtt-v5.0-os.html#_Toc3901178">
    /// MQTT specification</seealso> for more information.
    /// </para>
    /// </summary>
    public SubAckReasonCode SubscribeReasonCode { get; set; }

    /// <summary>
    /// Gets or sets the reason code (result) for the unsubscribe call.
    /// <para>
    /// See <seealso cref="UnsubAckReasonCode">UnsubAckReasonCode</seealso> or the
    /// <seealso href="https://docs.oasis-open.org/mqtt/mqtt/v5.0/os/mqtt-v5.0-os.html#_Toc3901194">
    /// MQTT specification</seealso> for more information.
    /// </para>
    /// </summary>
    public UnsubAckReasonCode UnsubscribeReasonCode { get; set; }
}
