using CloudSynkr.Services.Interfaces;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Util.Store;

namespace CloudSynkr.Services;

public class AuthService : IAuthService
{
    private readonly List<string> _scopes = [DriveService.ScopeConstants.Drive];
    private readonly string clientInfoPath = Path.Combine(Directory.GetCurrentDirectory(), "credentials.json");
    private UserCredential? _userCredential;
    // private readonly ILogger _logger;

    // public AuthService(ILogger logger)
    // {
    //     _logger = logger;
    // }
    
    public async Task<UserCredential?> Login(CancellationToken cancellationToken)
    {
        if (_userCredential != null)
            return _userCredential;

        Console.WriteLine("Login");
        GoogleClientSecrets? clientSecrets = null;
        try
        {
            await using var stream = new FileStream(clientInfoPath, FileMode.Open, FileAccess.Read);
            clientSecrets = await GoogleClientSecrets.FromStreamAsync(stream, cancellationToken);
        }
        catch (DirectoryNotFoundException ex)
        {
            Console.WriteLine($"File '{clientInfoPath}' does not exists");
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