// See https://aka.ms/new-console-template for more information

using CloudSynkr;
using CloudSynkr.Models;

Console.WriteLine("Hello, World!");

// var response = await Auth.GetAllFoldersByParentId(new CancellationToken());
const string LocalFolderTemp = @"C:\Projs\CloudSynkr\CloudSynkr\Backup";
// var response = await LocalStorageService.GetLocalFolders(LocalFolderTemp, new Folder());
var credentials = await Auth.Login(new CancellationToken());
var response = CloudStorageService.DownloadFile("1hWpaOWd6p08qPJyQ0ItvQZG_c9jO-RBx", credentials);
LocalStorageService.SaveStreamAsFile($"{LocalFolderTemp}\\Test\\1\\2", response, "Nintendo - Snes M.lpl");
// Console.ReadLine();