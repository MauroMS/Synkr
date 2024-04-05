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
            logger.LogError(ex, Constants.Exceptions.ErrorOccurred);
            return false;
        }

        return true;
    }

    private async Task<bool> Download(CancellationToken cancellationToken)
    {
        logger.LogInformation(Constants.Information.StartedDownload);

        var mappedFolders = syncBackupConfig.Value.Mappings
            .Where(m => m.ActionType is BackupActionType.DownloadOnly or BackupActionType.Sync).ToList();

        if (mappedFolders.Count == 0)
            logger.LogInformation(Constants.Information.NoFilesFoldersToDownload);
        else
            await downloadService.Download(mappedFolders, cancellationToken);

        logger.LogInformation(Constants.Information.FinishedDownload);

        return true;
    }

    private async Task<bool> Upload(CancellationToken cancellationToken)
    {
        logger.LogInformation(Constants.Information.StartingUpload);

        var mappedFolders = syncBackupConfig.Value.Mappings
            .Where(m => m.ActionType is BackupActionType.UploadOnly or BackupActionType.Sync).ToList();

        if (mappedFolders.Count == 0)
            logger.LogInformation(Constants.Information.NoFilesFoldersToUpload);
        else
            await uploadService.Upload(mappedFolders, cancellationToken);

        logger.LogInformation(Constants.Information.FinishedUpload);

        return true;
    }
}