namespace HiveMQtt.Client.Options;

/// <summary>
/// Options class for connect.
/// </summary>
public class ConnectOptions
{
    public ConnectOptions()
    {
        this.CleanStart = true;
        this.KeepAlive = 60;
    }

    public bool CleanStart { get; set; }

    public int KeepAlive { get; set; }

    public string? ClientId { get; set; }
}
