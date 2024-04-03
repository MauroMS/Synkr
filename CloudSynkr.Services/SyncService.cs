using CloudSynkr.Models;
using CloudSynkr.Models.Exceptions;
using CloudSynkr.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CloudSynkr.Services;

public class SyncService(
    IOptions<SyncBackup> syncBackupConfig,
    IDownloadService downloadService,
    IUploadService uploadService,
    ILogger<SyncService> logger)
    : ISyncService
{
    public async Task<bool> Run(CancellationToken cancellationToken)
    {
        try
        {
            await Download(cancellationToken);
            await Upload(cancellationToken);
        }
        catch (LoginException)
        {
            return false;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred...");
            return false;
        }

        return true;
    }

    private async Task<bool> Download(CancellationToken cancellationToken)
    {
        logger.LogInformation("Starting Download");

        var mappedFolders = syncBackupConfig.Value.Mappings
            .Where(m => m.ActionType is BackupActionType.DownloadOnly or BackupActionType.Sync).ToList();
        await downloadService.Download(mappedFolders, cancellationToken);

        logger.LogInformation("Finished Download");

        return true;
    }

    private async Task<bool> Upload(CancellationToken cancellationToken)
    {
        logger.LogInformation("Starting Upload");

        var mappedFolders = syncBackupConfig.Value.Mappings
            .Where(m => m.ActionType is BackupActionType.UploadOnly or BackupActionType.Sync).ToList();
        await uploadService.Upload(mappedFolders, cancellationToken);

        logger.LogInformation("Finished Upload");

        return true;
    }
}