using System;
using System.Windows;
using System.Windows.Interop;
using TOTP.Core.Security.Interfaces;

namespace TOTP.Services;

public sealed class HelloPromptWindowHandleProvider : IHelloPromptWindowHandleProvider
{
    public nint GetActiveWindowHandle()
    {
        var application = Application.Current;
        if (application == null)
        {
            return nint.Zero;
        }

        if (!application.Dispatcher.CheckAccess())
        {
            return application.Dispatcher.Invoke(GetActiveWindowHandle);
        }

        var window = application.MainWindow;
        if (window == null)
        {
            return nint.Zero;
        }

        return new WindowInteropHelper(window).EnsureHandle();
    }
}
