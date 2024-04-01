using CloudSynkr.Models;
using CloudSynkr.Repositories.Interfaces;
using CloudSynkr.Services.Interfaces;
using CloudSynkr.Utils;
using Google.Apis.Auth.OAuth2;
using File = CloudSynkr.Models.File;

namespace CloudSynkr.Services;

public class DownloadService : IDownloadService
{
    private readonly ICloudStorageRepository _cloudStorageRepository;
    private readonly ILocalStorageRepository _localStorageRepository;

    public DownloadService(ICloudStorageRepository cloudStorageRepository,
        ILocalStorageRepository localStorageRepository)
    {
        _cloudStorageRepository = cloudStorageRepository;
        _localStorageRepository = localStorageRepository;
    }

    public async Task<bool> Download(UserCredential credentials, List<Mapping> mappings,
        CancellationToken cancellationToken)
    {
        foreach (var folderMap in mappings)
        {
            //TODO: ADD LOGIC TO MAP FILE TO RESULT... PROBABLY A DICTIONARY... ADD RETRY LOGIC?
            var folderStructure = await GetFolderStructureToDownload(credentials,
                folderMap.CloudFolderParentId, folderMap.CloudFolderParentName, folderMap.CloudFolder,
                cancellationToken);

            await DownloadFilesFromFolders(credentials, folderStructure, folderMap.LocalFolder,
                cancellationToken);
        }

        return true;
    }

    public async Task<bool> DownloadFilesFromFolders(UserCredential credentials, List<Folder> folderStructure,
        string localFolder, CancellationToken cancellationToken)
    {
        foreach (var folder in folderStructure)
        {
            var subFolder = Path.Combine(localFolder, folder.Name);
            await DownloadFiles(credentials, folder.Files, subFolder);
            await DownloadFilesFromFolders(credentials, folder.Children, subFolder, cancellationToken);
        }

        return true;
    }

    public async Task<List<Folder>> GetFolderStructureToDownload(UserCredential credentials, string parentId,
        string parentName, string folderName, CancellationToken cancellationToken)
    {
        var folder =
            await _cloudStorageRepository.GetBasicFolderInfoByNameAndParentId(credentials, parentId, folderName,
                cancellationToken);

        if (folder == null)
        {
            //TODO: MOVE TO LOG
            Console.WriteLine($"Folder '{folderName}' doesn't exists on parent '{parentName}'");
            return [];
        }

        var folderStructure = await _cloudStorageRepository.GetAllFoldersByParentId(credentials, folder.Id, folder.Name,
            folder.ParentId, folder.Name, new CancellationToken());

        return folderStructure;
    }

    public async Task<bool> DownloadFiles(UserCredential credentials, List<File> files, string localFolder)
    {
        MemoryStream? fileStream;
        var localFiles = _localStorageRepository.GetLocalFiles(localFolder);
        foreach (var cloudFile in files)
        {
            //TODO: LOG/RETURN SOMETHING INDICATING FILES DOWNLOADED OR NOT
            var localFile = localFiles.FirstOrDefault(f => f.Name == cloudFile.Name);

            if (localFile != null &&
                DateHelper.CheckIfDateIsNewer(localFile.LastModified, cloudFile.LastModified))
            {
                Console.WriteLine(
                    $"File {cloudFile.Name} was not downloaded as its version is older than the local version.");
                continue;
            }

            fileStream = await _cloudStorageRepository.DownloadFile(cloudFile.Id, credentials);
            _localStorageRepository.SaveStreamAsFile(localFolder, fileStream, cloudFile.Name);
            Console.WriteLine($"File {cloudFile.Name} was downloaded to {localFolder}");
        }

        return true;
    }
}