namespace HiveMQtt;

/// <summary>
/// Options class for connect.
/// </summary>
public class ConnectOptions
{
    public ConnectOptions() { }

    public bool CleanStart { get; set; }

    public int KeepAlive { get; set; }

    public string? ClientId { get; set; }
}
