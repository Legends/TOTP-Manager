using System.Threading.Tasks;

namespace TOTP.Security;

public interface IGlobalProfileStore
{
    Task<GlobalProfile?> LoadAsync();
    Task SaveAsync(GlobalProfile profile);
}
