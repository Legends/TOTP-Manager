using TOTP.ViewModels;

namespace TOTP.Infrastructure.Extensions
{
    public static class AccountItemViewModelExtensions
    {
        public static bool ValueEquals(this AccountViewModel a, AccountViewModel b)
            => AccountViewModelValueComparer.Default.Equals(a, b);
    }

}
