using CloudSynkr.App;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);
builder.ConfigureServices();

var host = builder.Build();

//Use this if you don't want the application to close, but atm is pointless as this is run/done app.
//await host.RunAsync();

await host.StartAsync();

// await app.Run(new CancellationToken());
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