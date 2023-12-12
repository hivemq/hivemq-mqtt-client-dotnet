namespace HiveMQtt.Client.Options;

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
        this.options.TopicFilters.Add(topicFilter);
        return this;
    }

    /// <summary>
    /// Adds a list of topic filters to the UnsubscribeOption.
    /// </summary>
    /// <param name="topicFilters">The list of TopicFilters to add to the UnsubscribeOptions.</param>
    /// <returns>The UnsubscribeOptionsBuilder instance.</returns>
    public UnsubscribeOptionsBuilder WithTopicFilters(IEnumerable<TopicFilter> topicFilters)
    {
        this.options.TopicFilters.AddRange(topicFilters);
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
        this.options.TopicFilters.Add(topicFilter);
        return this;
    }

    public UnsubscribeOptions Build()
    {
        this.options.Validate();
        return this.options;
    }
}
