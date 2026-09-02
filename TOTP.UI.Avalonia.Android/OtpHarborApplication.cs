using Android.App;
using Android.Runtime;
using Avalonia;
using Avalonia.Android;
using TOTP.Avalonia.Mobile;

namespace TOTP.Avalonia.Android;

[Application(AllowBackup = false, UsesCleartextTraffic = false)]
public class OtpHarborApplication : AvaloniaAndroidApplication<MobileApp>
{
    protected OtpHarborApplication(nint javaReference, JniHandleOwnership transfer)
        : base(javaReference, transfer)
    {
    }

    protected override AppBuilder CustomizeAppBuilder(AppBuilder builder)
    {
        return base.CustomizeAppBuilder(builder);
    }
}
