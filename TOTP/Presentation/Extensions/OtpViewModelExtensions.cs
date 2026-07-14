using TOTP.ViewModels;

namespace TOTP.Presentation.Extensions
{
    public static class OtpViewModelExtensions
    {
        public static bool ValueEquals(this OtpViewModel a, OtpViewModel b)
            => OtpViewModelValueComparer.Default.Equals(a, b);
    }

}
