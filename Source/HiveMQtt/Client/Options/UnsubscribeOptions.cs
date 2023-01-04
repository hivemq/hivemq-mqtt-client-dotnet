namespace HiveMQtt.Client.Options;

using System.Collections;

public class UnsubscribeOptions
{
    public UnsubscribeOptions() => this.UserProperties = new Dictionary<string, string>();

    public Dictionary<string, string> UserProperties { get; set; }
}
