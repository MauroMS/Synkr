using CloudSynkr.Models;
using CloudSynkr.Services.Interfaces;
using Microsoft.Extensions.Options;

namespace CloudSynkr.App;

public class SyncService : ISyncService
{
    private readonly IAuthService _authService;
    private readonly IOptions<SyncBackup> _syncBackupConfig;
    private readonly IDownloadService _downloadService;

    public SyncService(IAuthService authService, IOptions<SyncBackup> syncBackupConfig, IDownloadService downloadService)
    {
        _authService = authService;
        _syncBackupConfig = syncBackupConfig;
        _downloadService = downloadService;
    }

    public async Task<bool> Run(CancellationToken cancellationToken)
    {
        var credentials = await _authService.Login(cancellationToken);
        if (credentials == null)
            return false;

        var mappedFolders = _syncBackupConfig.Value.Mappings
            .Where(m => m.ActionType == BackupActionType.DownloadOnly).ToList();
        await _downloadService.Download(credentials, mappedFolders, cancellationToken);

        return true;
    }
}