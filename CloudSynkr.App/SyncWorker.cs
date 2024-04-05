using CloudSynkr.Models;
using CloudSynkr.Services.Interfaces;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CloudSynkr.App;

public class SyncWorker(ILogger<SyncWorker> logger, ISyncService syncService) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation(Constants.Information.ApplicationStarted);
        await syncService.Run(cancellationToken);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation(Constants.Information.ApplicationStopped);
        await Task.CompletedTask;
    }
}