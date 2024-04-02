using CloudSynkr.Services.Interfaces;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Util.Store;
using Microsoft.Extensions.Logging;

namespace CloudSynkr.Services;

public class AuthService : IAuthService
{
    private readonly List<string> _scopes = [DriveService.ScopeConstants.Drive];
    private readonly string clientInfoPath = Path.Combine(Directory.GetCurrentDirectory(), "credentials.json");
    private UserCredential? _userCredential;
    private readonly ILogger<AuthService> _logger;

    public AuthService(ILogger<AuthService> logger)
    {
        _logger = logger;
    }
    
    public async Task<UserCredential?> Login(CancellationToken cancellationToken)
    {
        if (_userCredential != null)
            return _userCredential;

        _logger.LogInformation("Started Login");
        GoogleClientSecrets? clientSecrets = null;
        try
        {
            await using var stream = new FileStream(clientInfoPath, FileMode.Open, FileAccess.Read);
            clientSecrets = await GoogleClientSecrets.FromStreamAsync(stream, cancellationToken);
        }
        catch (DirectoryNotFoundException ex)
        {
            _logger.LogError($"File '{clientInfoPath}' does not exists");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message);
        }

        _userCredential = await GoogleWebAuthorizationBroker.AuthorizeAsync(clientSecrets?.Secrets, _scopes, "user",
            cancellationToken, new FileDataStore("Synkr"));

        return _userCredential;
    }
}