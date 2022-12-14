namespace HiveMQtt.Client.Options;

public class SubscribeOptions
{
    public SubscribeOptions() { }

    public bool Retain { get; set; }

    public int Qos { get; set; }
}
