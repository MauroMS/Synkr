using CloudSynkr.Models;
using File = CloudSynkr.Models.File;

namespace CloudSynkr.Services.Interfaces;

public interface IUploadService
{
    Task<bool> Upload(List<Mapping> mappings, CancellationToken cancellationToken);

    Task<bool> UploadFilesToFolders(List<Folder> folderStructure,
        string cloudFolderPath, string parentId, CancellationToken cancellationToken);

    Task<List<Folder>> GetFolderStructureToUpload(string folderPath);

    Task<bool> UploadFiles(List<File> files, string localFolder,
        CancellationToken cancellationToken);
}