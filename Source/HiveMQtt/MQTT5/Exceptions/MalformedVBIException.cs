namespace HiveMQtt.MQTT5.Exceptions;

using System;

public class MalformedVBIException : ArgumentException
{
    public MalformedVBIException()
    {
    }

    // public MalformedVBIException(byte[] bytes)
    //     : base(String.Format("Malformed VBI: {0}", bytes))
    // {
    // }
}
