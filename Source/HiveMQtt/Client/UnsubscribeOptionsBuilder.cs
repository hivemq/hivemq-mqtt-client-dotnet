namespace HiveMQtt.Client;

using HiveMQtt.Client.Options;
using HiveMQtt.MQTT5.Types;

public class UnsubscribeOptionsBuilder
{
    private readonly UnsubscribeOptions options;

    public UnsubscribeOptionsBuilder()
    {
        this.options = new UnsubscribeOptions();
    }

    /// <summary>
    /// Adds a single subscription to the UnsubscribeOption.
    ///
    /// You can retrieve existing subscriptions in the HiveMQClient.Subscriptions property or
    /// alternatively using the HiveMQClient.GetSubscriptionByTopic method.
    /// </summary>
    /// <param name="subscription">A topic subscription.</param>
    /// <returns>The UnsubscribeOptionsBuilder instance.</returns>
    public UnsubscribeOptionsBuilder WithSubscription(Subscription subscription)
    {
        this.options.Subscriptions.Add(subscription);
        return this;
    }

    /// <summary>
    /// Adds a list of subscriptions to the UnsubscribeOption.
    ///
    /// You can retrieve existing subscriptions in the HiveMQClient.Subscriptions property or
    /// alternatively using the HiveMQClient.GetSubscriptionByTopic method.
    /// </summary>
    /// <param name="subscriptions">A list of topic subscriptions.</param>
    /// <returns>The UnsubscribeOptionsBuilder instance.</returns>
    public UnsubscribeOptionsBuilder WithSubscriptions(IEnumerable<Subscription> subscriptions)
    {
        foreach (var subscription in subscriptions)
        {
            this.options.Subscriptions.Add(subscription);
        }

        return this;
    }

    /// <summary>
    /// Adds a user property to be sent along with the unsubscribe request.
    ///
    /// You can read more about user properties here:
    /// https://www.hivemq.com/blog/mqtt5-essentials-part6-user-properties/
    /// </summary>
    /// <param name="key">The key of the user property.</param>
    /// <param name="value">The value of the user property.</param>
    /// <returns>The UnsubscribeOptionsBuilder instance.</returns>
    public UnsubscribeOptionsBuilder WithUserProperties(string key, string value)
    {
        this.options.UserProperties.Add(key, value);
        return this;
    }

    public UnsubscribeOptions Build()
    {
        this.options.Validate();
        return this.options;
    }
}
