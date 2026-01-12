
using TOTP.ViewModels;

namespace TOTP.Extensions
{
    public static class SecretItemViewModelExtensions
    {
        public static bool ValueEquals(this SecretItemViewModel a, SecretItemViewModel b)
            => SecretItemViewModelValueComparer.Default.Equals(a, b);
    }

}
