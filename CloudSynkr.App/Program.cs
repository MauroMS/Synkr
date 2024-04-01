using CloudSynkr.App;
using CloudSynkr.Models;
using CloudSynkr.Repositories;
using CloudSynkr.Repositories.Interfaces;
using CloudSynkr.Services;
using CloudSynkr.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);
var configuration = EnvironmentConfiguration.ConfigureAppSettings([]);
builder.Services.Configure<SyncBackup>(configuration.GetSection("SyncBackup"));
builder.Services.AddScoped<ISyncService, SyncService>();
builder.Services.AddSingleton<IAuthService, AuthService>();
builder.Services.AddTransient<IUploadService, UploadService>();
builder.Services.AddTransient<IDownloadService, DownloadService>();
builder.Services.AddTransient<ICloudStorageRepository, CloudStorageRepository>();
builder.Services.AddTransient<ILocalStorageRepository, LocalStorageRepository>();
builder.Services.AddSerilog(config =>
{
    config.ReadFrom.Configuration(builder.Configuration)
        .WriteTo.Console();
    // .WriteTo.File()
});
// builder.Logging.AddSerilog()
using var host = builder.Build();



var app = host.Services.GetRequiredService<ISyncService>();

//TODO: ADD WORKER TEMPLATE

await host.StartAsync();

await app.Run(new CancellationToken());
// var auth = host.Services.GetRequiredService<IAuthService>();
// var credentials = await auth.Login(new CancellationToken());
// var upload = host.Services.GetRequiredService<ICloudStorageRepository>();
// var response = upload.CreateFile(credentials, @"C:\\Projs\\CloudSynkr\\CloudSynkr.App\\Backup 8", TODO, "Nintendo - Snes M.lpl", FileType.Lpl);

await host.StopAsync();

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

// var startTime = DateTime.Now;
// var endTime = DateTime.Now;
// var serialized = JsonSerializer.Serialize(response);
// Console.WriteLine((endTime - startTime).TotalSeconds);
// Console.WriteLine("Finished");
// Console.ReadLine();