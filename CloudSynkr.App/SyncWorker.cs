using CloudSynkr.Services.Interfaces;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CloudSynkr.App;

public class SyncWorker : IHostedService
{
    private readonly ILogger<SyncWorker> _logger;
    private readonly ISyncService _syncService;

    public SyncWorker(ILogger<SyncWorker> logger, ISyncService syncService)
    {
        _logger = logger;
        _syncService = syncService;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Application Started");
        await _syncService.Run(cancellationToken);
        // await Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Application Stopped");
        await Task.CompletedTask;
    }
}