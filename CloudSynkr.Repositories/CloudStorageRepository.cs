using CloudSynkr.Models;
using CloudSynkr.Models.Extensions;
using CloudSynkr.Repositories.Interfaces;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Download;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Upload;
using Microsoft.Extensions.Logging;
using File = CloudSynkr.Models.File;

namespace CloudSynkr.Repositories;

public class CloudStorageRepository : ICloudStorageRepository
{
    private readonly ILogger<CloudStorageRepository> _logger;

    public CloudStorageRepository(ILogger<CloudStorageRepository> logger)
    {
        _logger = logger;
    }

    public async Task<Folder?> CreateFolder(UserCredential credentials, string folderName, string parentId,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating Folder: {folderName}", folderName);

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
        request.Fields = "id, name, mimeType, modifiedTime, parents";
        var file = await request.ExecuteAsync(cancellationToken);
        // Prints the created folder id.
        _logger.LogInformation("Created Folder ID: {fileId}", file.Id);
        _logger.LogInformation("Created Folder Name: {fileName}", file.Name);

        return file?.MapFolder();
    }

    public async Task<Folder?> GetBasicFolderInfoByNameAndParentId(UserCredential credentials, string parentId,
        string folderName,
        CancellationToken cancellationToken)
    {
        using var driveService = new DriveService(new BaseClientService.Initializer()
            { HttpClientInitializer = credentials, ApplicationName = "Synkr" });

        //TODO: MOVE THIS BLOCK TO ANOTHER METHOD
        var foldersRequest = driveService.Files.List();
        foldersRequest.Fields = "files(id, name, mimeType, modifiedTime, parents)";
        foldersRequest.Q =
            $"name = '{folderName}' and mimeType = '{FileType.Folder}' and trashed=false and '{parentId}' in parents";
        var listFoldersRequest = await foldersRequest.ExecuteAsync(cancellationToken);
        //TODO: MOVE THIS BLOCK TO ANOTHER METHOD

        var folder = listFoldersRequest.Files.FirstOrDefault();

        return folder?.MapFolder();
    }

    public async Task<Folder?> GetBasicFolderInfoByNameAndParentName(UserCredential credentials, string parentName,
        string folderName,
        CancellationToken cancellationToken)
    {
        using var driveService = new DriveService(new BaseClientService.Initializer()
            { HttpClientInitializer = credentials, ApplicationName = "Synkr" });

        //TODO: MOVE THIS BLOCK TO ANOTHER METHOD
        var foldersRequest = driveService.Files.List();
        foldersRequest.Fields = "files(id, name, mimeType, modifiedTime, parents)";
        foldersRequest.Q =
            $"name = '{folderName}' and mimeType = '{FileType.Folder}' and trashed=false";
        var listFoldersRequest = await foldersRequest.ExecuteAsync(cancellationToken);
        //TODO: MOVE THIS BLOCK TO ANOTHER METHOD

        var folders = listFoldersRequest.Files;
        Google.Apis.Drive.v3.Data.File? folder = null;
        if (folders.Count == 1)
        {
            folder = folders.FirstOrDefault();
        }
        else
        {
            foreach (var fold in listFoldersRequest.Files)
            {
                var folderInfo =
                    await GetBasicFolderInfoByNameAndParentId(credentials, fold.Parents[0], fold.Name,
                        cancellationToken);
                if (folderInfo != null && folderInfo.Name.Equals(parentName))
                    return folderInfo;
            }
        }

        return folder?.MapFolder();
    }

    public async Task<Folder?> GetBasicFolderInfoById(UserCredential credentials, string folderId,
        CancellationToken cancellationToken)
    {
        //TODO: DO I NEED THIS METHOD??
        using var driveService = new DriveService(new BaseClientService.Initializer()
            { HttpClientInitializer = credentials, ApplicationName = "Synkr" });

        //TODO: MOVE THIS BLOCK TO ANOTHER METHOD
        var foldersRequest = driveService.Files.List();
        foldersRequest.Fields = "files(id, name, mimeType, modifiedTime, parents)";
        foldersRequest.Q = $"mimeType = '{FileType.Folder}' and trashed=false";
        var listFoldersRequest = await foldersRequest.ExecuteAsync(cancellationToken);
        //TODO: MOVE THIS BLOCK TO ANOTHER METHOD

        var folder = listFoldersRequest.Files.FirstOrDefault(folder => folder.Id == folderId);

        return folder?.MapFolder();
    }

    public async Task<List<Folder>> GetAllFoldersByParentId(UserCredential credentials, string folderId,
        string folderName, string parentId, string fullPath, CancellationToken cancellationToken)
    {
        var folders = new List<Folder>();
        var folder = new Folder()
        {
            Id = folderId,
            Name = folderName,
            Type = FileType.Folder,
            ParentId = parentId,
            Path = fullPath
        };

        using var driveService = new DriveService(new BaseClientService.Initializer()
            { HttpClientInitializer = credentials, ApplicationName = "Synkr" });

        //TODO: MOVE THIS BLOCK TO ANOTHER METHOD
        var foldersRequest = driveService.Files.List();
        foldersRequest.Fields = "files(id, name, mimeType, modifiedTime, parents)";
        foldersRequest.Q = $"trashed=false and '{folderId}' in parents";
        var listFoldersRequest = await foldersRequest.ExecuteAsync(cancellationToken);
        //TODO: MOVE THIS BLOCK TO ANOTHER METHOD

        folder.Files.AddRange(listFoldersRequest.Files.Where(f => f.MimeType != FileType.Folder).Select(f => new File()
        {
            Name = f.Name,
            Id = f.Id,
            ParentId = folderId,
            LastModified = f.ModifiedTimeDateTimeOffset,
            MimeType = f.MimeType,
            ParentName = folderName,
        }).ToList());

        foreach (var subFolderPath in listFoldersRequest.Files.Where(f => f.MimeType == FileType.Folder))
        {
            fullPath += @$"\{subFolderPath.Name}";
            var subFolders =
                await GetAllFoldersByParentId(credentials, subFolderPath.Id, subFolderPath.Name, folderId, fullPath,
                    cancellationToken);
            folder.Children.AddRange(subFolders);
        }

        folders.Add(folder);

        return folders;
    }

