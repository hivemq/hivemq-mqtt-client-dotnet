namespace HiveMQtt.MQTT5.Exceptions;

using System;

public class MQTTProtocolException : ArgumentException
{
    public MQTTProtocolException()
    {
    }

   public MQTTProtocolException(string message)
        : base(message)
    {
    }

    public MQTTProtocolException(string message, Exception inner)
        : base(message, inner)
    {
    }
}
