using System.Threading.Tasks;

namespace TOTP.Services.Interfaces;

public interface IAutoUpdateService
{
    Task InitializeAsync();
    Task CheckForUpdatesInteractiveAsync();
}
