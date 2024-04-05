using CloudSynkr.Models;
using CloudSynkr.Repositories.Interfaces;
using CloudSynkr.Services.Interfaces;
using CloudSynkr.Utils;
using Microsoft.Extensions.Logging;
using File = CloudSynkr.Models.File;

namespace CloudSynkr.Services;

public class DownloadService(
    ICloudStorageRepository cloudStorageRepository,
    ILocalStorageRepository localStorageRepository,
    ILogger<DownloadService> logger,
    IAuthService authService)
    : IDownloadService
{
    public async Task<bool> Download(List<Mapping> mappings,
        CancellationToken cancellationToken)
    {
        foreach (var folderMap in mappings)
        {
            if (folderMap.FilesToSync.Count > 0)
            {
                await DownloadSpecificFilesFromFolder(folderMap, cancellationToken);
                continue;
            }

            var folderStructure = await GetFolderStructureToDownload(folderMap.CloudFolderParentId,
                folderMap.CloudFolderParentName, folderMap.CloudFolder,
                cancellationToken);

            await DownloadFilesFromFolders(folderStructure, folderMap.LocalFolder,
                cancellationToken);
        }

        return true;
    }

    public async Task<bool> DownloadSpecificFilesFromFolder(Mapping folderMap, CancellationToken cancellationToken)
    {
        try
        {
            var credentials = await authService.Login(cancellationToken);

            var folder = await cloudStorageRepository.GetBasicFolderInfoByNameAndParentId(credentials,
                folderMap.CloudFolderParentId, folderMap.CloudFolder, cancellationToken);
            var allFiles =
                await cloudStorageRepository.GetAllFilesFromFolder(credentials, folder.Id, folder.Name,
                    cancellationToken);
            folder.Files = allFiles.Where(f => folderMap.FilesToSync.Contains(f.Name)).ToList();

            await DownloadFiles(folder.Files, folderMap.LocalFolder, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, Constants.Exceptions.FailedToDownloadFilesFrom, folderMap.CloudFolder);
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
        var credentials = await authService.Login(cancellationToken);

        var folder =
            await cloudStorageRepository.GetBasicFolderInfoByNameAndParentId(credentials, parentId, folderName,
                cancellationToken);

        if (folder == null)
        {
            logger.LogInformation(Constants.Information.FolderDoesntExistsOn, folderName,
                parentName);
            return [];
        }

        var folderStructure = await cloudStorageRepository.GetAllFoldersByParentId(credentials, folder.Id, folder.Name,
            folder.ParentId, folder.Name, new CancellationToken());

        return folderStructure;
    }

    public async Task<bool> DownloadFiles(List<File> files, string localFolder, CancellationToken cancellationToken)
    {
        MemoryStream? fileStream;
        var credentials = await authService.Login(cancellationToken);
        var localFiles = localStorageRepository.GetLocalFiles(localFolder);
        foreach (var cloudFile in files)
        {
            var localFile = localFiles.FirstOrDefault(f => f.Name == cloudFile.Name);

            if (localFile != null &&
                DateHelper.CheckIfDateIsNewer(localFile.LastModified, cloudFile.LastModified))
            {
                logger.LogInformation(Constants.Information.CloudFileIsOlderThanLocalFile
                    ,
                    cloudFile.Name);
                continue;
            }

            fileStream = await cloudStorageRepository.DownloadFile(cloudFile.Id, credentials);
            if (fileStream == null)
            {
                logger.LogWarning(Constants.Warning.FailedToDownloadFile, cloudFile.Name);
                return false;
            }

            localStorageRepository.SaveStreamAsFile(localFolder, fileStream, cloudFile.Name);
            logger.LogInformation(Constants.Information.FailedToDownloadFileToFolder, cloudFile.Name,
                localFolder);
        }

        return true;
    }
}