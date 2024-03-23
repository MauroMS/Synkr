using CloudSynkr.Models;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Download;
using Google.Apis.Drive.v3;
using Google.Apis.Services;

namespace CloudSynkr;

public static class CloudStorageService
{
    private const string ParentIdTemp = "1FZPuYuAYRO3DkcN4H8Bfy8zh1884gjju";
    
     public static async Task<string> CreateFolder(string folderName, UserCredential credentials, string parentId, CancellationToken cancellationToken)
    {
        Console.WriteLine("CreateFolder");

        using var driveService = new DriveService(new BaseClientService.Initializer()
            { HttpClientInitializer = credentials, ApplicationName = "Synkr" });
        
        // File metadata
        var fileMetadata = new Google.Apis.Drive.v3.Data.File()
        {
            Name = folderName,
            MimeType = "application/vnd.google-apps.folder",
            Parents = new List<string>
            {
                parentId
            }
        };
        
        // Create a new folder on drive.
        var request = driveService.Files.Create(fileMetadata);
        request.Fields = "id";
        var file = await request.ExecuteAsync(cancellationToken);
        // Prints the created folder id.
        Console.WriteLine("Folder ID: " + file.Id);
        Console.WriteLine("Folder Name: " + file.Name);
        
        return file.Id;
    }
    
    public static async Task<List<Folder>> GetAllFoldersByParentId(UserCredential credentials, CancellationToken cancellationToken)
    {
        Console.WriteLine("GetAllFoldersByParentId");
        // var credentials = await Login(cancellationToken);
        var folders = new List<Folder>();

        using var driveService = new DriveService(new BaseClientService.Initializer()
            { HttpClientInitializer = credentials, ApplicationName = "Synkr" });
        
        var foldersRequest = driveService.Files.List();
        foldersRequest.Fields = "files(id, name, mimeType, modifiedTime, parents)";
        foldersRequest.Q = $"mimeType = '{FileType.Folder}' and trashed=false and '{ParentIdTemp}' in parents";
        var listFoldersRequest = await foldersRequest.ExecuteAsync(cancellationToken);
        folders.AddRange(listFoldersRequest.Files.Select(folder => new Folder() { Name = folder.Name, Id = folder.Id, ParentId = folder.Parents[0] }));

        // Console.WriteLine($"Id: {folder.Id}");
        // Console.WriteLine($"Name: {folder.Name}");
        // Console.WriteLine($"Type: {folder.MimeType}");
        // if (folder.Parents != null)
        //     foreach (var parent in folder.Parents)
        //     {
        //         Console.WriteLine($"{parent}");
        //     }
        //
        // Console.WriteLine($"-----------------------------------");
        // Console.WriteLine($"-----------------------------------");

        return folders;
    }

    public static async Task<List<string>> GetAllFilesFromFolder(UserCredential credentials, CancellationToken cancellationToken)
    {
        Console.WriteLine("GetAllFilesFromFolder");
        using var driveService = new DriveService(new BaseClientService.Initializer()
            { HttpClientInitializer = credentials, ApplicationName = "Synkr" });
        
        var filesRequest = driveService.Files.List();
        filesRequest.Fields = "files(id, name, mimeType, modifiedTime)";
        filesRequest.Q = $"mimeType = '{FileType.Folder}'";

        var listFilesRequest = await filesRequest.ExecuteAsync(cancellationToken);

        foreach (var file in listFilesRequest.Files)
        {
            Console.WriteLine($"{file.Name} - {file.MimeType}");
        }

        return null;
    }

    // public static async Task<List<string>> GetAllItemsByQuery(UserCredential credentials, string query, CancellationToken cancellationToken)
    // {
    //     Console.WriteLine("GetAllFoldersByParentId");
    //     // var credentials = await Login(cancellationToken);
    //     var items = new List<Folder>();
    //
    //     using var driveService = new DriveService(new BaseClientService.Initializer()
    //         { HttpClientInitializer = credentials, ApplicationName = "Synkr" });
    //     
    //     var itemsRequest = driveService.Files.List();
    //     itemsRequest.Fields = "files(id, name, mimeType, modifiedTime, parents)";
    //     itemsRequest.Q = $"{query} and trashed=false";
    //     var listFoldersRequest = await itemsRequest.ExecuteAsync(cancellationToken);
    //     items.AddRange(listFoldersRequest.Files.Select(folder => new Folder() { Name = folder.Name, Id = folder.Id, ParentId = folder.Parents[0] }));
    //
    //     return items;
    // }

    public static MemoryStream? DownloadFile(string fileId, UserCredential credentials)
    {
        try
        {
            // Create Drive API service.
            var service = new DriveService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credentials,
                ApplicationName = "Synkr"
            });

            var request = service.Files.Get(fileId);
            var stream = new MemoryStream();

            // Add a handler which will be notified on progress changes.
            // It will notify on each chunk download and when the
            // download is completed or failed.
            request.MediaDownloader.ProgressChanged +=
                progress =>
                {
                    switch (progress.Status)
                    {
                        case DownloadStatus.Downloading:
                        {
                            Console.WriteLine(progress.BytesDownloaded);
                            break;
                        }
                        case DownloadStatus.Completed:
                        {
                            Console.WriteLine($"IDownloadProgress: {progress.Status} ({progress.BytesDownloaded})");
                            break;
                        }
                        case DownloadStatus.Failed:
                        {
                            Console.WriteLine("Download failed.");
                            break;
                        }
                    }
                };
            request.Download(stream);
            
            return stream;
        }
        catch (Exception e)
        {
            // TODO(developer) - handle error appropriately
            if (e is AggregateException)
            {
                Console.WriteLine("Credential Not found");
            }
            else
            {
                throw;
            }
        }

        return null;
    }
}