using CloudSynkr.Models;
using CloudSynkr.Models.Exceptions;
using CloudSynkr.Repositories.Interfaces;
using CloudSynkr.Services.Interfaces;
using CloudSynkr.Utils;
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

            var cloudFolder = await GetCloudFolder(folderMap.CloudFolder, folderMap.CloudFolderParentName,
                folderMap.CloudFolderParentId,
                cancellationToken);

            if (cloudFolder == null) return false;

            await UploadFilesToFolders(folderStructure, cloudFolder.Name,
                cloudFolder.Id, true, cancellationToken);
        }

        return true;
    }

    private async Task<Folder?> GetCloudFolder(string folderName, string subFolder,
        string parentId, CancellationToken cancellationToken)
    {
        var credentials = await authService.Login(cancellationToken);
        if (string.IsNullOrEmpty(folderName))
            return new Folder { Id = parentId };

        var cloudFolder =
            await cloudStorageRepository.GetBasicFolderInfoByNameAndParentId(credentials, parentId, folderName,
                cancellationToken) ??
            await cloudStorageRepository.CreateFolder(credentials, folderName, parentId, cancellationToken);

        if (cloudFolder != null) return cloudFolder;

        logger.LogError(Constants.Exceptions.FailedToRetrieveCreateFolderOn, folderName,
            subFolder);
        return null;
    }

    public async Task<bool> UploadSpecificFilesToFolder(Mapping folderMap, CancellationToken cancellationToken)
    {
        var cloudFolder = await GetCloudFolder(folderMap.CloudFolder, folderMap.CloudFolderParentName,
            folderMap.CloudFolderParentId, cancellationToken);

        if (cloudFolder == null) return false;

        var files = localStorageRepository.GetLocalFiles(folderMap.LocalFolder)
            .Where(f => folderMap.FilesToSync.Contains(f.Name)).ToList();

        await UploadFiles(files, cloudFolder.Id, cancellationToken);

        return true;
    }

    public async Task<bool> UploadFilesToFolders(List<Folder> folderStructure,
        string cloudFolderPath, string parentId, bool skipTopFolderCreation, CancellationToken cancellationToken)
    {
        foreach (var folder in folderStructure)
        {
            var subFolder = Path.Combine(cloudFolderPath, folder.Name);
            var cloudFolder = await GetCloudFolder(skipTopFolderCreation ? "" : folder.Name, folder.ParentName,
                parentId, cancellationToken);
            
            if (cloudFolder == null) return false;

            await UploadFiles(folder.Files, cloudFolder.Id, cancellationToken);
            await UploadFilesToFolders(folder.Children, subFolder, cloudFolder.Id, false, cancellationToken);
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
            File? cloudFile = null;

            try
            {
                var mimeType = MimeTypeMapHelper.GetMimeType(localFile.Name);
                if (string.IsNullOrEmpty(mimeType))
                {
                    logger.LogWarning(Constants.Exceptions.MimeTypeDoesntExistsOnMapping, localFile.Name,
                        MimeTypeMapHelper.GetDefaultMimeType());
                    mimeType = MimeTypeMapHelper.GetDefaultMimeType();
                }

                cloudFile = cloudFiles.FirstOrDefault(f => f.Name == localFile.Name);

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
            catch (MimeTypeException ex)
            {
                logger.LogWarning(Constants.Exceptions.FailedToUploadFileTo, cloudFile?.Name,
                    cloudFile?.ParentName ?? localFile.Path);
                logger.LogError(ex, ex.Message);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, Constants.Exceptions.FailedToUploadFilesTo,
                    cloudFile?.ParentName ?? localFile.Path);
            }
        }

        return true;
    }
}