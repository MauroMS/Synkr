namespace CloudSynkr.Models;

public static class Constants
{
    public static class Exceptions
    {
        public const string FileNotFound = "File not found";
        public const string CredentialsNotFound = "Credentials not found";
        public const string ErrorOccurred = "Error occurred...";
        public const string ErrorGettingLocalFolders = "Error getting local folders";
        public const string FailedToCreateFolder = "Failed to create Folder '{filePath}'";
        public const string FailedToSaveFileToFolder = "Failed to save file '{fileName}' to '{filePath}'";
        public const string FailedToRetrieveFilesFrom = "Failed to retrieve files from '{fileFullPath}'";
        public const string FailedToDownloadFilesFrom = "Failed to download files from '{fileFullPath}'";
        public const string FailedToUploadFilesTo = "Failed to upload files to '{cloudPath}'";
        public const string FailedToUploadFileTo = "Failed to upload file '{fileName}' to '{cloudPath}'";
        
        public const string FileDoesntExists = "File '{clientInfoPath}' does not exists";
        public const string LoginFailed = "Login failed";
        public const string UnableToRetrieveCredentials = "Unable to retrieve credentials";

        public const string FailedToRetrieveCreateFolderOn =
            "Error retrieving/creating folder '{folderName}' on path '{path}'";

        public const string MimeTypeDoesntExistsOnMapping = "MimeType '{0}' doesn't exists on mapping. MimeType set to default value '{1}'.";
        public const string MimeTypeCannotBeNull = "MimeType of a file cannot be null.";
    }

    public static class Information
    {
        public const string StartedLogin = "Started Login";
        public const string StartedDownload = "Starting Download";
        public const string FinishedDownload = "Finished Download";
        public const string ApplicationStarted = "Application Started";
        public const string ApplicationStopped = "Application Stopped";
        public const string StartingUpload = "Starting Upload";
        public const string FinishedUpload = "Finished Upload";

        public const string DownloadedFileProgress =
            "Downloaded File: '{fileId}' -- Progress: {progress} -- Bytes Downloaded: {bytes}";

        public const string Synkr = "Synkr";
        public const string FolderDoesntExistsOn = "Folder '{folderName}' doesn't exists on parent '{parentName}'";
        public const string StartedToUploadFileProgress = "Started to Upload file: '{fileName}' to '{fileParentName}'";
        public const string CreatedFileNameId = "Created File Name: '{fileName}' ID: '{fileId}'";
        public const string UploadingFileProgress = "Uploading file: '{fileName}' to '{fileParentName}'";
        public const string UploadedFileProgress = "File '{fileName}' successfully uploaded to '{fileParentName}'";
        public const string NoFilesFoldersToDownload = "No folders/files to download";
        public const string NoFilesFoldersToUpload = "No folders/files to upload";
        public const string CreatingFolder = "Creating Folder: {folderName}";
        public const string CreatedFolderName = "Created Folder Name: '{folderName}' ID: '{folderId}'";
        public const string GetAllFilesFromFolder = "Get All Files From Folder: {folderName}";
        public const string DownloadingFileProgress = "Downloading file id: '{fileId}' -- progress: {progress}";

        public const string LocalFileIsOlderThanCloudFile =
            "File {localFileName} was not uploaded as its version is older than the cloud version";

        public const string CloudFileIsOlderThanLocalFile =
            "File '{cloudFileName}' was not downloaded as its version is older than the local version";

        public const string FailedToDownloadFileToFolder = "File {cloudFileName} was downloaded to {localFolder}";
    }

    public static class Warning
    {
        public const string UploadFailedProgress = "Error uploading file '{fileName}'";
        public const string FailedToDownloadFile = "Failed to download file {fileId}";
        public const string FolderDoesntExists = "Folder '{folderPath}' doesn't exists";
    }
}