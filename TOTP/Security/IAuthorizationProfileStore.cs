using System.Threading.Tasks;

namespace TOTP.Security;

public interface IAuthorizationProfileStore
{
    Task<AuthorizationProfile?> LoadAsync();
    Task SaveAsync(AuthorizationProfile profile);
    Task ClearAsync();
}