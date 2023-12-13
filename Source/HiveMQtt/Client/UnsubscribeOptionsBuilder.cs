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
    /// Adds a single topic filter to the UnsubscribeOption.
    /// </summary>
    /// <param name="topicFilter">The TopicFilter to add.</param>
    /// <returns>The UnsubscribeOptionsBuilder instance.</returns>
    public UnsubscribeOptionsBuilder WithTopicFilter(TopicFilter topicFilter)
    {
        var subscription = new Subscription(topicFilter);
        this.options.Subscriptions.Add(subscription);
        return this;
    }

    /// <summary>
    /// Adds a list of topic filters to the UnsubscribeOption.
    /// </summary>
    /// <param name="topicFilters">The list of TopicFilters to add to the UnsubscribeOptions.</param>
    /// <returns>The UnsubscribeOptionsBuilder instance.</returns>
    public UnsubscribeOptionsBuilder WithTopicFilters(IEnumerable<TopicFilter> topicFilters)
    {
        for (var i = 0; i < topicFilters.Count(); i++)
        {
            var subscription = new Subscription(topicFilters.ElementAt(i));
            this.options.Subscriptions.Add(subscription);
        }

        return this;
    }

    /// <summary>
    /// Adds a single topic filter to the UnsubscribeOption.
    /// </summary>
    /// <param name="topic">The topic for the topic filter.</param>
    /// <returns>The UnsubscribeOptionsBuilder instance.</returns>
    public UnsubscribeOptionsBuilder WithTopic(string topic)
    {
        var topicFilter = new TopicFilter(topic);
        var subscription = new Subscription(topicFilter);
        this.options.Subscriptions.Add(subscription);
        return this;
    }

    /// <summary>
    /// Adds a single subscription to the UnsubscribeOption.
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
