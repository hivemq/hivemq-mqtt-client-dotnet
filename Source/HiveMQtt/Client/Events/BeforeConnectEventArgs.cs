namespace HiveMQtt.Client.Events;

using HiveMQtt.Client.Options;

public class BeforeConnectEventArgs : EventArgs
{
    public BeforeConnectEventArgs(HiveMQClientOptions options) => this.Options = options;

    public HiveMQClientOptions Options { get; set; }
}
