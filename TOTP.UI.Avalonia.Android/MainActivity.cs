using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using Avalonia.Android;

namespace TOTP.Avalonia.Android;

[Activity(
    Label = "OTP Harbor",
    MainLauncher = true,
    Exported = true,
    ConfigurationChanges = ConfigChanges.Orientation
        | ConfigChanges.ScreenSize
        | ConfigChanges.UiMode
        | ConfigChanges.KeyboardHidden)]
public class MainActivity : AvaloniaMainActivity
{
    protected override void OnCreate(Bundle? savedInstanceState)
    {
        Window?.SetFlags(WindowManagerFlags.Secure, WindowManagerFlags.Secure);
        base.OnCreate(savedInstanceState);
    }
}
