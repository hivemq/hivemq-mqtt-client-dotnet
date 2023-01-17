namespace HiveMQtt.Client.Events;

using HiveMQtt.Client.Results;

public class AfterSubscribeEventArgs : EventArgs
{
    public AfterSubscribeEventArgs(SubscribeResult results) => this.SubscribeResult = results;

    public SubscribeResult SubscribeResult { get; set; }
}
