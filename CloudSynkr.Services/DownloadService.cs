using CloudSynkr.Models;
using CloudSynkr.Repositories.Interfaces;
using CloudSynkr.Services.Interfaces;
using CloudSynkr.Utils;
using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.Logging;
using File = CloudSynkr.Models.File;

namespace CloudSynkr.Services;

public class DownloadService : IDownloadService
{
    private readonly ICloudStorageRepository _cloudStorageRepository;
    private readonly ILocalStorageRepository _localStorageRepository;
    private readonly ILogger<DownloadService> _logger;
    private readonly IAuthService _authService;

    public DownloadService(ICloudStorageRepository cloudStorageRepository,
        ILocalStorageRepository localStorageRepository, ILogger<DownloadService> logger, IAuthService authService)
    {
        _cloudStorageRepository = cloudStorageRepository;
        _localStorageRepository = localStorageRepository;
        _logger = logger;
        _authService = authService;
    }

    public async Task<bool> Download(List<Mapping> mappings,
        CancellationToken cancellationToken)
    {
        var credentials = await _authService.Login(cancellationToken);

        foreach (var folderMap in mappings)
        {
            //TODO: ADD LOGIC TO MAP FILE TO RESULT... PROBABLY A DICTIONARY... ADD RETRY LOGIC?
            var folderStructure = await GetFolderStructureToDownload(folderMap.CloudFolderParentId,
                folderMap.CloudFolderParentName, folderMap.CloudFolder,
                cancellationToken);

            await DownloadFilesFromFolders(folderStructure, folderMap.LocalFolder,
                cancellationToken);
        }

        return true;
    }

    public async Task<bool> DownloadFilesFromFolders(List<Folder> folderStructure,
        string localFolder, CancellationToken cancellationToken)
    {
        foreach (var folder in folderStructure)
        {
            var subFolder = Path.Combine(localFolder, folder.Name);
            await DownloadFiles(folder.Files, subFolder, cancellationToken);
            await DownloadFilesFromFolders(folder.Children, subFolder, cancellationToken);
        }

        return true;
    }

    public async Task<List<Folder>> GetFolderStructureToDownload(string parentId,
        string parentName, string folderName, CancellationToken cancellationToken)
    {
        var credentials = await _authService.Login(cancellationToken);
        
        var folder =
            await _cloudStorageRepository.GetBasicFolderInfoByNameAndParentId(credentials, parentId, folderName,
                cancellationToken);

        if (folder == null)
        {
            _logger.LogInformation($"Folder '{folderName}' doesn't exists on parent '{parentName}'");
            return [];
        }

        var folderStructure = await _cloudStorageRepository.GetAllFoldersByParentId(credentials, folder.Id, folder.Name,
            folder.ParentId, folder.Name, new CancellationToken());

        return folderStructure;
    }

    public async Task<bool> DownloadFiles(List<File> files, string localFolder, CancellationToken cancellationToken)
    {
        MemoryStream? fileStream;
        var credentials = await _authService.Login(cancellationToken);
        var localFiles = _localStorageRepository.GetLocalFiles(localFolder);
        foreach (var cloudFile in files)
        {
            //TODO: LOG/RETURN SOMETHING INDICATING FILES DOWNLOADED OR NOT
            var localFile = localFiles.FirstOrDefault(f => f.Name == cloudFile.Name);

            if (localFile != null &&
                DateHelper.CheckIfDateIsNewer(localFile.LastModified, cloudFile.LastModified))
            {
                _logger.LogInformation(
                    $"File {cloudFile.Name} was not downloaded as its version is older than the local version.");
                continue;
            }

            fileStream = await _cloudStorageRepository.DownloadFile(cloudFile.Id, credentials);
            _localStorageRepository.SaveStreamAsFile(localFolder, fileStream, cloudFile.Name);
            _logger.LogInformation($"File {cloudFile.Name} was downloaded to {localFolder}");
        }

        return true;
    }
}