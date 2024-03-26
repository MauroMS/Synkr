using CloudSynkr;
using CloudSynkr.App;
using CloudSynkr.Models;
using CloudSynkr.Repositories;
using CloudSynkr.Repositories.Interfaces;
using CloudSynkr.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);
var configuration = EnvironmentConfiguration.ConfigureAppSettings([]);
builder.Services.Configure<SyncBackup>(configuration.GetSection("SyncBackup"));
builder.Services.AddSingleton<IAuthService, AuthService>();
builder.Services.AddScoped<ISyncService, SyncService>();
builder.Services.AddTransient<IDownloadService, DownloadService>();
builder.Services.AddTransient<ICloudStorageRepository, CloudStorageRepository>();
builder.Services.AddTransient<ILocalStorageRepository, LocalStorageRepository>();
using IHost host = builder.Build();

Console.WriteLine("Hello, World!");
var app = host.Services.GetRequiredService<ISyncService>();

await host.StartAsync();

await app.Run(new CancellationToken());

await host.StopAsync();

// string
//     localFolderTemp =
//         @"C:\Projs\CloudSynkr\CloudSynkr\Backup"; // Directory.GetCurrentDirectory();// @"C:\Projs\CloudSynkr\CloudSynkr\Backup";
// string parentIdTemp = "1FZPuYuAYRO3DkcN4H8Bfy8zh1884gjju";

// var credentials = await Auth.Login(new CancellationToken());
// var fileFullPath = $"{localFolderTemp}\\Backup\\Test\\1\\2\\Copy of Nintendo - Snes M.lpl";
//
// var response = await LocalStorageService.GetLocalFolders(localFolderTemp);

// var response = CloudStorageService.DownloadFile("1hWpaOWd6p08qPJyQ0ItvQZG_c9jO-RBx", credentials);

// var response = await CloudStorageService.GetBasicFolderInfoById(credentials, "1FZPuYuAYRO3DkcN4H8Bfy8zh1884gjju", new CancellationToken());
// var response = await CloudStorageService.GetFolderById(credentials, "1FZPuYuAYRO3DkcN4H8Bfy8zh1884gjju", new CancellationToken());
// var response = await CloudStorageService.GetAllFilesFromFolder(credentials, "1-8MWLWyS9PSOUV8aXElPUdGWbwqTw3E8", "ParentName",new CancellationToken());

// LocalStorageService.SaveStreamAsFile($"{localFolderTemp}\\Test\\1\\2", response, "Nintendo - Snes M.lpl");

// var response = CloudStorageService.UploadNewFile(credentials, $"{localFolderTemp}\\Test\\1\\2", "Nintendo - Snes M.lpl");
// var response = CloudStorageService.CreateFile(credentials, $"{localFolderTemp}\\Test\\1\\2", "Nintendo - Snes M.lpl");
// var response = await CloudStorageService.UpdateFile(credentials, fileFullPath, new File()
// {
//     ParentId = "1-8MWLWyS9PSOUV8aXElPUdGWbwqTw3E8",
//     ParentName = "Test 2",
//     LastModified = DateTimeOffset.Now,
//     MimeType = FileType.Lpl,
//     Name = "Copy of Nintendo - Snes M.lpl",
//     Id = "1wCekBG6SAPxteDW26vOQmCwW9w5fYNXd"
// });

// var folder = await CloudStorageService.GetBasicFolderInfoById(credentials, parentIdTemp, new CancellationToken());
// var startTime = DateTime.Now;
// var response = await CloudStorageService.GetAllFoldersByParentId(credentials, folder.Id, folder.Name, folder.ParentId,
//     folder.Name, new CancellationToken());
// var endTime = DateTime.Now;
// var serialized = JsonSerializer.Serialize(response);
// Console.WriteLine((endTime - startTime).TotalSeconds);
// Console.WriteLine("Finished");
// Console.ReadLine();