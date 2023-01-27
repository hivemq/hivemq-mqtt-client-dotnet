namespace HiveMQtt.Client.Events;

using HiveMQtt.MQTT5.Types;

public class BeforeUnsubscribeEventArgs : EventArgs
{
    public BeforeUnsubscribeEventArgs(List<Subscription> subscriptions) => this.Subscriptions = subscriptions;

    public List<Subscription> Subscriptions { get; set; }
}
