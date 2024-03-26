using CloudSynkr.Models;
using Google.Apis.Auth.OAuth2;
using File = CloudSynkr.Models.File;

namespace CloudSynkr.Services.Interfaces;

public interface IDownloadService
{
    Task<bool> Download(UserCredential credentials, List<Mapping> mappings, CancellationToken cancellationToken);
    
    Task<List<Folder>> GetFolderStructureToDownload(UserCredential credentials, string parentId,
        string parentName, string folderName, CancellationToken cancellationToken);

    Task<bool> DownloadFilesFromFolders(UserCredential credentials, List<Folder> folderStructure, string localFolder,
        CancellationToken cancellationToken);

    Task<bool> DownloadFiles(UserCredential credentials, List<File> files, string localFolder);
}