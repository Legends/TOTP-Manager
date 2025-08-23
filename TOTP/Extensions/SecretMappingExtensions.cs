using TOTP.Core.Models;
using TOTP.ViewModels;

namespace TOTP.Extensions;

public static class SecretMappingExtensions
{
    public static SecretItem ToDomain(this SecretItemViewModel vm)
        => new(vm.Platform ?? string.Empty, vm.Secret ?? string.Empty, vm.Account);

    public static SecretItemViewModel ToViewModel(this SecretItem s)
        => new(
             s.Platform,
              s.Secret
        //Account = s.Account
        );
}
