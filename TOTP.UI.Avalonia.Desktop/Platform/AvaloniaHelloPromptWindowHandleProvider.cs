using Avalonia.Threading;
using TOTP.Core.Security.Interfaces;

namespace TOTP.Avalonia.Desktop.Platform;

public sealed class AvaloniaHelloPromptWindowHandleProvider(
    AvaloniaWindowCoordinator windowCoordinator) : IHelloPromptWindowHandleProvider
{
    private readonly AvaloniaWindowCoordinator _windowCoordinator =
        windowCoordinator ?? throw new ArgumentNullException(nameof(windowCoordinator));

    public nint GetActiveWindowHandle()
    {
        if (!Dispatcher.UIThread.CheckAccess())
        {
            return Dispatcher.UIThread
                .InvokeAsync(GetActiveWindowHandle)
                .GetAwaiter()
                .GetResult();
        }

        return _windowCoordinator.CurrentActivationTarget?
            .TryGetPlatformHandle()?
            .Handle ?? nint.Zero;
    }
}
