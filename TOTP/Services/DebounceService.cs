using System;
using System.Collections.Concurrent;
using System.Windows;
using System.Windows.Threading;
using TOTP.Services.Interfaces;

namespace TOTP.Services;

public class DebounceService : IDebounceService
{
    private readonly ConcurrentDictionary<string, DispatcherTimer> _timers = new();

    public void Debounce(string key, int milliseconds, Action action)
    {
        if (_timers.TryRemove(key, out var existingTimer))
        {
            existingTimer.Stop();
        }

        // make sure we are on the UI Dispatcher
        var timer = new DispatcherTimer(DispatcherPriority.Normal, Application.Current.Dispatcher);
        _timers[key] = timer;

        timer.Interval = TimeSpan.FromMilliseconds(milliseconds);
        EventHandler? onTimerTick = null;
        onTimerTick = (_, _) =>
        {
            timer.Stop();
            timer.Tick -= onTimerTick;
            _timers.TryRemove(key, out _);
            action();
        };

        timer.Tick += onTimerTick;

        timer.Start();
    }

    public void Cancel(string key)
    {
        if (_timers.TryRemove(key, out var timer)) timer.Stop();
    }
}
