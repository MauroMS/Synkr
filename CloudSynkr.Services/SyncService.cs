using CloudSynkr.Models;
using CloudSynkr.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CloudSynkr.Services;

public class SyncService : ISyncService
{
    private readonly IAuthService _authService;
    private readonly IOptions<SyncBackup> _syncBackupConfig;
    private readonly IDownloadService _downloadService;
    private readonly IUploadService _uploadService;
    private readonly ILogger<SyncService> _logger;

    public SyncService(IAuthService authService, IOptions<SyncBackup> syncBackupConfig,
        IDownloadService downloadService, IUploadService uploadService, ILogger<SyncService> logger)
    {
        _authService = authService;
        _syncBackupConfig = syncBackupConfig;
        _downloadService = downloadService;
        _uploadService = uploadService;
        _logger = logger;
    }

    public async Task<bool> Run(CancellationToken cancellationToken)
    {
        var credentials = await _authService.Login(cancellationToken);
        if (credentials == null)
            return false;

        await Download(cancellationToken);
        await Upload(cancellationToken);

        return true;
    }

    private async Task<bool> Download(CancellationToken cancellationToken)
    {
        _logger.LogInformation($@"Starting Download.");
        
        var mappedFolders = _syncBackupConfig.Value.Mappings
            .Where(m => m.ActionType is BackupActionType.DownloadOnly or BackupActionType.Sync).ToList();
        await _downloadService.Download(mappedFolders, cancellationToken);

        _logger.LogInformation($@"Finished Download.");

        return true;
    }

    private async Task<bool> Upload(CancellationToken cancellationToken)
    {
        _logger.LogInformation($@"Starting Upload.");

        var mappedFolders = _syncBackupConfig.Value.Mappings
            .Where(m => m.ActionType is BackupActionType.UploadOnly or BackupActionType.Sync).ToList();
        await _uploadService.Upload(mappedFolders, cancellationToken);

        _logger.LogInformation($@"Finished Upload.");

        return true;
    }
}