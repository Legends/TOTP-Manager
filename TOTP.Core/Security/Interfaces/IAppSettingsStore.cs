using FluentResults;
using System.Threading.Tasks;

namespace TOTP.Core.Security.Interfaces;

public interface IAppSettingsDAL
{
    Task<Result<IAppSettings?>> LoadAsync();
    Task<Result> SaveAsync(IAppSettings profile);
}
