namespace HiveMQtt;

using System;
using System.Linq;

/// <summary>
/// A class to manage the MQTT options available in the Client.
/// </summary>
public class ClientOptions
{
    // The set of valid characters that a client identifier can consist of
    private readonly string clientIdCharset = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";

    public ClientOptions()
    {
        this.GenerateClientID();
        this.Host = "127.0.0.1";
        this.Port = 1883;
        this.KeepAlive = 60; // FIXME: Taken from Java client; Seems low
        this.CleanStart = true;
    }

    // Client Identifier to be used in the Client.  Will be set automatically if not specified.
    public string? ClientId { get; set; }

    // IP Address or DNS Hostname of the MQTT Broker to connect to
    public string Host { get; set; }

    // The port to connect to on the MQTT Broker
    public int Port { get; set; }

    // The the maximum time interval that is permitted to elapse between the point at which the Client
    // finishes transmitting one MQTT Control Packet and the point it starts sending the next.
    // Valid values: 0..65535
    public int KeepAlive { get; set; }

    // Specifies whether the Connection starts a new Session or is a continuation of an existing Session.
    public bool CleanStart { get; set; }

    // The MQTT CONNECT packet supports basic authentication of a Network Connection using the User Name
    // and Password fields. While these fields are named for a simple password authentication, they can
    // be used to carry other forms of authentication such as passing a token as the Password.
    public string? UserName { get; set; }

    // The MQTT CONNECT packet supports basic authentication of a Network Connection using the User Name
    // and Password fields. While these fields are named for a simple password authentication, they can
    // be used to carry other forms of authentication such as passing a token as the Password.
    public string? Password { get; set; }

    /// <summary>
    /// Generate a semi-random client identifier to be used in <c>Client</c> connections.
    /// hivec#-pid-randomstring
    /// </summary>
    public void GenerateClientID()
    {
        var rand = new Random();
        var pid_string = Convert.ToString(Environment.ProcessId, System.Globalization.CultureInfo.InvariantCulture);
        var stampLength = 23 - pid_string.Length;

        var clientID = new string(Enumerable.Range(0, stampLength)
                                            .Select(_ =>
                                            this.clientIdCharset[rand.Next(this.clientIdCharset.Length)])
                                            .ToArray());

        this.ClientId = pid_string + "-" + clientID;
    }

    /// <summary>
    /// Validate that the options specified in this class are all sane.
    /// </summary>
    public void ValidateOptions()
    {
        // Data Validation
        if (this.KeepAlive < 0)
        {
            // FIXME: Warn bad KeepAlive value
            this.KeepAlive = 0;
        }
        else if (this.KeepAlive > 65535)
        {
            // FIXME: Warn bad KeepAlive value
            this.KeepAlive = 65535;
        }

        if (this.ClientId == null)
        {
            // FIXME: Forced regeneration of client id
            this.GenerateClientID();
        }
        else if (this.ClientId.Length > 23)
        {
            // FIXME: Warn on exceeded length; may not work...
        }
    }
}
