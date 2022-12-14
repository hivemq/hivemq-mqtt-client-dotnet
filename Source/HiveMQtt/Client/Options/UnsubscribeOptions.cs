namespace HiveMQtt.Client.Options;

using System.Collections;

public class UnsubscribeOptions
{
    public UnsubscribeOptions() => this.UserProperties = new Hashtable();

    public Hashtable UserProperties { get; set; }
}
