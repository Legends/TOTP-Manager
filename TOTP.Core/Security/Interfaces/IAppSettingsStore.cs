using System.Threading.Tasks;

namespace TOTP.Core.Security.Interfaces;

public interface IAppSettingsDAL
{
    Task<IAppSettings?> LoadAsync();
    Task SaveAsync(IAppSettings profile);
}
