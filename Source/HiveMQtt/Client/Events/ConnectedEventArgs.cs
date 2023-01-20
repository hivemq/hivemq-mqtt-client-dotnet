namespace HiveMQtt.Client.Events;

using HiveMQtt.Client.Results;

public class ConnectedEventArgs : EventArgs
{
    public ConnectedEventArgs(ConnectResult connectionResult) => this.ConnectResult = connectionResult;

    public ConnectResult ConnectResult { get; set; }
}
