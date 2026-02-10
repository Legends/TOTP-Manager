using System.Threading.Tasks;
using TOTP.Security.Models;

namespace TOTP.Security.Interfaces;

public interface IGlobalProfileStore
{
    Task<GlobalProfile?> LoadAsync();
    Task SaveAsync(GlobalProfile profile);
}
