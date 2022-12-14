namespace HiveMQtt.Client.Options;

public class PublishOptions
{
    public PublishOptions() { }

    public bool Retain { get; set; }

    public int Qos { get; set; }
}
