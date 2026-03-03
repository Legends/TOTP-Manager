namespace TOTP.Core.Security.Interfaces;

public interface ISettingsService
{
    IAppSettings Current { get; }
    Task<IAppSettings> LoadAsync();
    Task SaveAsync();
    //Task InitializeAsync();
}
