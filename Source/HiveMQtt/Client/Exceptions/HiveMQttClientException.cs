namespace HiveMQtt.Client.Exceptions;

[Serializable]

public class
HiveMQttClientException : Exception
{
    public HiveMQttClientException()
    {
    }

    public HiveMQttClientException(string message)
        : base(message)
    {
    }

    public HiveMQttClientException(string message, Exception inner)
        : base(message, inner)
    {
    }

    protected HiveMQttClientException(
        System.Runtime.Serialization.SerializationInfo info,
        System.Runtime.Serialization.StreamingContext context)
            : base(info, context)
    {
    }
}
