using CloudSynkr.Models;
using File = CloudSynkr.Models.File;

namespace CloudSynkr.Repositories.Interfaces;

public interface ILocalStorageRepository
{
    Task<List<Folder>> GetLocalFolders(string folderPath);

    void CreateFolder(string filePath);

    void SaveStreamAsFile(string filePath, MemoryStream inputStream, string fileName);

    List<File> GetLocalFiles(string fileFullPath);
}