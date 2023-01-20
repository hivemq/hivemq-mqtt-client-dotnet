namespace HiveMQtt.Client.Events;

using HiveMQtt.Client.Options;

public class BeforeConnectEventArgs : EventArgs
{
    public BeforeConnectEventArgs(HiveClientOptions options) => this.Options = options;

    public HiveClientOptions Options { get; set; }
}
