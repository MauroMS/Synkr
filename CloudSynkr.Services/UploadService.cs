using CloudSynkr.Models;
using CloudSynkr.Repositories.Interfaces;
using CloudSynkr.Services.Interfaces;
using CloudSynkr.Utils;
using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.Logging;
using File = CloudSynkr.Models.File;

namespace CloudSynkr.Services;

public class UploadService : IUploadService
{
    private readonly ICloudStorageRepository _cloudStorageRepository;
    private readonly ILocalStorageRepository _localStorageRepository;
    private readonly ILogger<UploadService> _logger;
    private readonly IAuthService _authService;

    public UploadService(ICloudStorageRepository cloudStorageRepository,
        ILocalStorageRepository localStorageRepository, ILogger<UploadService> logger, IAuthService authService)
    {
        _cloudStorageRepository = cloudStorageRepository;
        _localStorageRepository = localStorageRepository;
        _logger = logger;
        _authService = authService;
    }

    public async Task<bool> Upload(List<Mapping> mappings,
        CancellationToken cancellationToken)
    {
        foreach (var folderMap in mappings)
        {
            var folderStructure = await GetFolderStructureToUpload(folderMap.LocalFolder);

            await UploadFilesToFolders(folderStructure, folderMap.CloudFolder,
                folderMap.CloudFolderParentId, cancellationToken);
        }

        return true;
    }

    public async Task<bool> UploadFilesToFolders(List<Folder> folderStructure,
        string cloudFolderPath, string parentId, CancellationToken cancellationToken)
    {
        var credentials = await _authService.Login(cancellationToken);
        foreach (var folder in folderStructure)
        {
            var subFolder = Path.Combine(cloudFolderPath, folder.Name);

            var cloudFolder =
                await _cloudStorageRepository.GetBasicFolderInfoByNameAndParentId(credentials, parentId, folder.Name,
                    cancellationToken) ??
                await _cloudStorageRepository.CreateFolder(credentials, folder.Name, parentId, cancellationToken);

            if (cloudFolder == null)
            {
                _logger.LogError($"Error creating folder '{folder.Name}' on path '{subFolder}'");
                return false;
            }

            await UploadFiles(folder.Files, cloudFolder.Id, cancellationToken);
            await UploadFilesToFolders(folder.Children, subFolder, cloudFolder.Id, cancellationToken);
        }

        return true;
    }

    public async Task<List<Folder>> GetFolderStructureToUpload(string folderPath)
    {
        return await _localStorageRepository.GetLocalFolders(folderPath);
    }

    public async Task<bool> UploadFiles(List<File> files, string parentId,
        CancellationToken cancellationToken)
    {
        var credentials = await _authService.Login(cancellationToken);
        if (files.Count == 0)
            return false;

        var cloudFiles =
            await _cloudStorageRepository.GetAllFilesFromFolder(credentials, parentId, files[0].ParentName,
                cancellationToken);
        foreach (var localFile in files)
        {
            var mimeType = MimeTypeMapHelper.GetMimeType(localFile.Name);
            var cloudFile = cloudFiles.FirstOrDefault(f => f.Name == localFile.Name);

            if (cloudFile == null)
            {
                _cloudStorageRepository.CreateFile(credentials, localFile.Path, parentId, localFile.Name, mimeType);
            }
            else if (DateHelper.CheckIfDateIsNewer(cloudFile.LastModified, localFile.LastModified))
            {
                _logger.LogInformation(
                    $"File {cloudFile.Name} was not uploaded as its version is older than the cloud version.");
            }
            else
            {
                localFile.Id = cloudFile.Id;
                localFile.ParentId = cloudFile.ParentId;
                await _cloudStorageRepository.UpdateFile(credentials, localFile.Path, localFile);
            }
        }

        return true;
    }
}