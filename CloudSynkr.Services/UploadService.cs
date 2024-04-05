using CloudSynkr.Models;
using CloudSynkr.Repositories.Interfaces;
using CloudSynkr.Services.Interfaces;
using CloudSynkr.Utils;
using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.Logging;
using File = CloudSynkr.Models.File;

namespace CloudSynkr.Services;

public class UploadService(
    ICloudStorageRepository cloudStorageRepository,
    ILocalStorageRepository localStorageRepository,
    ILogger<UploadService> logger,
    IAuthService authService)
    : IUploadService
{
    public async Task<bool> Upload(List<Mapping> mappings,
        CancellationToken cancellationToken)
    {
        foreach (var folderMap in mappings)
        {
            if (folderMap.FilesToSync.Count > 0)
            {
                await UploadSpecificFilesToFolder(folderMap, cancellationToken);
                continue;
            }

            var folderStructure = await GetFolderStructureToUpload(folderMap.LocalFolder);

            await UploadFilesToFolders(folderStructure, folderMap.CloudFolder,
                folderMap.CloudFolderParentId, cancellationToken);
        }

        return true;
    }

    public async Task<bool> UploadSpecificFilesToFolder(Mapping folderMap, CancellationToken cancellationToken)
    {
        var credentials = await authService.Login(cancellationToken);

        var cloudFolder =
            await cloudStorageRepository.GetBasicFolderInfoByNameAndParentId(credentials, folderMap.CloudFolderParentId,
                folderMap.CloudFolder,
                cancellationToken) ??
            await cloudStorageRepository.CreateFolder(credentials, folderMap.CloudFolder, folderMap.CloudFolderParentId,
                cancellationToken);

        if (cloudFolder == null)
        {
            logger.LogError(Constants.Exceptions.FailedToRetrieveCreateFolderOn, folderMap.CloudFolder,
                folderMap.CloudFolderParentName);
            return false;
        }

        var files = localStorageRepository.GetLocalFiles(folderMap.LocalFolder)
            .Where(f => folderMap.FilesToSync.Contains(f.Name)).ToList();

        await UploadFiles(files, cloudFolder.Id, cancellationToken);

        return true;
    }

    public async Task<bool> UploadFilesToFolders(List<Folder> folderStructure,
        string cloudFolderPath, string parentId, CancellationToken cancellationToken)
    {
        var credentials = await authService.Login(cancellationToken);
        foreach (var folder in folderStructure)
        {
            var subFolder = Path.Combine(cloudFolderPath, folder.Name);

            var cloudFolder =
                await cloudStorageRepository.GetBasicFolderInfoByNameAndParentId(credentials, parentId, folder.Name,
                    cancellationToken) ??
                await cloudStorageRepository.CreateFolder(credentials, folder.Name, parentId, cancellationToken);

            if (cloudFolder == null)
            {
                logger.LogError(Constants.Exceptions.FailedToRetrieveCreateFolderOn, folder.Name,
                    subFolder);
                return false;
            }

            await UploadFiles(folder.Files, cloudFolder.Id, cancellationToken);
            await UploadFilesToFolders(folder.Children, subFolder, cloudFolder.Id, cancellationToken);
        }

        return true;
    }

    public async Task<List<Folder>> GetFolderStructureToUpload(string folderPath)
    {
        return await localStorageRepository.GetLocalFolders(folderPath);
    }

    public async Task<bool> UploadFiles(List<File> files, string folderId,
        CancellationToken cancellationToken)
    {
        var credentials = await authService.Login(cancellationToken);
        if (files.Count == 0)
            return false;

        var cloudFiles =
            await cloudStorageRepository.GetAllFilesFromFolder(credentials, folderId, files[0].ParentName,
                cancellationToken);

        foreach (var localFile in files)
        {
            var mimeType = MimeTypeMapHelper.GetMimeType(localFile.Name);
            var cloudFile = cloudFiles.FirstOrDefault(f => f.Name == localFile.Name);

            if (cloudFile == null)
            {
                cloudStorageRepository.CreateFile(credentials, localFile.Path, folderId, localFile.Name, mimeType);
            }
            else if (DateHelper.CheckIfDateIsNewer(cloudFile.LastModified, localFile.LastModified))
            {
                logger.LogInformation(Constants.Information.LocalFileIsOlderThanCloudFile, cloudFile.Name);
            }
            else
            {
                localFile.Id = cloudFile.Id;
                localFile.ParentId = cloudFile.ParentId;
                await cloudStorageRepository.UpdateFile(credentials, localFile.Path, localFile);
            }
        }

        return true;
    }
}