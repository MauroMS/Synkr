using CloudSynkr.Models;
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
            logger.LogInformation(Constants.Information.StartedLogin);

            clientInfoPath = GetClientInfoPath();

            if (_userCredential != null)
                return _userCredential;

            await using var stream = new FileStream(clientInfoPath, FileMode.Open, FileAccess.Read);
            var clientSecrets = await GoogleClientSecrets.FromStreamAsync(stream, cancellationToken);

            _userCredential = await GoogleWebAuthorizationBroker.AuthorizeAsync(clientSecrets?.Secrets, _scopes, "user",
                cancellationToken, new FileDataStore(Constants.Information.Synkr));

            if (_userCredential == null)
                throw new LoginException(Constants.Exceptions.UnableToRetrieveCredentials);
        }
        catch (DirectoryNotFoundException ex)
        {
            logger.LogError(ex, Constants.Exceptions.FileDoesntExists, clientInfoPath);
            throw new LoginException();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, Constants.Exceptions.LoginFailed);
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