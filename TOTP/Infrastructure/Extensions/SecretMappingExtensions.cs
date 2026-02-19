using TOTP.Core.Models;
using TOTP.ViewModels;

namespace TOTP.Infrastructure.Extensions;

public static class AccountMappingExtensions
{
    public static AccountItem ToDomain(this AccountViewModel vm)
        => new(vm.ID, vm.Platform ?? string.Empty, vm.Secret ?? string.Empty, vm.Account ?? string.Empty);

    public static AccountViewModel ToViewModel(this AccountItem s)
        => new(
              s.ID,
              s.Platform,
              s.Secret,
              s.Account
        );
}
