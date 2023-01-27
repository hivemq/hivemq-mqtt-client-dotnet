namespace HiveMQtt.Client.Events;

using HiveMQtt.Client.Options;

public class BeforeSubscribeEventArgs : EventArgs
{
    public BeforeSubscribeEventArgs(SubscribeOptions options) => this.Options = options;

    public SubscribeOptions Options { get; set; }
}
