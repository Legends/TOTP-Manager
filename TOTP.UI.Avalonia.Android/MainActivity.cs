using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using Avalonia.Android;

namespace TOTP.Avalonia.Android;

[Activity(
    Label = "OTP Harbor",
    Theme = "@style/OtpHarborTheme",
    Icon = "@drawable/app_icon",
    MainLauncher = true,
    Exported = true,
    ConfigurationChanges = ConfigChanges.Orientation
        | ConfigChanges.ScreenSize
        | ConfigChanges.UiMode
        | ConfigChanges.KeyboardHidden)]
public class MainActivity : AvaloniaMainActivity
{
    internal event Action<int, Result, Intent?>? ActivityResultReceived;

    protected override void OnCreate(Bundle? savedInstanceState)
    {
        Window?.SetFlags(WindowManagerFlags.Secure, WindowManagerFlags.Secure);
        base.OnCreate(savedInstanceState);
        if (Application is OtpHarborApplication host) host.AttachActivity(this);
    }

    protected override void OnStart()
    {
        base.OnStart();
        if (Application is OtpHarborApplication host)
            host.NotifyReturnedToForeground();
    }

    protected override void OnStop()
    {
        if (!IsChangingConfigurations && Application is OtpHarborApplication host)
            host.NotifyEnteredBackground(IsDeviceUnavailable());
        base.OnStop();
    }

    protected override void OnDestroy()
    {
        if (Application is OtpHarborApplication host) host.DetachActivity(this);
        base.OnDestroy();
    }

    protected override void OnActivityResult(int requestCode, Result resultCode, Intent? data)
    {
        base.OnActivityResult(requestCode, resultCode, data);
        ActivityResultReceived?.Invoke(requestCode, resultCode, data);
    }

    private bool IsDeviceUnavailable()
    {
        var keyguard = GetSystemService(Context.KeyguardService) as KeyguardManager;
        var power = GetSystemService(Context.PowerService) as PowerManager;
        return keyguard?.IsDeviceLocked == true || power?.IsInteractive == false;
    }
}
