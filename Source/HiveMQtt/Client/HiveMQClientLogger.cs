namespace HiveMQtt.Client;

using Microsoft.Extensions.Logging;

/// <inheritdoc />
public partial class HiveMQClient : IDisposable, IHiveMQClient
{
    private ILogger<HiveMQClient> _logger;

    public void AttachLogger(ILogger<HiveMQClient> logger) => this._logger = logger;
}
