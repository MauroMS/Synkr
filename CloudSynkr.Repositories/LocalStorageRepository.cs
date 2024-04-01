using CloudSynkr.Models;
using CloudSynkr.Repositories.Interfaces;
using File = CloudSynkr.Models.File;

namespace CloudSynkr.Repositories;

public class LocalStorageRepository : ILocalStorageRepository
{
    public LocalStorageRepository()
    {
        
    }
    
    public async Task<List<Folder>> GetLocalFolders(string folderPath)
    {
        var folders = new List<Folder>();
        try
        {
            var foldersArray = Directory.GetDirectories(folderPath);

            var folder = new Folder
            {
                Name = GetName(folderPath),
                Path = folderPath,
                Files = GetLocalFiles(folderPath),
                ParentId = GetParentName(folderPath)
            };

            foreach (var subFolderPath in foldersArray)
            {
                var subFolders = await GetLocalFolders(subFolderPath);
                folder.Children.AddRange(subFolders);
            }

            folders.Add(folder);
        }
        catch (DirectoryNotFoundException ex)
        {
            Console.WriteLine($"Folder '{folderPath}' does not exists.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception: {ex.InnerException}");
        }

        return folders;
    }

    public void CheckIfFolderExistsAndCreate(string filePath)
    {
        try
        {
            var info = new DirectoryInfo(filePath);
            if (!info.Exists)
            {
                info.Create();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Folder '{filePath}' could not be created.");
            throw;
        }
    }

    public void SaveStreamAsFile(string filePath, MemoryStream inputStream, string fileName)
    {
        CheckIfFolderExistsAndCreate(filePath);
        inputStream.Position = 0;
        var path = Path.Combine(filePath, fileName);
        
        try
        {
            using (var outputFileStream = new FileStream(path, FileMode.Create))
            {
                inputStream.CopyTo(outputFileStream);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Could not save file '{fileName}'");
            Console.WriteLine($"Error: {ex.Message}");
            throw;
        }
    }

    public List<File> GetLocalFiles(string fileFullPath)
    {
        var files = new List<File>();
        try
        {
            var filesPaths = Directory.GetFiles(fileFullPath);
            foreach (var file in filesPaths)
            {
                files.Add(new File()
                {
                    Name = GetName(file),
                    Path = file,
                    LastModified = System.IO.File.GetLastWriteTimeUtc(file),
                    ParentName = GetParentName(file)
                });
            }
        }
        catch (DirectoryNotFoundException ex)
        {
            Console.WriteLine($"Folder '{fileFullPath}' does not exists");
            return [];
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Cannot get files from '{fileFullPath}' due to: {ex.Message}");
        }

        return files;
    }

    private string GetName(string fullPath)
    {
        return fullPath.Split(@"\").Last();
    }

    //TODO: POPULATE ParentId with path
    private string GetParentName(string fullPath)
    {
        var paths = fullPath.Split(@"\");
        return paths.Length > 1 ? paths[^2] : paths[^1];
    }
}