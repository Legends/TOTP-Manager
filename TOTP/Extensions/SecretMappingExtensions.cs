using TOTP.Core.Models;
using TOTP.ViewModels;

namespace TOTP.Extensions;

public static class SecretMappingExtensions
{
    public static SecretItem ToDomain(this SecretItemViewModel vm)
        => new(vm.ID, vm.Platform ?? string.Empty, vm.Secret ?? string.Empty, vm.Account ?? string.Empty);

    public static SecretItemViewModel ToViewModel(this SecretItem s)
        => new(
              s.ID,
              s.Platform,
              s.Secret,
              s.Account
        );
}
