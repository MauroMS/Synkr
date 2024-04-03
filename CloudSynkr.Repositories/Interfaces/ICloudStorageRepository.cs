using CloudSynkr.Models;
using Google.Apis.Auth.OAuth2;
using File = CloudSynkr.Models.File;

namespace CloudSynkr.Repositories.Interfaces;

public interface ICloudStorageRepository
{
    Task<Folder?> CreateFolder(UserCredential credentials, string folderName, string parentId,
        CancellationToken cancellationToken);

    Task<Folder?> GetBasicFolderInfoByNameAndParentId(UserCredential credentials, string parentId, string folderName,
        CancellationToken cancellationToken);

    Task<Folder?> GetBasicFolderInfoById(UserCredential credentials, string folderId,
        CancellationToken cancellationToken);

    Task<List<Folder>> GetAllFoldersByParentId(UserCredential credentials, string folderId,
        string folderName, string parentId, string fullPath, CancellationToken cancellationToken);

    Task<List<File>> GetAllFilesFromFolder(UserCredential credentials, string parentId,
        string parentName, CancellationToken cancellationToken);

    Task<MemoryStream?> DownloadFile(string? fileId, UserCredential credentials);

    string? CreateFile(UserCredential credentials, string localFilePath, string parentId, string name, string mimeType);

    Task<string?> UpdateFile(UserCredential credentials, string filePath, File file);
}