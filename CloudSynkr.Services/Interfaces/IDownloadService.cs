using CloudSynkr.Models;
using Google.Apis.Auth.OAuth2;
using File = CloudSynkr.Models.File;

namespace CloudSynkr.Services.Interfaces;

public interface IDownloadService
{
    Task<bool> Download(List<Mapping> mappings, CancellationToken cancellationToken);
    
    Task<List<Folder>> GetFolderStructureToDownload(string parentId,
        string parentName, string folderName, CancellationToken cancellationToken);

    Task<bool> DownloadFilesFromFolders(List<Folder> folderStructure, string localFolder,
        CancellationToken cancellationToken);

    Task<bool> DownloadFiles(List<File> files, string localFolder, CancellationToken cancellationToken);

    Task<Dictionary<string, string>> GetNewFileMimeTypes(CancellationToken cancellationToken);
}