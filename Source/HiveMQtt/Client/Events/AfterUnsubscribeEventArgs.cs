namespace HiveMQtt.Client.Events;

using HiveMQtt.Client.Results;

public class AfterUnsubscribeEventArgs : EventArgs
{
    public AfterUnsubscribeEventArgs(UnsubscribeResult results) => this.UnsubscribeResult = results;

    public UnsubscribeResult UnsubscribeResult { get; set; }
}
