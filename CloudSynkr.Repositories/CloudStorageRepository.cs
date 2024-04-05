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

public class CloudStorageRepository(ILogger<CloudStorageRepository> logger) : ICloudStorageRepository
{
    public async Task<Folder?> CreateFolder(UserCredential credentials, string folderName, string parentId,
        CancellationToken cancellationToken)
    {
        logger.LogInformation(Constants.Information.CreatingFolder, folderName);

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
        logger.LogInformation(Constants.Information.CreatedFolderName, file.Name, file.Id);

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

    public async Task<List<File>> GetAllFilesFromFolder(UserCredential credentials, string folderId,
        string folderName, CancellationToken cancellationToken)
    {
        logger.LogInformation(Constants.Information.GetAllFilesFromFolder, folderName);
        using var driveService = new DriveService(new BaseClientService.Initializer()
            { HttpClientInitializer = credentials, ApplicationName = Constants.Information.Synkr });

        var filesRequest = driveService.Files.List();
        filesRequest.Fields = "files(id, name, mimeType, modifiedTime, parents)";
        filesRequest.Q = $"'{folderId}' in parents and mimeType != '{FileType.Folder}' and trashed = false";

        var listFilesRequest = await filesRequest.ExecuteAsync(cancellationToken);
        var files = listFilesRequest.Files.Select(f => new File()
        {
            Name = f.Name,
            Id = f.Id,
            ParentId = f.Parents[0],
            LastModified = f.ModifiedTimeDateTimeOffset,
            MimeType = f.MimeType,
            ParentName = folderName
        }).ToList();

        return files;
    }

    public async Task<MemoryStream?> DownloadFile(string? fileId, UserCredential credentials)
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
                            logger.LogDebug(Constants.Information.DownloadingFileProgress, fileId,
                                progress.BytesDownloaded.ToString());
                            break;
                        }
                        case DownloadStatus.Completed:
                        {
                            logger.LogInformation(Constants.Information.DownloadedFileProgress, fileId, progress.Status,
                                progress.BytesDownloaded);
                            break;
                        }
                        case DownloadStatus.Failed:
                        {
                            logger.LogWarning(Constants.Warning.FailedToDownloadFile, fileId);
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
                logger.LogError(e, Constants.Exceptions.CredentialsNotFound);
            }
            else
            {
                logger.LogError(e, Constants.Warning.FailedToDownloadFile, fileId);
                return null;
            }
        }

        return null;
    }

    public string? CreateFile(UserCredential credentials, string localFilePath, string parentId, string name,
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
            logger.LogInformation(Constants.Information.CreatedFileNameId, file.Name, file.Id);
            return file.Id;
        }
        catch (Exception e)
        {
            switch (e)
            {
                // TODO(developer) - handle error appropriately
                case AggregateException:
                    logger.LogError(e, Constants.Exceptions.CredentialsNotFound);
                    break;
                case FileNotFoundException:
                    logger.LogError(e, Constants.Exceptions.FileNotFound);
                    break;
                default:
                    logger.LogError(e, Constants.Exceptions.FailedToUploadFilesTo, parentId);
                    throw;
            }
        }

        return null;
    }

    public async Task<string?> UpdateFile(UserCredential credentials, string filePath, File file)
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
                        logger.LogInformation(Constants.Information.StartedToUploadFileProgress, file.Name,
                            file.ParentName);
                        break;
                    case UploadStatus.Uploading:
                        logger.LogInformation(Constants.Information.UploadingFileProgress, file.Name, file.ParentName);
                        break;
                    case UploadStatus.Completed:
                        logger.LogInformation(Constants.Information.UploadedFileProgress, file.Name, file.ParentName);
                        break;
                    case UploadStatus.Failed:
                        logger.LogWarning(result.Exception, Constants.Warning.UploadFailedProgress, file.Name);
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
                    logger.LogError(e, Constants.Exceptions.CredentialsNotFound);
                    break;
                case FileNotFoundException:
                    logger.LogError(e, Constants.Exceptions.FileNotFound);
                    break;
                default:
                    throw;
            }
        }

        return null;
    }
    
    public async Task<List<Folder>> GetAllFoldersPlainList(UserCredential credentials, string parentId,
        string parentName, CancellationToken cancellationToken)
    {
        //TODO: MAYBE SPEED CAN BE IMPROVED BY LOADING ALL FOLDERS WHEN IT FIRST RUN INSTEAD OF NAVIGATING LEVELS AS IT'S NOW.
        var folders = new List<Folder>();
        
        using var driveService = new DriveService(new BaseClientService.Initializer()
            { HttpClientInitializer = credentials, ApplicationName = "Synkr" });

        //TODO: MOVE THIS BLOCK TO ANOTHER METHOD
        var foldersRequest = driveService.Files.List();
        foldersRequest.Fields = "files(id, name, mimeType, modifiedTime, parents)";
        foldersRequest.Q = $"trashed=false and mimeType = '{FileType.Folder}'";
        var listFoldersRequest = await foldersRequest.ExecuteAsync(cancellationToken);
        //TODO: MOVE THIS BLOCK TO ANOTHER METHOD

        foreach (var foldr in listFoldersRequest.Files)
        {
            folders.Add(new Folder()
            {
                Name = foldr.Name,
                Id = foldr.Id,
                ParentId = foldr.Parents?[0] ?? "",
                Type = FileType.Folder,
            });
        }

        foreach (var fold in folders)
        {
            fold.ParentName = folders.FirstOrDefault(f => f.Id == fold.ParentId)?.Name ?? "";
        }

        return folders;
    }
}