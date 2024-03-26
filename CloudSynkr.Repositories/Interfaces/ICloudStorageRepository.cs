using CloudSynkr.Models;
using Google.Apis.Auth.OAuth2;
using File = CloudSynkr.Models.File;

namespace CloudSynkr.Repositories.Interfaces;

public interface ICloudStorageRepository
{
    Task<string> CreateFolder(string folderName, UserCredential credentials, string parentId,
        CancellationToken cancellationToken);

    Task<Folder?> GetBasicFolderInfoByName(UserCredential credentials, string parentId, string folderName,
        CancellationToken cancellationToken);

    Task<Folder?> GetBasicFolderInfoById(UserCredential credentials, string folderId,
        CancellationToken cancellationToken);

    Task<List<Folder>> GetAllFoldersByParentId(UserCredential credentials, string folderId,
        string folderName, string parentId, string fullPath, CancellationToken cancellationToken);

    Task<List<File>> GetAllFilesFromFolder(UserCredential credentials, string parentId,
        string parentName, CancellationToken cancellationToken);

    Task<MemoryStream?> DownloadFile(string fileId, UserCredential credentials);

    string UploadNewFile(UserCredential credentials, string filePath, string name);

    Task<string> UpdateFile(UserCredential credentials, string filePath, File file);
}