    public async Task<List<File>> GetAllFilesFromFolder(UserCredential credentials, string parentId,
        string parentName, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Get All Files From Folder: {folderName}", parentName);
        using var driveService = new DriveService(new BaseClientService.Initializer()
            { HttpClientInitializer = credentials, ApplicationName = "Synkr" });

        var filesRequest = driveService.Files.List();
        filesRequest.Fields = "files(id, name, mimeType, modifiedTime, parents)";
        filesRequest.Q = $"'{parentId}' in parents and mimeType != '{FileType.Folder}' and trashed = false";

        var listFilesRequest = await filesRequest.ExecuteAsync(cancellationToken);
        var files = listFilesRequest.Files.Select(f => new File()
        {
            Name = f.Name,
            Id = f.Id,
            ParentId = f.Parents[0],
            LastModified = f.ModifiedTimeDateTimeOffset,
            MimeType = f.MimeType,
            ParentName = parentName
        }).ToList();

        return files;
    }

    public async Task<MemoryStream?> DownloadFile(string fileId, UserCredential credentials)
    {
        try
        {
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
                            _logger.LogDebug("Downloading file id: {fileId} -- progress: {progress}", fileId,
                                progress.BytesDownloaded.ToString());
                            break;
                        }
                        case DownloadStatus.Completed:
                        {
                            _logger.LogInformation(
                                "Download File: {fileId} -- Progress: {progress} - Bytes Downloaded: {bytes}", fileId,
                                progress.Status, progress.BytesDownloaded);
                            break;
                        }
                        case DownloadStatus.Failed:
                        {
                            _logger.LogWarning("Download failed for file {fileId}.", fileId);
                            break;
                        }
                    }
                };
            await request.DownloadAsync(stream);

            return stream;
        }
        catch (Exception e)
        {
            // TODO(developer) - handle error appropriately
            if (e is AggregateException)
            {
                _logger.LogError(e, "Credential Not found");
            }
            else
            {
                _logger.LogError(e, "Filed to download file {fileid}", fileId);
                return null;
            }
        }

        return null;
    }

    public string CreateFile(UserCredential credentials, string localFilePath, string parentId, string name,
        string mimeType)
    {
        try
        {
            var service = new DriveService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credentials,
                ApplicationName = "Synkr"
            });

            var fileMetadata = new Google.Apis.Drive.v3.Data.File()
            {
                Name = name,
                Parents =
                [
                    parentId
                ]
            };
            FilesResource.CreateMediaUpload request;

            using (var stream = new FileStream(localFilePath,
                       FileMode.Open))
            {
                request = service.Files.Create(fileMetadata, stream, mimeType);
                request.Fields = "id, name";
                request.Upload();
            }

            var file = request.ResponseBody;
            _logger.LogInformation("Created File Name: {fileName} ID: {fileId}", file.Name, file.Id);
            return file.Id;
        }
        catch (Exception e)
        {
            switch (e)
            {
                // TODO(developer) - handle error appropriately
                case AggregateException:
                    _logger.LogError(e, "Credentials Not found");
                    break;
                case FileNotFoundException:
                    _logger.LogError(e, "File not found");
                    break;
                default:
                    throw;
            }
        }

        return null;
    }

    public async Task<string> UpdateFile(UserCredential credentials, string filePath, File file)
    {
        try
        {
            var service = new DriveService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credentials,
                ApplicationName = "Synkr"
            });

            var updateFileBody = new Google.Apis.Drive.v3.Data.File()
            {
                Name = file.Name,
                MimeType = file.MimeType
            };

            await using (var uploadStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                var updateRequest = service.Files.Update(updateFileBody, file.Id, uploadStream, file.MimeType);
                var result = await updateRequest.UploadAsync(CancellationToken.None);

                switch (result.Status)
                {
                    case UploadStatus.Starting:
                        _logger.LogInformation("Start to Upload file: '{file.Name}' to '{file.ParentName}'", file.Name,
                            file.ParentName);
                        break;
                    case UploadStatus.Uploading:
                        _logger.LogInformation("Uploading file: '{file.Name}' to '{file.ParentName}'", file.Name,
                            file.ParentName);
                        break;
                    case UploadStatus.Completed:
                        _logger.LogInformation("File '{file.Name}' successfully uploaded to '{file.ParentName}'",
                            file.Name, file.ParentName);
                        break;
                    case UploadStatus.Failed:
                        _logger.LogWarning(result.Exception, "Error uploading file '{file.Name}'", file.Name);
                        break;
                }
            }

            return file.Id;
        }
        catch (Exception e)
        {
            switch (e)
            {
                // TODO(developer) - handle error appropriately
                case AggregateException:
                    _logger.LogError(e, "Credentials not found.");
                    break;
                case FileNotFoundException:
                    _logger.LogError(e, "File not found");
                    break;
                default:
                    throw;
            }
        }

        return null;
    }
}