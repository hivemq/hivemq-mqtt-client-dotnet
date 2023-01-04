namespace HiveMQtt.Client.Options;

using System.Collections;
using HiveMQtt.MQTT5.Types;

public class SubscribeOptions
{
    public SubscribeOptions()
    {
        this.UserProperties = new Hashtable();
        this.TopicFilters = new List<TopicFilter>();
    }

    /// <summary>
    /// Gets or sets the Subscription Identifier.
    /// </summary>
    public int? SubscriptionIdentifier { get; set; }

    /// <summary>
    /// Gets or sets the User Properties for this subscribe.
    /// </summary>
    public Hashtable UserProperties { get; set; }

    /// <summary>
    /// Gets or sets the Topic Filters for this subscribe.
    /// </summary>
    public List<TopicFilter> TopicFilters { get; set; }

    /// <summary>
    /// Validate that the options in this instance are valid.
    /// </summary>
    /// <exception cref="HiveMQttClientException">The exception raised if some value is out of range or invalid.</exception>
    public void ValidateOptions()
    {
    }
}
