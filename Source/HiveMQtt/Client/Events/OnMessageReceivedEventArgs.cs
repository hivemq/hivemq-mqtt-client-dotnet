namespace HiveMQtt.Client.Events;

using HiveMQtt.MQTT5.Types;

public class OnMessageReceivedEventArgs : EventArgs
{
    public OnMessageReceivedEventArgs(MQTT5PublishMessage message) => this.PublishMessage = message;

    public MQTT5PublishMessage PublishMessage { get; set; }
}
