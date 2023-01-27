namespace HiveMQtt.Client.Events;

using HiveMQtt.Client.Results;

public class OnConnectedEventArgs : EventArgs
{
    public OnConnectedEventArgs(ConnectResult connectionResult) => this.ConnectResult = connectionResult;

    public ConnectResult ConnectResult { get; set; }
}
