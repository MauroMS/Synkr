// See https://aka.ms/new-console-template for more information

using CloudSynkr;


Console.WriteLine("Hello, World!");

// var response = await Auth.GetAllFoldersByParentId(new CancellationToken());
string localFolderTemp = Directory.GetCurrentDirectory();// @"C:\Projs\CloudSynkr\CloudSynkr\Backup";
// var response = await LocalStorageService.GetLocalFolders(LocalFolderTemp, new Folder());
var credentials = await Auth.Login(new CancellationToken());
// var response = CloudStorageService.DownloadFile("1hWpaOWd6p08qPJyQ0ItvQZG_c9jO-RBx", credentials);
// LocalStorageService.SaveStreamAsFile($"{localFolderTemp}\\Test\\1\\2", response, "Nintendo - Snes M.lpl");

var response = CloudStorageService.UploadNewFile(credentials, $"{localFolderTemp}\\Test\\1\\2", "Nintendo - Snes M.lpl");

// Console.ReadLine();