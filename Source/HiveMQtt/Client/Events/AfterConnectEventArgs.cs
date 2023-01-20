namespace HiveMQtt.Client.Events;

using HiveMQtt.Client.Results;

public class AfterConnectEventArgs : EventArgs
{
    public AfterConnectEventArgs(ConnectResult results) => this.ConnectResult = results;

    public ConnectResult ConnectResult { get; set; }
}
