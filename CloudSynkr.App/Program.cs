using CloudSynkr.App;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);
builder.ConfigureServices();

var host = builder.Build();

//Use this if you don't want the application to close, but atm is pointless as this is run/done app.
//await host.RunAsync();

await host.StartAsync();

await host.StopAsync();