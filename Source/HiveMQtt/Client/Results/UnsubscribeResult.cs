namespace HiveMQtt.Client.Results;

using HiveMQtt.MQTT5.Types;

public class UnsubscribeResult
{
    public UnsubscribeResult() { }

    public List<Subscription> Subscriptions;
}
