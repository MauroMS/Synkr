using CloudSynkr.Services.Interfaces;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Util.Store;

namespace CloudSynkr.App;

public class AuthService : IAuthService
{
    private readonly List<string> _scopes = [DriveService.ScopeConstants.Drive];
    private const string ClientInfoPath = @"credentials.json";
    private UserCredential? _userCredential;

    public async Task<UserCredential?> Login(CancellationToken cancellationToken)
    {
        if (_userCredential != null)
            return _userCredential;

        Console.WriteLine("Login");
        GoogleClientSecrets? clientSecrets = null;
        try
        {
            await using var stream = new FileStream(ClientInfoPath, FileMode.Open, FileAccess.Read);
            clientSecrets = await GoogleClientSecrets.FromStreamAsync(stream, cancellationToken);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }

        _userCredential = await GoogleWebAuthorizationBroker.AuthorizeAsync(clientSecrets?.Secrets, _scopes, "user",
            cancellationToken, new FileDataStore("Synkr"));

        return _userCredential;
    }
}