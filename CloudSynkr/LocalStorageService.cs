using CloudSynkr.Models;
using File = CloudSynkr.Models.File;

namespace CloudSynkr;

public static class LocalStorageService
{
    private const string LocalFolderTemp = @"C:\Projs\CloudSynkr\CloudSynkr\Backup\";

    public static async Task<List<Folder>> GetLocalFolders(string folderPath, Folder? folder = null)
    {
        var foldersArray = Directory.GetDirectories(folderPath);
        var folders = new List<Folder>();
        foreach (var subFolderPath in foldersArray)
        {
            var folderName = subFolderPath.Split(@"\").Last();
            Console.WriteLine(folderName);
            folder = GetLocalFiles(subFolderPath);
            folder.Name = folderName;
            folder.Path = subFolderPath;
            var subFolders = await GetLocalFolders(subFolderPath, folder);
            folder.Children.AddRange(subFolders);
            folders.Add(folder);
        }

        return folders;
    }

    public static Folder GetLocalFiles(string fileFullPath)
    {
        var folder = new Folder();
        var files = Directory.GetFiles(fileFullPath);
        foreach (var file in files)
        {
            folder.Files.Add(new File()
            {
                Name = GetName(file),
                Path = file,
                LastModified = System.IO.File.GetLastWriteTimeUtc(file)
            });
        }

        return folder;
    }

    private static string GetName(string fullPath)
    {
        return fullPath.Split(@"\").Last();
    }
}