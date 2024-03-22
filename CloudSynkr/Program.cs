// See https://aka.ms/new-console-template for more information

using CloudSynkr;
using CloudSynkr.Models;

Console.WriteLine("Hello, World!");

// var response = await Auth.GetAllFoldersByParentId(new CancellationToken());
const string LocalFolderTemp = @"C:\Projs\CloudSynkr\CloudSynkr\Backup";
var response = await LocalStorageService.GetLocalFolders(LocalFolderTemp, new Folder());

Console.ReadLine();