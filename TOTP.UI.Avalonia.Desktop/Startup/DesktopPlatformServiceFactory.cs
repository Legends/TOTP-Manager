using TOTP.Core.Services.Interfaces;
#if TOTP_PLATFORM_WINDOWS
using TOTP.Platform.Windows;
#elif TOTP_PLATFORM_MACOS
using TOTP.Platform.MacOS;
using TOTP.Platform.Unix;
#elif TOTP_PLATFORM_LINUX
using TOTP.Platform.Linux;
using TOTP.Platform.Unix;
#endif

namespace TOTP.Avalonia.Desktop.Startup;

internal static class DesktopPlatformServiceFactory
{
    public static DesktopPlatformServices Create()
    {
#if TOTP_PLATFORM_WINDOWS
        return new(new WindowsApplicationPaths(), new WindowsFileSecurity());
#elif TOTP_PLATFORM_MACOS
        return new(new MacOSApplicationPaths(), new UnixFileSecurity());
#elif TOTP_PLATFORM_LINUX
        return new(new LinuxApplicationPaths(), new UnixFileSecurity());
#else
        throw new PlatformNotSupportedException(
            "The Avalonia desktop host must be built on Windows, macOS, or Linux.");
#endif
    }
}

internal sealed record DesktopPlatformServices(
    IPlatformApplicationPaths ApplicationPaths,
    IPlatformFileSecurity FileSecurity);
