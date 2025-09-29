using TOTP.Comparer;
using TOTP.ViewModels;

namespace TOTP.Extensions
{
    public static class SecretItemViewModelExtensions
    {
        public static bool ValueEquals(this SecretItemViewModel a, SecretItemViewModel b)
            => SecretItemValueComparer.Default.Equals(a, b);
    }

}
