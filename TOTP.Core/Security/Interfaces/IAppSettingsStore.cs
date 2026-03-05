using FluentResults;
using System.Threading.Tasks;

namespace TOTP.Core.Security.Interfaces;

public interface IAppSettingsDAL : IDisposable
{
    Task<Result<IAppSettings?>> LoadAsync();
    Task<Result> SaveAsync(IAppSettings profile);
}
