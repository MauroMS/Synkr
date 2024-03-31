using CloudSynkr.Models;
using Google.Apis.Auth.OAuth2;
using File = CloudSynkr.Models.File;

namespace CloudSynkr.Services.Interfaces;

public interface IUploadService
{
    Task<bool> Upload(UserCredential credentials, List<Mapping> mappings,
        CancellationToken cancellationToken);

    Task<bool> UploadFilesToFolders(UserCredential credentials, List<Folder> folderStructure,
        string cloudFolderPath, string parentId, CancellationToken cancellationToken);

    Task<List<Folder>> GetFolderStructureToUpload(string folderPath);

    Task<bool> UploadFiles(UserCredential credentials, List<File> files, string localFolder,
        CancellationToken cancellationToken);
}