namespace HiveMQtt.Client.Results;

using HiveMQtt.MQTT5.Packets;
using HiveMQtt.MQTT5.Types;

/// <summary>
/// Results of the Publish operation.
/// </summary>
public class PublishResult
{
    public PublishResult(MQTT5PublishMessage message) => this.Message = message;

    public PublishResult(MQTT5PublishMessage message, PubAckPacket pubAckPacket)
    {
        this.Message = message;
        this.PubAckPacket = pubAckPacket;
    }

    public PublishResult(MQTT5PublishMessage message, PubRecPacket pubRecPacket)
    {
        this.Message = message;
        this.PubRecPacket = pubRecPacket;
    }

    MQTT5PublishMessage Message;

    PubAckPacket? PubAckPacket;

    PubRecPacket? PubRecPacket;
}
