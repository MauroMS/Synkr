using Microsoft.Extensions.Configuration;

namespace CloudSynkr.App;

public static class EnvironmentConfiguration
{
    public static IConfiguration ConfigureAppSettings(string[] args)
    {
        IConfiguration configurationBuilder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional:false , reloadOnChange:true)
            .AddEnvironmentVariables()
            .AddCommandLine(args)
            .Build();
        
        return configurationBuilder;
    }
}