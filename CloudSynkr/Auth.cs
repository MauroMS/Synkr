using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Util.Store;

namespace CloudSynkr;

public static class Auth
{
    private static readonly List<string> Scopes = new List<string>() { DriveService.ScopeConstants.Drive };
    private const string ClientInfoPath = @"credentials.json";

    public static async Task<UserCredential> Login(CancellationToken cancellationToken)
    {
        Console.WriteLine("Login");
        GoogleClientSecrets clientSecrets = null;
        //try
        //{
        await using (var stream = new FileStream(ClientInfoPath, FileMode.Open, FileAccess.Read))
        {
            clientSecrets = GoogleClientSecrets.FromStream(stream);
        }
        //}
        //catch (Exception ex)
        //{
        //    Console.WriteLine(ex.Message);
        //}

        return await GoogleWebAuthorizationBroker.AuthorizeAsync(clientSecrets?.Secrets, Scopes, "user",
            cancellationToken, new FileDataStore("Synkr"));
    }
}