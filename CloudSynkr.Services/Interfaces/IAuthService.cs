using Google.Apis.Auth.OAuth2;

namespace CloudSynkr.Services.Interfaces;

public interface IAuthService
{
    Task<UserCredential> Login(CancellationToken cancellationToken);
}