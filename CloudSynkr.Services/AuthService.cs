using CloudSynkr.Models.Exceptions;
using CloudSynkr.Services.Interfaces;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Util.Store;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CloudSynkr.Services;

public class AuthService(ILogger<AuthService> logger, IConfiguration configuration) : IAuthService
{
    private readonly List<string> _scopes = [DriveService.ScopeConstants.Drive];
    private UserCredential? _userCredential;

    public async Task<UserCredential> Login(CancellationToken cancellationToken)
    {
        var clientInfoPath = "";
        try
        {
            logger.LogInformation("Started Login");

            clientInfoPath = GetClientInfoPath();

            if (_userCredential != null)
                return _userCredential;

            await using var stream = new FileStream(clientInfoPath, FileMode.Open, FileAccess.Read);
            var clientSecrets = await GoogleClientSecrets.FromStreamAsync(stream, cancellationToken);

            _userCredential = await GoogleWebAuthorizationBroker.AuthorizeAsync(clientSecrets?.Secrets, _scopes, "user",
                cancellationToken, new FileDataStore("Synkr"));

            if (_userCredential == null)
                throw new LoginException("Unable to retrieve credentials");
        }
        catch (DirectoryNotFoundException ex)
        {
            logger.LogError(ex, "File '{clientInfoPath}' does not exists", clientInfoPath);
            throw new LoginException();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Login failed");
            throw new LoginException();
        }

        return _userCredential;
    }

    private string GetClientInfoPath()
    {
        var email = configuration["email"];
        var fileName = "";
        if (!string.IsNullOrEmpty(email))
            fileName = $@"{email}-";

        fileName = $@"{fileName}credentials.json";

        return Path.Combine(Directory.GetCurrentDirectory(), fileName);
    }
}