using CloudSynkr.Models;
using CloudSynkr.Services.Interfaces;
using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.Options;

namespace CloudSynkr.Services;

public class SyncService : ISyncService
{
    private readonly IAuthService _authService;
    private readonly IOptions<SyncBackup> _syncBackupConfig;
    private readonly IDownloadService _downloadService;
    private readonly IUploadService _uploadService;

    public SyncService(IAuthService authService, IOptions<SyncBackup> syncBackupConfig, IDownloadService downloadService, IUploadService uploadService)
    {
        _authService = authService;
        _syncBackupConfig = syncBackupConfig;
        _downloadService = downloadService;
        _uploadService = uploadService;
    }

    public async Task<bool> Run(CancellationToken cancellationToken)
    {
        var credentials = await _authService.Login(cancellationToken);
        if (credentials == null)
            return false;


        await Download(credentials, cancellationToken);
        await Upload(credentials, cancellationToken);

        return true;
    }

    private async Task<bool> Download(UserCredential credentials, CancellationToken cancellationToken)
    {
        var mappedFolders = _syncBackupConfig.Value.Mappings
            .Where(m => m.ActionType is BackupActionType.DownloadOnly or BackupActionType.Sync).ToList();
        await _downloadService.Download(credentials, mappedFolders, cancellationToken);

        return true;
    }
    
    private async Task<bool> Upload(UserCredential credentials, CancellationToken cancellationToken)
    {
        var mappedFolders = _syncBackupConfig.Value.Mappings
            .Where(m => m.ActionType is BackupActionType.UploadOnly or BackupActionType.Sync).ToList();

        await _uploadService.Upload(credentials, mappedFolders, cancellationToken);
        
        return true;
    }
}