namespace HiveMQtt.Client.Events;

using HiveMQtt.Client.Options;

public class BeforeSubscribeEventArgs : EventArgs
{
    public BeforeSubscribeEventArgs(SubscribeOptions options) => this.SubscribeOptions = options;

    public SubscribeOptions SubscribeOptions { get; set; }
}
