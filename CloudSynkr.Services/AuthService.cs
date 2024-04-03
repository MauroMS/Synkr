using CloudSynkr.Services.Interfaces;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Util.Store;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CloudSynkr.Services;

public class AuthService : IAuthService
{
    private readonly List<string> _scopes = [DriveService.ScopeConstants.Drive];
    private UserCredential? _userCredential;
    private readonly ILogger<AuthService> _logger;
    private readonly IConfiguration _configuration;

    public AuthService(ILogger<AuthService> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    public async Task<UserCredential?> Login(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Started Login");

        if (_userCredential != null)
            return _userCredential;

        var clientInfoPath = GetClientInfoPath();

        GoogleClientSecrets? clientSecrets = null;
        try
        {
            await using var stream = new FileStream(clientInfoPath, FileMode.Open, FileAccess.Read);
            clientSecrets = await GoogleClientSecrets.FromStreamAsync(stream, cancellationToken);
        }
        catch (DirectoryNotFoundException ex)
        {
            _logger.LogError(ex, "File '{clientInfoPath}' does not exists", clientInfoPath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Login failed");
        }

        _userCredential = await GoogleWebAuthorizationBroker.AuthorizeAsync(clientSecrets?.Secrets, _scopes, "user",
            cancellationToken, new FileDataStore("Synkr"));

        return _userCredential;
    }

    private string GetClientInfoPath()
    {
        var email = _configuration["email"];
        var fileName = "";
        if (!string.IsNullOrEmpty(email))
            fileName = $@"{email}-";
        
        fileName = $@"{fileName}credentials.json";

        return Path.Combine(Directory.GetCurrentDirectory(), fileName);
    }
}