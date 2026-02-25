using TOTP.Core.Models;
using TOTP.ViewModels;

namespace TOTP.Infrastructure.Extensions;

public static class AccountMappingExtensions
{
    public static OtpEntry ToDomain(this AccountViewModel vm)
        => new(vm.ID, vm.Issuer ?? string.Empty, vm.Secret ?? string.Empty, vm.AccountName ?? string.Empty);

    public static AccountViewModel ToViewModel(this OtpEntry s)
        => new(
              s.ID,
              s.Issuer,
              s.Secret,
              s.AccountName
        );
}
