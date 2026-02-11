
using TOTP.ViewModels;

namespace TOTP.Extensions
{
    public static class SecretItemViewModelExtensions
    {
        public static bool ValueEquals(this AccountViewModel a, AccountViewModel b)
            => AccountViewModelValueComparer.Default.Equals(a, b);
    }

}
