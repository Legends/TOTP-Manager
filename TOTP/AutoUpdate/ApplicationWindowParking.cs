using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace TOTP.AutoUpdate;

internal sealed class ApplicationWindowParking
{
    private readonly HashSet<Window> _hiddenWindows = [];

    public void Park(Window updaterWindow)
    {
        ArgumentNullException.ThrowIfNull(updaterWindow);

        var application = Application.Current;
        if (application == null)
        {
            return;
        }

        foreach (var window in application.Windows.OfType<Window>())
        {
            if (!ShouldPark(window, updaterWindow) || _hiddenWindows.Contains(window))
            {
                continue;
            }

            _hiddenWindows.Add(window);
            window.Hide();
        }
    }

    public void Restore()
    {
        foreach (var window in _hiddenWindows
                     .OrderBy(window => window.Owner != null)
                     .ToArray())
        {
            if (window == null)
            {
                continue;
            }

            if (!window.IsVisible)
            {
                window.Show();
            }
        }

        _hiddenWindows.Clear();
    }

    public static bool IsParked(Window? window)
    {
        return window is { IsVisible: false };
    }

    private static bool ShouldPark(Window candidate, Window updaterWindow)
    {
        return candidate.IsVisible
            && !ReferenceEquals(candidate, updaterWindow)
            && candidate is not AutoUpdateDialogWindow;
    }
}
